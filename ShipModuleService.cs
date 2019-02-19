namespace Bos {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Linq;
    using UDebug = UnityEngine.Debug;
    using Bos.Debug;
    using Ozh.Tools.Functional;

    public class ShipModuleService : SaveableGameBehaviour, IShipModuleService {

        public List<ShipModuleInfo> Modules { get; } = new List<ShipModuleInfo>();
        private Dictionary<int, int> ModuleCounters { get; } = new Dictionary<int, int>();

        #region IShipModuleService
        public void Setup(object data = null) {
            //UDebug.Log($"ShipModuleService.Setup()".Colored(ConsoleTextColor.fuchsia));
            StartCoroutine(UpdateLockedModulesImpl());
        }

        public void UpdateResume(bool pause)
            => UDebug.Log($"{nameof(ShipModuleService)}.{nameof(UpdateResume)}() => {pause}");


        public bool IsOpened(int moduleId)
            => GetModule(moduleId).State == ShipModuleState.Opened;

        public ShipModuleInfo GetModule(int moduleId) {
            return Modules.FirstOrDefault(module => module.Id == moduleId);
        }

        private void AddModuleCounter(int moduleId, int count) {
            if(ModuleCounters.ContainsKey(moduleId)) {
                ModuleCounters[moduleId] += count;
            } else {
                ModuleCounters.Add(moduleId, count);
            }
        }

        public ModuleTransactionState BuyModule(int moduleId) {
            ShipModuleInfo module = GetModule(moduleId);
            if(module.State == ShipModuleState.Available) {
                IPlayerService playerService = Services.PlayerService;
                if(playerService.IsEnough(module.Data.Currency)) {
                    playerService.RemoveCurrency(module.Data.Currency);
                    AddModuleCounter(module.Id, 1);
                    module.SetState(ShipModuleState.Opened);
                    return ModuleTransactionState.Success;
                } else {
                    return ModuleTransactionState.NotEnoughCurrency;
                }
            } else {
                return ModuleTransactionState.NotValidState;
            }
        }

        public void ForceOpenModule(int moduleId ) {
            ShipModuleInfo module = GetModule(moduleId);
            if (module != null) {
                AddModuleCounter(module.Id, 1);
                module.SetState(ShipModuleState.Opened);
            }
        }

        public void ForceModuleAvailable(int moduleId ) {
            ShipModuleInfo module = GetModule(moduleId);
            if(module != null && module.State == ShipModuleState.Locked ) {
                module.SetState(ShipModuleState.Available);
            }
        }

        public bool IsAllowBuyModule(int moduleId, out ModuleTransactionState status) {
            status = ModuleTransactionState.Success;
            IPlayerService playerService = Services.PlayerService;
            var module = GetModule(moduleId);
            if(module.State != ShipModuleState.Available) {
                status = ModuleTransactionState.NotValidState;
                return false;
            }
            if(!playerService.IsEnough(module.Data.Currency)) {
                status = ModuleTransactionState.NotEnoughCurrency;
                return false;
            }
            return true;
        }

        public Option<int> GetModuleCounter() {
            int sum = ModuleCounters.Values.Sum();
            if(sum > 0 ) {
                return F.Some(sum);
            }
            return F.None;
            /*
            if(ModuleCounters.ContainsKey(moduleId) ) {
                if(ModuleCounters[moduleId] > 0) {
                    return F.Some(ModuleCounters[moduleId]);
                }
            }
            return F.None;*/
        }

        public bool IsAllowByAnyModule() {
            foreach(var module in Modules) {
                if(!IsOpened(module.Id)) {
                    ModuleTransactionState state;
                    if(IsAllowBuyModule(module.Id, out state)) {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Unity events
        public override void OnEnable() {
            base.OnEnable();
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
        }
        public override void OnDisable() {
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            base.OnDisable();
        }

        private IEnumerator UpdateLockedModulesImpl() {
            var planetService = Services.GetService<IPlanetService>();
            yield return new WaitUntil(() => planetService.IsLoaded && this.IsLoaded && Services.ResourceService.IsLoaded);
            IEnumerable<PlanetInfo> openedPlanets = planetService.GetOpenedPlanets();
            foreach(PlanetInfo openedPlanet in openedPlanets) {
                MakeAvailableModuleForPlanet(openedPlanet.Data.Id);
            }
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if(newState == PlanetState.Opened ) {
                MakeAvailableModuleForPlanet(planet.Data.Id);
            }
        }

        private void MakeAvailableModuleForPlanet(int planetId) {
            foreach (var module in FilterLockedModules(planetId)) {
                module.SetState(ShipModuleState.Available);
            }
        }

        private List<ShipModuleInfo> FilterLockedModules(int planetId) 
            => Modules.Where(module => module.Data.PlanetId <= planetId && module.State == ShipModuleState.Locked).ToList();

        public bool IsAllModulesOpened 
            => Modules.All(module => module.State == ShipModuleState.Opened);

        #endregion

        #region Saveable Game Behaviour
        public override string SaveKey => "ship_module_service";

        public override Type SaveType => typeof(ShipModuleServiceSave);

        public override object GetSave() {
            List<ShipModuleSave> moduleSaves = new List<ShipModuleSave>();
            Modules.ForEach(module => moduleSaves.Add(module.GetSave()));
            return new ShipModuleServiceSave {
                modules = moduleSaves,
                moduleCounters = ModuleCounters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        private void CreateEmptyModules() {
            Modules.Clear();
            foreach (int moduleId in Services.ResourceService.Defaults.moduleIds) {
                Modules.Add(new ShipModuleInfo(moduleId));
            }
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }
        public override void ResetByInvestors() {
            IsLoaded = true;
        }
        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            CreateEmptyModules();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            ShipModuleServiceSave save = obj as ShipModuleServiceSave;
            if(save != null ) {
                Modules.Clear();
                if(save.modules != null ) {
                    foreach(var moduleSave in save.modules) {
                        Modules.Add(new ShipModuleInfo(moduleSave));
                    }
                } else {
                    CreateEmptyModules();
                }
                save.Validate();
                ModuleCounters.Clear();
                ModuleCounters.CopyFrom(save.moduleCounters);

                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        #endregion
    }

    public interface IShipModuleService : Bos.IGameService, ILoadable {
        List<ShipModuleInfo> Modules { get; }
        ShipModuleInfo GetModule(int moduleId);
        ModuleTransactionState BuyModule(int moduleId);
        bool IsAllModulesOpened { get; }
        bool IsAllowBuyModule(int moduleId, out ModuleTransactionState status);
        bool IsOpened(int moduleId);
        Option<int> GetModuleCounter();
        bool IsAllowByAnyModule();
        void ForceOpenModule(int moduleId);
        void ForceModuleAvailable(int moduleId);
    }

    public interface ILoadable {
        bool IsLoaded { get; }
    }

    [Serializable]
    public class ShipModuleServiceSave {
        public List<ShipModuleSave> modules;
        public Dictionary<int, int> moduleCounters;

        public void Validate() {
            if(moduleCounters == null ) {
                moduleCounters = new Dictionary<int, int>();
            }
        }
    }

    public enum ModuleTransactionState {
        Success = 0,
        NotValidState = 1,
        NotEnoughCurrency = 2
    }


}