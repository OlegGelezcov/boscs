namespace Bos {
    using AppodealAds.Unity.Api;
    using AppodealAds.Unity.Common;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;

    public class AdService : SaveableGameBehaviour, IRewardedVideoAdListener, IAdService {

        private Action rewardAction = null;
        private bool isAdWaiting = false;

        private readonly Subject<bool> videoCompletedSubject = new Subject<bool>();


        #region IAdService
        public void Setup(object obj) {
            StartCoroutine(SetupImpl());
        }

        private IEnumerator SetupImpl()
        {
            yield return new WaitUntil(() => Services.ResourceService.IsLoaded);

            videoCompletedSubject.AsObservable().SubscribeOnMainThread().Subscribe(_ => {
                rewardAction?.Invoke();
                TotalAdsWatched++;
                isAdWaiting = false;
                GameEvents.OnRewardedVideoFinished(TotalAdsWatched);
            }).AddTo(gameObject);

            Appodeal.disableNetwork("amazon_ads");
            Appodeal.disableNetwork("inmobi");
            Appodeal.disableNetwork("mmedia");
            Appodeal.disableNetwork("ogury");
            Appodeal.disableNetwork("tapjoy");
            Appodeal.disableNetwork("yandex");
            Appodeal.disableNetwork("chartboost");

            string appKey = Services.ResourceService.Profiles.activeProfile.Appodeal;//"025ce74922313fb7d611103b2b3064404acfd456ae82cbc2";
            UnityEngine.Debug.Log($"appodeal api key: {appKey}".Attrib(color: "y", bold: true, italic: true));

            //Appodeal.setTesting(true);
            Appodeal.disableLocationPermissionCheck();
            Appodeal.setAutoCache(Appodeal.REWARDED_VIDEO, true);
            Appodeal.initialize(appKey, Appodeal.REWARDED_VIDEO);
            Appodeal.setRewardedVideoCallbacks(this);
        }

        public void UpdateResume(bool pause) {

        }

        public int TotalAdsWatched { get; private set; }

        public void WatchX2Ad() {
#if UNITY_EDITOR
            Services.GetService<IX2ProfitService>().SetNewProfit();
            FacebookEventUtils.LogADEvent("BoostProfit");
#else
        WatchAd("BoostProfit", () =>
        {
            Services.GetService<IX2ProfitService>().SetNewProfit();
            FacebookEventUtils.LogADEvent("BoostProfit");
        });
#endif
        }

        public void WatchAd(string contentType, Action action ) {


#if UNITY_EDITOR
            rewardAction = () => {
                Services.GetService<IFacebookService>()?.AddTotalAdsCount(contentType, 1);
                action?.Invoke();
            };
            rewardAction.Invoke();
#else
            if(isAdWaiting) {
                return;
            }
            if(!Appodeal.isLoaded(Appodeal.REWARDED_VIDEO)) {
                WaitAdData waitData = new WaitAdData {
                    ContentType = contentType,
                    Action = action
                };
                ViewService.Show(ViewType.WaitAdView, new ViewData {
                    UserData = waitData,
                    ViewDepth = ViewService.NextViewDepth
                });
                return;
            }
            rewardAction = () => {
                Services.GetService<IFacebookService>()?.AddTotalAdsCount(contentType, 1);
                action?.Invoke();
            };
            GameEvents.AdWillBePlayedSubject.OnNext(true);
            Appodeal.show(Appodeal.REWARDED_VIDEO);
            isAdWaiting = true;
#endif
        }
        #endregion

        #region IRewardedVideoAdListener
        public void onRewardedVideoClosed(bool finished) {
            isAdWaiting = false;
            GameEvents.AdWillBePlayedSubject.OnNext(false);
        }

        public void onRewardedVideoExpired() {

        }

        public void onRewardedVideoFailedToLoad() {
            isAdWaiting = false;
            GameEvents.AdWillBePlayedSubject.OnNext(false);
        }

        public void onRewardedVideoFinished(double amount, string name) {

            videoCompletedSubject.OnNext(true);
            GameEvents.AdWillBePlayedSubject.OnNext(false);
            /*
            try {

            } catch(System.Exception exception ) {
                UnityEngine.Debug.Log(exception.StackTrace);
                UnityEngine.Debug.LogError(exception.Message);
            }*/
        }

        public void onRewardedVideoLoaded(bool precache) {
            
        }

        public void onRewardedVideoShown() {
            
        }


#endregion

#region SaveableGameBehaviour

        public override string SaveKey => "ad_service";

        public override Type SaveType => typeof(AdServiceSave);

        public override object GetSave() {
            return new AdServiceSave {
                totalAdWatched = TotalAdsWatched
            };
        }

        public override void LoadDefaults() {
            TotalAdsWatched = 0;
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            AdServiceSave save = obj as AdServiceSave;
            if(save != null ) {
                TotalAdsWatched = save.totalAdWatched;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetByWinGame() {
            IsLoaded = true;
        }

        public override void ResetFull() {
            IsLoaded = true;
        }
#endregion
    }


    public interface IAdService : IGameService {
        int TotalAdsWatched { get; }
        void WatchAd(string contentType, Action action);
        void WatchX2Ad();
        bool IsLoaded { get; }
    }

    public class AdServiceSave {
        public int totalAdWatched;
    }
}