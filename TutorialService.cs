using UniRx;

namespace Bos {
    using Bos.Data;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public class TutorialService : SaveableGameBehaviour, ITutorialService {

        public bool isForceTutorial = false;
        public bool IsWasAnyRollback { get; private set; } = false;
        public bool IsAnyUpgradePurchased { get; private set; } = false;
        public bool IsAnyDailyBonusClaimed { get; private set; } = false;
        public bool IsWasSoldToInvestors { get; private set; } = false;
        public bool IsWasTransfer { get; private set; } = false;
        public bool IsMegaBoostWasActivated { get; private set; } = false;
        public bool IsWasCasinoSpins { get; private set; } = false;

        //public bool IsWasCasinoSpin { get; private set; } = 

        public bool IsPaused { get; private set; } = false;
        
 
        private readonly List<TutorialStateName> completedStates = new List<TutorialStateName>();

        public List<TutorialState> States { get; } = new List<TutorialState>() {
            new NoneTutorialState(),
            new HelloTextState(),
            new HintBuyRickshawState(),
            new ClickGenerateRickshawState(),
            new WaitForCashOnSecondRickshawState(),
            new WaitCashForFirstManagerState(),
            new MakeRollbackState(),
            new BuyTaxiState(),
            new PlaySlotstate(),
            new BuyUpgradeState(),
            new DailyBonusState(),
            new BuyBusState(),
            new MegaBoostState(),
            new BuyTransportCountState(),
            //new TransferPersonalCashState(),
            //new BuyPersonalProductState(),
            new EnhanceManagerState(),
            new BankState(),
            new MoonState(),
            new PlanetTransportState(),
            new ReportState(),
            new BreakLinesState(),
            new TeleportState(),
            new MarsState(),
            new TutorialMechanicState(),
            new SpaceShipState(),
            
        };

        private Dictionary<string, TutorialFinger> Fingers { get; } = new Dictionary<string, TutorialFinger>();

        //private readonly UpdateTimer activateTimer = new UpdateTimer();
        //private readonly UpdateTimer updateStateTimer = new UpdateTimer();
        private readonly string HightlightRegionPrefabId = "highlightarea";

        private ShowTransferAndPersonalPurchasesState transferState = null;
        private bool isInitialized = false;

        public void Setup(object obj) {

            //add state for showing transfer and purchasing....
            var target = States.Find(t => t.Name == TutorialStateName.ShowTransferAnsPersonalPurchasesState);
            if (target == null) {
                transferState = new ShowTransferAndPersonalPurchasesState(Services);
                States.Add(transferState);
            }

            target = States.Find(t => t.Name == TutorialStateName.ShowUpgradeEfficiencyState);
            if (target == null) {
                States.Add(new ShowUpgradeEfficiencyState(Services));
            }

            if (false == isInitialized) {
                Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                    if (IsLoaded && GameMode.IsLoaded && GameMode.IsGame) {
                        if (!IsPaused) {
                            if (!HasActiveState) {
                                foreach (var state in States) {
                                    if (!state.IsActive && !state.IsCompleted) {
                                        if (state.TryActivate(Services)) {
                                            break;
                                        }
                                    }
                                }
                            }

                            TryActivateTransferState();
                        }
                    }
                }).AddTo(gameObject);

                Observable.Interval(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => {
                    if (IsLoaded && GameMode.IsLoaded && GameMode.IsGame) {
                        if (!IsPaused) {
                            States.ForEach(state => {
                                if (state.IsActive && !state.IsCompleted) {
                                    state.OnUpdate(Services, 0.2f);
                                }
                            });
                        }
                    }              
                    
                }).AddTo(gameObject);
                isInitialized = true;
            }
        }

        public void UpdateResume(bool pause) 
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        private bool TryActivateTransferState() {
            if(transferState != null ) {
                if(!transferState.IsActive && !transferState.IsCompleted) {
                    return transferState.TryActivate(Services);
                }
            }
            return false;
        }



        public override void OnEnable() {
            base.OnEnable();
            GameEvents.TutorialEvent += OnTutorialEvent;
            GameEvents.GameModeChanged += OnGameModeChanged;
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.TransportManagerHired += OnManagerHired;
            GameEvents.ManagerKickBack += OnManagerRollback;
            GameEvents.UpgradeAdded += OnUpgradeAdded;
            GameEvents.DailyBonusClaimed += OnDailyBonusClaimed;
            GameEvents.GeneratorResearched += OnGeneratorResearched;
            GameEvents.X20BoostMultStarted += OnMegaBoost;
            GameEvents.BusinessWasSoldToInvestors += OnWasSoldToInvestors;
            GameEvents.PrizeWheelTriesChanged += OnPrizeWheelTriesChanged;
            GameEvents.ProductPurchased += OnPersonalProductPurchased;
            GameEvents.GeneratorEnhanced += OnGeneratorEnhanced;
            GameEvents.SecretaryCountChanged += OnSecretaryCountChanged;
            GameEvents.SplitTriesChanged += OnSplitLinesTriesChanged;
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
            GameEvents.MechanicAdded += OnMechanicAdded;
            GameEvents.ShipModuleStateChanged += OnShipModuleStateChanged;
            GameEvents.X20BoostStateChanged += OnMegaboostStateChanged;
            GameEvents.ViewHided += OnViewHided;
            GameEvents.RollbackLevelChanged += OnRollbackLevelChanged;
            GameEvents.EfficiencyLevelChanged += OnEfficiencyLevelChanged;
        }

        public override void OnDisable() {
            GameEvents.TutorialEvent -= OnTutorialEvent;
            GameEvents.GameModeChanged -= OnGameModeChanged;
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.TransportManagerHired -= OnManagerHired;
            GameEvents.ManagerKickBack -= OnManagerRollback;
            GameEvents.UpgradeAdded -= OnUpgradeAdded;
            GameEvents.DailyBonusClaimed -= OnDailyBonusClaimed;
            GameEvents.GeneratorResearched -= OnGeneratorResearched;
            GameEvents.X20BoostMultStarted -= OnMegaBoost;
            GameEvents.BusinessWasSoldToInvestors -= OnWasSoldToInvestors;
            GameEvents.PrizeWheelTriesChanged -= OnPrizeWheelTriesChanged;
            GameEvents.ProductPurchased -= OnPersonalProductPurchased;
            GameEvents.GeneratorEnhanced -= OnGeneratorEnhanced;
            GameEvents.SecretaryCountChanged -= OnSecretaryCountChanged;
            GameEvents.SplitTriesChanged -= OnSplitLinesTriesChanged;
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            GameEvents.MechanicAdded -= OnMechanicAdded;
            GameEvents.ShipModuleStateChanged -= OnShipModuleStateChanged;
            GameEvents.X20BoostStateChanged -= OnMegaboostStateChanged;
            GameEvents.ViewHided -= OnViewHided;
            GameEvents.RollbackLevelChanged -= OnRollbackLevelChanged;
            GameEvents.EfficiencyLevelChanged -= OnEfficiencyLevelChanged;
            base.OnDisable();
        }

        private void OnEfficiencyLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel level ) {
            if(IsLoaded) {
                States.ForEach(s => s.OnEfficiencyLevelChanged(oldLevel, newLevel, level));
            }
        }

        private void OnRollbackLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel level) {
            if(IsLoaded) {
                States.ForEach(s => s.OnRollbackLevelChanged(oldLevel, newLevel, level));
            }
        }

        private void OnMegaboostStateChanged(BoostState oldState, BoostState newState ) {
            if(IsLoaded) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.MegaboostStateChanged, newState));
            }
        }

        private void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo module ) {
            if(IsLoaded ) {
                if(newState == ShipModuleState.Opened ) {
                    SkipState(TutorialStateName.SpaceShip);
                }
            }
        }

        private void OnMechanicAdded(MechanicInfo mechanic ) {
            if(IsLoaded ) {
                SkipState(TutorialStateName.Mechanic);
            }
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if(IsLoaded) {
                if(planet.Id == PlanetConst.MARS_ID ) {
                    if(newState == PlanetState.Opened || newState == PlanetState.Opening || newState == PlanetState.ReadyToOpen ) {
                        SkipState(TutorialStateName.Mars);
                    }
                }
            }
        }

        private void OnSplitLinesTriesChanged(int oldCount, int newCount ) {
            if (IsLoaded) {
                if (oldCount > newCount) {
                    SkipState(TutorialStateName.BreakLines);
                }
            }
        }

        private void OnSecretaryCountChanged(int oldCount, int newCount, SecretaryInfo secretary) {
            if (IsLoaded) {
                if (newCount > 0) {
                    SkipState(TutorialStateName.Report);
                }
            }
        }

        private void OnGeneratorEnhanced(GeneratorInfo generator ) {
            if(IsLoaded) {
                SkipState(TutorialStateName.EnhanceManager);
            }
        }

        private void OnPersonalProductPurchased(ProductData personalProduct ) {
            if(IsLoaded ) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.PersonalProductPurchased, personalProduct));
                //SkipState(TutorialStateName.BuyPersonalProduct);
            }
        }

        private void OnPrizeWheelTriesChanged(int oldTries, int newTries) {
            if (IsLoaded) {
                if (oldTries > newTries) {
                    IsWasCasinoSpins = true;
                    SkipState(TutorialStateName.PlaySlot);
                }
            }
        }

        private void OnWasSoldToInvestors(int planetId, double securitiesAdded, int interval) {
            if (IsLoaded) {
                IsWasSoldToInvestors = true;
            }
        }

        private void OnMegaBoost(bool isOn) {
            if(IsLoaded) {
                IsMegaBoostWasActivated = true;
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.MegaBoostActivated));
                //SkipState(TutorialStateName.MegaBoost);
            }
        }

        private void OnGeneratorResearched(GeneratorInfo generator) {
            if(IsLoaded ) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.GeneratorResearched, generator));
                if(generator.GeneratorId == 2) {
                    SkipState(TutorialStateName.BuyBus);
                } else if(generator.GeneratorId == 9) {
                    SkipState(TutorialStateName.Teleport);
                }
            }
        }

        private void OnDailyBonusClaimed(DailyBonusItem item) {
            if (IsLoaded) {
                IsAnyDailyBonusClaimed = true;
                SkipState(TutorialStateName.DailyBonus);
            }
        }

        private void OnUpgradeAdded(UpgradeData upgradeData ) {
            if(IsLoaded ) {
                IsAnyUpgradePurchased = true;
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.UpgradePurchased, upgradeData));
                SkipState(TutorialStateName.BuyUpgrade);
            }
        }

        private void OnTutorialEvent(TutorialEventData eventData ) {
            if(IsLoaded) {
                SendEventToNonCompleted(eventData);
                if(eventData.EventName == TutorialEventName.LegalTransferCompleted ||
                   eventData.EventName == TutorialEventName.IllegalTransferCompleted) {
                    IsWasTransfer = true;
                    //SkipState(TutorialStateName.TransferPersonalCash);
                } 
            }
        }

        private void OnManagerRollback(double payed, bool isFirst, ManagerInfo manager ) {
            if (IsLoaded) {
                IsWasAnyRollback = true;
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.ManagerRollback, manager.Id));
                SkipState(TutorialStateName.MakeRollbackState);
            }
        }

        private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue  ) {
            if (IsLoaded) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.CopanyCashChanged, newValue.Value));
            }
        }
        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode ) {
            if (IsLoaded) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.GameModeChanged, newGameMode));
            }
        }

        private void OnUnitCountChanged(TransportUnitInfo unit ) {
            if(IsLoaded ) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.UnitCountChanged, unit));

                if(unit.TotalCount > 0 && unit.GeneratorId == 1 ) {
                    SkipState(TutorialStateName.BuyTaxi);
                }
            }
        }

        private void OnViewShowed(ViewType viewType ) {
            if(IsLoaded ) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.ViewOpened, viewType));

                if (viewType == ViewType.BankView) {
                    SkipState(TutorialStateName.Bank);
                }

            }
        }

        private void OnViewHided(ViewType viewType ) {
            if(IsLoaded ) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.ViewHided, viewType));
            }
        }

        private void OnManagerHired(ManagerInfo manager) {
            if(IsLoaded ) {
                SendEventToNonCompleted(new TutorialEventData(TutorialEventName.ManagerHired, manager.Id));
                SkipState(TutorialStateName.WaitCashForFirstManager);
            }
        }

        private bool HasActiveState
            => States.Any(state => state.IsActive);

        #region ITutorialService




        public void CompleteState(TutorialStateName stateName) {
            StartCoroutine(CompleteStateImpl(stateName));
        }

        private IEnumerator CompleteStateImpl(TutorialStateName stateName ) {
            yield return new WaitUntil(() => IsLoaded);
            GetState(stateName)?.SetCompleted(Services, true);
        }

        public bool IsCompleted
            => States.All(state => state.IsCompleted);

        public bool IsStateCompleted(TutorialStateName name) {
            return (GetState(name)?.IsCompleted ?? false) == true;
        }

        public bool IsStateActive(TutorialStateName name) {
            return (GetState(name)?.IsActive ?? false) == true;
        }

        public bool IsStateActiveOnStage(TutorialStateName stateName, int stage) {
            var state = GetState(stateName);
            if(state != null ) {
                return state.IsActive && (state.Stage == stage);
            }
            return false;
        }

        public void SetStage(TutorialStateName state, int stage)
            => GetState(state)?.SetStage(stage);

        public TutorialPositionObject FindPositionObject(string positionObjectName) {
            return FindObjectsOfType<TutorialPositionObject>().FirstOrDefault(obj => obj.tutorialPositionName == positionObjectName);
        }

        public bool IsHasTutorialPositionObject(string tutorialPositionObjectName)
            => FindPositionObject(tutorialPositionObjectName) != null;


        public TutorialFinger CreateFinger(Transform parent, TutorialFingerData fingerData) {
            if(Fingers.ContainsKey(fingerData.Id)) {
                RemoveFinger(fingerData.Id);
            }
            GameObject fingerObj = GameObject.Instantiate(Services.ResourceService.Prefabs.GetPrefab("finger"), parent, false);
            TutorialFinger finger = fingerObj.GetComponent<TutorialFinger>();
            finger.Setup(fingerData);
            Fingers.Add(finger.Data.Id, finger);
            return finger;
        }



        public TutorialFinger CreateFinger( string tutorialPositionObjectName, TutorialFingerData fingerData) {
            var positionObject = FindPositionObject(tutorialPositionObjectName);
            if(positionObject) {
                return CreateFinger( positionObject.transform, fingerData);
            } else {
                StartCoroutine(TryCreateFingerNTimes(20, tutorialPositionObjectName, fingerData));
            }
            return null;
        }

        private IEnumerator TryCreateFingerNTimes(int count, string tutorialPositionObjectName, TutorialFingerData fingerData ) {
            for(int i = 0; i < count; i++ ) {
                var positionObject = FindPositionObject(tutorialPositionObjectName);
                if(positionObject) {
                    CreateFinger(positionObject.transform, fingerData);
                    yield break;
                } else {
                    yield return new WaitForSeconds(.1f);
                }
            }
        }

        public void RemoveFinger(string fingerId ) {
            if(Fingers.ContainsKey(fingerId)) {
                TutorialFinger finger = Fingers[fingerId];
                if(finger && finger.gameObject) {
                    Destroy(finger.gameObject);
                }
                Fingers.Remove(fingerId);
            }
        }

        public void RemoveAllFingers() {
            var keys = Fingers.Keys.ToList();
            keys.ForEach(fingerId => RemoveFinger(fingerId));
        }

        public TutorialState ForceStart(int stateName) {
            try {
                var state = GetState((TutorialStateName)stateName);
                state?.ForceStart(Services);
                return state;
            } catch(Exception exception ) {
                UnityEngine.Debug.LogError(exception.Message);
                UnityEngine.Debug.LogError(exception.StackTrace);
            }
            return null;
        }

        public bool IsLockedByState(string lockedElementName ) {
            foreach(var state in States ) {
                if(state.IsActive ) {
                    if(state.IsElementLocked(lockedElementName)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CreateHighlightRegion(HighlightParams highlightParams ) {
            HightlightRegionPrefabId
                .CreateGameObject<HighlightArea>(
                    ViewService.GetCanvasTransform(CanvasType.UI))
                    .Setup(highlightParams);
        }

        public void RemoveHighlightRegion() {
            
            foreach(var hr in FindObjectsOfType<HighlightArea>() ) {
                if(hr && hr.gameObject ) {
                    Destroy(hr.gameObject);
                }
            }
        }


        public void PrintTutorialStates() {
            var sb = new StringBuilder();
            foreach(var state in States ) {
                sb.AppendLine($"state = {state.Name}, is active = {state.IsActive}, is completed = {state.IsCompleted}");
            }
            print(sb.ToString());
        }

        public string GetValidationDump()
        {
            StringBuilder sb = new StringBuilder();
            States.ForEach(s =>
            {
                sb.AppendLine(s.GetValidationDescription(Services));
                sb.AppendLine("----------------------------------");
            });
            return sb.ToString();
        }

        #endregion

        #region Private Methods
        public TutorialState GetState(TutorialStateName name) 
            => States.FirstOrDefault(state => state.Name == name);

        private void SendEventToNonCompleted(TutorialEventData data) {
            if (!IsPaused || (data.EventName == TutorialEventName.MegaBoostActivated)) {
                States.ForEach(state => {
                    if (!state.IsCompleted) {
                        state.OnEvent(Services, data);
                    }
                });
            }
        }

        public void SkipTutorial() {
            States.ForEach(state => state.ForceSetCompleted());
        }

        private void ForceComplete(TutorialStateName state)
            => States.FirstOrDefault(s => s.Name == state)?.ForceSetCompleted();

        private void SkipState(TutorialStateName state) {
            var stateObj = GetState(state);
            if (stateObj != null) {
                if (!stateObj.IsCompleted && !stateObj.IsActive) {
                    ForceComplete(state);
                }
            }
        }

        public void SetPaused(bool value, bool removeFingers = true) {
            bool oldPaused = IsPaused;
            IsPaused = value;

            UnityEngine.Debug.Log($"TUTORIAL PAUSED: {IsPaused}".Attrib(bold: true, italic: true, color: "g", size: 20));
            if( (!oldPaused && IsPaused) && removeFingers ) {
                RemoveAllFingers();
            }
        }

        public void SetPausedOnInterval(float interval, Action endCallback) {
            SetPaused(true, removeFingers: false);
            StartCoroutine(UnpauseAfterInterval(interval, endCallback));
        }

        private IEnumerator UnpauseAfterInterval(float interval, Action endCallback) {
            yield return new WaitForSeconds(interval);
            SetPaused(false, removeFingers: false);
            endCallback?.Invoke();
        }

        public bool ExistsFinger(string fingerName) {
            return Fingers.ContainsKey(fingerName);
        }
        #endregion

        #region SaveableGameBehaviour
        public override string SaveKey => "tutorial_service";

        public override Type SaveType => typeof(TutorialServiceSave);

        public override object GetSave() {

            List<TutorialStateSave> stateSaves = new List<TutorialStateSave>();
            States.ForEach(state => stateSaves.Add(state.GetSave()));
            return new TutorialServiceSave {
                isWasAnyRollback = IsWasAnyRollback,
                stateSaves = stateSaves,
                isAnyUpgradePurchased = IsAnyUpgradePurchased,
                isAnyDailyBonusClaimed = IsAnyDailyBonusClaimed,
                isWasSoldToInvestors = IsWasSoldToInvestors,
                isWasTransfer = IsWasTransfer,
                isMegaBoostWasActivated = IsMegaBoostWasActivated,
                isPaused = IsPaused,
                isWasCasinoSpins = IsWasCasinoSpins
            };
        }

        public override void LoadDefaults() {
            States.ForEach(state => state.Reset());
            IsWasAnyRollback = false;
            IsAnyUpgradePurchased = false;
            IsAnyDailyBonusClaimed = false;
            IsMegaBoostWasActivated = false;
            IsPaused = false;
            IsLoaded = true;
            foreach (var state in States) {
                state.Setup(Services);
            }
        }
        
        public override void ResetByWinGame() {
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            TutorialServiceSave save = obj as TutorialServiceSave;
            if(save != null ) {
                save.Validate();
                save.stateSaves.ForEach(sv => GetState(sv.stateName)?.Load(sv));
                foreach (var state in States) {
                    state.Setup(Services);
                }

                IsWasAnyRollback = save.isWasAnyRollback;
                IsAnyUpgradePurchased = save.isAnyUpgradePurchased;
                IsAnyDailyBonusClaimed = save.isAnyDailyBonusClaimed;
                IsWasSoldToInvestors = save.isWasSoldToInvestors;
                IsWasTransfer = save.isWasTransfer;
                IsMegaBoostWasActivated = save.isMegaBoostWasActivated;
                IsPaused = save.isPaused;
                IsWasCasinoSpins = save.isWasCasinoSpins;
                StartCoroutine(OnEnterImpl());


                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        private IEnumerator OnEnterImpl() {
            yield return new WaitUntil(() => Services.GameModeService.GameModeName == GameModeName.Game);
            States.ForEach(state => {
                if (state.IsActive) {
                    if (!IsPaused) {
                        state.OnEnter(Services);
                    } else {
                        ForceComplete(state.Name);
                    }
                }
            });
        }

        public override void ResetByInvestors() {
            IsLoaded = true;   
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetFull() {
            IsLoaded = true;
        } 
        #endregion
    }

    [System.Serializable]
    public class TutorialServiceSave {

        public List<TutorialStateSave> stateSaves;

        public bool isWasAnyRollback;
        public bool isAnyUpgradePurchased;
        public bool isAnyDailyBonusClaimed;
        public bool isWasSoldToInvestors;
        public bool isWasTransfer;
        public bool isMegaBoostWasActivated;
        public bool isWasCasinoSpins;
        public bool isPaused;

        public void Validate() {
            if(stateSaves == null ) {
                stateSaves = new List<TutorialStateSave>();
            }
        }
    }

    public interface ITutorialService : IGameService {
        void CompleteState(TutorialStateName stateName);
        bool IsCompleted { get; }
        TutorialPositionObject FindPositionObject(string positionObjectName);
        TutorialFinger CreateFinger( Transform parent, TutorialFingerData fingerData);
        TutorialFinger CreateFinger( string tutorialPositionObjectName, TutorialFingerData fingerData);
        void RemoveFinger(string fingerId);
        void RemoveAllFingers();
        bool ExistsFinger(string fingerName);

        bool IsWasAnyRollback { get; }
        bool IsAnyUpgradePurchased { get; }
        bool IsAnyDailyBonusClaimed { get; }
        bool IsWasSoldToInvestors { get; }
        bool IsWasTransfer { get; }
        bool IsMegaBoostWasActivated { get; }
        bool IsWasCasinoSpins { get; }

        bool IsHasTutorialPositionObject(string tutorialPositionObjectName);

        bool IsStateCompleted(TutorialStateName name);
        bool IsStateActive(TutorialStateName name);
        List<TutorialState> States { get; }
        void SkipTutorial();
        bool IsPaused { get; }
        void SetPaused(bool value, bool removeFingers = true);
        bool IsStateActiveOnStage(TutorialStateName stateName, int stage);
        void SetStage(TutorialStateName state, int stage);
        TutorialState ForceStart(int stateName);
        bool IsLockedByState(string lockedElementName);
        void CreateHighlightRegion(HighlightParams highlightParams);
        void RemoveHighlightRegion();
        void PrintTutorialStates();
        TutorialState GetState(TutorialStateName name);
        string GetValidationDump();
        void SetPausedOnInterval(float interval, Action endCallback);
    }
}