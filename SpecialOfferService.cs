namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;
    using Ozh.Tools.Functional;
    using Newtonsoft.Json;
    using System.Text;
    using Ozh.Tools.Dotnet;
    using Bos.UI;
    using Bos.Data;

    public class SpecialOfferService : SaveableGameBehaviour, ISpecialOfferService {

        private readonly int FiveMinutes = 5 * 60;
        public const int kOfferProductId = 31;

        private SpecialOfferState offerState = SpecialOfferState.NotStarted;
        private int expireTime = 0;
        private int planetStartTime = 0;

        private GameObject specialOfferButtonObject;



        #region Events

        public override void Start() {
            base.Start();

            Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(val => {
                ToggleMainScreenButton();
            }).AddTo(gameObject);

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                UpdateOfferState();
            }).AddTo(gameObject);
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
            GameEvents.LegacyViewRemoved += OnLegacyViewRemoved;
            GameEvents.StoreProductPurchasedEvent += OnStoreProductPurchased;
        }

        public override void OnDisable() {
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            GameEvents.LegacyViewRemoved -= OnLegacyViewRemoved;
            GameEvents.StoreProductPurchasedEvent -= OnStoreProductPurchased;
            base.OnDisable();
        }

        private void OnStoreProductPurchased(StoreProductData data) {
            if(IsLoaded ) {
                ToggleMainScreenButton();
            }
        }

        private void OnLegacyViewRemoved(string view) {
            if(IsLoaded ) {
                ToggleMainScreenButton();
            }
        }

        private void OnViewHided(ViewType viewType ) {
            if(IsLoaded ) {
                ToggleMainScreenButton();
            }
        }

        private void OnViewShowed(ViewType viewType ) {
            if(IsLoaded) {
                HideMainScreenButton();
            }
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if(IsLoaded ) {
                if(newState == PlanetState.Opened && planet.Id != PlanetConst.EARTH_ID ) {
                    planetStartTime = Services.TimeService.UnixTimeInt;
                    offerState = SpecialOfferState.NotStarted;
                }
            }
        }

        private void StartOffer() {
            offerState = SpecialOfferState.Started;
            expireTime = Services.TimeService.UnixTimeInt + 600;
            StartCoroutine(ShowOfferViewImpl());
        }

        private IEnumerator ShowOfferViewImpl() {
            yield return new WaitUntil(() => GameMode.GameModeName == GameModeName.Game && ViewService.IsNoModalAndLegacyViews);
            ViewService.Show(ViewType.SpecialOfferView);
        }

        private void UpdateOfferState() {
            if (IsLoaded && GameMode.IsGame) {
                switch (offerState) {
                    case SpecialOfferState.NotStarted: {
                            if (Planets.CurrentPlanetId.Id != PlanetConst.EARTH_ID) {
                                if (PlanetStartInterval > FiveMinutes) {
                                    StartOffer();
                                }
                            }
                        }
                        break;
                    case SpecialOfferState.Started: {
                            if (IsExpired) {
                                offerState = SpecialOfferState.Expired;
                            }
                        }
                        break;
                    case SpecialOfferState.Expired: {

                        }
                        break;
                    case SpecialOfferState.Completed: {

                        }
                        break;
                }
            }
        }
        #endregion

        #region ISpecialOfferService

        public void Setup(object data = null) {

        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(SpecialOfferService)}.{nameof(UpdateResume)}() => {pause}");


        public bool IsMainScreenViewVisible
            => IsLoaded &&
               GameMode.IsLoaded &&
               GameMode.GameModeName == GameModeName.Game &&
               Planets.IsLoaded &&
               Planets.CurrentPlanetId.Id != PlanetConst.EARTH_ID &&
               PlanetStartInterval >= FiveMinutes &&
               !IsExpired &&
               !IsCompleted;

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"State = {offerState}");
            stringBuilder.AppendLine($"expire time = {expireTime}, current time = {TimeService.UnixTimeInt}, et-ct = {expireTime - TimeService.UnixTimeInt}");
            stringBuilder.AppendLine($"planet start time = {planetStartTime}, current time = {TimeService.UnixTimeInt}, ct - pt = {TimeService.UnixTimeInt - planetStartTime}");
            stringBuilder.AppendLine("Main Screen Visible");
            stringBuilder.AppendLineTabbed(1, $"is loaded = {IsLoaded}, game mode loaded = {GameMode.IsLoaded}, game mode is Game = {GameMode.GameModeName == GameModeName.Game}");
            stringBuilder.AppendLineTabbed(1, $"Planets loaded = {Planets.IsLoaded}, current planet not EARTH = {Planets.CurrentPlanetId.Id != PlanetConst.EARTH_ID}");
            stringBuilder.AppendLineTabbed(1, $"planet start time > 5min = {PlanetStartInterval >= FiveMinutes }");
            stringBuilder.AppendLineTabbed(1, $"not expired = {!IsExpired}, not completed = {!IsCompleted}");
            stringBuilder.AppendLineTabbed(1, $"modal count = {ViewService.ModalCount}, legacy count = {ViewService.LegacyCount}");
            return stringBuilder.ToString();
        }


        public int ExpireInterval {
            get {
                int interval = expireTime - Services.TimeService.UnixTimeInt;
                if(interval < 0 ) {
                    interval = 0;
                }
                return interval;
            }
        }

        public bool IsExpired {
            get {
                return ExpireInterval == 0;
            }
        }

        public void BuyOffer() {
            //FindObjectOfType<IAPManager>()?.Purchase(kOfferProductId);
            var productData = ResourceService.Products.GetProduct(kOfferProductId);
            if (productData != null) {
                Services.Inap.PurchaseProduct(productData);
            } else {
                UnityEngine.Debug.LogError($"not found product id {kOfferProductId}");
            }
        }


        public void OnOfferPurchased() {
            offerState = SpecialOfferState.Completed;
            Player.AddPlayerCash(new CurrencyNumber(PlayerCashReward));
            Player.AddCompanyCash(ComplanyCashReward);
            Player.AddCoins(CoinsReward);
        }

        public double PlayerCashReward
            => 1000 * Player.MaxPlayerCash;

        public double ComplanyCashReward
            => 1000 * Player.MaxCompanyCash;

        public int CoinsReward {
            get {
                int planetId = Planets.CurrentPlanetId.Id;
                int coins = 0;
                foreach (var generatorLocalData in ResourceService.GeneratorLocalData.LocalDatas) {
                    generatorLocalData.GetResearchPrice(planetId).Match(() => {
                        return F.None;
                    }, (price) => {
                        coins += price.price;
                        return F.Some(price);
                    });
                }
                return coins;
            }
        }

        public int MaxBalanceBonus => 1000;

        public void ForceStart() {
            StartOffer();
        }
        #endregion


        private void ToggleMainScreenButton() {
            if (IsLoaded && GameMode.IsLoaded) {
                if ((ViewService.ModalCount == 0 && ViewService.LegacyCount == 0) && (IsMainScreenViewVisible)) {
                    ShowMainScreenButton();
                } else {
                    HideMainScreenButton();
                }
            }
        }

        private GameObject SpecialOfferButtonObject {
            get {
                if(specialOfferButtonObject == null ) {
                    OfferButtonContainer container = FindObjectOfType<OfferButtonContainer>();
                    specialOfferButtonObject = container?.specialOfferButtonObject ?? null;
                }
                return specialOfferButtonObject;
            }
        }

        private void ShowMainScreenButton() {

            SpecialOfferButtonObject?.Activate();
        }

        private void HideMainScreenButton() {

            SpecialOfferButtonObject?.Deactivate();
        }



        private int PlanetStartInterval
            => Services.TimeService.UnixTimeInt - planetStartTime;

        public bool IsCompleted {
            get {
                return offerState == SpecialOfferState.Completed;
            }
        }

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "special_offer_service";

        public override Type SaveType => typeof(SpecialOfferServiceSave);


        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetByWinGame() {
            IsLoaded = true;
        }

        public override object GetSave() {
            return new SpecialOfferServiceSave {
                expireTime = expireTime,
                offerState = offerState,
                planetStartTime = planetStartTime
            };
        }

        public override void LoadDefaults() {
            StartCoroutine(LoadDefaultPlanetStartTime());
            offerState = SpecialOfferState.NotStarted;
            IsLoaded = true;
        }

        private IEnumerator LoadDefaultPlanetStartTime() {
            yield return new WaitUntil(() => Services.TimeService.IsValid);
            planetStartTime = Services.TimeService.UnixTimeInt;
        }

        public override void LoadSave(object obj) {
            SpecialOfferServiceSave save = obj as SpecialOfferServiceSave;
            if(save != null ) {
                expireTime = save.expireTime;
                planetStartTime = save.planetStartTime;
                offerState = save.offerState;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }



        #endregion
    }

    public class SpecialOfferServiceSave {

        public Dictionary<int, bool> completedOffsers;
        public int expireTime;
        public int planetStartTime;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public SpecialOfferState offerState;
    }


    public interface ISpecialOfferService : IGameService {

        bool IsMainScreenViewVisible { get; }
        int ExpireInterval { get; }
        void BuyOffer();
        void OnOfferPurchased();
        double PlayerCashReward { get; }
        double ComplanyCashReward { get;  }
        int CoinsReward { get; }
        int MaxBalanceBonus { get; }
        bool IsExpired { get; }
        void ForceStart();
        bool IsCompleted { get; }
    }

    public enum SpecialOfferState {
        NotStarted,
        Started,
        Expired,
        Completed
    }
}