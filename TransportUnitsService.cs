namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UDebug = UnityEngine.Debug;

    public class TransportUnitsService : SaveableGameBehaviour, ITransportUnitsService {

        public const int kBusId = 2;
        public const int kSubmarinId = 6;

        public Dictionary<int, TransportUnitInfo> Units { get; } = new Dictionary<int, TransportUnitInfo>();
        //public Dictionary<int, int> BrokenUnits { get; } = new Dictionary<int, int>();

        private readonly UpdateTimer updateTimer = new UpdateTimer();
        //private readonly UpdateTimer updateConstRepairTimer = new UpdateTimer();

        private IMechanicService _mechanicService;
        private IPlanetService _planetService;
        private IResourceService _resourceService;

        private IMechanicService MechanicService
            => (_mechanicService != null) ? _mechanicService : (_mechanicService = Services.GetService<IMechanicService>());

        private IPlanetService PlanetService
            => (_planetService != null) ? _planetService : (_planetService = Services.GetService<IPlanetService>());

        private IResourceService ResourceService
            => (_resourceService != null) ? _resourceService : (_resourceService = Services.ResourceService);


        private bool isNeedUpdateOnResume = true;

        //public void Construct(IMechanicService mechanicService, IPlanetService planetService, IResourceService resourceService) {
        //    this.mechanicService = mechanicService;
        //    this.planetService = planetService;
        //    this.resourceService = resourceService;
        //}

        public bool IsWakeUpCompleted { get; private set; }

        public override void Update() {
            base.Update();
            updateTimer.Update();
            //updateConstRepairTimer.Update();
        }

        public override void OnEnable() {
            base.OnEnable();
            //GameEvents.MechanicAdded += OnMechanicAdded;
        }

        public override void OnDisable() {
            base.OnDisable();
            //GameEvents.MechanicAdded -= OnMechanicAdded;
        }

        private void OnApplicationPause(bool isPause) {
            UpdateResume(isPause);
        }

        private void OnApplicationFocus(bool isFocus ) {
            UpdateResume(!isFocus);
        }

        public void UpdateResume(bool pause) {
            //UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");
            UpdateBrokeOnResume(pause);
        }

        private void UpdateBrokeOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
                IsWakeUpCompleted = false;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateBrokeOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateBrokeOnResumeImpl() {
            yield return new WaitUntil( () => Services.ResourceService.IsLoaded && IsLoaded && Services.GameModeService.IsGame);
            ISleepService sleepService = Services.GetService<ISleepService>();
            yield return new WaitUntil(() => sleepService.IsRunning && IsLoaded && Services.GameModeService.IsGame);
            UpdateBroke(sleepService.SleepInterval);
            IsWakeUpCompleted = true;
        }

        private bool UpdateBroke(float deltaTime) {
            bool isWasBroked = false;

            if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                if (MechanicService != null) {
                    foreach (var kvp in Units) {
                        GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(kvp.Key);

                        if(generatorData.Type == GeneratorType.Normal ) {
                            if (Services.ManagerService.IsHired(kvp.Key)) {

                                MechanicData mechanicData = ResourceService.MechanicDataRepository.GetMechanicData(kvp.Key);
                                int mechanicCount = MechanicService.GetMechanicCount(kvp.Key);
                                int unitsWhichCanBrokeCount = GetUnitsWhichCanBroke(kvp.Key, mechanicData, mechanicCount);
                                if (unitsWhichCanBrokeCount > 0) {
                                    int minLiveCount = MechanicService.GetServicedUnitCount(generatorData.Id);

                                    int totalUnitCount = GetUnitTotalCount(generatorData.Id);

                                    float speed = GetUnitBrokenSpeed(mechanicData, totalUnitCount);

                                    bool isManagerHired = Services.ManagerService.IsHired(kvp.Key);
                                    bool isBrokeAllowed = Services.MechanicService.IsUnlocked;

                                    if (isManagerHired && isBrokeAllowed) {
                                        if (kvp.Value.UpdateBroke(deltaTime, speed, minLiveCount, Services)) {
                                            isWasBroked = true;
                                            GameEvents.OnGeneratorUnitsCountChanged(kvp.Value);
                                        }
                                    }
                                }
                            }
                        }


                    }
                }
                //UDebug.Log($"was broked => {isWasBroked}");
            }
            return isWasBroked;
        }

        public void ForceBroke(int generatorId, int count) {
            int liveCount = GetUnitLiveCount(generatorId);
            var unit = GetUnit(generatorId);
            unit.Broke(count);
            if(liveCount != unit.LiveCount ) {
                GameEvents.OnGeneratorUnitsCountChanged(unit);
            }
        }

        public TransportUnitInfo GetUnit(int generatorId) {
            if(Units.ContainsKey(generatorId)) {
                return Units[generatorId];
            } else {
                TransportUnitInfo unit = new TransportUnitInfo(generatorId, 0);
                Units.Add(generatorId, unit);
                return unit;
            }
        }
        
        public int GetUnitsWhichCanBroke(int generatorId, MechanicData mechanicData, int mechanicCount) {
            int minLiveCount = Services.MechanicService.GetServicedUnitCount(generatorId);
            int countToBroke = GetUnitLiveCount(generatorId) - minLiveCount;
            if(countToBroke < 0 ) {
                countToBroke = 0;
            }
            return countToBroke;
        }

        public float GetUnitsBrokenedPerHour(MechanicData mechanicData, int unitCountCanBroke) {
            //int countToBroke = GetUnitsWhichCanBroke(generatorId, mechanicData);
            if (mechanicData != null && IsLoaded) {
                return mechanicData.FatigueUntisPercentPerHour * unitCountCanBroke;
            } else {
                return 0f;
            }
        }

        public float GetUnitBrokenSpeed(MechanicData mechanicData, int unitCountCanBroke) {
            return GetUnitsBrokenedPerHour(mechanicData, unitCountCanBroke) / 3600.0f;
        }




        #region ITransportUnitsService
        public void Setup(object data = null) {
            updateTimer.Setup(1.0f, (delay) => {
                if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                    UpdateBroke(delay);
                }
            });
            //updateConstRepairTimer.Setup(1, dt => UpdateConstMechanicRepair(dt));
        }

        public void AddLiveUnits(int generatorId, int count) {
            int oldCount = 0;
            if(Units.ContainsKey(generatorId)) {
                oldCount = Units[generatorId].LiveCount;
                Units[generatorId].AddLive(count);
            } else {
                Units.Add(generatorId, new TransportUnitInfo(generatorId, count));
            }
            GameEvents.OnGeneratorUnitsCountChanged( Units[generatorId]);
        }

        public void SetLiveUnits(int generatorId, int count) {
            int oldCount = 0;
            if(Units.ContainsKey(generatorId)) {
                oldCount = Units[generatorId].LiveCount;
            }
            Units[generatorId].SetLive(count);
            GameEvents.OnGeneratorUnitsCountChanged( Units[generatorId]);
        }

        public int GetUnitLiveCount(int generatorId) {
            if(!Units.ContainsKey(generatorId)) {
                return 0;
            }
            return Units[generatorId].LiveCount;
        }

        public int GetUnitBrokenedCount(int generatorId)
            => Units.ContainsKey(generatorId) ? Units[generatorId].BrokenedCount : 0;

        public int GetUnitTotalCount(int generatorId) {
            if(!Units.ContainsKey(generatorId)) {
                return 0;
            }
            return Units[generatorId].TotalCount;
        }

        public bool HasUnits(int generatorId) {
            return GetUnitTotalCount(generatorId) > 0;
        }

        public int TotalCountOfGenerators
            => Units.Count;

        public bool IsCountOfUnitsForGeneratorsGreaterOrEqualThan(int number) {
            return Units.All(unit => unit.Value.TotalCount >= number);
        }

        public int Repair(int generatorId, int count) {
            if(Units.ContainsKey(generatorId)) {
                var unitInfo = Units[generatorId];
                int repaired =  unitInfo.Repair(count);
                GameEvents.OnGeneratorUnitsCountChanged(unitInfo);
                return repaired;
            }
            return 0;
        }

        #endregion

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "transport_units_service";

        public override Type SaveType => typeof(TransportUnitsServiceSave);

        public override object GetSave() {
            Dictionary<int, TransportUnitInfoSave> units = new Dictionary<int, TransportUnitInfoSave>();
            foreach(var kvp in Units) {
                units.Add(kvp.Key, kvp.Value.GetSave());
            }
            return new TransportUnitsServiceSave {
                units = units
            };
        }


        public override void ResetByInvestors() {
            //ResetFull();
            foreach(var unit in Units) {
                unit.Value.ResetByInvestors();
            }
        }
        public override void ResetByPlanets() {
            ResetFull();
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            Units.Clear();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            TransportUnitsServiceSave save = obj as TransportUnitsServiceSave;
            
            if(save != null && save.units != null) {
                Units.Clear();
                foreach (var kvp in save.units) {
                    Units.Add(kvp.Key, new TransportUnitInfo(kvp.Value));
                }
                IsLoaded = true;
            }  else {
                LoadDefaults();
            }
        }

        #endregion
    }

    public interface ITransportUnitsService : IGameService {
        void AddLiveUnits(int generatorId, int count);
        void SetLiveUnits(int generatorId, int count);
        int GetUnitLiveCount(int generatorId);
        int GetUnitBrokenedCount(int generatorId);

        int GetUnitTotalCount(int generatorId);
        bool HasUnits(int generatorId);
        int TotalCountOfGenerators { get; }
        bool IsCountOfUnitsForGeneratorsGreaterOrEqualThan(int number);


        float GetUnitBrokenSpeed(MechanicData mechanicData, int unitCountCanBroke);
        int GetUnitsWhichCanBroke(int generatorId, MechanicData mechanicData, int mechanicCount);
        float GetUnitsBrokenedPerHour(MechanicData mechanicData, int unitCountCanBroke);
        int Repair(int generatorId, int count);
        bool IsLoaded { get; }
        bool IsWakeUpCompleted { get; }

        TransportUnitInfo GetUnit(int generatorId);
        void ForceBroke(int generatorId, int count);
    }

    public class TransportUnitsServiceSave {
        public Dictionary<int, TransportUnitInfoSave> units;
    }



}