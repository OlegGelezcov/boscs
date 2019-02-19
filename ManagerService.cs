namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Analytics;
    using UnityEngine.SceneManagement;
    using UniRx;
    using UDBG = UnityEngine.Debug;

    public class ManagerService : SaveableGameBehaviour, IManagerService {

        public const int kFirstManagerCashOnHand = 1000;

        public Dictionary<int, ManagerInfo> Managers { get; } = new Dictionary<int, ManagerInfo>();
        public Dictionary<int, ManagerEfficiencyRollbackLevel> ManagerEfficiencyRollbackLevels { get; } = new Dictionary<int, ManagerEfficiencyRollbackLevel>();

        private readonly UpdateTimer efficiencyUpdateTimer = new UpdateTimer();

        private bool isNeedUpdateOnResume = true;
        public bool IsWakeUpCompleted { get; private set; }

        public KickbackInfo CurrentRollbackInfo { get; } = new KickbackInfo();

        private bool isInitialized = false;

        public void Setup(object obj) {
            efficiencyUpdateTimer.Setup(1.0f, (delta) => {
                if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                    UpdateManagersEfficiency(delta);
                }
            });

            if(!isInitialized) {
                GameEvents.ManagerDataReplacedSubject.Subscribe(data => {
                    if(Managers.ContainsKey(data.Id)) {
                        Managers[data.Id].UpdateData(data);
                    }
                }).AddTo(gameObject);
                isInitialized = true;
            }
        }

        public bool IsAnyHired
            => Managers.Values.Where(m => m.IsHired).Count() > 0;

        private void UpdateManagersEfficiency(float deltaTime) {

            bool isMoonOpened = Planets.IsMoonOpened;

            if (IsLoaded && Services.ResourceService.IsLoaded) {
                foreach (var kvp in Managers) {
                    if (kvp.Value.IsHired) {
                        if (isMoonOpened) {
                            kvp.Value.UpdateEfficiency(deltaTime, Services);
                        }

                        kvp.Value.TrackEfficiencyChange(Services);
                    }

                }

            }
        }

        public override void Update() {
            base.Update();
            efficiencyUpdateTimer.Update();
        }

        private void OnApplicationPause(bool pause) {
            UpdateResume(pause);
        }

        private void OnApplicationFocus(bool focus) {
            UpdateResume(!focus);
        }

        public void UpdateResume(bool pause) {
            //UDBG.Log($"{nameof(ManagerService)}.{nameof(UpdateResume)}() => {pause}");
            UpdateOnResume(pause);
        }

        private void UpdateOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
                IsWakeUpCompleted = false;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateOnResumeImpl() {
            yield return new WaitUntil(() => Services.ResourceService.IsLoaded && IsLoaded && Services.GameModeService.IsGame && Services.SecretaryService.IsLoaded);
            ISleepService sleepService = Services.GetService<ISleepService>();
            yield return new WaitUntil(() => sleepService.IsRunning);
            UpdateManagersEfficiency(sleepService.SleepInterval);
            //UnityEngine.Debug.Log($"Managers resume".Colored(ConsoleTextColor.green));
            IsWakeUpCompleted = true;
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ReportCountChanged += OnReportCountChanged;
        }

        public override void OnDisable() {
            GameEvents.ReportCountChanged -= OnReportCountChanged;
            base.OnDisable();
        }

        private void OnReportCountChanged(int oldCount, int newCount, ReportInfo reportInfo ) {
            if(IsLoaded) {
                /*
                int countOfCompleted = oldCount - newCount;
                if(countOfCompleted > 0 ) {
                    var manager = GetManager(reportInfo.ManagerId);
                    manager.AddEfficiency(Services.SecretaryService.GetRestoredEfficiency(reportInfo.ManagerId, countOfCompleted));
                    if(reportInfo.ReportCount == 0 ) {
                        manager.SetEfficiency(manager.MaxEfficiency, this);
                        UnityEngine.Debug.Log($"report count changed, mgr: {manager.Id}, eff: {manager.Efficiency}, max eff: {manager.MaxEfficiency}");
                    }
                }*/
            }
        }

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "manager_service";


        public override Type SaveType => typeof(ManagerServiceSave);

        public override object GetSave() {
            Dictionary<int, ManagerInfoSave> managerSaves = new Dictionary<int, ManagerInfoSave>();
            foreach (var kvp in Managers) {
                managerSaves.Add(kvp.Key, kvp.Value.GetSave());
            }
            return new ManagerServiceSave {
                managers = managerSaves,
                efficiencyRollbackImproveLevels = ManagerEfficiencyRollbackLevels.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetSave())
            };
        }

        public override void ResetByInvestors() {
            foreach(var kvp in Managers) {
                kvp.Value.ResetByInvestors();
            }
        }

        public override void ResetByPlanets() {
            ResetByGameplay();
            
        }

        private void BaseReset() {
            Dictionary<int, ManagerInfo> newManagers = new Dictionary<int, ManagerInfo>();
            foreach (var kvp in Managers) {
                ManagerData managerData = Services.ResourceService.Managers.GetManagerData(kvp.Key);
                GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(kvp.Key);

                ManagerInfo newValue = new ManagerInfo(managerData, generatorData);
                newValue.SetHasKickBacks(kvp.Value.HasKickBacks);
                newManagers.Add(newValue.Id, newValue);
            }
            Managers.Clear();
            foreach (var kvp in newManagers) {
                Managers.Add(kvp.Key, kvp.Value);
            }

            if (Services.GameModeService.IsFirstTimeLaunch) {
                SetCashOnHand(0, kFirstManagerCashOnHand);
            }
        }

        private void ResetByGameplay() {
            BaseReset();
            ManagerEfficiencyRollbackLevels.Clear();
            IsLoaded = true;
        }

        public override void ResetFull() {

            LoadDefaults();

        }

        public override void LoadDefaults() {
            StartCoroutine(LoadDefaultsImpl());
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        private IEnumerator LoadDefaultsImpl() {
            yield return new WaitUntil(() => Services.ResourceService.IsLoaded);
            yield return new WaitUntil(() => Services.GameModeService.IsLoaded);
            Managers.Clear();
            foreach (var data in Services.ResourceService.Managers.ManagerCollections) {
                GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(data.Id);
                Managers.Add(data.Id, new ManagerInfo(data, generatorData));
            }
            if(Services.GameModeService.IsFirstTimeLaunch) {
                SetCashOnHand(0, kFirstManagerCashOnHand);
            }

            ManagerEfficiencyRollbackLevels.Clear();

            IsLoaded = true;
        }

        

        public override void LoadSave(object obj) {
            ManagerServiceSave save = obj as ManagerServiceSave;
            if (save != null) {
                if (save.managers != null) {
                    Managers.Clear();
                    foreach (var kvp in save.managers) {
                        var managerData = Services.ResourceService.Managers.GetManagerData(kvp.Key);
                        GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(kvp.Key);
                        ManagerInfo manager = new ManagerInfo(managerData, generatorData, kvp.Value);
                        Managers.Add(manager.Id, manager);
                    }

                    save.Validate();
                    ManagerEfficiencyRollbackLevels.Clear();
                    foreach(var me in save.efficiencyRollbackImproveLevels) {
                        ManagerEfficiencyRollbackLevels.Add(me.Key, new ManagerEfficiencyRollbackLevel(me.Value));
                    }

                    IsLoaded = true;
                } else {
                    LoadDefaults();
                }
            } else {
                LoadDefaults();
            }
        } 
        #endregion


        public void AddCashOnHand(int managerId, double value) {
            var manager = GetManager(managerId);
            double oldCash = manager.CashOnHand;
            manager.AddCash(value);
            if (oldCash != manager.CashOnHand) {
                GameEvents.OnManagerCashOnHandChanged(oldCash, manager.CashOnHand, manager);
            }
        }

        public void SetCashOnHand(int managerId, double value) {
            var manager = GetManager(managerId);
            double oldCash = manager.CashOnHand;
            manager.SetCashOnHand(value);
            if(oldCash != manager.CashOnHand) {
                GameEvents.OnManagerCashOnHandChanged(oldCash, manager.CashOnHand, manager);
            }
        }

        public ManagerInfo GetManager(int id) {
            if(Managers.ContainsKey(id)) {
                return Managers[id];
            } else {
                //var data = GameData.instance.managers.FirstOrDefault(m => m.Id == id);
                var data = Services.ResourceService.Managers.GetManagerData(id);
                GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(id);
                if(data == null ) {
                    throw new UnityException($"manager data {id} not founded");
                }
                ManagerInfo manager = new ManagerInfo(data, generatorData);
                Managers.Add(manager.Id, manager);
                return manager;
            }
        }

        public ManagerEfficiencyRollbackLevel GetManagerEfficiencyRollbackLevel(int managerId ) {
            if(ManagerEfficiencyRollbackLevels.ContainsKey(managerId)) {
                return ManagerEfficiencyRollbackLevels[managerId];
            } else {
                ManagerEfficiencyRollbackLevel level = new ManagerEfficiencyRollbackLevel(managerId);
                ManagerEfficiencyRollbackLevels.Add(managerId, level);
                return level;
            }
        }

        public void StartRollback(int managerId, int planetId) {
            var manager = GetManager(managerId);
            double percent = UnityEngine.Random.Range(0.05f, (float)manager.MaxRollback);

            //make as 5 multiplier
            int percentInt = (int)(percent * 100);
            int div = percentInt / 5;
            int newPercentInt = div * 5;
            percent = newPercentInt / 100.0;

            CurrentRollbackInfo.Setup(managerId, planetId, percent);
            Services.ViewService.Show(ViewType.LoadingView, new ViewData {
                UserData = new LoadSceneData{
                    BuildIndex = 4,
                    Mode = LoadSceneMode.Additive,
                    LoadAction = () => {
                        UnityEngine.Debug.Log($"scene 4 loaded".Colored(ConsoleTextColor.orange).Bold());
                    }
                }
            });
        }


        public double FinishRollback() {
            if (CurrentRollbackInfo.IsValid) {
                var manager = GetManager(CurrentRollbackInfo.ManagerId);
                bool isFirstKickback = false;
                double payed = manager.KickBack(CurrentRollbackInfo.KickBackPercent, out isFirstKickback);
                if(payed > 0 ) {
                    Services.PlayerService.AddGenerationCompanyCash(payed);
                }
                GameEvents.OnManagerKickBack(payed, isFirstKickback, manager);
                return payed;
            }
            return 0.0;
        }

        public void Hire(int managerId ) {
            var manager = GetManager(managerId);
            bool oldHired = manager.IsHired;
            if(!oldHired) {
                manager.Hire();
                GameEvents.OnTransportManagerHired(manager);
            }
        }

        public bool IsHired(int managerId) {
            return GetManager(managerId).IsHired;
        }

        public int HiredCount
            => Managers.Where(manager => manager.Value.IsHired).Count();




        public double GetManagerFatigueEfficiencySpeed(int managerId) {
            var manager = GetManager(managerId);
            var secretaryData = Services.ResourceService.SecretaryDataRepository.GetSecretaryData(Services.PlanetService.CurrentPlanetId);

            if(managerId == 0 ) {
                UDBG.Log($"secretary fatig of manager {secretaryData.FatigueOfEfficiency}, man max effici: {manager.MaxEfficiency}, speed: {secretaryData.FatigueOfEfficiency * manager.MaxEfficiency / 3600}".Attrib(bold: true, color: "g", italic: true));
            }
            return secretaryData.FatigueOfEfficiency * manager.MaxEfficiency / (60.0f * 60.0f);
        }

        public float MinManagerEfficiency => 0.05f;

        //public void OnEfficiencyChanged(double efficiencyChange, ManagerInfo manager) {
        //    GameEvents.OnEfficiencyChanged(efficiencyChange, manager);
        //}

        private int CurrentPlanetId
            => Services.GetService<IPlanetService>().CurrentPlanet.Id;

        public ManagerTransactionState BuyEfficiencyLevel(int managerId) {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);

            if(!manager.IsHired) {
                return ManagerTransactionState.ManagerIsNotHired;
            }

            IManagerImprovementRepository improveRepo = Services.ResourceService.ManagerImprovements;
            if(mgrLevel.EfficiencyImproveLevel >= improveRepo.MaxLevel) {
                return ManagerTransactionState.AlreadyMaxLevel;
            }
            int currentLevel = mgrLevel.EfficiencyImproveLevel;
            int nextLevel = currentLevel + 1;
            var improveData = improveRepo.GetEfficiencyImproveData(nextLevel);
            Bos.Data.Currency price = Bos.Data.Currency.CreateCoins(improveData.CoinsPrice);
            if(!Services.PlayerService.IsEnough(price)) {
                return ManagerTransactionState.DontEnoughCoins;
            }
            Services.PlayerService.RemoveCurrency(price);
            manager.AddMaxEfficiency(improveData.EfficiencyIncrement, Services);
            mgrLevel.SetEfficiencyLevel(nextLevel);
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState BuyRollbackLevel(int managerId) {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);

            if (!manager.IsHired) {
                return ManagerTransactionState.ManagerIsNotHired;
            }
            IManagerImprovementRepository improveRepo = Services.ResourceService.ManagerImprovements;
            if(mgrLevel.RollbackImrpoveLevel >= improveRepo.MaxLevel) {
                return ManagerTransactionState.AlreadyMaxLevel;
            }

            int currentLevel = mgrLevel.RollbackImrpoveLevel;
            int nextLevel = currentLevel + 1;
            var improveData = improveRepo.GetRollbackImproveData(nextLevel);
            Bos.Data.Currency price = Bos.Data.Currency.CreateCoins(improveData.CoinsPrice);
            if(!Services.PlayerService.IsEnough(price)) {
                return ManagerTransactionState.DontEnoughCoins;
            }

            Services.PlayerService.RemoveCurrency(price);
            manager.AddMaxRollback(improveData.RollbackIncrement);
            mgrLevel.SetRollbackImproveLevel(nextLevel);
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState BuyMegaUpgrade(int managerId)
        {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);
            IManagerImprovementRepository improveRepo = Services.ResourceService.ManagerImprovements;
            
            if (!manager.IsHired) {
                return ManagerTransactionState.ManagerIsNotHired;
            }

            if (!mgrLevel.IsRollbackMaxLevel(improveRepo) || !mgrLevel.IsEfficiencyMaxLevel(improveRepo))
            {
                return ManagerTransactionState.ManagerCantUpgradeMega;
            }
 
            var improveData = improveRepo.MegaImprovement.CoinPrice;
            Bos.Data.Currency price = Bos.Data.Currency.CreateCoins(improveData);
            if(!Services.PlayerService.IsEnough(price)) {
                return ManagerTransactionState.DontEnoughCoins;
            }
            
            Services.PlayerService.RemoveCurrency(price);
            manager.AddMaxRollback(improveRepo.MegaImprovement.RollbackIncrement);
            manager.AddMaxEfficiency(improveRepo.MegaImprovement.EfficiencyIncrement, Services);
            mgrLevel.ApplyMega();
            
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState GetBuyEfficiencyLevelStatus(int managerId) {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);

            if (!manager.IsHired) {
                return ManagerTransactionState.ManagerIsNotHired;
            }
            IManagerImprovementRepository improveRepo = Services.ResourceService.ManagerImprovements;
            if(mgrLevel.EfficiencyImproveLevel >= improveRepo.MaxLevel ) {
                return ManagerTransactionState.AlreadyMaxLevel;
            }

            int currentLevel = mgrLevel.EfficiencyImproveLevel;
            int nextLevel = currentLevel + 1;
            var improveData = improveRepo.GetEfficiencyImproveData(nextLevel);
            Bos.Data.Currency price = Bos.Data.Currency.CreateCoins(improveData.CoinsPrice);
            if (!Services.PlayerService.IsEnough(price)) {
                return ManagerTransactionState.DontEnoughCoins;
            }

            return ManagerTransactionState.Success;
        }

        public int GetNextEfficiencyLevel(int managerId ) {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);
            var managerImprovements = ResourceService.ManagerImprovements;
            return (mgrLevel.EfficiencyImproveLevel < managerImprovements.MaxLevel) ? mgrLevel.EfficiencyImproveLevel + 1 : managerImprovements.MaxLevel;
        }  
        
        public int GetNextRollbackLevel(int managerId ) {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);
            var managerImprovements = ResourceService.ManagerImprovements;
            return (mgrLevel.RollbackImrpoveLevel < managerImprovements.MaxLevel) ? mgrLevel.RollbackImrpoveLevel + 1 : managerImprovements.MaxLevel;
        }

        public ManagerTransactionState GetBuyRollbackLevelStatus(int managerId) {
            var manager = GetManager(managerId);
            var mgrLevel = GetManagerEfficiencyRollbackLevel(managerId);

            if (!manager.IsHired) {
                return ManagerTransactionState.ManagerIsNotHired;
            }
            IManagerImprovementRepository improveRepo = Services.ResourceService.ManagerImprovements;
            if (mgrLevel.RollbackImrpoveLevel >= improveRepo.MaxLevel) {
                return ManagerTransactionState.AlreadyMaxLevel;
            }

            int currentLevel = mgrLevel.RollbackImrpoveLevel;
            int nextLevel = currentLevel + 1;
            var improveData = improveRepo.GetRollbackImproveData(nextLevel);
            Bos.Data.Currency price = Bos.Data.Currency.CreateCoins(improveData.CoinsPrice);
            if (!Services.PlayerService.IsEnough(price)) {
                return ManagerTransactionState.DontEnoughCoins;
            }

            return ManagerTransactionState.Success;
        }

        public double GetManagerPrice(int generatorId)
            => GetManager(generatorId)?.Cost ?? double.MaxValue;

        public bool HasAvailableManager {
            get {
                foreach (var manager in Services.ResourceService.Managers.ManagerCollections)
                {

                    var managerInfo = GetManager(manager.Id);
                    
                    if (managerInfo.Cost < Services.PlayerService.CompanyCash.Value &&
                        (false == IsHired(manager.Id)) &&
                        Services.TransportService.HasUnits(manager.Id)) {
                        return true;
                    }
                }

                return false;
            }
        }

        public IEnumerable<ManagerInfo> HiredManagers {
            get {
                foreach(var kvp in Managers ) {
                    if (IsHired(kvp.Key)) {
                        yield return kvp.Value;
                    }
                }
            }
        }

        public void Enhance(GeneratorInfo generator) {
            var playerService = Services.PlayerService;
            var viewService = Services.ViewService;

            Services.GenerationService.Enhance(generator.GeneratorId);
            Analytics.CustomEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_BUY");

            /*
            BosUtils.If(() => playerService.IsEnoughCoins(generator.Data.EnhancePrice),
                trueAction: () => {
                    Services.GenerationService.Enhance(generator.GeneratorId);
                    Analytics.CustomEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_BUY");
                    //playerService.RemoveCoins(generator.Data.EnhancePrice);
                    //FacebookEventUtils.LogCoinSpendEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_BUY", generator.Data.EnhancePrice, playerService.Coins);
                },
                falseAction: () => {
                    viewService.Show(ViewType.CoinRequiredView, new ViewData {
                        UserData = generator.Data.EnhancePrice
                    });
                    Analytics.CustomEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_CLICK_NOCOINS");
                });*/

        }

        public bool IsRallbackAllowed(int managerId) {
            return GetManager(managerId).CashOnHand > 0;
        }

        public void HireManager(int manId, bool free = false) {
            IManagerRepository managerRepository = ResourceService.Managers;

            var man = managerRepository.GetManagerData(manId);
            var genId = manId;
            Hire(genId);
            Services.GenerationService.Generators.SetAutomatic(genId, true);

            ManagerInfo manager = GameServices.Instance.ManagerService.GetManager(manId);
            if (!free) {
                Player.RemoveCompanyCash(manager.Cost);
                Player.CheckNonNegativeCompanyCash();
            }

            if (manager.CashOnHand < 1) {
                manager.ResetNextKickBackTime();
            }

            if (!Convert.ToBoolean(PlayerPrefs.GetInt("FirstManagerHire_" + genId, 0))) {
                FacebookEventUtils.LogFirstHireManagerEvent(genId);
                PlayerPrefs.SetInt("FirstManagerHire_" + genId, 1);
            }

        }

        public void HireManagerFree(int genId) {
            Hire(genId);
            Services.GenerationService.Generators.SetAutomatic(genId, true);
        }

        public bool IsMegaEfficiencyLevel(ManagerInfo manager) {
            var level = GetManagerEfficiencyRollbackLevel(manager.Id);
            return level.IsEfficiencyMaxLevel(ResourceService.ManagerImprovements);
        }

        public bool IsMegaRollbackLevel(ManagerInfo manager) {
            var level = GetManagerEfficiencyRollbackLevel(manager.Id);
            return level.IsRollbackMaxLevel(ResourceService.ManagerImprovements);
        }

        public double CurrentEfficiency(int managerId)
            => GetManager(managerId).Efficiency(Services);

        public double MaxEfficiency(int managerId)
            => GetManager(managerId).MaxEfficiency;

    }



    public interface IManagerService : IGameService {
        void AddCashOnHand(int managerId, double value);
        void SetCashOnHand(int managerId, double value);
        ManagerInfo GetManager(int id);

        bool IsHired(int managerId);
        void Hire(int managerId);
        void HireManager(int manId, bool free = false);
        void HireManagerFree(int genId);
        int HiredCount { get; }

        double GetManagerFatigueEfficiencySpeed(int managerId);
        float MinManagerEfficiency { get; }
        double CurrentEfficiency(int managerId);
        double MaxEfficiency(int managerId);

        Dictionary<int, ManagerInfo> Managers { get; }
        bool IsLoaded { get; }
        ManagerTransactionState BuyEfficiencyLevel(int managerId);
        ManagerTransactionState BuyRollbackLevel(int managerId);
        ManagerTransactionState BuyMegaUpgrade(int managerId);
        ManagerTransactionState GetBuyEfficiencyLevelStatus(int managerId);
        ManagerTransactionState GetBuyRollbackLevelStatus(int managerId);

        void StartRollback(int managerId, int planetId);
        double FinishRollback();
        KickbackInfo CurrentRollbackInfo { get; }
        bool HasAvailableManager { get; }
        IEnumerable<ManagerInfo> HiredManagers { get; }
        double GetManagerPrice(int generatorId);
        void Enhance(GeneratorInfo generator);
        bool IsAnyHired { get; }
        bool IsRallbackAllowed(int managerId);
        bool IsWakeUpCompleted { get; }
        int GetNextEfficiencyLevel(int managerId);
        int GetNextRollbackLevel(int managerId);
        ManagerEfficiencyRollbackLevel GetManagerEfficiencyRollbackLevel(int managerId);
        bool IsMegaEfficiencyLevel(ManagerInfo manager);
        bool IsMegaRollbackLevel(ManagerInfo manager);
    }





    public class ManagerServiceSave {
        public Dictionary<int, ManagerInfoSave> managers;
        public Dictionary<int, ManagerEfficiencyRollbackLevelSave> efficiencyRollbackImproveLevels;

        public void Validate() {
            if(efficiencyRollbackImproveLevels == null ) {
                efficiencyRollbackImproveLevels = new Dictionary<int, ManagerEfficiencyRollbackLevelSave>();
            }
        }
    }

    public class KickbackInfo {
        public int ManagerId { get; set; } = -1;
        public int PlanetId { get; set; }
        public double KickBackPercent { get; set; }

        public bool IsValid
            => ManagerId >= 0;

        public void Setup(int managerId, int planetId, double kickbackPercent) {
            this.ManagerId = managerId;
            this.PlanetId = planetId;
            this.KickBackPercent = kickbackPercent;
        }

        public int RollbackPercentInt
            => (int)(KickBackPercent * 100);
    }

    public enum ManagerTransactionState {
        Success = 0,
        DontEnoughCoins = 1,
        AlreadyMaxLevel = 2,
        ManagerIsNotHired = 3,
        ManagerCantUpgradeMega
    }
}