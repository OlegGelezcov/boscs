namespace Bos {
    using Bos.Data;
    using Facebook.Unity;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UDBG = UnityEngine.Debug;
    using UnityProductType = UnityEngine.Purchasing.ProductType;


    public class InapService : SaveableGameBehaviour, IInapService {

        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;
        private ConfigurationBuilder configurationBuilder;
        private Dictionary<string, int> PurchasedItems { get; } = new Dictionary<string, int>();

        private StoreProductReward ProductReward { get; set; }

        public bool IsInitialized
            => storeController != null && storeExtensionProvider != null;

        public void Setup(object data = null) {
            ProductReward = new StoreProductReward(Services);
            StartCoroutine(InitializeImpl());
        }

        public void UpdateResume(bool pause)
            => UDBG.Log($"{nameof(InapService)}.{nameof(UpdateResume)}() => {pause}");

        private IEnumerator InitializeImpl() {
            yield return new WaitUntil(() => ResourceService.IsLoaded && GameMode.IsLoaded && GameMode.IsGame);
            if (!IsInitialized) {
                InitializePurchasing();
            }

            
        }

        private void InitializePurchasing() {
            configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach(var productData in ResourceService.Products.Products) {
                //UDBG.Log($"add product {productData.StoreId}, consumable: {productData.IsConsumable}"
                //.Attrib(bold: true, italic: true, color: "magenta"));
                configurationBuilder.AddProduct(productData.StoreId, productData.IsConsumable ? UnityProductType.Consumable : UnityProductType.Subscription);
            }
            UnityPurchasing.Initialize(this, configurationBuilder);
        }


        #region IStoreListener
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            storeController = controller;
            storeExtensionProvider = extensions;
            UDBG.Log("UnityPurchasing:OnInitialized".Attrib(bold: true, italic: true, color: "g"));
            
        }

        public void OnInitializeFailed(InitializationFailureReason error) {
            UDBG.Log($"UnityPurchasing:OnInitializeFailed: {error}".Attrib(bold: true, italic: true, color: "r"));
        }

        public void OnPurchaseFailed(Product i, PurchaseFailureReason p) {
            RemoveLoadingScreen();
            UDBG.Log($"UnityPurchasing: Purchase product: {i.definition.id} failed with reason: {p}".Attrib(bold: true, italic: true, color: "r"));
        }

        private void AddPurchasedItem(Product product ) {
            if(PurchasedItems.ContainsKey(product.definition.id)) {
                PurchasedItems[product.definition.id]++;
            } else {
                PurchasedItems.Add(product.definition.id, 1);
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
            RemoveLoadingScreen();
            var productData = ResourceService.Products.GetProductByStoreId(e.purchasedProduct.definition.id);
            if(productData == null ) {
                UDBG.Log($"UnityPurchasing: Process purchase error, not found resource product for purchasing: {e.purchasedProduct.definition.id}".Attrib(bold: true, italic: true, color: "r"));
                return PurchaseProcessingResult.Pending;
            }

            if(false == InapUtils.IsProductReceiptValid(e.purchasedProduct)) {
                UDBG.Log($"receipt for product {productData.StoreId} invalid".Attrib(bold: true, italic: true, color: "r"));
                return PurchaseProcessingResult.Pending;
            }

            if(InapUtils.IsDeviceJailBrokened()) {
                UDBG.Log($"Error: is device jail brokened".Attrib(bold: true, italic: true, color: "r"));
                return PurchaseProcessingResult.Pending;
            }
            AddPurchasedItem(e.purchasedProduct);
            ProductReward.Execute(productData);

            bool validPurchase = true;

#if UNITY_IOS
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        var appleConfig = builder.Configure<IAppleConfiguration>();
        var receiptData = Convert.FromBase64String(appleConfig.appReceipt);
        IEnumerator verifyReceipt = InapUtils.VerifyIosReceipt(receiptData);
        while (verifyReceipt.MoveNext())
        {
            if (verifyReceipt.Current != null)
            {
                UnityEngine.Debug.Log(verifyReceipt.Current as bool? ?? false);
                validPurchase = verifyReceipt.Current as bool? ?? false;
            }
        }
#endif
            if (validPurchase)
            {
                LogPurchaseAppFirebase(e.purchasedProduct);
                LogPurchaseFacebook(e.purchasedProduct);
            }
           
            try {
                Services.SaveService.SaveAll();
            } catch(Exception ex) { }

            return PurchaseProcessingResult.Complete;
        }
        #endregion


        #region IInapService


        public void PurchaseProduct(StoreProductData product ) {

            UDBG.Log($"start purchasing with store id: {product.StoreId}");
            
#if UNITY_EDITOR || BOSDEBUG
            ProductReward.Execute(product);
            return;
#else
            if(product == null ) {
                UDBG.Log($"Product what you try purchase is NULL".Attrib(bold: true, italic: true, color: "r"));
                return;
            }
            if(!IsInitialized) {
                UDBG.LogError($"UnityPurchasing is not initialized".Attrib(bold: true, italic: true, color: "r"));
                return;
            }

            Product unityProduct = storeController.products.WithID(product.StoreId);
            if(unityProduct == null ) {
                UDBG.Log($"Purchase error: product with store ID: {product.StoreId} is NULL".Attrib(bold: true, italic: true, color: "r"));
                return;
            }
            if(false == unityProduct.availableToPurchase) {
                UDBG.Log($"Purchase error: product with store ID: {product.StoreId} is not available to purchase".Attrib(bold: true, italic: true, color: "r"));
                return;
            }

            CreateLoadingScreen();
            storeController.InitiatePurchase(unityProduct);
#endif
        }

        public Option<Product> GetProductByResourceId(int id) {
            if (IsInitialized) {
                StoreProductData productData = ResourceService.Products.GetProduct(id);
                if (productData != null) {
                    return GetProductByStoreId(productData.StoreId);
                }
            }
            return F.None;
        }

        public Option<Product> GetProductByStoreId(string storeId ) {
            if(IsInitialized ) {
                Product product = storeController.products.WithID(storeId);
                if (product != null) {
                    return F.Some(product);
                }
            }
            return F.None;
        }
#endregion

#region Private members
        private void CreateLoadingScreen() {
            ViewService.Show(UI.ViewType.IAPLoadingView);
        }

        private void RemoveLoadingScreen() {
            ViewService.Remove(UI.ViewType.IAPLoadingView);
        }

        private void LogPurchaseAppFirebase(Product product) {
            try {
                var param1 = new Firebase.Analytics.Parameter("productId", product.definition.id);
                var param2 = new Firebase.Analytics.Parameter("localizedPrice", product.metadata.localizedPrice.ToString());
                var param3 = new Firebase.Analytics.Parameter("isoCurrencyCode", product.metadata.isoCurrencyCode);
                Firebase.Analytics.FirebaseAnalytics.LogEvent("purchase", param1, param2, param3);
                
            } catch(Exception ex) { }
        }

        private void LogPurchaseFacebook(Product product) {
            try {
                var iapParameters = new Dictionary<string, object>();
                iapParameters["productId"] = product.definition.id;
                if (FB.IsInitialized)
                    FB.LogPurchase(
                        (float)product.metadata.localizedPrice,
                        product.metadata.isoCurrencyCode,
                        iapParameters
                    );
            } catch(Exception ex) {}
        }
        #endregion


        #region ISaveable
        public override void ResetByInvestors() { LoadDefaults(); }
        public override void ResetByPlanets() { LoadDefaults();  }
        public override void ResetByWinGame() { LoadDefaults();  }
        public override void ResetFull() { LoadDefaults(); }

        public override void LoadDefaults() {
            IsLoaded = true;
        }
        public override string SaveKey => "inap_service";
        public override Type SaveType => typeof(InapServiceSave);
        public override void LoadSave(object obj) {
            InapServiceSave save = obj as InapServiceSave;
            if(save == null ) {
                LoadDefaults();
            } else {
                save.Validate();
                PurchasedItems.Clear();
                PurchasedItems.CopyFrom(save.purchasedItems);
                IsLoaded = true;
            }
        }
        public override object GetSave() {
            return new InapServiceSave {
                purchasedItems = PurchasedItems.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
        #endregion
    }


    public interface IInapService : IGameService, IStoreListener {
        void PurchaseProduct(StoreProductData product);
        Option<Product> GetProductByResourceId(int id);
        Option<Product> GetProductByStoreId(string storeId);
    }

    public class InapServiceSave {
        public Dictionary<string, int> purchasedItems;

        public void Validate() {
            if(purchasedItems == null ) {
                purchasedItems = new Dictionary<string, int>();
            }
        }
    }
}