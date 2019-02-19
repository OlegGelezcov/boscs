namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UDebug = UnityEngine.Debug;

    public class MechanicService : SaveableGameBehaviour, IMechanicService {

        public Dictionary<int, MechanicInfo> Mechanics { get; } = new Dictionary<int, MechanicInfo>();

        public int ReparedViewCount { get; private set; }
        private bool isNeedUpdateOnResume = true;

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        private Lazy<ITempMechanicService> TempMechanicService
            = new Lazy<ITempMechanicService>(() => {
                return GameServices.Instance.TempMechanicService;
            });

        #region IMechanicService
        public void Setup(object data = null) {
            updateTimer.Setup(1f, dt => {
                if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                    UpdateOnInterval(dt);
                }
            });
        }

        private void UpdateOnInterval(float deltaTime ) {
            foreach(var kvp in Mechanics ) {
                kvp.Value.Update(deltaTime, TempMechanicService.Value);
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.MechanicUnitHandled += OnMechanicUnitHandled;
            GameEvents.MechanicStateChanged += OnMechanicStateChanged;
            GameEvents.MechanicWorkCircleCompleted += OnMechanicWorkCompleted;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
        }

        public override void OnDisable() {
            GameEvents.MechanicUnitHandled -= OnMechanicUnitHandled;
            GameEvents.MechanicStateChanged -= OnMechanicStateChanged;
            GameEvents.MechanicWorkCircleCompleted -= OnMechanicWorkCompleted;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

        private void OnApplicationPause(bool pause) 
            => UpdateResume(pause);

        private void OnApplicationFocus(bool focus)
            => UpdateResume(!focus);

        public void UpdateResume(bool pause) {
            //UnityEngine.Debug.Log($"{nameof(MechanicService)}.{nameof(UpdateResume)}() => {pause}");
            UpdateMechanicsOnResume(pause);
        }

        private void UpdateMechanicsOnResume(bool isPause) {
            if(isPause) {
                isNeedUpdateOnResume = true;
            } else {
                if(isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateMechanicsOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateMechanicsOnResumeImpl() {
            yield return new WaitUntil(() =>
                Services.ResourceService.IsLoaded &&
                Services.GameModeService.IsGame &&
                Services.SleepService.IsRunning &&
                IsLoaded &&
                Services.TransportService.IsLoaded &&
                Services.TimeChangeService.IsLoaded &&
                Services.TransportService.IsWakeUpCompleted);
            UpdateOnInterval(Services.SleepService.SleepInterval);
            Services.GetService<IConsoleService>().AddOutput("Mechanics Resumed..", ConsoleTextColor.green, true);
        }

        public void ResetRepairedViewCount()
            => ReparedViewCount = 0;

        public void AddRepairedViewCount(int count)
            => ReparedViewCount += count;

        public bool IsAllowBuyMechanic(int generatorId, out BosError error) {

            bool isManagerHired = Services.ManagerService.IsHired(generatorId);
            int planetId = Services.PlanetService.CurrentPlanet.Id;

            if(!isManagerHired) {
                error = BosError.ManagerNotHired;
                return false;
            }

            int price = GetNextMechanicPrice(generatorId);
            Currency currencyPrice = Currency.CreateCoins(price);
            bool isEnoughCurrency = Services.PlayerService.IsEnough(currencyPrice);
            if(!isEnoughCurrency) {
                error = BosError.NoEnoughCoins;
                return false;
            }

            error = BosError.Ok;
            return isManagerHired && isEnoughCurrency;
        }


        public BosError BuyMechanic(int generatorId) {
            bool isManagerHired = Services.GetService<IManagerService>().IsHired(generatorId);

            if (isManagerHired) {
                int planetId = Services.PlanetService.CurrentPlanet.Id;
                MechanicData mechanicData = Services.ResourceService.MechanicDataRepository.GetMechanicData(planetId);
                int price = GetNextMechanicPrice(generatorId);
                Currency currencyPrice = Currency.CreateCoins(price);
                if (Services.PlayerService.IsEnough(currencyPrice)) {
                    Services.PlayerService.RemoveCurrency(currencyPrice);
                    AddMechanic(generatorId, 1);
                    return BosError.Ok;
                } else {
                    return BosError.NoEnoughCoins;
                }
            } else {
                return BosError.ManagerNotHired;
            }
        }

        public int GetServicedUnitCount(int generatorId) {
            int mechanicCount = GetMechanicCount(generatorId);
            var mechanicData = Services.ResourceService.MechanicDataRepository.GetMechanicData(Services.PlanetService.CurrentPlanet.Id);
            int count =  mechanicData?.UnitCountService * mechanicCount ?? 1;
            if(count == 0 ) {
                count = 1;
            }
            return count;
        }

        public float ServiceSpeed
            => 1f / 10f;


        public int GetMechanicCount(int generatorId) {
            if(Mechanics.ContainsKey(generatorId)) {
                return Mechanics[generatorId].Count;
            }
            return 0;
        }

        public int GetNextMechanicPrice(int generatorId) {
            var data = GetMechanicData();
            return data.PriceForFirstMechanic + GetMechanicCount(generatorId) * data.PriceIncreasingForNextMechanic;
        }


        public void AddMechanic(int generatorId, int count) {

            MechanicInfo mechanic = null;
            if(Mechanics.ContainsKey(generatorId)) {
                mechanic = Mechanics[generatorId];
                mechanic.AddMechanic(count);
            } else {
                mechanic = new MechanicInfo(generatorId, count);
                Mechanics.Add(mechanic.Id, mechanic);
                GameEvents.OnMechanicAdded(mechanic);
            }
            
        }

        public MechanicInfo GetMechanic(int id) {
            if(Mechanics.ContainsKey(id)) {
                return Mechanics[id];
            }
            return null;
        }

        #endregion

        private MechanicData GetMechanicData() {
            int planetId = Services.PlanetService.CurrentPlanet.Id;
            MechanicData mechanicData = Services.ResourceService.MechanicDataRepository.GetMechanicData(planetId);
            return mechanicData;
        }

        public bool IsUnlocked
            => Services.PlanetService.IsOpened(PlanetConst.MARS_ID);

        #region Game Events 
        private void OnMechanicUnitHandled(int count)
            => AddRepairedViewCount(count);

        private void OnMechanicStateChanged(MechanicState oldState, MechanicState newState, MechanicInfo mechanic ) {
            //if(mechanic.Count > 0 ) {
            //    if(mechanic.State == MechanicState.Completed ) {
            //        int brokenedCount = Services.TransportService.GetUnitBrokenedCount(mechanic.Id);
            //        if(brokenedCount > 0 ) {
            //            int repairCount = Mathf.Min(brokenedCount, mechanic.Count);
            //            int repaired = Services.TransportService.Repair(mechanic.Id, repairCount);
            //            UDebug.Log($"mechanic => {mechanic.Id} was repaired => {repaired} transports...".Colored(ConsoleTextColor.green));
            //        }
            //    }
            //}
        }

        private void OnMechanicWorkCompleted(MechanicInfo mechanic, int workCyrcles ) {
            if(mechanic.Count > 0 ) {
                int brokenedCount = Services.TransportService.GetUnitBrokenedCount(mechanic.Id);
                if (brokenedCount > 0) {
                    int repairCount = Mathf.Min(brokenedCount, mechanic.Count * workCyrcles);
                    int repaired = Services.TransportService.Repair(mechanic.Id, repairCount);
                    //UDebug.Log($"mechanic => {mechanic.Id} was repaired => {repaired} transports...".Colored(ConsoleTextColor.green));
                }
            }
        }

        private void OnViewShowed(ViewType viewType) {
            if(viewType == ViewType.ManagementView) {
                updateTimer.SetInterval(0.0167f);
            }
        }

        private void OnViewHided(ViewType viewType ) {
            if(viewType == ViewType.ManagementView) {
                updateTimer.SetInterval(1f);
            }
        }

        /// <summary>
        /// Repair units
        /// </summary>
        /// <param name="generatorId">Target transport Id to repair</param>
        /// <param name="countToRepair">Count of transport to repair</param>
        /// <returns>Count of repaired transport</returns>
        public int ForceRepair(int generatorId, int countToRepair) {
            return Services.TransportService.Repair(generatorId, countToRepair);
        }

        #endregion

        #region SaveableGameBehaviour overrides
        public override string SaveKey
            => "mechanic_service";

        public override Type SaveType => typeof(MechanicServiceSave);

        public override object GetSave() {
            Dictionary<int, MechanicInfoSave> mechanicSaves =
                Mechanics.Select(kvp => kvp).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetSave());
            return new MechanicServiceSave {
                mechanics = mechanicSaves,
                repairedViewCount = ReparedViewCount
            };
        }


        public override void ResetByPlanets() {
            ResetFull();
        }

        public override void ResetByInvestors() {
            //ResetFull();
            ReparedViewCount = 0;
            IsLoaded = true;
        }
        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            Mechanics.Clear();
            ReparedViewCount = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            MechanicServiceSave save = obj as MechanicServiceSave;
            if(save != null ) {
                Mechanics.Clear();
                save.Validate();
                foreach(var kvp in save.mechanics) {
                    Mechanics.Add(kvp.Key, new MechanicInfo(kvp.Value));
                }


                ReparedViewCount = save.repairedViewCount;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        #endregion
    }


    public interface IMechanicService : IGameService {
        BosError BuyMechanic(int generatorId);
        int GetNextMechanicPrice(int generatorId);
        int GetMechanicCount(int generatorId);
        void AddMechanic(int generatorId, int count);
        bool IsAllowBuyMechanic(int generatorId, out BosError error);
        bool IsUnlocked { get; }
        int ReparedViewCount { get; }
        void ResetRepairedViewCount();
        void AddRepairedViewCount(int count);
        int GetServicedUnitCount(int generatorId);
        float ServiceSpeed { get; }
        MechanicInfo GetMechanic(int id);
        int ForceRepair(int generatorId, int countToRepair);
    }

    [Serializable]
    public class MechanicServiceSave {
        public Dictionary<int, MechanicInfoSave> mechanics;
        public int repairedViewCount;

        public void Validate() {
            if(mechanics == null) {
                mechanics = new Dictionary<int, MechanicInfoSave>();
            }
        }
    }




}