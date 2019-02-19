namespace Bos.UI {
    using Bos.Data;
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProductView : GameBehaviour, IListItemView<ProductData> {



        public Text nameText;
        //public Image iconImage;
        public Text pointsText;
        public Text pointsCountText;
        public GameObject checkObject;
        public Button buyButton;
        public Text priceText;
        public GameObject particlesPrefab;

        private ProductData cachedProductData = null;

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ProductPurchased += OnProductPurchased;
            GameEvents.PlayerCashChanged += OnPlayerCashChanged;
        }

        public override void OnDisable() {
            GameEvents.ProductPurchased -= OnProductPurchased;
            GameEvents.PlayerCashChanged -= OnPlayerCashChanged;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

        public ProductData Data { get; private set; }

        public void Setup(ProductData productData ) {
            this.Data = productData;
            this.cachedProductData = productData;
            IResourceService resourceService = Services.ResourceService;
            ILocalizationRepository localization = resourceService.Localization;
            nameText.text = localization.GetString(productData.name_id);
            pointsText.text = "profile_status".GetLocalizedString();
            
            var points = BosUtils.GetCurrencyStringSimple(new CurrencyNumber(productData.status_points));
            pointsCountText.text = $"+{points}";

            if (Services.PlayerService.IsProductPurchased(productData.id)) {
                checkObject.Activate();
                buyButton.Deactivate();
            } else {
                checkObject.Deactivate();
                buyButton.Activate();
                var price = productData.price; //Services.GenerationService.CalculateProfit20Minute(productData.transport_Id, productData.transport_count, productData.planet_Id);
                priceText.text = BosUtils.GetCurrencyStringSimple(Bos.Data.Currency.CreatePlayerCash(price));
            }

            buyButton.SetListener(() => {
                if (Services.PlayerService.PurchaseProduct(productData) == TransactionState.Success) {
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.buyUpgrade);
                    //playe buy effect
                    CreateParticles();
                    ScaleEffect();

                }
            });

            updateTimer.Setup(0.3f, (deltaTime) => {
                UpdateButtonState(productData);
            }, invokeImmediatly: true);
        }

        private void ScaleEffect() {
            Vector3Animator scaleAnimator = gameObject.GetOrAdd<Vector3Animator>();
            RectTransform rectTransform = GetComponent<RectTransform>();
            scaleAnimator.StartAnimation(rectTransform.ConstructScaleAnimationData(Vector3.one, 0.85f * Vector3.one, 0.2f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                scaleAnimator.StartAnimation(rectTransform.ConstructScaleAnimationData(0.85f * Vector3.one, Vector3.one, 0.2f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => { }));
            }));
        }

        private void CreateParticles() {
            GameObject inst = Instantiate(particlesPrefab, transform, false);
            RectTransform particlesTransform = inst.GetComponent<RectTransform>();
            particlesTransform.anchoredPosition = new Vector2(131, -147 );
            var basePlayerView = FindObjectOfType<BasePlayerView>();
            if(!basePlayerView) {
                Destroy(inst);
            } else {
                particlesTransform.SetParent(basePlayerView.progressParticles.transform.parent, true);
                particlesTransform.GetComponent<BezierMover>().Setup(AnimUtils.GetBezierQubicData(particlesTransform.GetComponent<RectTransformPositionObject>(),
                    start: particlesTransform.anchoredPosition,
                    end: basePlayerView.progressParticles.GetComponent<RectTransform>().anchoredPosition,
                    interval: 1.5f,
                    onComplete: (go) => {
                        Services.Execute(() => Destroy(inst), 0.5f);
                    }));
            }
        }


        private void UpdateButtonState(ProductData data) {
            if (data != null) {
                TransactionState state;
                buyButton.interactable = Services.PlayerService.IsAllowPurchaseProduct(data, out state);
                if (state == TransactionState.DontEnoughCurrency) {
                    //red is BAD
                    //priceText.color = Color.red;
                    priceText.color = Color.white;
                } else {
                    priceText.color = Color.white;
                }
            } else {
                Debug.LogError($"product data is null for id => {cachedProductData?.id ?? -1}");
            }
        }

        private void OnProductPurchased(ProductData data) {

            if(cachedProductData != null && data.id == cachedProductData.id) {
                Setup(data);
            }
        }

        private void OnPlayerCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) {
            if (cachedProductData != null) {
                UpdateButtonState(cachedProductData);
            } else {
                buyButton.interactable = false;
            }
        }
    }

}