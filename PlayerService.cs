namespace Bos {
    using Bos.Data;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;

    public class PlayerService : SaveableGameBehaviour, IPlayerService {

        private readonly CurrencyInfo currency = new CurrencyInfo();
        private bool isNeedResetCashAfterPlanets;

        private double sessionEarningCompanyCash = 0.0;
        public double MaxCompanyCash => currency.MaxCompanyCash;
        public double MaxSecurities => currency.MaxSecurities;
        public double MaxPlayerCash => currency.MaxPlayerCash;
        public Gender Gender {get; private set;} = Gender.Male;
        public int Level {get; private set;} = 1;
        public int CurrentXP {get; private set;} = 0;
        public int XPLevelLimit {get; private set;} = 2000;
        public double LifetimeEarnings { get; private set; } = 0.0;
        public double LifetimeEarningsInPlanet { get; private set; } = 0.0;
        public bool IsHasMicromanager { get; private set; }

        public PlayerData LegacyPlayerData { get; } = new PlayerData();

        public List<int> PurchasedProducts { get; } = new List<int>();

        public long StatusPoints { get; private set; }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GameModeChanged += OnGameModeChanged;
            GameEvents.Pause += OnPause;
            GameEvents.UnofficialTransfer += OnUnofficialTransfer;
            GameEvents.OfficialTransfer += OnOfficialTransfer;
            GameEvents.ProductPurchased += OnProductPurchased;
        }

        public override void OnDisable() {
            GameEvents.GameModeChanged -= OnGameModeChanged;
            GameEvents.Pause -= OnPause;
            GameEvents.UnofficialTransfer -= OnUnofficialTransfer;
            GameEvents.OfficialTransfer -= OnOfficialTransfer;
            GameEvents.ProductPurchased -= OnProductPurchased;
            base.OnDisable();
        }
        public ProductAvailableNotifier ProductNotifier { get; } = new ProductAvailableNotifier();

        public void Setup(object data = null) {
            ProductNotifier.Setup(Services);
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(PlayerService)}.{nameof(UpdateResume)} => {pause}");


        #region Game events

        private void OnProductPurchased(ProductData data ) {
            ProductNotifier.OnProductPurchased();
        }

        private void OnOfficialTransfer(TransferCashInfo info ) {
            ProductNotifier.OnOfficialTransfer(info);
        }
        private void OnUnofficialTransfer(UnofficialTransferCashInfo info ) {
            ProductNotifier.OnUnofficialTransfer(info);
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode) {
            if(newGameMode == GameModeName.Game ) {
                if(isNeedResetCashAfterPlanets) {
                    isNeedResetCashAfterPlanets = false;
                    SetCompanyCash(Services.GenerationService.CalculatePrice(count: 1, ownedCount: 0, generatorId: 0) + 1);
                }
            }
        }

        private void OnPause() {
            if(IsLoaded ) {
                LocalData.LastPauseBalance = CompanyCash.Value;
            }
        }
        #endregion

        #region Saveable implementation
        public override void ResetByInvestors() {
            //do nothing
        }

        public override void ResetByPlanets() {
            //do nothing
            isNeedResetCashAfterPlanets = true;
            LifetimeEarningsInPlanet = 0;
        }

        public override void ResetFull() {
            LoadDefaults();
        }
        public override void Register() {
            base.Register();
            currency.Register();
        }

        public override string SaveKey => "player";

        public override Type SaveType => typeof(PlayerSave);

        public override object GetSave() {
            List<int> purchasedProducts = new List<int>();
            foreach(int id in PurchasedProducts) {
                purchasedProducts.Add(id);
            }

            return new PlayerSave {
                gender = (int)Gender,
                level = Level,
                currentXP = CurrentXP,
                XPLevelLimit = this.XPLevelLimit,
                purchasedProducts = purchasedProducts,
                statusPoints = StatusPoints,
                lifetimeEarnings = LifetimeEarnings,
                lifetimeEarningsInPlanet = LifetimeEarningsInPlanet,
                //maxCompanyCash = MaxCompanyCash,
                isHasMicromanager = IsHasMicromanager
            };
        }

        public override void LoadDefaults() {
            Gender = Gender.Male;
            Level = 1;
            CurrentXP = 0;
            XPLevelLimit = 2000;
            PurchasedProducts.Clear();
            StatusPoints = 0;
            LifetimeEarnings = 0.0;
            LifetimeEarningsInPlanet = 0.0;
            //MaxCompanyCash = 0.0;
            IsHasMicromanager = false;

            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            Level = 1;
            CurrentXP = 0;
            XPLevelLimit = 2000;
            PurchasedProducts.Clear();
            StatusPoints = 0;
            LifetimeEarnings = 0f;
            LifetimeEarningsInPlanet = 0f;
            //MaxCompanyCash = 0f;
            IsHasMicromanager = false;
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            PlayerSave save = obj as PlayerSave;
            if(save != null ) {
                Gender = (Gender)save.gender;
                Level = save.level;
                CurrentXP = save.currentXP;
                XPLevelLimit = save.XPLevelLimit;

                PurchasedProducts.Clear();
                if(save.purchasedProducts == null ) {
                    save.purchasedProducts = new List<int>();
                }
                foreach(int id in save.purchasedProducts) {
                    PurchasedProducts.Add(id);
                }
                StatusPoints = save.statusPoints;
                LifetimeEarnings = save.lifetimeEarnings;
                LifetimeEarningsInPlanet = save.lifetimeEarningsInPlanet;
                //MaxCompanyCash = save.maxCompanyCash;
                IsHasMicromanager = save.isHasMicromanager;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        #endregion

        #region IPlayerService
        public void SetHasMicromanager(bool value) {
            bool oldValue = IsHasMicromanager;
            IsHasMicromanager = value;
            if (oldValue != value) {
                GameEvents.OnMicromanagerStateChanged(oldValue, value);
            }
        }

        public bool IsProductPurchased(int productId)
            => PurchasedProducts.Contains(productId);


        public TransactionState PurchaseProduct(ProductData data) {
            if (!IsProductPurchased(data.id))
            {
                var price = data.price;
                Bos.Data.Currency currency = Bos.Data.Currency.CreatePlayerCash(price);
                if (IsEnough(currency)) {
                    RemoveCurrency(currency);
                    AddStatusPoints(data.status_points);
                    PurchasedProducts.Add(data.id);
                    GameEvents.OnProductPurchased(data);
                    return TransactionState.Success;
                } else {
                    return TransactionState.DontEnoughCurrency;
                }
            } else {
                return TransactionState.AlreadyPurchased;
            }
        }

        public bool IsAllowPurchaseProduct(ProductData data, out TransactionState state) {
            state = TransactionState.Success;

            if (IsProductPurchased(data.id)) {
                state = TransactionState.AlreadyPurchased;
                return false;
            }

            var price = data.price;
            if (IsEnough(Bos.Data.Currency.CreatePlayerCash(price)) == false) {
                state = TransactionState.DontEnoughCurrency;
                return false;
            }
            return true;
        }


        public void AddStatusPoints(long count) {
            long oldCount = StatusPoints;

            StatusPoints += count;
            if (oldCount != StatusPoints) {
                GameEvents.OnStatusPointsChanged(oldCount, StatusPoints);
                int oldLevel = Services.ResourceService.PersonalImprovements.GetStatusLevel(oldCount);
                int newLevel = Services.ResourceService.PersonalImprovements.GetStatusLevel(StatusPoints);
                if (newLevel != oldLevel) {
                    GameEvents.OnStatusLevelChanged(oldLevel, newLevel);
                }
            }
        }

        public bool HasPurchasedProducts() {
            return PurchasedProducts.Count > 0;
        }


        public void SetGender(Gender value) {
            Gender oldValue = Gender;
            Gender = value;
            if (oldValue != Gender) {
                GameEvents.OnGenderChanged(oldValue, Gender);
            }
        }

        public void AddXPLevelLimit(int value) {
            int oldValue = XPLevelLimit;
            XPLevelLimit += Mathf.Abs(value);
            if(oldValue != XPLevelLimit) {
                GameEvents.OnXPLevelLimitChanged(oldValue, XPLevelLimit);
            }
        }
        public void AddXP(int count) {
            int oldCount = CurrentXP;
            CurrentXP += Mathf.Abs( count ); 
            if(oldCount != CurrentXP) {
                GameEvents.OnXPChanged(oldCount, CurrentXP);
            }
        }

        public float ExpProgress01
            => (float)CurrentXP / (float)XPLevelLimit;
        

        public void RemoveXP(int count) {
            int oldCount =CurrentXP;
            CurrentXP -= Mathf.Abs(count);
            if(oldCount != CurrentXP) {
                GameEvents.OnXPChanged(oldCount, CurrentXP);
            }
        }

        public void AddLevel(int count) {
            for(int i = 0; i < count; i++ ) {
                int oldLevel = Level;
                Level++;
                GameEvents.OnLevelChanged(oldLevel, Level);
            }
        }

        public void AddGenerationCompanyCash(double value) {
            double mult = Player.LegacyPlayerData.TempMultiplier;

            AddCompanyCash(value * mult);


        }

        private void AddLifeTimeEarnings(double count) {
            LifetimeEarnings += Math.Abs(count);
            LifetimeEarningsInPlanet += Math.Abs(count);
        }

        public void SetLifetimeEarnings(double value) {
            LifetimeEarnings = value;
        }

        public double SessionEarningsCompanyCash
            => sessionEarningCompanyCash;

        /*
        public void SetMaxCompanyCash(double value) {
            double oldMaxCompanyCash = MaxCompanyCash;
            MaxCompanyCash = value;
            if(oldMaxCompanyCash != MaxCompanyCash) {
                GameEvents.OnMaxCompanyCashChanged(oldMaxCompanyCash, MaxCompanyCash);
            }
        }*/

        public void AddSessionEarningsCompanyCash(double value)
            => sessionEarningCompanyCash += value;
        
        public void SetSessionEarningsCompanyCash(double value)
            => sessionEarningCompanyCash = 0.0;

        public bool IsEnoughCurrencies(params Bos.Data.Currency[] currencies) {
            bool isEnough = true;
            foreach(var currency in currencies) {
                if(!IsEnough(currency)) {
                    isEnough = false;
                    break;
                }
            }
            return isEnough;
        }

        public bool IsEnough(Bos.Data.Currency currency) {
            switch (currency.Type) {
                case Data.CurrencyType.Coins: {
                        return Coins >= (int)currency.Value;
                    }
                case Data.CurrencyType.CompanyCash: {
                        return CompanyCash.Value >= currency.Value;
                    }
                case Data.CurrencyType.PlayerCash: {
                        return ((double)PlayerCash.Value) >= currency.Value;
                    }
                case Data.CurrencyType.Securities: {
                        return ((double)Securities.Value) >= currency.Value;
                    }
                default: {
                        throw new InvalidOperationException($"invalid currency type {currency.Type}");
                    }
            }
        }

        public bool IsEnoughCompanyCash(double value) {
            return IsEnough(Bos.Data.Currency.CreateCompanyCash(value));
        }

        public bool IsEnoughSecurities(double value) {
            return IsEnough(Bos.Data.Currency.CreateSecurities(value));
        }

        public bool IsEnoughCoins(int value) {
            return IsEnough(Bos.Data.Currency.CreateCoins(value));
        }

        public void RemoveCurrency(Bos.Data.Currency currency) {
            switch(currency.Type) {
                case Data.CurrencyType.Coins: {
                        RemoveCoins((int)currency.Value);
                    }
                    break;
                case Data.CurrencyType.CompanyCash: {
                        RemoveCompanyCash(currency.Value);
                    }
                    break;
                case Data.CurrencyType.PlayerCash: {
                        RemovePlayerCash(currency.Value);
                    }
                    break;
                case Data.CurrencyType.Securities: {
                        RemoveSecurities(currency.Value);
                    }
                    break;
                default: {
                        throw new InvalidOperationException($"invalid currency type {currency.Type}");
                    }
            }
        }

        public void AddCurrency(Bos.Data.Currency currency) {
            switch(currency.Type) {
                case Data.CurrencyType.Coins: {
                        AddCoins((int)currency.Value);
                    }
                    break;
                case Data.CurrencyType.CompanyCash: {
                        AddCompanyCash(currency.Value);
                    }
                    break;
                case Data.CurrencyType.PlayerCash: {
                        AddPlayerCash(currency.Value.ToCurrencyNumber());
                    }
                    break;
                case Data.CurrencyType.Securities: {
                        AddSecurities(currency.Value.ToCurrencyNumber());
                    }
                    break;
                default: {
                        throw new InvalidOperationException($"invalid currency type {currency.Type}");
                    }
            }
        }

        public void AddCurrencies(Bos.Data.Currency[] currencies) {
            foreach(Bos.Data.Currency currency in currencies) {
                AddCurrency(currency);
            }
        }


        public void AddCompanyCash(double value) {
            var oldValue = currency.CompanyCash;
            currency.AddCompanyCash(value);
            AddSessionEarningsCompanyCash(value);
            AddLifeTimeEarnings(value);
            /*
            if (CompanyCash.Value > MaxCompanyCash) {
                SetMaxCompanyCash(CompanyCash.Value);
            }*/
            if (currency.CompanyCash != oldValue) {
                GameEvents.OnCompanyCashChanged(oldValue.ToCurrencyNumber(), currency.CompanyCash.ToCurrencyNumber());
            }
        }

        public void RemoveCompanyCash(double value) {
            double oldValue = currency.CompanyCash;
            currency.RemoveCompanyCash(value);
            if(currency.CompanyCash != oldValue) {
                GameEvents.OnCompanyCashChanged(oldValue.ToCurrencyNumber(), currency.CompanyCash.ToCurrencyNumber());
            }
        }

        public void RemovePlayerCash(double value) {
            double oldValue = currency.PlayerCash;
            currency.RemovePlayerCash(value);
            if(currency.PlayerCash != oldValue) {
                GameEvents.OnPlayerCashChanged(oldValue.ToCurrencyNumber(), currency.PlayerCash.ToCurrencyNumber());
            }
        }

        public void CheckNonNegativeCompanyCash() {
            double oldValue = currency.CompanyCash;
            currency.CheckNonNegativeCompanyCash();
            if(currency.CompanyCash != oldValue) {
                GameEvents.OnCompanyCashChanged(oldValue.ToCurrencyNumber(), currency.CompanyCash.ToCurrencyNumber());
            }
        }

        public void SetCompanyCash(double value) {
            double oldValue = currency.CompanyCash;
            currency.SetCompanyCash(value);
            if(currency.CompanyCash != oldValue) {
                GameEvents.OnCompanyCashChanged(oldValue.ToCurrencyNumber(), currency.CompanyCash.ToCurrencyNumber());
            }
        }

        public void AddPlayerCash(CurrencyNumber value) {
            var oldValue = currency.PlayerCash;
            currency.AddPlayerCash(value.Value);
            if(currency.PlayerCash != oldValue) {
                GameEvents.OnPlayerCashChanged(oldValue.ToCurrencyNumber(), currency.PlayerCash.ToCurrencyNumber());
            }
        }

        public void AddCoins(int value, bool isFree = false) {
            var oldValue = currency.Coins;
            currency.AddCoins(value);
            if (!isFree)
                StatsCollector.Instance[Stats.COINS_BOUGHT] += value;
            StatsCollector.Instance[Stats.COINS_AQUIRED] += value;
            if (currency.Coins != oldValue) {
                GameEvents.OnCoinsChanged(oldValue, currency.Coins);
            }
        }

        public void SetCoins(int count) {
            var oldCount = currency.Coins;
            currency.SetCoins(count);
            if(currency.Coins != oldCount) {
                GameEvents.OnCoinsChanged(oldCount, currency.Coins);
            }
        }

        public void RemoveCoins(int value) {
            UnityEngine.Debug.Log($"REMOVE COINS: {value}".Attrib(bold: true, color: "yellow"));
            int oldValue = currency.Coins;
            currency.RemoveCoins(value);
            if(currency.Coins != oldValue) {
                GameEvents.OnCoinsChanged(oldValue, currency.Coins);
            }
        }

        public void AddSecurities(CurrencyNumber value) {
            var oldValue = currency.Securities;
            currency.AddSecurities(value.Value);
            if(currency.Securities != oldValue) {
                GameEvents.OnSecuritiesChanged(oldValue.ToCurrencyNumber(), currency.Securities.ToCurrencyNumber());
            }
        }

        public void RemoveSecurities(double value ) {
            var oldValue = currency.Securities;
            currency.RemoveSecurities(Math.Abs(value));
            if(currency.Securities != oldValue) {
                GameEvents.OnSecuritiesChanged(oldValue.ToCurrencyNumber(), currency.Securities.ToCurrencyNumber());
            }
        }

        public void SetSecurities(double value) {
            var oldSecurities = currency.Securities;
            currency.SetSecurities(value);
            if(currency.Securities != oldSecurities) {
                GameEvents.OnSecuritiesChanged(oldSecurities.ToCurrencyNumber(), currency.Securities.ToCurrencyNumber());
            }

        }

        public CurrencyNumber LifeTimeSecurities
            => currency.LifetimeSecurities.ToCurrencyNumber();

        public double GetCurrencyMaxValue(CurrencyType currencyType) {
            switch (currencyType) {
                case CurrencyType.CompanyCash: {
                    return MaxCompanyCash;
                }
                case CurrencyType.PlayerCash: {
                    return MaxPlayerCash;
                }
                case CurrencyType.Securities: {
                    return MaxSecurities;
                }
                default: {
                    throw new UnityException($"GetCurrencyMaxValue() => invalid currency type: {currencyType}");
                }
            }
        }

        public ICurrencyInfo Currency => currency;
        public int Coins => Currency.Coins;
        public CurrencyNumber CompanyCash => currency.CompanyCash.ToCurrencyNumber();
        public CurrencyNumber PlayerCash => currency.PlayerCash.ToCurrencyNumber();
        public CurrencyNumber Securities => currency.Securities.ToCurrencyNumber();

        public TransferCashInfo StartTransferCashOfficially() {
            TransferCashInfo info = new TransferCashInfo();
            double value = CompanyCash.Value;
            info.TransferOfficially(value, Services.ResourceService.PersonalImprovements.ConvertData.OfficialConvertPercent);
            FinishTransferCashOfficially(info);
            GameEvents.OnOfficialTransfer(info);
            return info;
        }

        public void FinishTransferCashOfficially(TransferCashInfo transferInfo) {
            RemoveCompanyCash(transferInfo.InputValue);
            AddPlayerCash(transferInfo.RemainValue.ToCurrencyNumber());
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.LegalTransferCompleted, transferInfo));
        }

        public UnofficialTransferCashInfo TransferIlegally(bool? isRewriteWithStatus) {
            UnofficialTransferCashInfo info = new UnofficialTransferCashInfo();
            double inputValue = CompanyCash.Value;
            float looseChance = Services.ResourceService.PersonalImprovements.ConvertData.UnofficialConvertPercent;
            float loosePercent = Services.ResourceService.PersonalImprovements.ConvertData.LoosePercent;
            info.Transfer(inputValue, looseChance, loosePercent, isRewriteWithStatus);
            if(info.IsSuccess) {
                RemoveCompanyCash(inputValue);
                AddPlayerCash(inputValue.ToCurrencyNumber());
            } else {
                RemoveCompanyCash(info.LooseValue);
            }
            GameEvents.OnUnofficialTransfer(info);
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.IllegalTransferCompleted, info));
            return info;
        }

        public int StatusLevel
            => Services.ResourceService.PersonalImprovements.GetStatusLevel(StatusPoints);

        public Option<ProductData> AvailablePersonalProduct {
            get {
                var product = (from p in ResourceService.PersonalProducts.ProductCollection
                               where !IsProductPurchased(p.id) && PlayerCash.Value >= p.price
                               orderby p.price
                               select p).FirstOrDefault();
                return (product != null) ? F.Some(product) : F.None;
            }
        }
        #endregion
    }

    public class PlayerSave {
        public int gender;
        public int level;
        public int currentXP;

        public int XPLevelLimit;

        public List<int> purchasedProducts;
        public long statusPoints;
        public double lifetimeEarnings;
        public double lifetimeEarningsInPlanet;
        //public double maxCompanyCash;
        public bool isHasMicromanager;
    }

    public interface IPlayerService : IGameService {
        ICurrencyInfo Currency { get; }
        int Coins { get; }
        CurrencyNumber CompanyCash { get; }
        CurrencyNumber PlayerCash { get; }
        CurrencyNumber Securities { get; }      
        CurrencyNumber LifeTimeSecurities { get; }
        double MaxCompanyCash { get; }
        double MaxPlayerCash { get; }
        double MaxSecurities { get; }
        bool IsLoaded { get; }
        double SessionEarningsCompanyCash { get; }
        Gender Gender { get; }
        int Level { get; }
        int CurrentXP { get; }
        int XPLevelLimit { get; }
        float ExpProgress01 { get; }
        double LifetimeEarnings { get; }
        double LifetimeEarningsInPlanet { get; }
        long StatusPoints { get; }
        int StatusLevel { get; }
        bool IsHasMicromanager { get; }

        void AddCompanyCash(double value);
        void RemoveCompanyCash(double value);
        void CheckNonNegativeCompanyCash();
        void SetCompanyCash(double value);
        void SetCoins(int count);
        void AddPlayerCash(CurrencyNumber value);
        void AddCoins(int value, bool isFree = false);
        void RemoveCoins(int value);
        void AddSecurities(CurrencyNumber value);
        void RemoveSecurities(double value);
        bool IsEnough(Bos.Data.Currency currency);
        bool IsEnoughCompanyCash(double value);
        bool IsEnoughSecurities(double value);
        bool IsEnoughCoins(int value);
        void RemoveCurrency(Bos.Data.Currency currency);
        void AddCurrency(Bos.Data.Currency currency);
        void AddCurrencies(Bos.Data.Currency[] currencies);
        void SetSecurities(double value);      
        bool IsEnoughCurrencies(params Bos.Data.Currency[] currencies);
        void AddSessionEarningsCompanyCash(double value);
        void SetSessionEarningsCompanyCash(double value);
        void AddGenerationCompanyCash(double value);
        void SetGender(Gender gender);
        void AddXP(int count);
        void RemoveXP(int count);
        void AddLevel(int count);
        void AddXPLevelLimit(int value);
        TransferCashInfo StartTransferCashOfficially();
        void FinishTransferCashOfficially(TransferCashInfo transferInfo);    
        UnofficialTransferCashInfo TransferIlegally(bool? isRewriteWithStatus);
        bool IsProductPurchased(int productId);
        TransactionState PurchaseProduct(ProductData data);
        bool IsAllowPurchaseProduct(ProductData data, out TransactionState state);
        void SetLifetimeEarnings(double value);
        void SetHasMicromanager(bool value);
        double GetCurrencyMaxValue(CurrencyType currencyType);
        void AddStatusPoints(long count);
        bool HasPurchasedProducts();

        ProductAvailableNotifier ProductNotifier { get; }

        /// <summary>
        /// Returns not purchased personal product for which personal cache is enough
        /// </summary>
        Option<ProductData> AvailablePersonalProduct { get; }
        PlayerData LegacyPlayerData { get; }
    }

    public class UnofficialTransferCashInfo {
        public bool IsSuccess {get; private set;}
        public double RemainValue { get; private set; }

        public double LooseValue { get; private set; }
        public double InputValue { get; private set; }

        public bool Transfer(double inputValue, float looseChance, float loosePercent, bool? isRewriteWithStatus) {
            this.InputValue = inputValue;

            if (isRewriteWithStatus.HasValue) {
                if (isRewriteWithStatus.Value) {
                    TransferWithSuccess(inputValue, looseChance, loosePercent);
                    return true;
                }
                else {
                    TransferWithFail(inputValue, looseChance, loosePercent);
                    return false;
                }         
            }
            else {
                if (UnityEngine.Random.value < looseChance) {
                    TransferWithFail(inputValue, looseChance, loosePercent);
                    return false;
                } else {
                    TransferWithSuccess(inputValue, looseChance, loosePercent);
                    return true;
                }
            }
        }

        private void TransferWithFail(double inputValue, float looseChance, float loosePercent) {
            IsSuccess = false;
            LooseValue = inputValue * loosePercent;
            RemainValue = 0;
        }

        private void TransferWithSuccess(double inputValue, float looseChance, float loosePercent) {
            IsSuccess = true;
            RemainValue = inputValue;
            LooseValue = 0;
        }

        public override string ToString() {
            return $"SUCCESS: {IsSuccess}, LOOSE VAL: {BosUtils.GetCurrencyString(LooseValue.ToCurrencyNumber())}, REMAIN VAL: {BosUtils.GetCurrencyString(RemainValue.ToCurrencyNumber())}";
        }
    }

    public class TransferCashInfo {
        public float TaxPercent {get; private set;}
        public double TaxValue { get; private set;}

        public double RemainValue { get; private set; }
        public double InputValue { get; private set; }


        public int TaxPercentInt {
            get {
                float taxPercent100 = TaxPercent * 100;
                int div = (int)(taxPercent100 / 5);
                int result = div * 5;
                if (result == 0) {
                    result = 5;
                }

                return result;
            }
        }

        public void TransferOfficially(double inputValue, float tax ) {
            this.InputValue = inputValue;
            TaxPercent = tax; //UnityEngine.Random.Range(minTax, maxTax);
            //TaxPercent = TaxPercentInt / 100.0f;
            
            TaxValue = inputValue * TaxPercent;
            RemainValue = inputValue - TaxValue;
        }

        public void TransferIlegally(double inputValue, float loosePercent, float taxPercent ) {
            InputValue = inputValue;
            TaxPercent = 0; 
            TaxValue = 0;
            if(UnityEngine.Random.value < loosePercent ) {
                TaxPercent = taxPercent;
                TaxValue = TaxPercent * inputValue;
            }
            RemainValue = inputValue - TaxValue;
        }

        public override string ToString(){
            return $"TAX: {(int)(100 * TaxPercent)}%, TAX VAL: {BosUtils.GetCurrencyString(TaxValue.ToCurrencyNumber())}, REMAIN VAL: {BosUtils.GetCurrencyString(RemainValue.ToCurrencyNumber())}";
        }
    }




    public enum Gender {
        Male = 0,
        Female = 1
    }

    public enum TransactionState {
        Success = 0,
        DontEnoughCurrency = 1,
        AlreadyPurchased = 2
    }
}