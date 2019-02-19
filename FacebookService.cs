using Firebase;

namespace Bos {
    using Bos.Debug;
    using Facebook.Unity;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FacebookService : SaveableGameBehaviour, IFacebookService {

        private const int VIDEO_10_ACHIEVMENT_ID = 1001;
        private const int VIDEO_100_ACHIEVMENT_ID = 1002;
        private const int VIDEO_1000_ACHIEVMENT_ID = 1003;

        private int TotalAdCount { get; set; }
        private Dictionary<string, bool> _firstAdWatchedByType = new Dictionary<string, bool>();

        private FirebaseApp _firebaseApp;

        public void Setup(object data = null) 
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available) {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    _firebaseApp = Firebase.FirebaseApp.DefaultInstance;

                    UnityEngine.Debug.Log("Firebase init success");
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                } else {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(FacebookService)}.{nameof(UpdateResume)}() => {pause}");


        public override void OnEnable() {
            base.OnEnable();
            GameEvents.AchievmentCompleted += OnAchievmentCompleted;
            GameEvents.RewardedVideoFinished += OnRewardedVideoWatched;
            GameEvents.ManagerKickBack += OnKickBackManager;
            GameEvents.CurrentPlanetChanged += OnPlanetChanged;
            GameEvents.ShipModuleStateChanged += OnShipModuleStateChanged;
            GameEvents.PrizeWheelTriesChanged += OnPrizeWheelTriesChanged;
            GameEvents.TreasureHuntTriesChanged += OnTreasureHuntTriesChanged;
            GameEvents.SplitTriesChanged += OnSplitGameTriesChanged;
            GameEvents.BankLevelChanged += OnBankLevelChanged;
        }

        public override void OnDisable() {
            GameEvents.AchievmentCompleted -= OnAchievmentCompleted;
            GameEvents.RewardedVideoFinished -= OnRewardedVideoWatched;
            GameEvents.ManagerKickBack -= OnKickBackManager;
            GameEvents.CurrentPlanetChanged -= OnPlanetChanged;
            GameEvents.ShipModuleStateChanged -= OnShipModuleStateChanged;
            GameEvents.PrizeWheelTriesChanged -= OnPrizeWheelTriesChanged;
            GameEvents.TreasureHuntTriesChanged -= OnTreasureHuntTriesChanged;
            GameEvents.SplitTriesChanged -= OnSplitGameTriesChanged;
            GameEvents.BankLevelChanged -= OnBankLevelChanged;
            base.OnDisable();
        }

        private void OnBankLevelChanged(int oldLevel, int newLevel ) {
            LogBankLevel(newLevel);
        }

        private void OnSplitGameTriesChanged(int oldTries, int newTries ) {
            if(newTries < oldTries ) {
                LogBreakLinesSpin(Services.GetService<ISplitGameService>().SpinCounter);
            }
        }

        private void OnPrizeWheelTriesChanged(int oldTries, int newTries ) {
            //when we spin tries count decrease, its occurs when spin
            if(newTries < oldTries ) {
                LogPrizeWheelSpin(Services.GetService<IPrizeWheelGameService>().SpinCounter);
            }
        }  
        private void OnTreasureHuntTriesChanged(int oldTries, int newTries) {
            //when we spin tries count decrease, its occurs when spin
            if(newTries < oldTries ) {
                LogTreasureHuntSpin(Services.GetService<ITreasureHuntService>().OpenCounter);
            }
        }
        

        private void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo info ) {
            if(oldState != newState ) {
                if(newState == ShipModuleState.Opened ) {
                    LogModuleOpenTime(info);
                    Services.Modules.GetModuleCounter().Match(
                        None: () => {
                            return F.None;
                        }, 
                        Some: count => {
                            LogModuleOpenCounter(count);
                            return F.Some(count);
                        });
                }
            }
        }

        private void OnPlanetChanged(PlanetInfo oldPlanet, PlanetInfo newPlanet ) {
            if (oldPlanet != null) {
                LogPlanetInterval(oldPlanet);
                if(Planets.PlanetOpenCounter > 0 ) {
                    LogPlanetOpenCounter(Planets.PlanetOpenCounter);
                }
            }
        }

        private void OnKickBackManager(double payed, bool isFirstKickBack, ManagerInfo manager) {
            if(isFirstKickBack) {
                FacebookEventUtils.LogFirstKickBackManagerEvent(manager.Id);
            }
        }

        private void OnAchievmentCompleted(IAchievment achievment) {
            LogAchievment(achievment);
        }

        private void OnRewardedVideoWatched(int totalCount) {

            if(totalCount >= 10 ) {
                if(!Player.LegacyPlayerData.rewardedVideoAchievmentStatus.achieved10) {
                    Player.LegacyPlayerData.rewardedVideoAchievmentStatus.achieved10 = true;
                    LogAchievment(new AchievmentInfo(VIDEO_10_ACHIEVMENT_ID, "Watched 10 videos"));
                    //playerData.Save();
                    return;
                }
            } 

            if(totalCount >= 100) {
                if(!Player.LegacyPlayerData.rewardedVideoAchievmentStatus.achieved100) {
                    Player.LegacyPlayerData.rewardedVideoAchievmentStatus.achieved100 = true;
                    LogAchievment(new AchievmentInfo(VIDEO_100_ACHIEVMENT_ID, "Watched 100 videos"));
                    //playerData.Save();
                    return;
                }
            }

            if(totalCount >= 1000) {
                if(!Player.LegacyPlayerData.rewardedVideoAchievmentStatus.achieved1000) {
                    Player.LegacyPlayerData.rewardedVideoAchievmentStatus.achieved1000 = true;
                    LogAchievment(new AchievmentInfo(VIDEO_1000_ACHIEVMENT_ID, "Watched 1000 videos"));
                    //playerData.Save();
                    return;
                }
            }
        }

        #region IFacebookService

        public void LogAchievment(IAchievment achievment) {
#if UNITY_EDITOR
            Services.GetService<IConsoleService>().AddOutput(
                $"Log achievment to fb {achievment.Id}:{achievment.Name}",
                ConsoleTextColor.cyan, true);
#elif UNITY_ANDROID || UNITY_IOS
            FB.LogAppEvent(AppEventName.UnlockedAchievement, 1, new Dictionary<string, object> {
                [AppEventParameterName.Description] = achievment.Name
            });
#endif
        }


        public void AddTotalAdsCount(string content, int count) {
            TotalAdCount += count;
            LogTotalAdCount(content);
        }
        #endregion

        #region SaveGameBehaviour overrides
        public override string SaveKey => "facebook_service";

        public override Type SaveType => typeof(FacebookServiceSave);

        public override void ResetFull() { IsLoaded = true; }

        public override void ResetByInvestors() { IsLoaded = true; }

        public override void ResetByPlanets() { IsLoaded = true; }

        public override void ResetByWinGame() { IsLoaded = true; }

        public override object GetSave() {
            return new FacebookServiceSave {
                totalAdCount = TotalAdCount,
                firstAdWatchedByType = _firstAdWatchedByType
            };
        }

        public override void LoadDefaults() {
            TotalAdCount = 0;
            IsLoaded = true;
            _firstAdWatchedByType.Clear();
        }

        public override void LoadSave(object obj) {
            FacebookServiceSave save = obj as FacebookServiceSave;
            if(save != null ) {
                TotalAdCount = save.totalAdCount;
                _firstAdWatchedByType.Clear();
                if (save.firstAdWatchedByType != null)
                    _firstAdWatchedByType = save.firstAdWatchedByType;
                
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        #endregion

        private void LogTotalAdCount(string contentType) {
            //print($"FacebookService::LoadTotalAdCount => {contentType} : {TotalAdCount}");

#if !UNITY_EDITOR
            var parameters = new Dictionary<string, object>();
            parameters[AppEventParameterName.ContentType] = contentType;
            FB.LogAppEvent(AppEventName.ViewedContent, TotalAdCount, parameters);
            LogSeparateVideo(contentType);
    
           // var appmetricParam = new Dictionary<string, object>();
           // appmetricParam["ContentType"] = contentType;
           // appmetricParam["Total"] = TotalAdCount;
           // AppMetrica.Instance.ReportEvent("ContentView", appmetricParam);
    
    
#endif
            var contentTypeParam = new Firebase.Analytics.Parameter("ContentType", contentType);
            var totalParam = new Firebase.Analytics.Parameter("Total", TotalAdCount);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ContentView", contentTypeParam, totalParam);
            
            if (!_firstAdWatchedByType.ContainsKey(contentType))
            {
                _firstAdWatchedByType.Add(contentType, true);
                LogFirstAdViewed(contentType);
            }
        }

        private void LogFirstAdViewed(string contentType)
        {
            //print($"FacebookService::LogFirstAdViewed => {contentType}");
#if !UNITY_EDITOR
            var parameters = new Dictionary<string, object>();
            parameters[AppEventParameterName.ContentType] = contentType;
            FB.LogAppEvent(
                "firstAdViewed",
                0,
                parameters
            );
		
           // var appmetricParam = new Dictionary<string, object>();
           // appmetricParam["ContentType"] = contentType;
           // AppMetrica.Instance.ReportEvent("firstAdViewed", appmetricParam);
#endif 
            Firebase.Analytics.FirebaseAnalytics.LogEvent("firstAdViewed", "ContentType", contentType);
            
        }
        
        
        private void LogSeparateVideo(string videoType ) {
            FB.LogAppEvent(videoType, 1, new Dictionary<string, object> { [AppEventParameterName.ContentType] = videoType });
        }

        private Dictionary<int, Action<PlanetInfo>> separatePlanetLoggers = null;

        private void InitSeparatePlanetLoggers() {
            if (separatePlanetLoggers == null || separatePlanetLoggers.Count == 0) {
                separatePlanetLoggers = new Dictionary<int, Action<PlanetInfo>>() {
                    [0] = LogEarthInterval,
                    [1] = LogMoonInterval,
                    [2] = LogMarsInterval,
                    [3] = LogAsteroidsInterval,
                    [4] = LogEuropeInterval,
                    [5] = LogTitanInterval
                };
            }
        }
        private void LogPlanetInterval(PlanetInfo planet) {
            FB.LogAppEvent("planet_interval", 1, new Dictionary<string, object> {
                ["planet_id"] = planet.Id,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            });
            InitSeparatePlanetLoggers();
            if (separatePlanetLoggers.ContainsKey(planet.Id)) {
                separatePlanetLoggers[planet.Id](planet);
            }
        }

        private void LogEarthInterval(PlanetInfo planet)
            => FB.LogAppEvent("planet_interval_earth", 1, new Dictionary<string, object> {
                ["planet_id"] = 0,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            });
        private void LogMoonInterval(PlanetInfo planet)
            => FB.LogAppEvent("planet_interval_moon", 1, new Dictionary<string, object> {
                ["planet_id"] = 1,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            });  
        private void LogMarsInterval(PlanetInfo planet)
            => FB.LogAppEvent("planet_interval_mars", 1, new Dictionary<string, object> {
                ["planet_id"] = 2,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            }); 
        private void LogAsteroidsInterval(PlanetInfo planet)
            => FB.LogAppEvent("planet_interval_asteroidbelt", 1, new Dictionary<string, object> {
                ["planet_id"] = 3,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            }); 
        private void LogEuropeInterval(PlanetInfo planet)
            => FB.LogAppEvent("planet_interval_europe", 1, new Dictionary<string, object> {
                ["planet_id"] = 4,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            }); 
        private void LogTitanInterval(PlanetInfo planet)
            => FB.LogAppEvent("planet_interval_titan", 1, new Dictionary<string, object> {
                ["planet_id"] = 5,
                ["interval"] = (planet.EndTime - planet.StartTime).ToTextMark()
            }); 
        

        private void LogPlanetOpenCounter(int counter)
            => FB.LogAppEvent("planet_open_counter", counter, new Dictionary<string, object> {});

        private void LogModuleOpenTime(ShipModuleInfo module)
            => FB.LogAppEvent("module_opened_time", 1, new Dictionary<string, object> {
                ["id"] = module.Id,
                ["time"] = (module.OpenTime - GameMode.GameStartTime).ToTextMark()
            });
        private void LogModuleOpenCounter(int count)
            => FB.LogAppEvent("module_open_counter", count, new Dictionary<string, object> {});

        private void LogPrizeWheelSpin(int count)
            => FB.LogAppEvent("prizewheel_game_spin", count, new Dictionary<string, object>());
        
        private void LogTreasureHuntSpin(int count)
            => FB.LogAppEvent("treasure_game_spin", count, new Dictionary<string, object>());

        private void LogBreakLinesSpin(int count)
            => FB.LogAppEvent("breaklines_spin", count, new Dictionary<string, object>());

        private void LogBankLevel(int level)
            => FB.LogAppEvent("bank_level", level, new Dictionary<string, object> {
                ["level"] = level
            });
    }


    public interface IFacebookService : IGameService {
        void LogAchievment(IAchievment achievment);
        void AddTotalAdsCount(string content, int count);
    }

    public class FacebookServiceSave {
        public int totalAdCount;
        public Dictionary<string, bool> firstAdWatchedByType;
    }
}