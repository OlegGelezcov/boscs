namespace Bos
{
    using Bos.Data;
    using Bos.UI;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UDBG = UnityEngine.Debug;

    public class PlanetService : SaveableGameBehaviour, IPlanetService {

        private readonly Dictionary<int, PlanetInfo> planets = new Dictionary<int, PlanetInfo>();

        public int PlanetOpenCounter { get; private set; }

        private PlanetInfo currentPlanet = null;
        private int currentPlanetId = PlanetConst.EARTH_ID;

        private readonly UpdateTimer updatePlanetsTimer = new UpdateTimer();
        private bool checkSleep = false;
        private bool IsInitialized { get; set; }

        public bool IsOpened(int planetId)
            => CurrentPlanet.Id >= planetId;

        public int NextPlanetId
            => currentPlanetId + 1;

        public bool HasNextPlanet
            => NextPlanetId <= 5;





        #region Game Events
        public void Setup(object data = null) {
            updatePlanetsTimer.Setup(.5f, (interval) => {
                foreach (var pair in planets) {
                    pair.Value.Update(interval);
                }
            });
            if(!IsInitialized) {
                StartCoroutine(BuyMissedModules());
                IsInitialized = true;
            }
        }

        //In past modules where not binded to planets and can be
        //situation when we has planet already opened, but module yet not purchased
        //In this case I look current  planet state and if it's opened then force buy all required modules for this planet
        //It's check occured when service loaded
        private IEnumerator BuyMissedModules() {
            yield return new WaitUntil(() => IsLoaded && Services.Modules.IsLoaded && GameMode.IsGame);
            if(CurrentPlanet.Id != PlanetConst.EARTH_ID ) {
                if(CurrentPlanet.LocalData.IsModuleRequired ) {
                    int maxModuleId = CurrentPlanet.LocalData.module_id;
                    for(int i = 0; i <= maxModuleId; i++ ) {
                        ShipModuleInfo module = Services.Modules.GetModule(i);
                        if(module != null ) {
                            if(module.State != ShipModuleState.Opened ) {
                                Services.Modules.ForceModuleAvailable(module.Id);
                                Services.Modules.ForceOpenModule(module.Id);

                            }
                        }
                    }
                }
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.PlanetsReceivedFromServer += OnPlanetsReceivedFromServer;
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
        }

        public override void OnDisable() {
            base.OnDisable();
            GameEvents.PlanetsReceivedFromServer -= OnPlanetsReceivedFromServer;
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if(newState == PlanetState.Opened  ) {
                
                ChangeCurrentPlanet(planet.Id);
                ResetAfterOpenedPlanet();
            }         
        }

        private void ResetAfterOpenedPlanet() {
            Services.SaveService.ResetByPlanets();
            Services.SaveService.SaveAll();
            StartCoroutine(ReloadGameImpl());
        }

        private IEnumerator ReloadGameImpl() {
            for(int i = 0; i < 5; i++ ) {
                yield return new WaitForEndOfFrame();
            }
            Services.ViewService.Show(UI.ViewType.LoadingView, new ViewData {
                UserData = new LoadSceneData {
                    BuildIndex = 0,
                    Mode = LoadSceneMode.Single,
                    LoadAction = ()=> Services.ViewService.Remove(ViewType.LoadingView)
                }
            });
        }

        public override void Update() {
            base.Update();
            updatePlanetsTimer.Update();

            if(IsLoaded && checkSleep) {
                StartCoroutine(AddSleepIntervalImpl());
                checkSleep = false;
            }
        }

        private IEnumerator AddSleepIntervalImpl() {
            ISleepService sleepService = Services.GetService<ISleepService>();
            yield return new WaitUntil(() => {
                return sleepService.IsRunning && Services.TimeService.IsValid;
            });
            foreach(var planet in planets) {
                planet.Value.RemoveFromUnlockTimer(sleepService.SleepInterval);
            }
        }

        private void OnPlanetsReceivedFromServer() {
            UDBG.Log("Update planet data from server".Attrib(bold: true, italic: true, color: "grey"));
            foreach (var planetPair in planets) {
                planetPair.Value.UpdateData();
            }
        }


        #endregion

        #region IPlanetService
        public PlanetInfo GetPlanet(int id) {
            return planets.ContainsKey(id) ? planets[id] : null;
        }

        public PlanetInfo CurrentPlanet {
            get {
                if (currentPlanet == null || (currentPlanet.Id != currentPlanetId)) {
                    currentPlanet = GetPlanet(currentPlanetId);
                }
                return currentPlanet;
            }
        } 

        public void ChangeCurrentPlanet(int newPlanetId ) {
            if(newPlanetId == NextPlanetId && HasNextPlanet) {
                if (GetPlanet(newPlanetId).State == PlanetState.Opened) {

                    PlanetInfo oldPlanet = CurrentPlanet;
                    oldPlanet.SetEndTime(TimeService.UnixTimeInt);

                    currentPlanetId = newPlanetId;
                    currentPlanet = CurrentPlanet;
                    currentPlanet.SetStartTime(TimeService.UnixTimeInt);
                    Services.SetLoadingPlanet(CurrentPlanet.Id);
                    PlanetOpenCounter++;
                    GameEvents.OnCurrentPlanetChanged(oldPlanet, CurrentPlanet);             
                }
            }
        }





        public PlanetActionStatus StartOpening(int planetId ) {
            var planet = GetPlanet(planetId);
            if(planet == null ) {
                return PlanetActionStatus.IsNull;
            }
            if(planet.State != PlanetState.Closed) {
                return PlanetActionStatus.AlreadyOpened;
            }
            double cashPrice = planet.Data.CompanyCashPrice;
            double securityPrice = planet.Data.SecuritiesPrice;

            if(Services.PlayerService.CompanyCash.Value < cashPrice || Services.PlayerService.Securities.Value < securityPrice ) {
                return PlanetActionStatus.NotEnoughCash;
            }

            Services.PlayerService.RemoveCompanyCash(cashPrice);
            Services.PlayerService.RemoveSecurities(securityPrice);
            planet.Open();
            return PlanetActionStatus.Success;
        }

        public void SetOpened(int planetId ) {
            var planet = GetPlanet(planetId);
            if(planet.State == PlanetState.ReadyToOpen) {
                planet.SetState(PlanetState.Opened);
            }
        }

        public void ForceSetOpened(int planetId ) {
            var planet = GetPlanet(planetId);
            planet.SetState(PlanetState.Opened);
        }

        public IEnumerable<PlanetInfo> GetOpenedPlanets() {
            List<PlanetInfo> openedPlanets = new List<PlanetInfo>();
            foreach(var planetPair in planets) {
                if(planetPair.Value.State == PlanetState.Opened) {
                    openedPlanets.Add(planetPair.Value);
                }
            }
            return openedPlanets;
        }

        #endregion

        #region ISaveable
        private void SetupPlanets() {
            planets.Clear();
            foreach (int planetId in PlanetConst.PlanetIds) {
                PlanetInfo planetInfo = new PlanetInfo(planetId, Services);
                planets.Add(planetId, planetInfo);
            }
        }

        private void LoadFromSaves(IEnumerable<PlanetSave> saves) {
            foreach (PlanetSave save in saves) {
                if (planets.ContainsKey(save.id)) {
                    planets[save.id].Load(save);
                }
            }
        }

        public override string SaveKey => "planet_service";

        public override Type SaveType => typeof(PlanetServiceSave);

        public override object GetSave() {
            List<PlanetSave> planetSaves = new List<PlanetSave>();
            foreach (var pair in planets) {
                planetSaves.Add(pair.Value.GetSave());
            }
            return new PlanetServiceSave {
                planets = planetSaves,
                currentPlanetId = currentPlanetId,
                planetOpenCounter = PlanetOpenCounter
            };
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            SetupPlanets();

            List<PlanetSave> defaultSaves = new List<PlanetSave>();
            foreach (int planetId in PlanetConst.PlanetIds) {
                if (planetId == PlanetConst.EARTH_ID) {
                    defaultSaves.Add(new PlanetSave { id = planetId, state = PlanetState.Opened, unlockTimer = 0 });
                } else {
                    defaultSaves.Add(new PlanetSave { id = planetId, state = PlanetState.Closed, unlockTimer = 0 });
                }
            }

            LoadFromSaves(defaultSaves);

            currentPlanetId = PlanetConst.EARTH_ID;
            currentPlanet = CurrentPlanet;
            StartCoroutine(SetEarthStartTime());

            IsLoaded = true;
        }

        private IEnumerator SetEarthStartTime() {
            yield return new WaitUntil(() => TimeService.IsValid);
            if (CurrentPlanet != null && CurrentPlanet.Id == PlanetConst.EARTH_ID) {
                CurrentPlanet?.SetStartTime(TimeService.UnixTimeInt);
            }
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();   
        }

        public override void LoadSave(object obj) {

            PlanetServiceSave save = obj as PlanetServiceSave;
            if (save != null) {
                SetupPlanets();
                if (save.planets != null) {
                    LoadFromSaves(save.planets);
                }
                currentPlanetId = save.currentPlanetId;
                currentPlanet = CurrentPlanet;
                PlanetOpenCounter = save.planetOpenCounter;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }

        }


        #endregion

        private void OnApplicationPause(bool isPause)
            => UpdateResume(isPause);

        private void OnApplicationFocus(bool isFocus)
            => UpdateResume(!isFocus);

        public void UpdateResume(bool pause) {
            //UnityEngine.Debug.Log($"{nameof(PlanetService)}.{nameof(UpdateResume)}() => {pause}");
            checkSleep = !pause;
        }

        public bool IsMoonOpened
            => IsOpened(PlanetConst.MOON_ID);

        public bool IsMarsOpened
            => IsOpened(PlanetConst.MARS_ID);

        public PlanetId CurrentPlanetId
            => new PlanetId(CurrentPlanet.Id);

        public Option<PlanetInfo> GetOpeningPlanet() {

            foreach(var kvp in planets) {
                var planet = kvp.Value;
                if(planet.State == PlanetState.Opening ) {
                    return F.Some(planet);
                }
            }
            return F.None;
        }
    }


    public enum PlanetActionStatus {
        Success,
        IsNull,
        AlreadyOpened,
        NotEnoughCash
    }

    public interface IPlanetService : IGameService {
        PlanetInfo CurrentPlanet { get; }
        PlanetInfo GetPlanet(int id);
        void ChangeCurrentPlanet(int newPlanetId);

        PlanetActionStatus StartOpening(int planetId);
        void SetOpened(int planetId);

        int NextPlanetId { get; }
        bool HasNextPlanet { get; }
        bool IsLoaded { get; }
        IEnumerable<PlanetInfo> GetOpenedPlanets();
        bool IsOpened(int planetId);
        bool IsMoonOpened { get; }
        bool IsMarsOpened { get; }
        void ForceSetOpened(int planetId);

        PlanetId CurrentPlanetId { get; }
        int PlanetOpenCounter { get; }
        Option<PlanetInfo> GetOpeningPlanet();
    }



    public enum PlanetState {
        Closed,
        Opening,
        ReadyToOpen,
        Opened
    }

    [Serializable]
    public class PlanetServiceSave {
        public List<PlanetSave> planets;
        public int currentPlanetId;
        public int planetOpenCounter;

        public void Validate() {
            if(planets == null ) {
                planets = new List<PlanetSave>();
            }
        }
    }

    public enum PlanetType {
        Earth = 0,
        Moon = 1,
        Mars = 2,
        Asteroid = 3,
        Europe = 4,
        Titan = 5
    }
}