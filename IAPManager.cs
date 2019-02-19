/*
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Facebook.Unity;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Analytics;
using UnityEngine.Purchasing.Security;
using Bos;
using Bos.Data;
using Ozh.Tools.Functional;

public class IAPManager : GameBehaviour, IStoreListener
{

    public static IAPManager instance;

    public override void Awake()
    {
        instance = this;
    }
    public bool IsInitialized { get { return _storeController != null && _storeExtensionProvider != null; } }
    private IStoreController _storeController;
    private IExtensionProvider _storeExtensionProvider;
    private ConfigurationBuilder _builder;

    public GameObject PurchaseLoadingScreen;



    public void Purchase(int id) {
        PurchaseCoins(id);
    }

    public void PurchaseCoins(int id)
    {
        var data = Services.ResourceService.Products.GetProduct(id);

        var storeId = data.StoreId;
        var price = data.Price;

        Analytics.CustomEvent(AnalyticsStrings.COIN_SHOP_BUY, new Dictionary<string, object>
            {
                { "productId", storeId}
            });

        BuyProduct(storeId);

    }


    public void AddCoins(int coins, bool free = false)
    {
        Services.PlayerService.AddCoins(coins, free);

        //Coins.Value += coins;


    }


    public override void Start()
    {
        PurchaseLoadingScreen.SetActive(false);
        if (_storeController == null)
        {
            InitializePurchasing();
        }
    }

    private void InitializePurchasing()
    {
        _builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var product in Services.ResourceService.Products.Products)
        {
            _builder.AddProduct(product.StoreId, UnityEngine.Purchasing.ProductType.Consumable);
        }

        _builder.AddProduct("tc_sub_10cpd", UnityEngine.Purchasing.ProductType.Subscription);

        UnityPurchasing.Initialize(this, _builder);
    }

    public void BuyProduct(string storeId)
    {

#if UNITY_EDITOR || BOSDEBUG
        var produt = Services.ResourceService.Products.GetProductByStoreId(storeId);
        GiveProduct(produt);
        return;
#else
        //if (Services.StoreService.IsDebug) {
        //    var produt = Services.ResourceService.Products.GetProductByStoreId(storeId);
        //    GiveProduct(produt);
        //} else {

            if (IsInitialized) {

                Product product = _storeController.products.WithID(storeId);

                if (product != null && product.availableToPurchase) {
                    PurchaseLoadingScreen.SetActive(true);

                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    _storeController.InitiatePurchase(product);
                } else {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            } else {
                Debug.Log("BuyProduct FAIL. Not initialized.");
            }
        //}
#endif
    }



    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized)
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = _storeExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) =>
            {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }
    
    private void GiveProduct(StoreProductData product ) {
        var price = product.Price;
        var coins = product.Coins;
        AddCoins(coins);
        PlayerPrefs.Save();
        StatsCollector.Instance[Stats.MONEY_SPENT] += price;

        if (product.HasBonus) {
            switch (product.BonusType) {
                case RewardType.SpeedUpgrade:
                    //GlobalRefs.PlayerData.GlobalProfitMultiplier *= product.BonusValue;
                    //Services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateTimeMult(product.BonusValue));
                    Services.GenerationService.Generators.AddTimeBoost(
                        boost: BoostInfo.CreateTemp(
                            id: $"iap_{product.StoreId}_".GuidSuffix(5),
                            value: product.BonusValue));
                    break;
                case RewardType.ProfitUpgrade:
                    //GlobalRefs.PlayerData.GlobalProfitMultiplier *= product.BonusValue;
                    //Services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateProfitMult(product.BonusValue));
                    Services.GenerationService.Generators.AddProfitBoost(
                        boost: BoostInfo.CreateTemp(
                            id: $"iap_{product.StoreId}_".GuidSuffix(5),
                            value: product.BonusValue));
                    break;
                case RewardType.MiniGameTry:
                    GlobalRefs.MiniGamesScreen.IncreaseTriesPaid(MiniGameType.Casino);
                    break;
                case RewardType.MiniGameRaceTry:
                    GlobalRefs.MiniGamesScreen.IncreaseTriesPaid(MiniGameType.Race);
                    break;
                case RewardType.Lootbox:
                    //GlobalRefs.PlayerData.AvailableRewards += product.BonusValue;
                    Services.RewardsService.AddAvailableRewards(product.BonusValue);
                    break;
                case RewardType.CashReward: {
                        Player.AddGenerationCompanyCash(Player.MaxCompanyCash * product.BonusValue);
                    }
                    break;
                case RewardType.SecuritiesReward: {
                        Services.PlayerService.AddSecurities(new CurrencyNumber(Services.PlayerService.MaxSecurities * product.BonusValue));
                    }
                    break;
                case RewardType.PlayerCashReward: {
                        Services.PlayerService.AddPlayerCash(new CurrencyNumber(Services.PlayerService.MaxPlayerCash * product.BonusValue));
                    }
                    break;
                case RewardType.SpecialOfferBundle: {
                        Services.GetService<ISpecialOfferService>().OnOfferPurchased();
                    }
                    break;
                case RewardType.None:
                    break;
            }
        }

        GameEvents.StoreProductPurchasedObservable.OnNext(product);
    }
    
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        var product = Services.ResourceService.Products.GetProductByStoreId(e.purchasedProduct.definition.id); //Products.FirstOrDefault(x => x.StoreIdentifier == e.purchasedProduct.definition.id);
        
        bool validPurchase = true; // Presume valid for platforms with no R.V.

        // Unity IAP's validation logic is only included on these platforms.
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        // Prepare the validator with the secrets we prepared in the Editor
        // obfuscation window.
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.identifier);

        try {
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(e.purchasedProduct.receipt);
            // For informational purposes, we list the receipt(s)
            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result) {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate);
                Debug.Log(productReceipt.transactionID);
            }
        } catch (IAPSecurityException) {
            Debug.Log("Invalid receipt, not unlocking content");
            validPurchase = false;
        }
#endif

        if (validPurchase && product != null) {
            
            Debug.Log($"ProcessPurchase: PASS. Product: '{e.purchasedProduct.definition.id}'");


            GiveProduct(product);


            if (!IsJailBrokenOrRooted()) // analitics
            {
                var dict = new Dictionary<string, object>();
                dict["productId"] = product.StoreId;
                dict["localizedPrice"] = e.purchasedProduct.metadata.localizedPrice.ToString();
                dict["isoCurrencyCode"] = e.purchasedProduct.metadata.isoCurrencyCode;
                AppMetrica.Instance.ReportEvent("In-app", dict);
            
                var iapParameters = new Dictionary<string, object>();
                iapParameters["productId"] = product.StoreId;
                if (FB.IsInitialized)
                    FB.LogPurchase(
                        (float)e.purchasedProduct.metadata.localizedPrice,
                        e.purchasedProduct.metadata.isoCurrencyCode,
                        iapParameters
                    ); 
            
                Analytics.CustomEvent(AnalyticsStrings.COIN_SHOP_BUY, new Dictionary<string, object>
                {
                    { "productId", product.StoreId}
                });
            }
        }
        
        PurchaseLoadingScreen.SetActive(false);
        return PurchaseProcessingResult.Complete;
    }

    private static bool IsJailBrokenOrRooted()
    {
        return InapUtils.IsDeviceJailBrokened();
    }

    public void CheckIfSubscriptionIsActive(Product product)
    {
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

        try
        {
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(product.receipt);

            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate);
                Debug.Log(productReceipt.transactionID);
            }
        }
        catch (IAPSecurityException)
        {
            Debug.Log("Invalid receipt, not unlocking content");
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        PurchaseLoadingScreen.SetActive(false);
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
    
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _storeExtensionProvider = extensions;

        Debug.Log("IAP::OnInitialized: PASS");
    }
    
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public Product GetProduct(int id)
    {
        var data = Services.ResourceService.Products.GetProduct(id);
        return (data != null) ? _storeController.products.WithID(data.StoreId) : null;

        //return _map.ContainsKey(id) ? _storeController.products.WithID(_map[id].StoreIdentifier) : null;
    }

    public Option<Product> Product(int id) {
        var prod = GetProduct(id);
        if(prod != null ) {
            return F.Some(prod);
        }
        return F.None;
    }
    
    public Product GetProductByShopId(string id)
    {
        return _storeController.products.WithID(id);
    }

    public StoreProductData GetStoreListingById(int id)
    {
        return Services.ResourceService.Products.GetProduct(id);
        //return _map.ContainsKey(id) ? _map[id] : null;
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        SaveCoins();
    }

    public void SaveCoins() {
        Services.SaveService.Save((ISaveable)Services.PlayerService.Currency);
        //PlayerPrefs.SetInt("Coins", Services.PlayerService.Currency.Coins);
        PlayerPrefs.Save();
    }
}

*/

