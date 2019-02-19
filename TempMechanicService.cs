namespace Bos {
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;
    using UDBG = UnityEngine.Debug;

    public class TempMechanicService : SaveableGameBehaviour, ITempMechanicService {

        public Dictionary<int, Dictionary<string, TempMechanicInfo>> TempMechanics { get; } 
            = new Dictionary<int, Dictionary<string, TempMechanicInfo>>();

        private readonly UpdateTimer updateTimer = new UpdateTimer();
        public Dictionary<int, int> SpeedMults { get; } = new Dictionary<int, int>();

        private bool isNeedUpdateOnResume = true;

        private bool IsInitialized { get; set; } = false;

        public void Setup(object obj) {
            updateTimer.Setup(1f, dt => {
                if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                    UpdateOnInterval(dt);
                }
            });
            if(!IsInitialized) {
                GameEvents.TempMechanicRepairedTransportObservable.Subscribe(info => {
                    int repairedCount = Services.TransportService.Repair(info.Mechanic.GeneratorId, info.RepairCount);
                    UDBG.Log($"repaied count by temp mechanic => {repairedCount}".Attrib(bold: true, italic: true, color: "g"));
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            //GameEvents.TempMechanicInfoStateChanged += OnTempMechanicStateChanged;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
        }

        public override void OnDisable() {
            //GameEvents.TempMechanicInfoStateChanged -= OnTempMechanicStateChanged;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            base.OnDisable();
        }

        private void UpdateOnInterval(float deltaTime ) {
            foreach (var kvp in TempMechanics) {
                foreach (var kvp2 in kvp.Value) {
                    kvp2.Value.Update(deltaTime, this);
                }
            }
            RemoveCompletedMechanics();
        }

        public override void Update() {
            base.Update();
            if (IsLoaded) {
                updateTimer.Update();
            }
        }
        private void OnApplicationPause(bool pause)
            => UpdateResume(pause);

        private void OnApplicationFocus(bool focus)
            => UpdateResume(!focus);

        public void UpdateResume(bool pause) {
            //UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");
            UpdateTempMechanicsOnResume(pause);
        }

        private void UpdateTempMechanicsOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateTempMechanicsOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateTempMechanicsOnResumeImpl() {
            yield return new WaitUntil(() =>
                Services.ResourceService.IsLoaded &&
                Services.GameModeService.IsGame &&
                Services.SleepService.IsRunning &&
                IsLoaded &&
                Services.TransportService.IsLoaded &&
                Services.TimeChangeService.IsLoaded);

            UpdateOnInterval(Services.SleepService.SleepInterval);
        }

        private void RemoveCompletedMechanics() {
            foreach (var kvp in TempMechanics) {
                RemoveCompletedMechanics(kvp.Value);
            }
        }

        private int RemoveCompletedMechanics(Dictionary<string, TempMechanicInfo> mechanics) {
            int removeCount = 0;
            List<string> completedIds = mechanics.Where(kvp => kvp.Value.IsCompleted).Select(kvp => kvp.Key).ToList();

            List<TempMechanicInfo> removedMechanics = new List<TempMechanicInfo>();

            foreach(string id in completedIds) {
                if (mechanics.ContainsKey(id)) {
                    var toRemove = mechanics[id];
                    if (mechanics.Remove(id)) {
                        removeCount++;
                        removedMechanics.Add(toRemove);
                    }
                }
            }

            if(removeCount > 0 ) {
                GameEvents.OnTempMechanicsRemoved(removedMechanics);
            }
            return removeCount;
        }



        #region ITempMechanic Service
        public bool IsConditionsForBuyValid(GeneratorInfo generator ) {
            bool isManagerHired = Services.ManagerService.IsHired(generator.GeneratorId);
            if(!isManagerHired) {
                return false;
            }

            int unitBrokened = Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId);
            if (unitBrokened <= 0) {
                return false;
            }
            double price = GetTempMechanicPrice(generator, unitBrokened);
            var currency = Bos.Data.Currency.CreateCompanyCash(price);
            if (!Services.PlayerService.IsEnough(currency)) {
                return false;
            }
            return true;
        }

        public BosError Buy(GeneratorInfo generator) {
            int unitBrokened = Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId);
            if(unitBrokened <= 0 ) {
                return BosError.NoBrokenedUnits;
            }
            double price = GetTempMechanicPrice(generator, unitBrokened);
            var currency = Bos.Data.Currency.CreateCompanyCash(price);
            if(!Services.PlayerService.IsEnough(currency)) {
                return BosError.NotEnoughCompanyCash;
            }

            var mechanicData = Services.ResourceService.MechanicDataRepository.GetMechanicData(planetId:
                Services.PlanetService.CurrentPlanet.Id);

            Services.PlayerService.RemoveCurrency(currency);

           // int unitCount = Mathf.Min(unitBrokened, mechanicData.ServiceUnitsRestoredPer10Seconds);
            TempMechanicInfo newMechanic = new TempMechanicInfo(generator.GeneratorId, unitBrokened, mechanicData.TempMechanicSpeed);
            AddTempMechanic(newMechanic);
            return BosError.Ok;
        }

        public double GetTempMechanicPrice(GeneratorInfo generator, int brokenedCount ) {
            int baseCount = BaseRestoredCount;
            if(baseCount == 0) { baseCount = 1; }
            float priceRatio = (float)brokenedCount / (float)baseCount;
            return GetTempMechanicPriceForBaseCount(generator) * priceRatio;
        }

        private double GetTempMechanicPriceForBaseCount(GeneratorInfo generator ) {
            int planetId = Services.PlanetService.CurrentPlanet.Id;
            int countOfRepair = BaseRestoredCount; //Services.ResourceService.MechanicDataRepository.GetMechanicData(planetId).ServiceUnitsRestoredPer10Seconds;
            var mechanicData = ResourceService.MechanicDataRepository.GetMechanicData(planetId);
            double pricePerOneUnit =  MechanicSecretaryHelper.GetUnitPriceForMechanicSecretaryPrice(generator, (int)(mechanicData?.ServiceCashPrice ?? 2.0)) / countOfRepair;
            return pricePerOneUnit * Mathf.Min(countOfRepair, Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId));
        }

        private int BaseRestoredCount => ResourceService.MechanicDataRepository.GetMechanicData(Planets.CurrentPlanetId.Id).ServiceUnitsRestoredPer10Seconds;


        private double PriceProfit(GeneratorInfo generator) {
            int totalCount = Services.TransportService.GetUnitTotalCount(generator.GeneratorId);
            int totalWithout5 = Services.TransportService.GetUnitTotalCount(generator.GeneratorId) - 5;
            if(totalWithout5 < 0 ) { totalWithout5 = 0; }

            double firstProfit = ProfitOfTransport(generator, totalCount);
            double secondProfit = (totalWithout5 != 0) ? ProfitOfTransport(generator, totalWithout5) : 0;
            return Math.Abs(firstProfit - secondProfit);
        }

        private double ProfitOfTransport(GeneratorInfo generator, int count)
            => GeneratorUtils.CalculateProfitOnTime(generator, count,
                Services.GenerationService.Generators.CreateProfitBoost().WithInvestorBoost().WithPlanetProfitBoost().WithTimeChangeBoost(), 1,
                Services.GenerationService.Generators.CreateTimeBoost().WithPlanetBoost);

        public void ApplyAd(int generatorId) {

            if (IsAdApplicable(generatorId)) {
                MultSpeed(generatorId, 2);
            }
        }

        public int GetSpeedMult(int generatorId) {
            if(!SpeedMults.ContainsKey(generatorId)) {
                SpeedMults.Add(generatorId, 1);
            }
            return SpeedMults[generatorId];
        }

        private void MultSpeed(int generatorId, int val) {
            if(!SpeedMults.ContainsKey(generatorId)) {
                SpeedMults.Add(generatorId, 1);
            }
            SpeedMults[generatorId] *= val;
            UniRx.MessageBroker.Default.Publish<SpeedMultChangedArgs>(new SpeedMultChangedArgs { SpeedModifier = this, Name = "mechanic", GeneratorId = generatorId });
        }

        public bool IsAdApplicable(int generatorId)
            => Planets.IsMarsOpened && Services.ManagerService.IsHired(generatorId);

        public Dictionary<string, TempMechanicInfo> GetMechanics(int generatorId) {
            if (!TempMechanics.ContainsKey(generatorId)) {
                TempMechanics.Add(generatorId, new Dictionary<string, TempMechanicInfo>());
            }
            return TempMechanics[generatorId];
        }

        public bool IsBusy(GeneratorInfo generator, out float progress ) {
            progress = 0;
            var mechanics = GetMechanics(generatorId: generator.GeneratorId);
            if(mechanics.Count > 0 ) {
                var busyMechanic = mechanics.Values.FirstOrDefault(m => !m.IsCompleted);
                if(busyMechanic != null ) {
                    progress = busyMechanic.TotalProgress;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region private members
        private void AddTempMechanic(TempMechanicInfo mechanic ) {
            if(TempMechanics.ContainsKey(mechanic.GeneratorId)) {
                TempMechanics[mechanic.GeneratorId].Add(mechanic.Id, mechanic);
            } else {
                Dictionary<string, TempMechanicInfo> generatorMechanics =
                    new Dictionary<string, TempMechanicInfo> {
                        [mechanic.Id] = mechanic
                    };
                TempMechanics.Add(mechanic.GeneratorId, generatorMechanics);
            }
            mechanic.StartMechanic();
            GameEvents.OnTempMechanicAdded(mechanic);
            UDBG.Log($"mechanic started with interval => {mechanic.Interval}");
        }

        private int GetCountOfActiveTempMechanics(int generatorId) {
            if (TempMechanics.ContainsKey(generatorId)) {
                return TempMechanics[generatorId].Where(kvp => !kvp.Value.IsCompleted).Count();
            }
            return 0;
        }


        #endregion

        #region SaveableGameBehaviour overrides

        public override string SaveKey => "temp_mechanic_service";

        public override Type SaveType => typeof(TempMechanicServiceSave);

        public override object GetSave() {
            Dictionary<int, Dictionary<string, TempMechanicInfoSave>> tempMechanicInfoSaves =
                new Dictionary<int, Dictionary<string, TempMechanicInfoSave>>();
            foreach(var kvp in TempMechanics) {
                if(kvp.Value.Count > 0 ) {
                    Dictionary<string, TempMechanicInfoSave> generatorMechanics = new Dictionary<string, TempMechanicInfoSave>();
                    foreach(var kvp2 in kvp.Value) {
                        TempMechanicInfoSave saveObj = kvp2.Value.GetSave();
                        generatorMechanics.Add(saveObj.id, saveObj);
                    }
                    tempMechanicInfoSaves.Add(kvp.Key, generatorMechanics);
                }
            }
            return new TempMechanicServiceSave {
                tempMechanics = tempMechanicInfoSaves,
                speedMults = SpeedMults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        public override void LoadDefaults() {
            TempMechanics.Clear();
            SpeedMults.Clear();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();   
        }

        public override void LoadSave(object obj) {
            TempMechanicServiceSave save = obj as TempMechanicServiceSave;
            if(save != null ) {
                TempMechanics.Clear();
                save.Validate();
                foreach(var kvp in save.tempMechanics) {
                    Dictionary<string, TempMechanicInfo> generatorMechanics = new Dictionary<string, TempMechanicInfo>();
                    foreach(var kvp2 in kvp.Value) {
                        generatorMechanics.Add(kvp2.Key, new TempMechanicInfo(kvp2.Value));
                    }
                    TempMechanics.Add(kvp.Key, generatorMechanics);
                }
                SpeedMults.Clear();
                SpeedMults.CopyFrom(save.speedMults);
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override void ResetByInvestors() {
            LoadDefaults();
        }

        public override void ResetByPlanets() {
            LoadDefaults();
        }

        public override void ResetFull() {
            LoadDefaults();
        }
        #endregion

        #region Game Events
        /*
        private void OnTempMechanicStateChanged(TempMechanicState oldState, TempMechanicState newState, TempMechanicInfo mechanic) {
            if(newState == TempMechanicState.Completed) {
                int repaired = Services.TransportService.Repair(mechanic.GeneratorId, mechanic.Count);
                UDBG.Log($"repaired by temp mechanic count => {repaired}");
            }
        }*/

        private void OnViewShowed(ViewType type) {
            if(type == ViewType.ManagementView) {
                updateTimer.SetInterval(0.005f);
            }
        }

        private void OnViewHided(ViewType type) {
            if(type == ViewType.ManagementView) {
                updateTimer.SetInterval(1);
            }
        }
        #endregion
    }

    public interface ITempMechanicService : IGameService, ISpeedModifier {
        bool IsConditionsForBuyValid(GeneratorInfo generator);
        BosError Buy(GeneratorInfo generator);
        double GetTempMechanicPrice(GeneratorInfo generator, int brokenedCount);
        //bool IsAdValid(int generatorId);
        bool IsAdApplicable(int generatorId);
        void ApplyAd(int generatorId);
        Dictionary<string, TempMechanicInfo> GetMechanics(int generatorId);
        bool IsBusy(GeneratorInfo generator, out float progress);
    }

    public class TempMechanicServiceSave {
        public Dictionary<int, Dictionary<string, TempMechanicInfoSave>> tempMechanics;
        public Dictionary<int, int> speedMults;

        public void Validate() {
            if(tempMechanics == null ) {
                tempMechanics = new Dictionary<int, Dictionary<string, TempMechanicInfoSave>>();
            }
            if(speedMults == null ) {
                speedMults = new Dictionary<int, int>();
            }
        }
    }

    public interface ISpeedModifier {
        //int SpeedMult { get; }
        int GetSpeedMult(int generatorId);
    }
}