namespace Bos {
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UDBG = UnityEngine.Debug;

    public class InvestorService : SaveableGameBehaviour, IInvestorService {

        private const float kDefaultEffectiveness = 0.01f;
        private const int kInvestorNeedForBonus  = 1000;
        private const int KSellCooldownInterval = 60;

        /// <summary>
        /// Ratio (Current Securities / Prev Securities) is condition to sell business to investors in earth
        /// </summary>
        private const double NextSecuirtiesRatioEarth = 100.0;
        private const double NextSecuirtiesRatioMoon = 100.0;
        private const double NextSecuirtiesRatioMars = 100.0;

        /// <summary>
        /// If we not sell business yet, need has MinSecuritiesCountForSirstSell to sell investors
        /// </summary>
        private const double MinSecuritiesCountForSirstSell = 10000.0;


        public int TriesCount { get; private set; }
        public float Effectiveness { get; private set; } = kDefaultEffectiveness;
        public bool IsFirstInvestorsScreenShowed { get; private set; } = false;
        private bool IsAlreadySolded { get; set; } = false;
        public int AllowSellTime { get; private set; } = 0;
        public List<HistoryEntry> History { get; } = new List<HistoryEntry>();
        public int StartTime { get; private set; }
        public int TimeOfAlertBlockedByStatus { get; private set; }
       

        /// <summary>
        /// Count of securities selling to investors (reset on each planet)
        /// </summary>
        public double LastSellSecuritiesOnCurrentPlanet { get; private set; }


        public InvestorAlertInfo AlertInfo { get; } = new InvestorAlertInfo();


        public override void OnEnable() {
            base.OnEnable();
            GameEvents.StatusLevelChanged += OnStatusLevelChanged;
            GameEvents.BusinessWasSoldToInvestors += OnBusinessWasSoldToInvestors;
            GameEvents.GameModeChanged += OnGameModeChanged;
            GameEvents.ViewShowed += OnViewShowed;
        }

        public override void OnDisable() {
            GameEvents.StatusLevelChanged -= OnStatusLevelChanged;
            GameEvents.BusinessWasSoldToInvestors -= OnBusinessWasSoldToInvestors;
            GameEvents.GameModeChanged -= OnGameModeChanged;
            GameEvents.ViewShowed -= OnViewShowed;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            firstInvestorTimer.Update();
        }

        private void OnStatusLevelChanged(int oldLevel, int newLevel ) {
            if(IsLoaded && (newLevel > oldLevel)) {
                if(TriesCount < 0 ) { TriesCount = 0; }
                AddTries((newLevel - oldLevel) * 2);
            }
        }



        private void AddTries(int count ) {
            int oldTries = TriesCount;
            TriesCount += count;
            if(TriesCount < 0 ) { TriesCount = 0; }
            if(oldTries != TriesCount ) {
                GameEvents.OnInvestorTriesChanged(this, new InvestorTriesChangedEventArgs(oldTries, TriesCount));
                GameEvents.OnInvestorTriesCountChanged(oldTries, TriesCount);
            }
        }

        private void RemoveTries(int count ) {
            int oldTries = TriesCount;
            TriesCount -= count;
            if (TriesCount < 0) { TriesCount = 0; }
            if(oldTries != TriesCount ) {
                GameEvents.OnInvestorTriesChanged(this, new InvestorTriesChangedEventArgs(oldTries, TriesCount));
                GameEvents.OnInvestorTriesCountChanged(oldTries, TriesCount);
            }
        }

        private void OnGameModeChanged(GameModeName oldName, GameModeName newName ) {
        }

        private void OnBusinessWasSoldToInvestors(int planetId, double securities, int interval) {
            LastSellSecuritiesOnCurrentPlanet = securities;
            UDBG.Log($"updated {nameof(LastSellSecuritiesOnCurrentPlanet)} = {LastSellSecuritiesOnCurrentPlanet}".Attrib(
                bold: true,
                color: "white"));

            History.Add(new HistoryEntry(planetId, securities, interval));

            GameMode.AddResetCount(1);
            FacebookEventUtils.LogSellBusinessToInvestorsEvent(GameServices.Instance.GameModeService.ResetCount);
            LocalData.TutorialInvestorsComplete = true;

            if (!IsAlreadySolded) {
                IsAlreadySolded = true;
                Services.ViewService.ShowDelayed(UI.ViewType.InvestorConfirmMessageBox,
                    BosUISettings.Instance.ViewShowDelay, new ViewData {
                        UserData = securities,
                        ViewDepth = ViewService.NextViewDepth
                    });
            } else {
                Sounds.PlayOneShot(SoundName.slotWin);
                ReloadAfterSold();
            }
           
        }
        private readonly UpdateTimer firstInvestorTimer = new UpdateTimer();
        private bool isInitialized = false;

        #region IInvestorService
        public void Setup(object data = null ) {

            firstInvestorTimer.Setup(2, dt => {
                if (IsLoaded) {
                    if (!IsFirstInvestorsScreenShowed) {
                        if (GameMode.GameModeName == GameModeName.Game) {
                            if (GetSecuritiesCountFromInvestors() > 1000) {
                                if (IsSellStateOk) {
                                    IsFirstInvestorsScreenShowed = true;
                                    StartCoroutine(ShowFirstInvestorView());
                                }
                            }
                        }
                    }
                }
            });

           if(!isInitialized ) {
                isInitialized = true;
            }
        }

        public void UpdateResume(bool pause)
            => UDBG.Log($"{nameof(InvestorService)}.{nameof(UpdateResume)}() => {pause}");


        private void OnViewShowed(ViewType type) {
            if(IsLoaded) {
                if(type == ViewType.InvestorsView ) {
                    SetTimeOfAlertBlockedBySTatus(TimeService.UnixTimeInt);
                }
            }
        }


        private IEnumerator ShowFirstInvestorView() {
            /*
            yield return new WaitUntil(() => IsLoaded);
            yield return new WaitUntil(() => GameMode.GameModeName == GameModeName.Game);
            yield return new WaitUntil(() => ViewService.ModalCount == 0 && ViewService.LegacyCount == 0);
            ViewService.Show(ViewType.FirstInvestorView, () => ViewService.ModalCount == 0 && ViewService.LegacyCount == 0, obj => { },
                new ViewData { ViewDepth = 200  });*/
            yield break;
        }

        public void SetFirstInvestorsScreenShowed(bool value)
            => IsFirstInvestorsScreenShowed = value;

        
        public void AddEffectiveness(float val) {
            float oldEffectiveness = Effectiveness;
            Effectiveness += Mathf.Abs(val);
            if(oldEffectiveness != Effectiveness) {
                GameEvents.OnInvestorEffectivenessChanged(oldEffectiveness, Effectiveness);
            }
        }

        public void RemoveEffectiveness(float val) {
            float oldEffectiveness = Effectiveness;
            Effectiveness -= Mathf.Abs(val);
            if(Effectiveness <= 0f ) {
                Effectiveness = 0f;
            }

            if (oldEffectiveness != Effectiveness) {
                GameEvents.OnInvestorEffectivenessChanged(oldEffectiveness, Effectiveness);
            }
        }

        public List<InvestorSellState> SellState {
            get {
               
                List<InvestorSellState> states = new List<InvestorSellState>();

                int numTries = TriesCount;
                if (numTries <= 0) {
                    states.Add(InvestorSellState.NoTries);
                }

                if (TimeService.UnixTimeInt < AllowSellTime) {
                    states.Add(InvestorSellState.Cooldown);
                }

                //If it is first purchasing we look at classic selling conditions: securities > 1000
                if (LastSellSecuritiesOnCurrentPlanet.Approximately(0.0)) {
                    if (GetSecuritiesCountFromInvestors() <= MinSecuritiesCountForSirstSell) {
                        states.Add(InvestorSellState.ZeroSecurities);
                    }
                } else {
                    //if it not first purchasing we look ration securitiesnext/securitiesprev > 100
                    double securitiesNext = GetSecuritiesCountFromInvestors();
                    double securitiesPrev = LastSellSecuritiesOnCurrentPlanet;
                    double ratio = securitiesNext / securitiesPrev;
                    if(ratio < GetRatio()) {
                        states.Add(InvestorSellState.BusinessIsSmall);
                    }
                }
                if (states.Count == 0) {
                    states.Add(InvestorSellState.Ok);
                }
                return states;
            }
        }

        private double GetRatio()
        {
            var currentPlanetId = Services.PlanetService.CurrentPlanetId.Id;
            switch (currentPlanetId)
            {
                case 0:
                    return NextSecuirtiesRatioEarth;
                case 1:
                    return NextSecuirtiesRatioMoon;
                default:
                    return NextSecuirtiesRatioMars;
            }
        }

        public void SetTimeOfAlertBlockedBySTatus(int time) {
            TimeOfAlertBlockedByStatus = time;
        }

        public bool IsSellStateOk {
            get {
                var sellStates = SellState;
                if(sellStates.Count == 1 ) {
                    return (sellStates[0] == InvestorSellState.Ok);
                }
                return false;
            }
        }




        public double GetSecuritiesCountFromInvestors() {
            ITransportUnitsService unitService = Services.TransportService;
            double lifetimeEarnings = Services.PlayerService.LifetimeEarningsInPlanet;
            double million = Math.Pow(10, 6);
            if (lifetimeEarnings < million) {
                return 0.0;
            }
            if (false == unitService.HasUnits(2)) {
                return 0;
            }
            double koef = 150.0 * Math.Sqrt(lifetimeEarnings / million);
            double lifetimeSecurities = Services.PlayerService.LifeTimeSecurities.Value;
            var result = koef - lifetimeSecurities;
            return result > 0 ? result : 0;
        }

        /// <summary>
        /// Compute count of company cash need to get for next sell to investors
        /// </summary>
        /// <returns></returns>
        public double GetCompanyCashRequiredToSellInvestors() {

            double targetSecurities = LastSellSecuritiesOnCurrentPlanet * GetRatio();
            if(targetSecurities.Approximately(0.0)) {
                targetSecurities = MinSecuritiesCountForSirstSell;
            }

            double securitiesInPlanet = Player.LifeTimeSecurities.Value;
            double companyCashInPlanet = Player.LifetimeEarningsInPlanet;

            double result = Math.Pow((targetSecurities + securitiesInPlanet) * (1000.0 / 150.0), 2.0) - companyCashInPlanet;
            if(result < 0.0 ) {
                result = 0.0;
            }
            return result;
        }

        public double SecuritiesProfitMult {
            get {
                return (1 + Math.Floor(Services.PlayerService.Securities.Value / kInvestorNeedForBonus) * Effectiveness);
            }
        }

        public void SellToInvestors(double multiplier = 1.0) {
            double value = GetSecuritiesCountFromInvestors();
            double addedSecurities = Math.Floor(value) * multiplier;
            Services.PlayerService.AddSecurities(addedSecurities.ToCurrencyNumber());
            AllowSellTime = Services.TimeService.UnixTimeInt + KSellCooldownInterval;
            //Services.GenerationService.MakeGeneratorsManual();
            Services.SaveService.ResetByInvestors();
            RemoveTries(1);
            GameEvents.OnBusinessWasSoldToInvestors(Services.PlanetService.CurrentPlanet.Id, addedSecurities, Services.TimeService.UnixTimeInt - StartTime);
            StartTime = Services.TimeService.UnixTimeInt;        
            Player.LegacyPlayerData.Save();
        }




        public double CalcInvestoreByMoney() {
            double earnings = Services.PlayerService.LifetimeEarningsInPlanet;
            double million = Math.Pow(10, 6);
            return 150.0f * Math.Sqrt(earnings / million);
        }


        public void ReloadAfterSold() {
            Services.ViewService.Show(ViewType.LoadingView, new ViewData {
                UserData = new LoadSceneData {
                    BuildIndex = 0,
                    LoadAction = () => { },
                    Mode = LoadSceneMode.Single
                }
            });
        }

        public bool IsBlockedBy(List<InvestorSellState> states, InvestorSellState targetState) {
            return states.Contains(targetState);
        }
        #endregion

        #region Private members

        /// <summary>
        /// Return history count of investor selling
        /// </summary>
        private int HistoryCount
            => History.Count;

        #endregion


        #region Saveable overrides
        public override string SaveKey => "investor_service";

        public override Type SaveType => typeof(InvestorServiceSave);

        public override object GetSave() {

            return new InvestorServiceSave {
                effectiveness = Effectiveness,             
                isFirstInvestorsScreenShowed = IsFirstInvestorsScreenShowed,
                allowSellTime = AllowSellTime,
                startTime = StartTime,
                history = History.Select(e => new HistoryEntrySave {  planetId = e.PlanetId, interval = e.Interval, securities = e.Securities }).ToList(),
                triesCount = TriesCount,
                isAlreadySolded = IsAlreadySolded,
                timeOfAlertBlockedByStatus = TimeOfAlertBlockedByStatus,
                lastSellSecuritiesOnCurrentPlanet = LastSellSecuritiesOnCurrentPlanet
            };
        }

        public override void ResetFull() {
            LoadDefaults();
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void ResetByInvestors() {

        }

        public override void ResetByPlanets() {
            LastSellSecuritiesOnCurrentPlanet = 0.0;
        }

        public override void LoadDefaults() {
            Effectiveness = kDefaultEffectiveness;
            TriesCount = 0;
            IsFirstInvestorsScreenShowed = false;
            AllowSellTime = 0;
            StartTime = Services.TimeService.UnixTimeInt;
            History.Clear();
            IsAlreadySolded = false;
            LastSellSecuritiesOnCurrentPlanet = 0.0;
            StartCoroutine(LoadDefaultsTimeOfAlertBlockedByStatusImpl());
            IsLoaded = true;
        }

        private IEnumerator LoadDefaultsTimeOfAlertBlockedByStatusImpl() {
            yield return new WaitUntil(() => TimeService.IsValid);
            TimeOfAlertBlockedByStatus = TimeService.UnixTimeInt;
        }

        public override void LoadSave(object obj) {
            InvestorServiceSave save = obj as InvestorServiceSave;
            if(save != null ) {
                save.Validate();
                Effectiveness = save.effectiveness;
                IsFirstInvestorsScreenShowed = save.isFirstInvestorsScreenShowed;
                AllowSellTime = save.allowSellTime;
                if(save.startTime == 0 ) {
                    StartTime = Services.TimeService.UnixTimeInt;
                } else {
                    StartTime = save.startTime;
                }
                History.Clear();
                if(save.history != null ) {
                    foreach(var entryObj in save.history) {
                        History.Add(new HistoryEntry(entryObj));
                    }
                }
                TriesCount = save.triesCount;
                IsAlreadySolded = save.isAlreadySolded;
                TimeOfAlertBlockedByStatus = save.timeOfAlertBlockedByStatus;
                LastSellSecuritiesOnCurrentPlanet = save.lastSellSecuritiesOnCurrentPlanet;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        
        #endregion
    }

    public class InvestorTriesChangedEventArgs : System.EventArgs {

        public int OldTries { get; private set; }
        public int NewTries { get; private set; }

        public InvestorTriesChangedEventArgs(int oldTries, int newTries) {
            OldTries = oldTries;
            NewTries = newTries;
        }
    }

    public interface IInvestorService : IGameService {
        void AddEffectiveness(float val);
        void RemoveEffectiveness(float val);
        float Effectiveness { get; }
        bool IsLoaded {get;}
        void SetFirstInvestorsScreenShowed(bool value);
        bool IsFirstInvestorsScreenShowed { get; }
        void SellToInvestors(double multiplier = 1.0);
        int AllowSellTime { get;}
        List<InvestorSellState> SellState { get; }
        double CalcInvestoreByMoney();
        double GetSecuritiesCountFromInvestors( );
        List<HistoryEntry> History { get; }
        double SecuritiesProfitMult { get; }
        int TriesCount { get; }
        void ReloadAfterSold();
        InvestorAlertInfo AlertInfo { get; }
        int TimeOfAlertBlockedByStatus { get; }
        void SetTimeOfAlertBlockedBySTatus(int time);
        bool IsBlockedBy(List<InvestorSellState> states, InvestorSellState targetState);
        bool IsSellStateOk { get; }
        double GetCompanyCashRequiredToSellInvestors();
    }

    [Serializable]
    public class InvestorServiceSave {
        public float effectiveness;
        public bool isFirstInvestorsScreenShowed;
        public int allowSellTime;
        public List<HistoryEntrySave> history;
        public int startTime;
        public int triesCount;
        public bool isAlreadySolded;
        public int timeOfAlertBlockedByStatus;
        public double lastSellSecuritiesOnCurrentPlanet;

        public void Validate() {}
    }

    public class HistoryEntry {
        public int PlanetId { get; private set; }
        public double Securities { get; private set; }
        public int Interval { get; private set; }

        public HistoryEntry(int planetId, double securities, int interval) {
            this.PlanetId = planetId;
            this.Securities = securities;
            this.Interval = interval;
        }

        public HistoryEntry(HistoryEntrySave save)
            : this(save.planetId, save.securities, save.interval) { }
    }

    public class HistoryEntrySave {
        public int planetId;
        public double securities;
        public int interval;
    }

    public enum InvestorSellState {
        Ok,
        NoTries,
        Cooldown,
        ZeroSecurities,
        BusinessIsSmall
    }


    public class InvestorAlertInfo {
        public bool IsAlertShowedInSession { get; private set; }
        public double CountOfSecuritiesToShowNextAlert { get; private set; }

        public void SetAlertShowedInSession(bool value) {
            IsAlertShowedInSession = value;
        }

        public void SetCountOfSecuritiesToShowNextAlert(double securities) {
            CountOfSecuritiesToShowNextAlert = securities;
        }

    }
    
}