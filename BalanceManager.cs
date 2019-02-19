//using Bos;
//using Bos.Data;
//using System;
//using UnityEngine;

//public class BalanceManager : GameBehaviour {
//    private double _tenToThe6h = Math.Pow(10, 6);
//    private IGameService _gameSvc;
//    public float OfflineTime;
//    public bool WasOfflineBalanceGenerated;
//    public PlayerData PlayerData;
//    public IAPManager IAPManager;
//    public GameUI GameUI;


//    public override void Awake() {
//        GlobalRefs.BalanceManager = this;
//    }

//    public override  void Start() {
//        _gameSvc = ServiceLocator.Instance.Resolve<IGameService>();
//    }

//    public override void OnEnable() {

//        GameEvents.Pause += OnPause;
//    }

//    public override void OnDisable() {

//        GameEvents.Pause -= OnPause;
//    }

//    private void OnPause() {
//        if (GameServices.Instance.PlayerService.Currency.IsLoaded) {
//            LocalData.LastPauseBalance = GameServices.Instance.PlayerService.CompanyCash.Value;
//        }
//    }



//    //public void BuyUpgradeViewModel(UpgradeData vm, bool free) {
//    //    if (vm.CurrencyType == CurrencyType.Securities)
//    //        BuyUpgradeViewModel_Investors(vm);
//    //    else
//    //        BuyUpgradeViewModel_Cash(vm, free);
//    //}

//    //private void BuyUpgradeViewModel_Investors(UpgradeData vm) {
//    //    var id = vm.GeneratorId;

//    //    switch (vm.UpgradeType) {
//    //        case UpgradeType.Profit:
//    //            BuyProfitUpgradeVM(vm, false, id, true);
//    //            break;
//    //        case UpgradeType.Time:
//    //            BuyTimeUpgradeVM(vm, false, id, true);
//    //            break;
//    //        case UpgradeType.FreeGenerators:
//    //            BuyFreeGenerators(vm, id);
//    //            break;
//    //        case UpgradeType.InvestorEffectiveness:
//    //            BuyInvestorEffectiveness(vm, id);
//    //            break;
//    //        default:
//    //            break;
//    //    }
//    //    GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
//    //    PlayerData.Save();
//    //}

//    //private void BuyFreeGenerators(UpgradeData vm, int id) {
//    //    ITransportUnitsService transportService = GameServices.Instance.TransportService;

//    //    if (transportService.HasUnits(id)) {
//    //        transportService.AddLiveUnits(id, (int)vm.Value);
//    //        GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
//    //    } else {
//    //        transportService.AddLiveUnits(id, (int)vm.Value);
//    //        GlobalRefs.LevelManager.AddXP(XPSources.BuyGenerator * 10);
//    //    }

        
//    //    //PlayerData.Investors -= vm.Price;
//    //    GameServices.Instance.PlayerService.RemoveSecurities(vm.Price(() => {
//    //        return BosUtils.GetUpgradePriceMult(ResourceService.Planets.GetPlanet(Planets.CurrentPlanet.Id), vm);
//    //    }));
//    //    GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
//    //}

//    //private void BuyInvestorEffectiveness(UpgradeData vm, int id) {
//    //    var perc = vm.Value / 100.0f;
//    //    IInvestorService investorService = GameServices.Instance.InvestorService;
//    //    investorService.AddEffectiveness((float)perc);
//    //    GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
//    //}

//    //private void BuyUpgradeViewModel_Cash(UpgradeData data, bool free) {
//    //    IPlayerService playerService = GameServices.Instance.PlayerService;

//    //    var id = data.GeneratorId;
//    //    switch (data.UpgradeType) {
//    //        case UpgradeType.Profit:
//    //            BuyProfitUpgradeVM(data, free, id, false);
//    //            break;
//    //        case UpgradeType.Time:
//    //            BuyTimeUpgradeVM(data, free, id, false);
//    //            break;
//    //        default:
//    //            break;
//    //    }

//    //    if(!free) {
//    //        playerService.CheckNonNegativeCompanyCash();
//    //    }
//    //    GlobalRefs.LevelManager.AddXP(XPSources.BuyUpgrade);
//    //    PlayerData.Save();
//    //}

//    //private void BuyProfitUpgradeVM(UpgradeData vm, bool free, int id, bool isInvestor) {
//    //    if (id == -1) {
//    //        GenerationService.Generators.AddProfitBoost(BoostInfo.CreateTemp(id: $"profit_upgrade_{vm.Id}_".GuidSuffix(5), value: vm.Value));
//    //    } else {
//    //        GenerationService.Generators.AddProfitBoost(
//    //            generatorId: id, 
//    //            boost: BoostInfo.CreateTemp(id: $"profit_upgrade_{vm.Id}_".GuidSuffix(5), value: vm.Value)
//    //            );
//    //    }
//    //    if (!free) {
//    //        if (isInvestor) {
 
//    //        } else {
//    //            Player.RemoveCompanyCash(vm.Price(() => {
//    //                return BosUtils.GetUpgradePriceMult(ResourceService.Planets.GetPlanet(Planets.CurrentPlanet.Id), vm);
//    //            }));
//    //        }
//    //    }
//    //}

//    //private void BuyTimeUpgradeVM(UpgradeData vm, bool free, int id, bool isInvestor) {
//    //    if (id == -1) {
//    //        GenerationService.Generators.AddTimeBoost(BoostInfo.CreateTemp(id: $"time_upgrade_{vm.Id}_".GuidSuffix(5), value: vm.Value));
//    //    } else {
//    //        GenerationService.Generators.AddTimeBoost(
//    //            generatorId: id,
//    //            boost: BoostInfo.CreateTemp(
//    //                id: $"time_upgrade_{vm.Id}_".GuidSuffix(5),
//    //                value: vm.Value));
//    //    }
//    //    if (!free) {
//    //        if (isInvestor) {

//    //        } else {
//    //            Player.RemoveCompanyCash(vm.Price(() => {
//    //                return BosUtils.GetUpgradePriceMult(ResourceService.Planets.GetPlanet(Planets.CurrentPlanet.Id), vm);
//    //            }));
//    //        }
//    //    }
//    //}


//}

