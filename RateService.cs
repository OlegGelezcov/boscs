namespace Bos {
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Bos.UI;

    public class RateService : SaveableGameBehaviour, IRateService {

        private const int kIntervalFromStartGame = 20 * 60;

        //is was show rate on concrete planet
        private Dictionary<int, bool> ShowedMap { get; }
            = new Dictionary<int, bool>();



        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public void Setup(object obj ) {

            updateTimer.Setup(interval: 2f,
                action: dt => {
                    if (!IsRateCompleted) {
                        if (IsAllowShowOnCurrentPlanet) {
                            if (GameMode.GameModeName == GameModeName.Game) {
                                if (IsBusOpened) {
                                    if (ViewService.ModalCount == 0 && ViewService.LegacyCount == 0 && (!ViewService.Exists(ViewType.TutorialDialogView))) {
                                        GameStartTime.Match(None: () => {
                                        }, Some: (val) => {
                                            int interval = TimeService.UnixTimeInt - val;
                                            if (interval >= kIntervalFromStartGame) {
                                                ShowedMap[Planets.CurrentPlanetId.Id] = true;
                                                //show rate app
                                                ViewService.Show(UI.ViewType.RateAppView, new ViewData {
                                                    ViewDepth = ViewService.NextViewDepth
                                                });
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                });
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(RateService)}.{nameof(UpdateResume)}() => {pause}");


        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

        private Option<int> GameStartTime
            => Services.GameModeService.GameStartTime == 0 ? F.None : F.Some(Services.GameModeService.GameStartTime);

        private bool IsAllowShowOnCurrentPlanet
            => (!IsShowedOnCurrentPlanet);

        private bool IsShowedOnCurrentPlanet {
            get {
                if (Planets.CurrentPlanet != null) {
                    if (ShowedMap.ContainsKey(Planets.CurrentPlanetId.Id)) {
                        return ShowedMap[Planets.CurrentPlanetId.Id];
                    }
                }
                return false;
            }
        }

        private bool IsBusOpened
            => Services.TransportService.HasUnits(TransportUnitsService.kBusId);

        #region IRateService 
        //is user click Rate App button!
        public bool IsRateCompleted { get; private set; }

        private void CompleteRate()
            => IsRateCompleted = true;

        public void Rate() {
            CompleteRate();
#if UNITY_ANDROID
        Application.OpenURL(Services.ResourceService.Defaults.androidRateLink);
#elif UNITY_IPHONE
        Application.OpenURL("itms-apps://itunes.apple.com/app/1313277760");
#endif
            Player.AddCoins(10, true);
        }
        #endregion

        #region Saveable pattern implementation
        public override string SaveKey => "rate_service";

        public override Type SaveType => typeof(RateServiceSave);

        
        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByInvestors() { IsLoaded = true; }

        public override void ResetByPlanets() { IsLoaded = true; }

        public override void ResetByWinGame() { IsLoaded = true; }

        public override object GetSave() {
            return new RateServiceSave {
                isRateCompleted = IsRateCompleted,
                showedMap = ShowedMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        public override void LoadDefaults() {
            ShowedMap.Clear();
            IsRateCompleted = false;
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            RateServiceSave save = obj as RateServiceSave;
            if(save != null ) {
                save.Validate();
                ShowedMap.Clear();
                ShowedMap.CopyFrom(save.showedMap);
                IsRateCompleted = save.isRateCompleted;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        #endregion
    }
    public interface IRateService : IGameService {
        bool IsRateCompleted { get; }
        //void CompleteRate();
        void Rate();
    }

    public class RateServiceSave {
        public Dictionary<int, bool> showedMap;
        public bool isRateCompleted;

        public void Validate() {
            if(showedMap == null ) {
                showedMap = new Dictionary<int, bool>();
            }
        }
    }
}