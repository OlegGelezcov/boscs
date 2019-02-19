using UniRx;

namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Bos.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UDebug = UnityEngine.Debug;

    public class GenerationService : SaveableGameBehaviour, IGenerationService {

        public const int TELEPORT_ID = 9;

        public GeneratorInfoCollection Generators { get; } = new GeneratorInfoCollection();


        private readonly UpdateTimer planetGeneratorStatesTimer = new UpdateTimer();

        private bool isNeedUpdateOnResume = true;

        public bool IsResumed { get; private set; } = false;
        public double TotalOfflineBalance { get; private set; } = 0.0;
        private int buyedGeneratorCount = 0;
        private bool IsInitialized { get; set; } = false;

        public void AddBuyedGeneratorCount(GeneratorInfo generator, int count) {
            int oldCount = buyedGeneratorCount;
            buyedGeneratorCount += count;
            if (oldCount != buyedGeneratorCount) {
                GameEvents.OnGeneratorLevelUpdated(generator.GeneratorId);
            }
        }

        private IPlanetService planetService = null;
        public IPlanetService PlanetService
            => (planetService != null) ? planetService :
            (planetService = Services.GetService<IPlanetService>());


        public void Setup(object data = null) {
            StartCoroutine(FirstUpdatePlanetGeneratorStatesImpl());
            planetGeneratorStatesTimer.Setup(0.5f, (delta) => {
                if(IsLoaded && Services.GameModeService.IsGame) {
                    UpdatePlanetStateGenerators();
                }
            });

            if (false == IsInitialized) {
                Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(val => {
                    if (IsLoaded && IsResumed && GameMode.GameModeName == GameModeName.Game) {
                        Generators.ClearExpiredBoosts(TimeService.UnixTimeInt);
                        //UDebug.Log($"clear expired boosts : {val}");
                    }
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GeneratorBalanceLoadedFromNet += OnGeneratorBalanceLoadedFromNet;
            //GameEvents.EfficiencyChanged += OnEfficiencyChanged;
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;
            GameEvents.GeneratorResearched += OnGeneratorResearched;
            GameEvents.BusinessWasSoldToInvestors += OnBusinessWasSoldToInvestors;
            GameEvents.TransportManagerHired += OnManagerHired;
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
            GameEvents.EfficiencyChangeEvent += OnEfficiencyChanged;
        }

        public override void OnDisable() {
            GameEvents.GeneratorBalanceLoadedFromNet -= OnGeneratorBalanceLoadedFromNet;
            //GameEvents.EfficiencyChanged -= OnEfficiencyChanged;
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            GameEvents.GeneratorResearched -= OnGeneratorResearched;
            GameEvents.BusinessWasSoldToInvestors -= OnBusinessWasSoldToInvestors;
            GameEvents.TransportManagerHired -= OnManagerHired;
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            GameEvents.EfficiencyChangeEvent -= OnEfficiencyChanged;
            base.OnDisable();
        }

        private void OnEfficiencyChanged(GameEvents.EfficiencyChange change ) {
            if(IsLoaded) {
                OnEfficiencyChanged(change.NewEfficiency - change.OldEfficiency, change.Manager);
            }
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if (IsLoaded) {

                if (newState == PlanetState.Opened) {
                    if (GameMode.IsGame) {
                        int countRemoved = RemoveBoosts(BoostSourceType.CoinUpgrade);
                        UDebug.Log($"count of boosts removed: {countRemoved}");
                    }
                    MakeGeneratorsManual();
                }
            }
        }


        private void OnApplicationPause(bool pause) 
            => UpdateResume(pause);

        private void OnApplicationFocus(bool focus) 
            => UpdateResume(!focus);

        public void UpdateResume(bool pause) {
            //UDebug.Log($"{nameof(GenerationService)}.{nameof(UpdateResume)}() => {pause}");
            UpdateOnResume(pause);
        }

        private void UpdateOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
                IsResumed = false;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;

                    
                    StartCoroutine(UpdateOnResumeImpl());
                }
            }
        }

        private System.Collections.IEnumerator UpdateOnResumeImpl() {

            Services.GetService<IConsoleService>()?.AddOnScreenText("UpdateOnResumeImpl() generation service...");

            yield return new WaitUntil(() => IsLoaded && Services.ResourceService.IsLoaded);
            yield return new WaitUntil(() => Services.TransportService.IsLoaded && Services.PlayerService.IsLoaded && Services.TimeChangeService.IsLoaded);
            yield return new WaitUntil(() => Services.InvestorService.IsLoaded);
            yield return new WaitUntil(() => Services.GetService<ISleepService>().IsRunning);
            
            yield return new WaitUntil(() => Player.LegacyPlayerData != null);

            ITransportUnitsService unitService = Services.TransportService;
            IInvestorService investorService = Services.InvestorService;
            ISleepService sleepService = Services.GetService<ISleepService>();
            IPlayerService playerService = Services.PlayerService;


            int interval = sleepService.SleepInterval;

            TotalOfflineBalance = 0.0;
            
            //clear expired timed boosts
            Generators.ClearExpiredBoosts(TimeService.UnixTimeInt);
            
            foreach(GeneratorInfo generator in Generators.PlanetGenerators) {

                if(generator.State == GeneratorState.Active) {

                    ProfitResult profitResult = generator.ConstructProfitResult(Generators);

                    if(generator.IsAutomatic) {
                        TotalOfflineBalance += generator.UpdateAutomaticAfterSleep(interval, Generators);

                    } else if(generator.IsManual && generator.IsGenerationStarted){
                        generator.AddGenerateTimer(interval);
                        if(generator.GenerateTimer >= profitResult.GenerationInterval) {
                            generator.SetGenerateTimer(0);
                            generator.SetGenerationStarted(false);
                            playerService.AddGenerationCompanyCash(profitResult.ValuePerRound);
                           // UDebug.Log($"added to planet manual generator => {generator.GeneratorId} after sleep => {interval} seconds, value => {profitResult.ValuePerRound}".Colored(ConsoleTextColor.orange).Bold());
                            TotalOfflineBalance += profitResult.ValuePerRound;
                        }
                    }
                }
            }

            foreach(GeneratorInfo generator in Generators.NormalGenerators) {
                if(generator.State == GeneratorState.Active) {
                    ProfitResult profitResult = generator.ConstructProfitResult(Generators);

                    if(generator.IsAutomatic) {
                        TotalOfflineBalance += generator.UpdateAutomaticAfterSleep(interval, Generators);
                        //UDebug.Log($"added to normal automatic generator => {generator.GeneratorId} after sleep => {interval} seconds, value => {value}".Colored(ConsoleTextColor.orange).Bold());
                    } else if(generator.IsManual && generator.IsGenerationStarted) {

                        UDebug.Log($"manul gen =>{generator.GeneratorId} sleep update".Colored(ConsoleTextColor.green).Bold());
                        generator.AddGenerateTimer(interval);
                        if (generator.GenerateTimer >= profitResult.GenerationInterval) {
                            generator.SetGenerateTimer(0);
                            generator.SetGenerationStarted(false);
                            generator.SetAccumulatedCash(0);
                            playerService.AddGenerationCompanyCash(profitResult.ValuePerRound);
                            TotalOfflineBalance += profitResult.ValuePerRound;
                            generator.FireProgressEvent();
                            UDebug.Log($"added to normal manual generator => {generator.GeneratorId} after sleep => {interval} seconds, value => {profitResult.ValuePerRound}".Colored(ConsoleTextColor.orange).Bold());
                        }
                        //
                        //generator.Update(interval, Generators);
                    }
                }
            }
            //Services?.GetService<IConsoleService>()?.AddOnScreenText($"Added to company cash => {TotalOfflineBalance}");
            IsResumed = true;

        }

        private System.Collections.IEnumerator FirstUpdatePlanetGeneratorStatesImpl() {
            yield return new WaitUntil(() => IsLoaded && Services.ResourceService.IsLoaded);
            yield return new WaitUntil(() => Services.GameModeService.IsGame);
            UpdatePlanetStateGenerators();
        }

        public override void Update() {
            base.Update();          
            if (Services.IsInitialized && IsLoaded) {
                Generators.Update(Time.deltaTime);
                planetGeneratorStatesTimer.Update();
            }

        }





        //called when business sold to investors event
        private void OnBusinessWasSoldToInvestors(int planetId, double securitiesAdded, int interval) {
            //we reset buy counters on all generators to multiplier x1
            Generators.Generators.Values
                .ToList()
                .ForEach(generator => 
                            generator.SetBuyCountButtonState((int)GeneratorButtonState.State_1));
        } 

        private void OnGeneratorResearched(GeneratorInfo info) {
            if(info.Data.Type == GeneratorType.Planet) {
                UpdatePlanetStateGenerators();
            }
        }

        private void OnUnitCountChanged(TransportUnitInfo unit) {
            UpdatePlanetStateGenerators();
            if (IsLoaded) {
                Generators.GetGeneratorInfo(unit.GeneratorId).Update(0, Generators);
            }
        }

        public bool IsLockedByMars(GeneratorInfo generator) {
            if(generator.GeneratorId == Services.ResourceService.Defaults.teleporterId ) {
                bool isMarsOpened = PlanetService.IsMarsOpened;
                if(!isMarsOpened) {
                    return true;
                }
            }
            return false;
        }

        private void UpdatePlanetStateGenerators() {
            var generatorCollection = Services.ResourceService.Generators.Generators.Values;
            ITransportUnitsService unitService = Services.TransportService;
            IPlayerService playerService = Services.PlayerService;

            foreach(GeneratorData data in generatorCollection) {
                if(data.Type == GeneratorType.Planet || data.Type == GeneratorType.Normal ) {
                    GeneratorInfo info = Generators.GetGeneratorInfo(data.Id);
                    if (!info.IsResearched) {

                        Generators.SetState(info.GeneratorId, GeneratorState.Researchable);

                    } else {
                        bool hasAnyUnits = unitService.HasUnits(info.GeneratorId);
                        bool playerHasCashForUnlock = playerService.IsEnoughCompanyCash(info.Cost);
                        bool noAnyUnits = !hasAnyUnits;
                        if(noAnyUnits && playerHasCashForUnlock) {
                            if (info.IsDependent) {
                                GeneratorInfo dependentInfo = Generators.GetGeneratorInfo(info.RequiredGeneratorId);
                                if(dependentInfo.State == GeneratorState.Active) {
                                    Generators.SetState(info.GeneratorId, GeneratorState.Unlockable);
                                } else {
                                    Generators.SetState(info.GeneratorId, GeneratorState.Locked);
                                }
                            } else {
                                Generators.SetState(info.GeneratorId, GeneratorState.Unlockable);
                            }
                        } else if(noAnyUnits && !playerHasCashForUnlock) {
                            Generators.SetState(info.GeneratorId, GeneratorState.Locked);
                        } else if(hasAnyUnits) {
                            Generators.SetState(info.GeneratorId, GeneratorState.Active);
                        }
                    }
                }
            }
        }

        private void OnManagerHired(ManagerInfo manager) {
            if (manager.IsHired) {
                StartCoroutine(UpdateManagerProfitMult(manager));
            }
        }

        private void OnEfficiencyChanged(double change, ManagerInfo manager) {
            if(manager.IsHired) {
                StartCoroutine(UpdateManagerProfitMult(manager));
            }
        }

        private System.Collections.IEnumerator UpdateManagerProfitMult(ManagerInfo manager) {
            yield return new WaitUntil(() => IsLoaded);
            if (manager.IsHired) {
                Generators.AddProfitBoost(
                    generatorId: manager.Id,
                    boost: BoostInfo.CreateTemp(manager.ManagerProfitMultId, manager.Efficiency(Services)));

                double boostValue = Generators.GetGeneratorInfo(manager.Id).GetProfitBoostValue(manager.ManagerProfitMultId);
                UDebug.Log($"Update generator {manager.Id} efficiency mult to => {boostValue}".Attrib(bold: true, italic: true, color: "y"));
            }
        }

        private void OnGeneratorBalanceLoadedFromNet() {
            if(IsLoaded) {
                Generators.UpdateGeneratorBalance();
            }
        }

        public void ApplyTimeDenominatorToAllGenerators(BoostInfo boost) {
            GeneratorInfo[] generatorInfoArray = Generators.Generators.Values.ToArray();
            for(int i = 0; i < generatorInfoArray.Length; i++) {
                Generators.AddTimeBoost(
                    generatorId: generatorInfoArray[i].GeneratorId,
                    boost: new BoostInfo(boost.Id, boost.Value, boost.IsPersist, 0, BoostSourceType.Unknown));              
            }
        }

        public void Clear() {
            Generators.Clear();
        }


        #region IGenerationService

        public bool IsEnhanced(int generatorId)
            => Generators.IsEnhanced(generatorId);

        public bool IsResearched(int generatorId)
            => Generators.IsResearched(generatorId);

        public void Research(int generatorId)
            => Generators.Research(generatorId);

        public void Enhance(int generatorId)
            => Generators.Enhance(generatorId);


        /*
        public ProfitResult CalculateProfitPerSecond(GeneratorInfo generator, int countOfOwnedGenerators, bool forceUpdateMults = false) {
            double perRound = Generators.GetCurrentValueForProfit(generator, countOfOwnedGenerators, forceUpdateMults);
            double generationInterval = generator.Time(Generators); //Generators.GetCurrentValueForTime(generator);  
            double perSecond = perRound / generationInterval;
            return new ProfitResult(perRound, perSecond, generationInterval);
        }

        public double CalculateTotalProfitOnInterval(float seconds ) {
            double value = 0.0;
            foreach(var generator in Generators.Generators) {
                if(generator.Value.IsAutomatic) {
                    if(generator.Value.State == GeneratorState.Active) {
                        var profit = CalculateProfitPerSecond(generator.Value, Services.TransportService.GetUnitLiveCount(generator.Key));
                        value += profit.ValuePerSecond * seconds;
                    }
                }
            }
            return value;
        }*/

      
        public double CalculateProfit20Minute(int generatorId, int count, int planetId)
        {

            var generator = GetGetenerator(generatorId);
            var planetInfo  = PlanetService.GetPlanet(planetId);
            double perRound = generator.Data.BaseGeneration * count * planetInfo.Data.ProfitMultiplier * Math.Pow(generator.Data.ProfitIncrementFactor, count);
            double generationInterval = generator.Data.TimeToGenerate;
            double perSecond = perRound / generationInterval;
            double intervalProfit = perSecond * 1200;
            return intervalProfit;
        }
        
        private double GeneratorUnitBuyKoefficient(GeneratorData data) {
            //UnityEngine.Debug.Log($"current planet => {Services.PlanetService.CurrentPlanet.Id} generator increment factor => {data.IncrementFactor}");
            double buyKoeff =  (PlanetService?.CurrentPlanet?.Data?.TransportUnityPriceMult ?? 1.0) * data.IncrementFactor;
            //UnityEngine.Debug.Log($"GeneratorUnitBuyCoefficitent(), transport unit mult ={Services.PlanetService.CurrentPlanet.Data.TransportUnityPriceMult}," +
            //$"coef => {buyKoeff}");

            //only for debug output
            //if (data.Id == 0 ) {
            //    UDebug.Log($"GeneratorUnitBuyKoefficient() => planet coeff {Planets.CurrentPlanet.Data.TransportUnityPriceMult} * gen increment factor: {data.IncrementFactor}, result: {Planets.CurrentPlanet.Data.TransportUnityPriceMult * data.IncrementFactor}");
            //}
            return buyKoeff;
        }

        public int GetMaxNumberBuyable(double cash, int countOfUnits, GeneratorInfo generatorInfo) {
            double buyCoefficient = GeneratorUnitBuyKoefficient(generatorInfo.Data);            
            return (int)Math.Floor(Math.Log(((cash * (buyCoefficient - 1)) / (generatorInfo.Cost * Math.Pow(buyCoefficient, countOfUnits)) + 1), buyCoefficient));
        }

        public double CalculatePrice(int count, int ownedCount, GeneratorInfo info) {
            double price = 0.0f;
            switch(info.State) {
                case GeneratorState.Unlockable:
                case GeneratorState.Locked:
                {
                    price = info.Cost;
                }
                    break;
                case GeneratorState.Active: {
                        double buyCoefficient = GeneratorUnitBuyKoefficient(info.Data);
                        price = info.Cost * ((Math.Pow(buyCoefficient, ownedCount) *
                            (Math.Pow(buyCoefficient, count) - 1.0f)) / (buyCoefficient - 1.0f));

                    /*
                        //only for debug output
                        if(info.GeneratorId == 0 ) {
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            sb.AppendLine($"BUY_KOEF: planet * incr factor => {buyCoefficient}");
                            sb.AppendLine($"info COST => {info.Cost}");
                            sb.AppendLine($"BUY_KOEF ** OWNED_CNT => {Math.Pow(buyCoefficient, ownedCount)}");
                            sb.AppendLine($"(BUY_KOEF ** OWNED_CNT - 1)/(BUY_KOEF - 1) => {(Math.Pow(buyCoefficient, count) - 1) / (buyCoefficient - 1)}");
                            sb.AppendLine($"RESULT: {price}");
                            UDebug.Log(sb.ToString().Bold().Colored(ConsoleTextColor.cyan));

                        }*/

                    }
                    break;
            }
            return price;
        }

        public double CalculatePrice(int count, int ownedCount, int generatorId) {
            return CalculatePrice(count, ownedCount, GetGetenerator(generatorId));
        }

        public GeneratorInfo GetGetenerator(int generatorId)
            => Generators.GetGeneratorInfo(generatorId: generatorId);

        public int BuyGenerator(GeneratorInfo generator, int count, bool isFree = false) {
            ITransportUnitsService unitService = Services.TransportService;
            IPlayerService playerService = Services.PlayerService;

            if(unitService.HasUnits(generator.GeneratorId)) {
                if(!isFree) {
                    double price = CalculatePrice(count, unitService.GetUnitTotalCount(generator.GeneratorId), generator);
                    if(playerService.IsEnoughCompanyCash(price)) {
                        playerService.RemoveCompanyCash(price);
                        
                    } else {
                        return 0;
                    }
                }
                unitService.AddLiveUnits(generator.GeneratorId, count);
                AddBuyedGeneratorCount(generator, count);
                GlobalRefs.LevelManager.AddXP(XPSources.BuyGenerator * count);
            } else {
                if(!isFree) {
                    double price = CalculatePrice(count, 0, generator);
                    if(playerService.IsEnoughCompanyCash(price)) {
                        playerService.RemoveCompanyCash(price);
                    } else {
                        return 0;
                    }
                }
                unitService.AddLiveUnits(generator.GeneratorId, count);
                AddBuyedGeneratorCount(generator, 1);
                if (!Convert.ToBoolean(PlayerPrefs.GetInt("UnlockGenerator_" + generator.GeneratorId, 0))) {
                    FacebookEventUtils.LogApplyGeneratorEvent(generator.GeneratorId.ToString());
                    PlayerPrefs.SetInt("UnlockGenerator_" + generator.GeneratorId, 1);
                }
                GlobalRefs.LevelManager.AddXP(XPSources.UnlockGenerator * count);
            }
            playerService.CheckNonNegativeCompanyCash();
            StatsCollector.Instance[Stats.UNITS_BOUGHT] += count;
            Player.LegacyPlayerData.Save();
            StatsCollector.Instance.Save();
            return unitService.GetUnitTotalCount(generator.GeneratorId);
        }

        public void BuyGenerator(GeneratorInfo generator, bool isFree = false) {
            BuyGenerator(generator, 1, isFree);
        }

        public double GetGeneratorUnlockPrice(int generatorId)
            => GetGetenerator(generatorId)?.Cost ?? double.MaxValue;

        public IEnumerable<GeneratorInfo> GeneratorEnumarable
            => Generators.Generators.Values.Where(g => g.Data.Type == GeneratorType.Normal).OrderBy(g => g.GeneratorId).Concat(
                Generators.Generators.Values.Where(g => g.Data.Type == GeneratorType.Planet).OrderBy(g => g.GeneratorId));

        public bool IsLocked(int generatorId) {
            return (GetGetenerator(generatorId).State == GeneratorState.Locked);
        }

        public double GetTotalProfitOnTime(double interval) {
            double result = 0f;
            SimpleGlobalGenerationContext globalContext = new SimpleGlobalGenerationContext(
                (genId) => Services.TransportService.GetUnitTotalCount(genId)) {
                ProfitBoostValue = Generators.ProfitBoostValue,
                TimeBoostValue = Generators.TimeBoostValue
            };

            foreach(var generator in Generators.Generators.Values) {
                if (generator.State == GeneratorState.Active && generator.IsAutomatic) {
                    var profit = generator.ConstructProfitResult(globalContext);
                    result += profit.ValuePerSecond * interval;
                }
            }
            return result;
        }

        public void MakeGeneratorsManual() {
            foreach (var generator in Generators.NormalGenerators) {
                Generators.SetAutomatic(generator.GeneratorId, false);
                generator.SetGenerateTimer(0);
            }
        }

        public int RemoveBoosts(BoostSourceType sourceType)
            => Generators.RemoveTypedBoosts(sourceType);

        #endregion

        #region ISaveable
        public override string SaveKey => "generation_service";

        public override Type SaveType => typeof(GenerationServiceSave);

        public override object GetSave() {

            List<GeneratorInfoSave> generatorSaves = new List<GeneratorInfoSave>();
            foreach(var pair in Generators.Generators) {
                generatorSaves.Add(pair.Value.GetSave());
            }

            return new GenerationServiceSave {
                generators = generatorSaves,
                profitSave = Generators.ProfitBoosts.GetSave(),
                timeSave = Generators.TimeBoosts.GetSave()
            };
        }

        public override void ResetFull() {
            Generators.ResetFull();
            IsLoaded = true;
        }

        public override void ResetByInvestors(){
            Generators.ResetByInvestors();
        }

        public override void ResetByPlanets() {
            Generators.ResetByPlanets();
        }

        public override void LoadDefaults() {
            Generators.ClearGenerators();
            Generators.ProfitBoosts.ClearAll();
            Generators.TimeBoosts.ClearAll();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            try {
                GenerationServiceSave save = obj as GenerationServiceSave;

                if (save != null) {
                    save.Guard();

                    if(save.generators != null ) {
                        Generators.ClearGenerators();
                        foreach(var item in save.generators) {
                            GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(item.generatorId);
                            GeneratorLocalData localData = Services.ResourceService.GeneratorLocalData.GetLocalData(item.generatorId);
                            Generators.AddGeneratorInner(new GeneratorInfo(item, generatorData, localData));
                        }
                        
                    }

                    Generators.ProfitBoosts.Load(save.profitSave);
                    Generators.TimeBoosts.Load(save.timeSave);
                    IsLoaded = true;
                } else {
                    LoadDefaults();
                }
            } catch (Exception exception ) {
                UnityEngine.Debug.LogException(exception);
                LoadDefaults();
            }
        } 
        #endregion
    }

    public class SimpleGlobalGenerationContext : IGenerationGlobalContext {

        private Func<int, int> unitCountFunc;

        public SimpleGlobalGenerationContext(Func<int, int> unitCountFunc) {
            this.unitCountFunc = unitCountFunc;
        }

        public double ProfitBoostValue {
            get; set;
        }

        public double TimeBoostValue {
            get; set;
        }

        public int GetUnitCount(int generatorId) {
            return unitCountFunc(generatorId);
        }
    }


    public interface IGenerationService : IGameService {
        void ApplyTimeDenominatorToAllGenerators(BoostInfo boost);
        void Clear();
        GeneratorInfoCollection Generators { get; }
        //ProfitResult CalculateProfitPerSecond(GeneratorInfo generator, int countOfOwnedGenerators, bool forceUpdateMults = false);
        bool IsEnhanced(int generatorId);
        bool IsResearched(int generatorId);
        bool IsLocked(int generatorId);
        void Research(int generatorId);
        void Enhance(int generatorId);
        int GetMaxNumberBuyable(double cash, int countOfUnits, GeneratorInfo generatorData);
        double CalculatePrice(int count, int ownedCount, GeneratorInfo info);
        double CalculatePrice(int count, int ownedCount, int generatorId);
        int BuyGenerator(GeneratorInfo generator, int count, bool isFree = false);
        void BuyGenerator(GeneratorInfo generator, bool isFree = false);

        //double CalculateTotalProfitOnInterval(float seconds );
        bool IsLockedByMars(GeneratorInfo generator);
        bool IsResumed { get; }
        double TotalOfflineBalance { get; }
        GeneratorInfo GetGetenerator(int generatorId);
        void AddBuyedGeneratorCount(GeneratorInfo generator, int count);
        double CalculateProfit20Minute(int generatorId, int count, int planetId);
        double GetGeneratorUnlockPrice(int generatorId);
        bool IsLoaded { get; }
        IEnumerable<GeneratorInfo> GeneratorEnumarable { get; }
        double GetTotalProfitOnTime(double interval);
        void MakeGeneratorsManual();

        int RemoveBoosts(BoostSourceType sourceType);
    }

    public class GenerationServiceSave {
        public List<GeneratorInfoSave> generators;
        public BoostCollectionSave profitSave;
        public BoostCollectionSave timeSave;

        public void Guard() {
            if(generators == null ) { generators = new List<GeneratorInfoSave>(); }
            if(profitSave == null ) {
                profitSave = new BoostCollectionSave();
                profitSave.Guard();
            }
            if(timeSave == null ) {
                timeSave = new BoostCollectionSave();
                timeSave.Guard();
            }
        }
    }

    public enum GeneratorState  : int {
        Unknown = 0,
        Active = 1,
        Unlockable = 2,
        Locked = 3,
        Special = 4,
        Researchable = 5
        
    }
}