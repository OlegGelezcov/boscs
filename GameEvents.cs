namespace Bos {
    using Bos.Data;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public static class GameEvents  {

        public class GameModeChangeData {
            public GameModeName OldGameMode { get; set; }
            public GameModeName NewGameMode { get; set; }
        }

        public class PlanetStateChangedData {
            public PlanetState OldState { get; set; }
            public PlanetState NewState { get; set; }
            public PlanetInfo Planet { get; set; }
        }

        public class EfficiencyDrop {
            public double Value { get; set; }
            public ManagerInfo Manager { get; set; }
        }

        public class EfficiencyChange {
            public double OldEfficiency { get; set; }
            public double NewEfficiency { get; set; }
            public ManagerInfo Manager { get; set; }
        }

        public class EfficiencyLevelChangedArgs {
            public int OldLevel { get; set; }
            public int NewLevel { get; set; }
            public ManagerEfficiencyRollbackLevel Manager { get; set; }
        }

        public class MaxRollbackLevelChangedArgs {
            public int OldLevel { get; set; }
            public int NewLevel { get; set; }
            public ManagerEfficiencyRollbackLevel Manager { get; set; }
        }

        public class BusinessSoldInvestorsEventArgs : EventArgs {
            public int PlanetId { get; }
            public double SecuritiesSold { get; }
            public int TimeInterval { get; }

            public BusinessSoldInvestorsEventArgs(int planetId, double securitiesSold, int interval) {
                this.PlanetId = planetId;
                this.SecuritiesSold = securitiesSold;
                this.TimeInterval = interval;
            }
        }

        public class TempMechanicRepairInfo {
            public TempMechanicInfo Mechanic { get; set; }
            public int RepairCount { get; set; }
        }

        public class AuditorRepairInfo {
            public Auditor Auditor { get; set; }
            public int RepairCount { get; set; }
        }

        public class ModuleStateChangedArgs : EventArgs
        {
            public ShipModuleState PrevState { get; set; }
            public ShipModuleState NewState { get; set; }
            public ShipModuleInfo Module { get; set; }
        }

        public class CurrencyNumberChangedArgs : EventArgs {
            public CurrencyNumber OldValue { get; set; }
            public CurrencyNumber NewValue { get; set; }
        }

        public class IntValueChangedArgs : EventArgs {
            public int OldValue { get; set; }
            public int NewValue { get; set; }
        }

        public class PromoCodeInfo {
            public string Code { get; set; }
            public int Count { get; set; }
            public bool IsSuccess { get; set; }
        }

        public static readonly Subject<PromoCodeInfo> PromoReceived = new Subject<PromoCodeInfo>();
        public static readonly Subject<TempMechanicRepairInfo> TempMechanicRepairedTransportObservable = new Subject<TempMechanicRepairInfo>();
        public static readonly Subject<AuditorRepairInfo> AuditorReportHandleObservable = new Subject<AuditorRepairInfo>();

        public static readonly Subject<GeneratorData> GeneratorDataReplacedSubject = new Subject<GeneratorData>();
        public static readonly Subject<ManagerData> ManagerDataReplacedSubject = new Subject<ManagerData>();


        public static readonly Subject<bool> AdWillBePlayedSubject = new Subject<bool>();
        private static IObservable<bool> adWillBePlayedObservable = null;

        public static IObservable<bool> AdWillBePlayed{
            get {
                if(adWillBePlayedObservable == null ) {
                    adWillBePlayedObservable = AdWillBePlayedSubject.AsObservable();
                }
                return adWillBePlayedObservable;
            }
        }

        public static readonly Subject<StoreProductData> StoreProductPurchasedObservable = new Subject<StoreProductData>();
        public static event Action<StoreProductData> StoreProductPurchasedEvent;

        public static void OnStoreProductPurchased(StoreProductData data)
            => StoreProductPurchasedEvent?.Invoke(data);

        public static readonly Subject<EfficiencyDrop> EfficiencyDropObservable = new Subject<EfficiencyDrop>();
        public static event Action<EfficiencyDrop> EfficiencyDropEvent;

        public static void OnEfficiencyDrop(EfficiencyDrop drop) {
            EfficiencyDropEvent?.Invoke(drop);
        }

        public static readonly Subject<EfficiencyChange> EfficiencyChangeObservanle = new Subject<EfficiencyChange>();


        public static event Action<EfficiencyChange> EfficiencyChangeEvent;

        public static void OnEfficiencyChangedEvent(EfficiencyChange change)
            => EfficiencyChangeEvent?.Invoke(change);

        public static readonly Subject<ScreenType> ScreenOpenedObservable = new Subject<ScreenType>();

        public static event Action<IShopItem> ShopItemPurchased;
        public static event Action<IAchievment> AchievmentCompleted;
        public static event Action<int> RewardedVideoFinished;

        public static event Action<ShipModuleState, ShipModuleState, ShipModuleInfo> ShipModuleStateChanged;
        public static IObservable<ModuleStateChangedArgs> ModuleStateChangedObservable
            = Observable.FromEvent<Action<ShipModuleState, ShipModuleState, ShipModuleInfo>, ModuleStateChangedArgs>(
                conversion: rxHandler => (oldState, newState, module) => rxHandler(new ModuleStateChangedArgs
                {
                    PrevState = oldState,
                    NewState = newState,
                    Module = module
                }),
                addHandler: h => ShipModuleStateChanged += h,
                removeHandler: h => ShipModuleStateChanged -= h);

        public static event Action<ViewType> ViewShowed;
        public static IObservable<ViewType> ViewShowedObservable =
            Observable.FromEvent<Action<ViewType>, ViewType>(
                conversion: rxHandler => type => rxHandler(type),
                addHandler: h => ViewShowed += h,
                removeHandler: h => ViewShowed -= h);

        public static event Action<ViewType> ViewHided;
        public static IObservable<ViewType> ViewHidedObservable =
            Observable.FromEvent<Action<ViewType>, ViewType>(
                conversion: rxHandler => type => rxHandler(type),
                addHandler: h => ViewHided += h,
                removeHandler: h => ViewHided -= h);



        public static event Action<ProductData> ProductPurchased;
        public static IObservable<ProductData> ProductPurchasedObservable
            = Observable.FromEvent<Action<ProductData>, ProductData>(
                conversion: rxhandler => product => rxhandler(product),
                addHandler: h => ProductPurchased += h,
                removeHandler: h => ProductPurchased -= h);

        public static event Action<double, double, ManagerInfo> MaxRollbackChanged;
        public static IObservable<RollbackChangedEventArgs> MaxRollbackChangedObservable
            = Observable.FromEvent<Action<double, double, ManagerInfo>, RollbackChangedEventArgs>(
                conversion: rxHandler =>
                    (oldValue, newValue, manager) => rxHandler(new RollbackChangedEventArgs(oldValue, newValue, manager)),
                addHandler: h => MaxRollbackChanged += h,
                removeHandler: h => MaxRollbackChanged -= h);

        public static event Action<GameModeName, GameModeName> GameModeChanged;

        public static IObservable<Tuple<GameModeName, GameModeName>> GameModeChangeObservable =
            Observable.FromEvent<Action<GameModeName, GameModeName>, Tuple<GameModeName, GameModeName>>(
                conversion: rxHandler => (oldGM, newGM) => rxHandler(Tuple.Create(oldGM, newGM)),
                addHandler: h => GameModeChanged += h,
                removeHandler: h => GameModeChanged -= h);

        public static readonly Lazy<IObservable<UpgradeData>> UpgradeAddedObservable =
            new Lazy<IObservable<UpgradeData>>(() =>
                Observable.FromEvent<Action<UpgradeData>, UpgradeData>(
                conversion: rxHandler => (uData) => rxHandler(uData),
                addHandler: h => UpgradeAdded += h,
                removeHandler: h => UpgradeAdded -= h));

        public static event EventHandler<InvestorTriesChangedEventArgs> InvestorTriesChanged;
        public static void OnInvestorTriesChanged(IInvestorService sender, InvestorTriesChangedEventArgs args)
            => InvestorTriesChanged?.Invoke(sender, args);

        public static System.Lazy<IObservable<EventPattern<InvestorTriesChangedEventArgs>>> InvestorTriesChangedObservable =
            new Lazy<IObservable<EventPattern<InvestorTriesChangedEventArgs>>>(() =>
            {
                return Observable.FromEventPattern<EventHandler<InvestorTriesChangedEventArgs>, InvestorTriesChangedEventArgs>(h => h, h => InvestorTriesChanged += h, h => InvestorTriesChanged -= h);
            });

        public static event Action<bool> X20BoostMultStarted;
        public static Lazy<IObservable<bool>> X20BoostObservable =
            new Lazy<IObservable<bool>>(() =>
            {
                return Observable.FromEvent<Action<bool>, bool>(
                    conversion: rxHandler => (state) => rxHandler(state),
                    addHandler: h => X20BoostMultStarted += h,
                    removeHandler: h => X20BoostMultStarted -= h);
            });

        public static event Action<TransferCashInfo> OfficialTransfer;
        public static Lazy<IObservable<TransferCashInfo>> OfficialTransferObservable =
            new Lazy<IObservable<TransferCashInfo>>(() =>
            {
                return Observable.FromEvent<Action<TransferCashInfo>, TransferCashInfo>(
                    conversion: rxHandler => (state) => rxHandler(state),
                    addHandler: h => OfficialTransfer += h,
                    removeHandler: h => OfficialTransfer -= h);
            });

        public static event Action<UnofficialTransferCashInfo> UnofficialTransfer;
        public static Lazy<IObservable<UnofficialTransferCashInfo>> UnofficialTransferObservable =
            new Lazy<IObservable<UnofficialTransferCashInfo>>(() =>
            {
                return Observable.FromEvent<Action<UnofficialTransferCashInfo>, UnofficialTransferCashInfo>(
                    conversion: rxHandler => (state) => rxHandler(state),
                    addHandler: h => UnofficialTransfer += h,
                    removeHandler: h => UnofficialTransfer -= h);
            });

        public static event Action<PlanetState, PlanetState, PlanetInfo> PlanetStateChanged;
        public static Lazy<IObservable<PlanetStateChangedData>> PlanetStateChangedObservable =
            new Lazy<IObservable<PlanetStateChangedData>>(() =>
            {
                return Observable.FromEvent<Action<PlanetState, PlanetState, PlanetInfo>, PlanetStateChangedData>(
                    conversion: rxHandler => (oldstate, newstate, planet) => rxHandler(new PlanetStateChangedData
                    {
                        OldState = oldstate,
                        NewState = newstate,
                        Planet = planet
                    }),
                    addHandler: h => PlanetStateChanged += h,
                    removeHandler: h => PlanetStateChanged -= h);
            });

        public static event Action<string> LegacyViewRemoved;
        public static Lazy<IObservable<string>> LegacyViewRemovedObservable =
            new Lazy<IObservable<string>>(() =>
            {
                return Observable.FromEvent<Action<string>, string>(
                    conversion: rxHandler => (viewName) => rxHandler(viewName),
                    addHandler: h => LegacyViewRemoved += h,
                    removeHandler: h => LegacyViewRemoved -= h);
            });

        public static event Action<int, int, ManagerEfficiencyRollbackLevel> EfficiencyLevelChanged;
        public static IObservable<EfficiencyLevelChangedArgs> EfficiencyLevelChangedObservable =
            Observable.FromEvent<Action<int, int, ManagerEfficiencyRollbackLevel>, EfficiencyLevelChangedArgs>(
                conversion: rxHandler => (oldLevel, newLevel, manager) => rxHandler(new EfficiencyLevelChangedArgs
                {
                    OldLevel = oldLevel,
                    NewLevel = newLevel,
                    Manager = manager
                }),
                addHandler: h => EfficiencyLevelChanged += h,
                removeHandler: h => EfficiencyLevelChanged -= h);


        public static event Action<int, int, ManagerEfficiencyRollbackLevel> RollbackLevelChanged;

        public static event Action<bool, ManagerEfficiencyRollbackLevel> ManagerMegaChanged;

        public static IObservable<MaxRollbackLevelChangedArgs> RollbackLevelChangedObservable =
            Observable.FromEvent<Action<int, int, ManagerEfficiencyRollbackLevel>, MaxRollbackLevelChangedArgs>(
                conversion: rxHandler => (oldLevel, newLevel, manager) => rxHandler(new MaxRollbackLevelChangedArgs
                {
                    OldLevel = oldLevel,
                    NewLevel = newLevel,
                    Manager = manager
                }),
                addHandler: h => RollbackLevelChanged += h,
                removeHandler: h => RollbackLevelChanged -= h);

        public static event Action<int, double, int> BusinessWasSoldToInvestors;
        public static IObservable<BusinessSoldInvestorsEventArgs> BusinessSoldInvestorsObservable =
            Observable.FromEvent<Action<int, double, int>, BusinessSoldInvestorsEventArgs>(
                conversion: rxHandler => (planet, secs, interval) => rxHandler(new BusinessSoldInvestorsEventArgs(planet, secs, interval)),
                addHandler: h => BusinessWasSoldToInvestors += h,
                removeHandler: h => BusinessWasSoldToInvestors -= h);

        public static event Action<CurrencyNumber, CurrencyNumber> CompanyCashChanged;
        public static IObservable<CurrencyNumberChangedArgs> CompanyCashChangedObservable =
            Observable.FromEvent<Action<CurrencyNumber, CurrencyNumber>, CurrencyNumberChangedArgs>(
                conversion: rxHandler => (o, n) => rxHandler(new CurrencyNumberChangedArgs { OldValue = o, NewValue = n }),
                addHandler: h => CompanyCashChanged += h,
                removeHandler: h => CompanyCashChanged -= h);


        public static event Action<CurrencyNumber, CurrencyNumber> PlayerCashChanged;
        public static IObservable<CurrencyNumberChangedArgs> PlayerCashChangedObservable =
            Observable.FromEvent<Action<CurrencyNumber, CurrencyNumber>, CurrencyNumberChangedArgs>(
                    conversion: rxHandler => (o, n) => rxHandler(new CurrencyNumberChangedArgs { OldValue = o, NewValue = n }),
                    addHandler: h => PlayerCashChanged += h,
                    removeHandler: h => PlayerCashChanged -= h);


        public static event Action<int, int> CoinsChanged;
        public static IObservable<IntValueChangedArgs> CoinsChangedObservable =
            Observable.FromEvent<Action<int, int>, IntValueChangedArgs>(
                conversion: rxHandler => (o, n) => rxHandler(new IntValueChangedArgs { OldValue = o, NewValue = n }),
                addHandler: h => CoinsChanged += h,
                removeHandler: h => CoinsChanged -= h);


        public static event Action<CurrencyNumber, CurrencyNumber> SecuritiesChanged;
        public static IObservable<CurrencyNumberChangedArgs> SecuritiesChangedObservable =
            Observable.FromEvent<Action<CurrencyNumber, CurrencyNumber>, CurrencyNumberChangedArgs>(
            conversion: rxHandler => (o, n) => rxHandler(new CurrencyNumberChangedArgs { OldValue = o, NewValue = n }),
            addHandler: h => SecuritiesChanged += h,
            removeHandler: h => SecuritiesChanged -= h);


        public static event Action PlanetsReceivedFromServer;

        public static event Action<PlanetInfo, PlanetInfo> CurrentPlanetChanged;
        

        public class X20BoostStateChangedArgs {
            public BoostState OldState { get; set; }
            public BoostState NewState { get; set; }
        }

        public static IObservable<X20BoostStateChangedArgs> X20BoostStateChangedObservable =
            Observable.FromEvent<Action<BoostState, BoostState>, X20BoostStateChangedArgs>(
                conversion: rxHandler => (os, ns) => rxHandler(new X20BoostStateChangedArgs { OldState = os, NewState = ns }),
                addHandler: h => X20BoostStateChanged += h,
                removeHandler: h => X20BoostStateChanged -= h);

        public static event Action<BoostState, BoostState> X20BoostStateChanged;


        public static readonly Subject<int> GeneratorCountButtonClickedObservable
            = new Subject<int>();

        
        public static event Action<TransportUnitInfo> GeneratorUnitsCountChanged;
        public static event Action<int, List<ExtendedAchievmentInfo>> GeneratorAchievmentsReceived;
        public static event Action<GeneratorInfo> GeneratorResearched;
        public static event Action<GeneratorInfo> GeneratorEnhanced;
        public static event Action<int, bool> AutomaticChanged;
        public static event Action GeneratorBalanceLoadedFromNet;
        public static event Action<ManagerInfo> TransportManagerHired;
        public static event Action<GeneratorState, GeneratorState, GeneratorInfo> GeneratorStateChanged;
        public static event Action Pause;
        public static event Action Resume;
        public static event Action Quit;
        public static event Action<double, double, ManagerInfo> ManagerCashOnHandChanged;
        public static event Action<double, bool, ManagerInfo> ManagerKickBack;
        public static event Action<float, float> InvestorEffectivenessChanged;
        public static event Action<int, int, ReportInfo> ReportCountChanged;
        public static event Action<int, int, SecretaryInfo> SecretaryCountChanged;
        public static event Action<int, int> BankAccumulatedCoinsChanged;
        public static event Action<int, int> BankLevelChanged;
        public static event Action<string> NetResourceLoaded;
        
        public static event Action<string> LegacyViewAdded;
        public static event Action<int, BuyInfo> GeneratorButtonStateChanged;
        public static event Action<int> GeneratorLevelUpdated;
        public static event Action<int, int> LegacyBuyMultiplierChanged;
        public static event Action<Gender, Gender> GenderChanged;
        public static event Action<int, int> XPChanged;
        public static event Action<int, int> LevelChanged;
        public static event Action<int, int> XPLevelLimitChanged;
        public static event Action<long, long> StatusPointsChanged;
        
        public class StatusLevelChangedArgs : EventArgs {
            public int OldStatus { get; set; }
            public int NewStatus { get; set; }
        }
        public static event Action<int, int> StatusLevelChanged;
        public static IObservable<StatusLevelChangedArgs> StatusLevelChangedObservable =
            Observable.FromEvent<Action<int, int>, StatusLevelChangedArgs>(
                conversion: rxHandler => (oldLevel, newLevel) => rxHandler(
                    new StatusLevelChangedArgs { OldStatus = oldLevel, NewStatus = newLevel })
                , addHandler: h => StatusLevelChanged += h,
                removeHandler: h => StatusLevelChanged -= h);


        public static IObservable<TutorialEventData> TutorialEventObservable =
            Observable.FromEvent<Action<TutorialEventData>, TutorialEventData>(
                conversion: rxHandler => (data) => rxHandler(data),
                addHandler: h => TutorialEvent += h,
                removeHandler: h => TutorialEvent -= h
                );
        public static event Action<TutorialEventData> TutorialEvent;

        public static event Action<double, double, ManagerInfo> MaxEfficiencyChanged;
        
        
        
        public static event Action<int, int> InvestorTriesCountChanged;
        

        public static event Action<UpgradeData> UpgradeAdded;
        public static event Action<bool> UpgradeQuickBuyResearched;
        public static event Action<Badge> BadgeAdded;
        public static event Action<int, int> AchievmentPointsChanged;
        public static event Action<int> SecretaryReportHandled;
        public static event Action<int> MechanicUnitHandled;
        
        public static event Action<double, double> MaxCompanyCashChanged;
        public static event Action<double, double> MaxSecuritiesChanged;
        public static event Action<double, double> MaxPlayerCashChanged;
        
        public static event Action<GeneratorInfo, double, double, ProfitResult> AccumulationProgressChanged;
        public static event Action<GeneratorInfo, ProfitResult> AccumulationCompleted;
        public static event Action<int, int> PrizeWheelMaxTriesChanged;
        public static event Action<int, int> PrizeWheelTriesChanged;        
        
        public static event Action<int, int> TreasureHuntMaxTriesChanged;
        public static event Action<int, int> TreasureHuntTriesChanged;
        public static event Action TreasureHuntReload;
        
        public static event Action<int, int> SplitMaxTriesChanged;
        public static event Action<int, int> SplitTriesChanged;
        public static event Action<int, int> SplitLevelChanged;

        public static event Action<int, int> AvailableRewardsChanged;
        public static event Action<bool, bool> MicromanagerStateChanged;
        
        public static event Action<bool, bool> TimeChangeServiceStateChanged;

        public static event Action<TempMechanicState, TempMechanicState, TempMechanicInfo> TempMechanicInfoStateChanged;
        public static event Action<TempMechanicInfo> TempMechanicAdded;
        public static event Action<List<TempMechanicInfo>> TempMechanicsRemoved;

        public static event Action<MechanicState, MechanicState, MechanicInfo> MechanicStateChanged;
        public static event Action<MechanicInfo> MechanicAdded;
        public static event Action<MechanicInfo, int> MechanicWorkCircleCompleted;

        public static event Action<AuditorState, AuditorState, Auditor> AuditorStateChanged;
        public static event Action<List<Auditor>> AuditorsRemoved;
        public static event Action<Auditor> AuditorAdded;

        public static event Action<SecretaryInfo, int> SecretaryWorkCircleCompleted;
        public static event Action<SecretaryState, SecretaryState, SecretaryInfo> SecretaryStateChanged;

        public static event Action<int, int> WinGame;



        public static event Action<DailyBonusItem> DailyBonusClaimed;

        public static event Action<Reward> PrizeWheelRewardClaimed;
        
        public static event Action<TutorialState> TutorialStateActivityChanged;
        public static event Action<TutorialState> TutorialStateCompletedChanged;

        public static event Action<TutorialState, int, int> TutorialStateStageChanged;

        public static void OnTutorialStateStageChanged(TutorialState state, int oldStage, int newStage)
            => TutorialStateStageChanged?.Invoke(state, oldStage, newStage);

        public static void OnOfficialTransfer(TransferCashInfo info)
            => OfficialTransfer?.Invoke(info);

        public static void OnTutorialStaticActivityChanged(TutorialState state)
            => TutorialStateActivityChanged?.Invoke(state);

        public static void OnTutorialStateCompletedChanged(TutorialState state)
            => TutorialStateCompletedChanged?.Invoke(state);

        public static void OnDailyBonusClaimed(DailyBonusItem item)
            => DailyBonusClaimed?.Invoke(item);
        
        public static void OnPrizeWheelRewardClaimed(Reward item)
            => PrizeWheelRewardClaimed?.Invoke(item);

        public static void OnTutorialEvent(TutorialEventData data)
            => TutorialEvent?.Invoke(data);

        public static void OnMaxSecuritiesChanged(double oldValue, double newValue)
            => MaxSecuritiesChanged?.Invoke(oldValue, newValue);

        public static void OnMaxPlayerCashChanged(double oldValue, double newValue)
            => MaxPlayerCashChanged?.Invoke(oldValue, newValue);

        public static void OnWinGame(int winCount, int coinRewardCount)
            => WinGame?.Invoke(winCount, coinRewardCount);
        
        public static void OnSecretaryStateChanged(SecretaryState oldState, SecretaryState newState, SecretaryInfo secretary)
            => SecretaryStateChanged?.Invoke(oldState, newState, secretary);

        public static void OnSecretaryWorkCircleCompleted(SecretaryInfo secretary, int circleCount)
            => SecretaryWorkCircleCompleted?.Invoke(secretary, circleCount);

        public static void OnMechanicWorkCircleCompleted(MechanicInfo mechanic, int circleCount)
            => MechanicWorkCircleCompleted?.Invoke(mechanic, circleCount);

        public static void OnMechanicAdded(MechanicInfo mechanic)
            => MechanicAdded?.Invoke(mechanic);

        public static void OnMechanicStateChanged(MechanicState oldState, MechanicState newState, MechanicInfo mechanic)
            => MechanicStateChanged?.Invoke(oldState, newState, mechanic);

        public static void OnAuditorAdded(Auditor auditor)
            => AuditorAdded?.Invoke(auditor);

        public static void OnAuditorsRemoved(List<Auditor> auditors)
            => AuditorsRemoved?.Invoke(auditors);

        public static void OnAuditorStateChanged(AuditorState oldState, AuditorState newState, Auditor auditor)
            => AuditorStateChanged?.Invoke(oldState, newState, auditor);

        public static void OnTempMechanicsRemoved(List<TempMechanicInfo> removedMechanics)
            => TempMechanicsRemoved?.Invoke(removedMechanics);

        public static void OnTempMechanicAdded(TempMechanicInfo mechanic)
            => TempMechanicAdded?.Invoke(mechanic);

        public static void OnTempMechanicInfoStateChanged(TempMechanicState oldState, TempMechanicState newState, TempMechanicInfo info)
            => TempMechanicInfoStateChanged?.Invoke(oldState, newState, info);

        public static void OnTimeChangeServiceStateChanged(bool oldValue, bool newValue, Func<bool> eventPredicate = null) {
            if(eventPredicate == null ) {
                TimeChangeServiceStateChanged?.Invoke(oldValue, newValue);
            } else {
                if(eventPredicate()) {
                    TimeChangeServiceStateChanged?.Invoke(oldValue, newValue);
                }
            }
        }

        public static void OnUnofficialTransfer(UnofficialTransferCashInfo info)
            => UnofficialTransfer?.Invoke(info);

        public static void OnMicromanagerStateChanged(bool oldValue, bool newValue)
            => MicromanagerStateChanged?.Invoke(oldValue, newValue);

        public static void OnAvailableRewardsChanged(int oldCount, int newCount)
            => AvailableRewardsChanged?.Invoke(oldCount, newCount);

        public static void OnSplitMaxTriesChanged(int oldCount, int newCount)
            => SplitMaxTriesChanged?.Invoke(oldCount, newCount);

        public static void OnSplitTriesChanged(int oldCount, int newCount)
            => SplitTriesChanged?.Invoke(oldCount, newCount);
        
        public static void OnSplitLevelChanged(int oldLevel, int newLevel)
            => SplitLevelChanged?.Invoke(oldLevel, newLevel);

        public static void OnPrizeWheelTriesChanged(int oldCount, int newCount)
            => PrizeWheelTriesChanged?.Invoke(oldCount, newCount);
        
        public static void OnPrizeWheelMaxTriesChanged(int oldCount, int newCount)
            => PrizeWheelMaxTriesChanged?.Invoke(oldCount, newCount);

        
        public static void OnTreasureHuntTriesChanged(int oldCount, int newCount)
            => TreasureHuntTriesChanged?.Invoke(oldCount, newCount);
        
        public static void OnTreasureHuntMaxTriesChanged(int oldCount, int newCount)
            => TreasureHuntMaxTriesChanged?.Invoke(oldCount, newCount);     
        
        public static void OnTreasureHuntReload()
            => TreasureHuntReload?.Invoke();
        
        public static void OnAccumulationCompleted(GeneratorInfo generator, ProfitResult profit) {
            AccumulationCompleted?.Invoke(generator, profit);
        }

        public static void OnAccumulationProgressChanged(GeneratorInfo generator, double timer, double interval, ProfitResult profit) {
            AccumulationProgressChanged?.Invoke(generator, timer, interval, profit);
        }
        public static void OnMaxCompanyCashChanged(double oldValue, double newValue)
            => MaxCompanyCashChanged?.Invoke(oldValue, newValue);

        public static void OnMechanicUnitHandled(int unitCount)
            => MechanicUnitHandled?.Invoke(unitCount);

        public static void OnSecretaryReportHandled(int count)
            => SecretaryReportHandled?.Invoke(count);

        public static void OnAchievmentPointsChanged(int oldCount, int newCount)
            => AchievmentPointsChanged?.Invoke(oldCount, newCount);

        public static void OnBadgeAdded(Badge badge)
            => BadgeAdded?.Invoke(badge);

        public static void OnUpgradeQuickBuyResearched(bool isResearched)
            => UpgradeQuickBuyResearched?.Invoke(isResearched);

        public static void OnUpgradeAdded(UpgradeData data)
            => UpgradeAdded?.Invoke(data);

        public static void OnViewShowed(ViewType viewType)
            => ViewShowed?.Invoke(viewType);

        public static void OnViewHided(ViewType viewType)
            => ViewHided?.Invoke(viewType);


            
        public static void OnBusinessWasSoldToInvestors(int planetId, double securitiesAdded, int interval)
            => BusinessWasSoldToInvestors?.Invoke(planetId, securitiesAdded, interval);

        public static void OnInvestorTriesCountChanged(int oldCount, int newCount)
            => InvestorTriesCountChanged?.Invoke(oldCount, newCount);
            
        public static void OnEfficiencyImproveLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel manager)
            => EfficiencyLevelChanged?.Invoke(oldLevel, newLevel, manager);

        public static void OnRollbackImproveLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel manager)
            => RollbackLevelChanged?.Invoke(oldLevel, newLevel, manager);
        
        public static void OnMegaImproveChanged(bool isMega, ManagerEfficiencyRollbackLevel manager)
            => ManagerMegaChanged?.Invoke(isMega, manager);

        public static void OnMaxEfficiencyChanged(double oldValue, double newValue, ManagerInfo manager)
            => MaxEfficiencyChanged?.Invoke(oldValue, newValue, manager);

        public static void OnMaxRollbackChanged(double oldValue, double newValue, ManagerInfo manager)
            => MaxRollbackChanged?.Invoke(oldValue,  newValue, manager);


        public static void OnStatusLevelChanged(int oldLevel, int newLevel)
            => StatusLevelChanged?.Invoke(oldLevel, newLevel);

        public static void OnProductPurchased(ProductData data)
            => ProductPurchased?.Invoke(data);

        public static void OnStatusPointsChanged(long oldCount, long newCount)
            => StatusPointsChanged?.Invoke(oldCount, newCount);

        public static void OnXPLevelLimitChanged(int oldCount, int newCount)
            => XPLevelLimitChanged?.Invoke(oldCount, newCount);
            
        public static void OnLevelChanged(int oldValue, int newValue)
            => LevelChanged?.Invoke(oldValue, newValue);

        public static void OnXPChanged(int oldCount, int newCount)
            => XPChanged?.Invoke(oldCount, newCount);

        public static void OnGenderChanged(Gender oldValue, Gender newValue)
            => GenderChanged?.Invoke(oldValue, newValue);

        public static void OnLegacyBuyMultiplierChanged(int generatorId, int value) 
            => LegacyBuyMultiplierChanged?.Invoke(generatorId, value);
            
        //public static event Action<int> MechanicCountChanged;

        public static void OnGeneratorLevelUpdated(int generatorId)
            => GeneratorLevelUpdated?.Invoke(generatorId);

        public static void OnGeneratorButtonStateChanged(int generatorId, BuyInfo buyInfo)
            => GeneratorButtonStateChanged?.Invoke(generatorId, buyInfo);

        public static void OnGeneratorEnhanced(GeneratorInfo generator)
            => GeneratorEnhanced?.Invoke(generator);

        public static void OnLegacyViewAdded(string viewName) => LegacyViewAdded?.Invoke(viewName);
        public static void OnLegacyViewRemoved(string viewName) => LegacyViewRemoved?.Invoke(viewName);


        public static void OnSecuritiesChanged(CurrencyNumber oldValue, CurrencyNumber newValue)
            => SecuritiesChanged?.Invoke(oldValue, newValue);

        public static void OnCoinsChanged(int oldValue, int newValue)
            => CoinsChanged?.Invoke(oldValue, newValue);

        public static void OnPlayerCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) 
            => PlayerCashChanged?.Invoke(oldValue, newValue);

        public static void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) 
            => CompanyCashChanged?.Invoke(oldValue, newValue);

        public static void OnShopItemPurchased(IShopItem shopItem) 
            => ShopItemPurchased?.Invoke(shopItem);

        public static void OnAchievmentCompleted(IAchievment achievment) 
            => AchievmentCompleted?.Invoke(achievment);

        public static void OnRewardedVideoFinished(int totalWatched) 
            => RewardedVideoFinished?.Invoke(totalWatched);

        public static void OnGameModeChanged(GameModeName prevGameMode, GameModeName newGameMode) 
            => GameModeChanged?.Invoke(prevGameMode, newGameMode);

        public static void OnPlanetsReceivedFromServer()
            => PlanetsReceivedFromServer?.Invoke();

        public static void OnCurrentPlanetChanged(PlanetInfo oldPlanet, PlanetInfo newPlanet)
            => CurrentPlanetChanged?.Invoke(oldPlanet, newPlanet);

        public static void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet)
            => PlanetStateChanged?.Invoke(oldState, newState, planet);

        public static void OnBoostX20StateChanged(BoostState oldState, BoostState newState)
            => X20BoostStateChanged?.Invoke(oldState, newState);

        public static void OnX20BoostMultStarted(bool isStarted)
            => X20BoostMultStarted?.Invoke(isStarted);

        public static void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo module)
            => ShipModuleStateChanged?.Invoke(oldState, newState, module);

        public static void OnGeneratorUnitsCountChanged(TransportUnitInfo info) {
            GeneratorUnitsCountChanged?.Invoke(info);
        }

        public static void OnGeneratorAchievmentsReceived(int generatorId, List<ExtendedAchievmentInfo> achievments)
            => GeneratorAchievmentsReceived?.Invoke(generatorId, achievments);

        public static void OnGeneratorResearched(GeneratorInfo generator)
            => GeneratorResearched?.Invoke(generator);

        public static void OnAutomaticChanged(int generatorId, bool isAuto)
            => AutomaticChanged?.Invoke(generatorId, isAuto);

        public static void OnGeneratorBalanceLoadedFromNet() {
            GeneratorBalanceLoadedFromNet?.Invoke();
        }

        public static void OnTransportManagerHired(ManagerInfo manager)
            => TransportManagerHired?.Invoke(manager);

        public static void OnGeneratorStateChanged(GeneratorState oldState, GeneratorState newState, GeneratorInfo generator)
            => GeneratorStateChanged?.Invoke(oldState, newState, generator);

        public static void OnPause() => Pause?.Invoke();
        public static void OnResume() => Resume?.Invoke();
        public static void OnQuit() => Quit?.Invoke();

        public static void OnManagerCashOnHandChanged(double oldCash, double newCash, ManagerInfo manager)
            => ManagerCashOnHandChanged?.Invoke(oldCash, newCash, manager);

        public static void OnManagerKickBack(double payedCount, bool isFirstKickback, ManagerInfo manager)
            => ManagerKickBack?.Invoke(payedCount, isFirstKickback, manager);

        //public static void OnMechanicAdded(int generatorId, int addedCount)
        //    => MechanicAdded?.Invoke(generatorId, addedCount);
        public static void OnInvestorEffectivenessChanged(float oldEffectiveness, float newEffectiveness)
            => InvestorEffectivenessChanged?.Invoke(oldEffectiveness, newEffectiveness);

        //public static void OnLeaseManagerAvailableChanged(LeaseManagerInfo manager)
        //    => LeaseManagerAvailableChanged?.Invoke(manager);

        //public static void OnEfficiencyChanged(double efficiencyChange, ManagerInfo manager)
        //    => EfficiencyChanged?.Invoke(efficiencyChange, manager);

        //public static void OnMechanicCountChanged(int generatorId)
        //    => MechanicCountChanged?.Invoke(generatorId);

        public static void OnReportCountChanged(int oldReport, int newReport, ReportInfo reportInfo)
            => ReportCountChanged?.Invoke(oldReport, newReport, reportInfo);

        //public static void OnAuditorAvailableChanged(AuditorInfo auditor)
        //    => AuditorAvailableChanged?.Invoke(auditor);

        public static void OnSecretaryCountChanged(int oldCount, int newCount, SecretaryInfo secretary)
            => SecretaryCountChanged?.Invoke(oldCount, newCount, secretary);

        public static void OnBankAccumulatedCoinsChanged(int oldCount, int newCount)
            => BankAccumulatedCoinsChanged?.Invoke(oldCount, newCount);

        public static void OnBankLevelChanged(int oldLevel, int newLevel)
            => BankLevelChanged?.Invoke(oldLevel, newLevel);

        public static void OnNetResourceLoaded(string resourceName)
            => NetResourceLoaded?.Invoke(resourceName);
    }


    public class DebugObserver<T> : IObserver<T> {

        public string Name { get; private set; }

        public DebugObserver(string name) {
            Name = name;
            //System.Tuple<int> t = System.Tuple.Create(3);
        }

        public void OnCompleted() {
            UnityEngine.Debug.Log($"completed");
        }

        public void OnError(Exception error) {
            UnityEngine.Debug.LogException(error);
        }

        public void OnNext(T value) {
            UnityEngine.Debug.Log($"Observer: {Name}, OnNext => {value.ToString()}");
        }
    }

    public class RollbackChangedEventArgs {
        public double OldValue { get; private set; }
        public double NewValue { get; private set; }
        public ManagerInfo Manager { get; private set; }

        public RollbackChangedEventArgs(double oldValue, double newValue, ManagerInfo manager) {
            OldValue = oldValue;
            NewValue = newValue;
            Manager = manager;
        }
    }

    public enum ScreenType {
        CoinUpgrades
    }

}