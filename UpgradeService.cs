using Newtonsoft.Json.Utilities;

namespace Bos {
    using Bos.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;

    public class UpgradeService : SaveableGameBehaviour, IUpgradeService {

        private readonly List<int> boughtCashUpgrades = new List<int>();
        private readonly List<int> boughtSecuritiesUpgrades = new List<int>();
        private readonly List<int> boughtCoinsUpgrades = new List<int>();
        private readonly List<int> upgradedGeneratorsIds = new List<int>();

        private Dictionary<int, int> ProfitUpgradeLevels { get; } = new Dictionary<int, int>();
        private Dictionary<int, int> SpeedUpgradeLevels { get; } = new Dictionary<int, int>();
        public int UpgradeCoinsScreenOpenedLastTime { get; private set; }

        
        private readonly List<UpgradeData> datas = new List<UpgradeData>();
        private bool isInitialized = false;

        private List<UpgradeData> Datas {
            get {
                if (datas.Count == 0) {
                    datas.AddRange(Services.ResourceService.CashUpgrades.UpgradeList);
                    datas.AddRange(Services.ResourceService.SecuritiesUpgrades.UpgradeList);
                }

                return datas;
            }
        }

        public bool IsQuickBuyResearched { get; private set; } = false;

        public int CashAndSecuritiesBoughtCount
            => boughtCashUpgrades.Count + boughtSecuritiesUpgrades.Count;

        #region IUpgradeService implementation
        public void Setup(object data) {
            if(!isInitialized) {
                GameEvents.ScreenOpenedObservable.Subscribe(screenType => {
                    UpgradeCoinsScreenOpenedLastTime = TimeService.UnixTimeInt;
                }).AddTo(gameObject);
                isInitialized = true;
            }
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        public bool IsBoughtCoinsUpgrade(int id) {
            return boughtCoinsUpgrades.Contains(id);
        }

        public List<int> BoughtCashAndSecuritiesUpgrades {
            get {
                List<int> result = new List<int>(boughtCashUpgrades.Count + boughtSecuritiesUpgrades.Count);
                result.AddRange(boughtCashUpgrades);
                result.AddRange(boughtSecuritiesUpgrades);
                return result;
            }
        }

        public List<int> BoughtSecuritiesUpgrades
            => boughtSecuritiesUpgrades;

        public bool IsBoughtCashUpgrade(int id)
            => boughtCashUpgrades.Contains(id);

        public bool IsBoughtSecurityUpgrade(int id)
            => boughtSecuritiesUpgrades.Contains(id);

        public void AddCashUpgrade(UpgradeData upgradeData ) {
            if(!boughtCashUpgrades.Contains(upgradeData.Id)) {
                boughtCashUpgrades.Add(upgradeData.Id);
                AddUpgradedGenerator(upgradeData);
                GameEvents.OnUpgradeAdded(upgradeData);
            }
        }

        public void AddSecurityUpgrade(UpgradeData upgradeData ) {
            if(!boughtSecuritiesUpgrades.Contains(upgradeData.Id)) {
                boughtSecuritiesUpgrades.Add(upgradeData.Id);
                AddUpgradedGenerator(upgradeData);
                GameEvents.OnUpgradeAdded(upgradeData);
            }
        }

        private void AddUpgradedGenerator(UpgradeData data) {
            upgradedGeneratorsIds.Add(data.GeneratorId);
        }

        public void BuyUpgrade(UpgradeData data ) {
            Services.UpgradeService.BuyUpgradeViewModel(data, false);
            if(data.CurrencyType == CurrencyType.CompanyCash) {
                AddCashUpgrade(data);
            } else if(data.CurrencyType == CurrencyType.Securities ) {
                AddSecurityUpgrade(data);
            }
        }

        public bool IsAllowBuy(UpgradeData data) {
            if(data.CurrencyType == CurrencyType.CompanyCash && IsBoughtCashUpgrade(data.Id)) {
                return false;
            }
            if(data.CurrencyType == CurrencyType.Securities && IsBoughtSecurityUpgrade(data.Id)) {
                return false;
            }
            Currency priceCurrency = Currency.Create(data.CurrencyType, data.Price(() => {
                return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
            }));
            if(data.GeneratorId >= 0 ) {
                if(Services.PlayerService.IsEnough(priceCurrency) && 
                    Services.TransportService.HasUnits(data.GeneratorId)) {
                    return true;
                }
            } else {
                return Services.PlayerService.IsEnough(priceCurrency);
            }
            return false;
        }

        public bool IsBought(UpgradeData data) {
            if(data.CurrencyType == CurrencyType.CompanyCash ) {
                return IsBoughtCashUpgrade(data.Id);
            } else {
                return IsBoughtSecurityUpgrade(data.Id);
            }
        }

        public void SetQuickBuyResearched(bool value) {
            bool oldValue = IsQuickBuyResearched;
            IsQuickBuyResearched = value;
            if(oldValue != IsQuickBuyResearched) {
                GameEvents.OnUpgradeQuickBuyResearched(value);
            }
        }

        public bool HasAvailableUpgrades {
            get {
                foreach (var data in Datas) {
                    double price = data.Price(() => {
                        return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                    });
                    var priceCurrency = Currency.Create(data.CurrencyType, price);
                    if (!IsBought(data) &&
                        Services.PlayerService.IsEnough(priceCurrency) &&
                        (Services.TransportService.HasUnits(data.GeneratorId) || data.GeneratorId == -1)) {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool HasAvailableCashUpgrades {
            get {
                return Datas.Any(data => {
                    if (data.CurrencyType == CurrencyType.CompanyCash) {
                        if (!IsBought(data) && (Services.TransportService.HasUnits(data.GeneratorId) || data.GeneratorId == -1)) {
                            double price = data.Price(() => {
                                return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                            });
                            var priceCurrency = Currency.Create(data.CurrencyType, price);
                            if (Player.IsEnough(priceCurrency)) {
                                return true;
                            }
                        }
                    }
                    return false;
                });
            }
        }

        public bool HasAvailableSecuritiesUpgrades {
            get {
                return Datas.Any(d => {
                    if(d.CurrencyType == CurrencyType.Securities) {
                        if(!IsBought(d) && (Services.TransportService.HasUnits(d.GeneratorId) || d.GeneratorId == -1) ) {
                            double price = d.Price(() => {
                                return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, d);
                            });
                            var priceCurrency = Currency.Create(d.CurrencyType, price);
                            return Player.IsEnough(priceCurrency);
                        }
                    }
                    return false;
                });
            }
        }

        
        public void BuyUpgrade(Upgrade up, bool free = false ) {
            int id = up.GeneratorIdToUpgrade;

            switch(up.UpgradeType) {
                case UpgradeType.Profit: {
                        BuyProfitUpgrade(up, free, id);
                    }
                    break;
                case UpgradeType.Time: {
                        BuyTimeUpgrade(up, free, id);
                    }
                    break;
            }

            if(!free) {
                Services.PlayerService.CheckNonNegativeCompanyCash();
            }
            GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);

        }

        #endregion

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "upgade_service";

        public override Type SaveType => typeof(UpgradeServiceSave);

        public override object GetSave() {
            return new UpgradeServiceSave {
                boughtCashUpgrades = boughtCashUpgrades.Select(u => u).ToList(),
                boughtSecuritiesUpgrades = boughtSecuritiesUpgrades.Select(u => u).ToList(),
                boughtCoinsUpgrades = boughtCoinsUpgrades.Select(u => u).ToList(),
                upgradedGeneratorIds = upgradedGeneratorsIds.Select(u => u).ToList(),
                isQuickBuyResearched = IsQuickBuyResearched,
                profitUpgradeLevels = ProfitUpgradeLevels,
                speedUpgradeLevels = SpeedUpgradeLevels,
                upgradeCoinsScreenOpenedLastTime = UpgradeCoinsScreenOpenedLastTime
            };
        }

        public override void LoadDefaults() {
            boughtCashUpgrades.Clear();
            boughtSecuritiesUpgrades.Clear();
            boughtCoinsUpgrades.Clear();
            upgradedGeneratorsIds.Clear();
            IsQuickBuyResearched = false;
            ProfitUpgradeLevels.Clear();
            SpeedUpgradeLevels.Clear();
            UpgradeCoinsScreenOpenedLastTime = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();   
        }

        public override void LoadSave(object obj) {
            UpgradeServiceSave save = obj as UpgradeServiceSave;
            if( save != null ) {
                save.CheckNotNullFields();

                boughtCashUpgrades.Clear();
                boughtSecuritiesUpgrades.Clear();
                boughtCoinsUpgrades.Clear();
                upgradedGeneratorsIds.Clear();

                boughtCashUpgrades.AddRange(save.boughtCashUpgrades);
                boughtSecuritiesUpgrades.AddRange(save.boughtSecuritiesUpgrades);
                boughtCoinsUpgrades.AddRange(save.boughtCoinsUpgrades);
                upgradedGeneratorsIds.AddRange(save.upgradedGeneratorIds);
                IsQuickBuyResearched = save.isQuickBuyResearched;
                UpgradeCoinsScreenOpenedLastTime = save.upgradeCoinsScreenOpenedLastTime;

                BosUtils.CopyDictionary(save.profitUpgradeLevels, ProfitUpgradeLevels);
                BosUtils.CopyDictionary(save.speedUpgradeLevels, SpeedUpgradeLevels);

                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override void ResetByInvestors() {
            boughtCashUpgrades.Clear();
            boughtSecuritiesUpgrades.Clear();
            upgradedGeneratorsIds.Clear();
            ProfitUpgradeLevels.Clear();
            SpeedUpgradeLevels.Clear();
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            boughtCashUpgrades.Clear();
            boughtSecuritiesUpgrades.Clear();
            upgradedGeneratorsIds.Clear();
            ProfitUpgradeLevels.Clear();
            SpeedUpgradeLevels.Clear();
            IsLoaded = true;
        }

        public override void ResetFull() {
            LoadDefaults();
        }
        #endregion

        private void BuyProfitUpgrade(Upgrade up, bool free, int id ) {
            if(Services.GenerationService.Generators.Contains(id)) {
                int count = GetProfitUpgradeLevel(id);
                //Services.GenerationService.Generators.ApplyProfit(id, ProfitMultInfo.Create(up.ProfitMultiplier));
                Services.GenerationService.Generators.AddProfitBoost(
                    generatorId: id,
                    boost: BoostInfo.CreateTemp(
                        id: $"upgrade_{up.GeneratorIdToUpgrade}_".GuidSuffix(5),
                        value: up.ProfitMultiplier));
                AddProfitUpgradeLevel(id, 1);
                if(!free) {
                    RemoveCost(Services, up.CostType, up.CalculateCost(count));
                }
            } else {
                //Services.GenerationService.Generators.ApplyProfit(id, ProfitMultInfo.Create(up.ProfitMultiplier));
                Services.GenerationService.Generators.AddProfitBoost(
                    generatorId: id,
                    boost: BoostInfo.CreateTemp(
                        id: $"upgrade_{up.GeneratorIdToUpgrade}_".GuidSuffix(5),
                        value: up.ProfitMultiplier
                        ));
                AddProfitUpgradeLevel(id, 1);
                if(!free) {
                    RemoveCost(Services, up.CostType, up.CalculateCost(0));
                }
            }
        }

        private void BuyTimeUpgrade(Upgrade up, bool free, int id ) {
            if(Services.GenerationService.Generators.Contains(id)) {
                int count = GetSpeedUpgradeLevel(id);
                //Services.GenerationService.Generators.ApplyTime(id, up.TimeMultiplier);
                Services.GenerationService.Generators.AddTimeBoost(
                    generatorId: id,
                    boost: BoostInfo.CreateTemp(
                        id: $"upgrade_time_{up.GeneratorIdToUpgrade}_".GuidSuffix(5),
                        value: up.TimeMultiplier));
                AddSpeedUpgradeLevel(id, 1);
                if(!free) {
                    RemoveCost(Services, up.CostType, up.CalculateCost(count));
                }
            } else {
                //Services.GenerationService.Generators.ApplyTime(id, up.TimeMultiplier);
                Services.GenerationService.Generators.AddTimeBoost(
                    generatorId: id,
                    boost: BoostInfo.CreateTemp(
                        id: $"upgrade_time_{up.GeneratorIdToUpgrade}_".GuidSuffix(5),
                        value: up.TimeMultiplier));
                AddSpeedUpgradeLevel(id, 1);
                if(!free) {
                    RemoveCost(Services, up.CostType, up.CalculateCost(0));
                }
            }
        }

        private void RemoveCost(IBosServiceCollection services, CostType type, double cost) {
            if (currencyRemovers.ContainsKey(type)) {
                currencyRemovers[type](services, cost);
            }
        }


        private readonly Dictionary<CostType, Action<IBosServiceCollection, double>> currencyRemovers = new Dictionary<CostType, Action<IBosServiceCollection, double>> {
            [CostType.Balance] = (services, val) => services.PlayerService.RemoveCompanyCash(val),
            [CostType.Investors] = (services, val) => services.PlayerService.RemoveSecurities(val),
            [CostType.Coins] = (services, val) => services.PlayerService.RemoveCoins((int)val)
        };

        private void AddSpeedUpgradeLevel(int id, int count) {
            if(SpeedUpgradeLevels.ContainsKey(id)) {
                SpeedUpgradeLevels[id] += count;
            } else {
                SpeedUpgradeLevels.Add(id, count);
            }
        }

        private void AddProfitUpgradeLevel(int id, int count ) {
            if(ProfitUpgradeLevels.ContainsKey(id)) {
                ProfitUpgradeLevels[id] += count;
            } else {
                ProfitUpgradeLevels.Add(id, count);
            }
        }

        private int GetSpeedUpgradeLevel(int id) 
            => SpeedUpgradeLevels.ContainsKey(id) ? SpeedUpgradeLevels[id] : 0;

        private int GetProfitUpgradeLevel(int id)
            => ProfitUpgradeLevels.ContainsKey(id) ? ProfitUpgradeLevels[id] : 0;

        public int GetUpgradeLevel(Upgrade up) {
            switch(up.UpgradeType) {
                case UpgradeType.Profit: {
                        return GetProfitUpgradeLevel(up.GeneratorIdToUpgrade);
                    }
                case UpgradeType.Time: {
                        return GetSpeedUpgradeLevel(up.GeneratorIdToUpgrade);
                    }
                default: {
                        return 0;
                    }
            }
        }


        #region from OLD API
        public void BuyUpgradeViewModel(UpgradeData vm, bool free) {
            if (vm.CurrencyType == CurrencyType.Securities)
                BuyUpgradeViewModel_Investors(vm);
            else
                BuyUpgradeViewModel_Cash(vm, free);
        }

        private void BuyUpgradeViewModel_Investors(UpgradeData vm) {
            var id = vm.GeneratorId;

            switch (vm.UpgradeType) {
                case UpgradeType.Profit:
                    BuyProfitUpgradeVM(vm, false, id, true);
                    break;
                case UpgradeType.Time:
                    BuyTimeUpgradeVM(vm, false, id, true);
                    break;
                case UpgradeType.FreeGenerators:
                    BuyFreeGenerators(vm, id);
                    break;
                case UpgradeType.InvestorEffectiveness:
                    BuyInvestorEffectiveness(vm, id);
                    break;
                default:
                    break;
            }
            GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
        }

        private void BuyFreeGenerators(UpgradeData vm, int id) {
            ITransportUnitsService transportService = GameServices.Instance.TransportService;

            if (transportService.HasUnits(id)) {
                transportService.AddLiveUnits(id, (int)vm.Value);
                GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
            } else {
                transportService.AddLiveUnits(id, (int)vm.Value);
                GlobalRefs.LevelManager.AddXP(XPSources.BuyGenerator * 10);
            }


            //PlayerData.Investors -= vm.Price;
            GameServices.Instance.PlayerService.RemoveSecurities(vm.Price(() => {
                return BosUtils.GetUpgradePriceMult(ResourceService.Planets.GetPlanet(Planets.CurrentPlanet.Id), vm);
            }));
            GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
        }

        private void BuyInvestorEffectiveness(UpgradeData vm, int id) {
            var perc = vm.Value / 100.0f;
            IInvestorService investorService = GameServices.Instance.InvestorService;
            investorService.AddEffectiveness((float)perc);
            GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
        }

        private void BuyUpgradeViewModel_Cash(UpgradeData data, bool free) {
            IPlayerService playerService = GameServices.Instance.PlayerService;

            var id = data.GeneratorId;
            switch (data.UpgradeType) {
                case UpgradeType.Profit:
                    BuyProfitUpgradeVM(data, free, id, false);
                    break;
                case UpgradeType.Time:
                    BuyTimeUpgradeVM(data, free, id, false);
                    break;
                default:
                    break;
            }

            if (!free) {
                playerService.CheckNonNegativeCompanyCash();
            }
            GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
        }

        private void BuyProfitUpgradeVM(UpgradeData vm, bool free, int id, bool isInvestor) {
            if (id == -1) {
                GenerationService.Generators.AddProfitBoost(BoostInfo.CreateTemp(id: $"profit_upgrade_{vm.Id}_".GuidSuffix(5), value: vm.Value));
            } else {
                GenerationService.Generators.AddProfitBoost(
                    generatorId: id,
                    boost: BoostInfo.CreateTemp(id: $"profit_upgrade_{vm.Id}_".GuidSuffix(5), value: vm.Value)
                    );
            }
            if (!free) {
                if (isInvestor) {

                } else {
                    Player.RemoveCompanyCash(vm.Price(() => {
                        return BosUtils.GetUpgradePriceMult(ResourceService.Planets.GetPlanet(Planets.CurrentPlanet.Id), vm);
                    }));
                }
            }
        }

        private void BuyTimeUpgradeVM(UpgradeData vm, bool free, int id, bool isInvestor) {
            if (id == -1) {
                GenerationService.Generators.AddTimeBoost(BoostInfo.CreateTemp(id: $"time_upgrade_{vm.Id}_".GuidSuffix(5), value: vm.Value));
            } else {
                GenerationService.Generators.AddTimeBoost(
                    generatorId: id,
                    boost: BoostInfo.CreateTemp(
                        id: $"time_upgrade_{vm.Id}_".GuidSuffix(5),
                        value: vm.Value));
            }
            if (!free) {
                if (isInvestor) {

                } else {
                    Player.RemoveCompanyCash(vm.Price(() => {
                        return BosUtils.GetUpgradePriceMult(ResourceService.Planets.GetPlanet(Planets.CurrentPlanet.Id), vm);
                    }));
                }
            }
        }

        #endregion
    }


    public interface IUpgradeService : IGameService {
        int CashAndSecuritiesBoughtCount { get; }
        bool IsBoughtCoinsUpgrade(int id);
        bool IsBoughtCashUpgrade(int id);
        bool IsBought(UpgradeData data);
        bool IsBoughtSecurityUpgrade(int id);
        List<int> BoughtCashAndSecuritiesUpgrades { get; }
        List<int> BoughtSecuritiesUpgrades { get; }
        void AddCashUpgrade(UpgradeData upgradeData);
        void AddSecurityUpgrade(UpgradeData upgradeData);
        void BuyUpgrade(UpgradeData data);
        bool IsAllowBuy(UpgradeData data);
        bool IsQuickBuyResearched { get; }
        void SetQuickBuyResearched(bool value);
        bool HasAvailableUpgrades { get; }

        void BuyUpgrade(Upgrade up, bool free = false);
        int GetUpgradeLevel(Upgrade up);

        bool HasAvailableCashUpgrades { get; }
        bool HasAvailableSecuritiesUpgrades { get; }
        void BuyUpgradeViewModel(UpgradeData vm, bool free);
        int UpgradeCoinsScreenOpenedLastTime {
            get;
        }
    }


    [System.Serializable]
    public class UpgradeServiceSave {
        public List<int> boughtCashUpgrades;
        public List<int> boughtSecuritiesUpgrades;
        public List<int> boughtCoinsUpgrades;
        public List<int> upgradedGeneratorIds;
        public bool isQuickBuyResearched;
        public int upgradeCoinsScreenOpenedLastTime;

        public Dictionary<int, int> profitUpgradeLevels;
        public Dictionary<int, int> speedUpgradeLevels;


        public void CheckNotNullFields() {
            if(boughtCashUpgrades == null ) {
                boughtCashUpgrades = new List<int>();
            }
            if(boughtSecuritiesUpgrades == null ) {
                boughtSecuritiesUpgrades = new List<int>();
            }
            if(boughtCoinsUpgrades == null ) {
                boughtCoinsUpgrades = new List<int>();
            }
            if(upgradedGeneratorIds == null ) {
                upgradedGeneratorIds = new List<int>();
            }
            if(profitUpgradeLevels == null ) {
                profitUpgradeLevels = new Dictionary<int, int>();
            }
            if(speedUpgradeLevels == null ) {
                speedUpgradeLevels = new Dictionary<int, int>();
            }
        }
    }
}