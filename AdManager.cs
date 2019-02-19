/*
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using Bos;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AdManager : GameBehaviour, IRewardedVideoAdListener
{
    private bool _watchAdTriggered = false;

    public GameManager GameManager;
    public IAPManager IAP;
    public BalanceManager BalanceManager;
    public GameObject NoAdsScreen;

    public float balanceRewardPercent;

    private PlayerData _pdata;



    public override void Awake()
    {
        Appodeal.disableNetwork("amazon_ads");
        Appodeal.disableNetwork("inmobi");
        Appodeal.disableNetwork("mmedia");
        Appodeal.disableNetwork("ogury");
        Appodeal.disableNetwork("tapjoy");
        Appodeal.disableNetwork("yandex");
        Appodeal.disableNetwork("chartboost");
        
        string  appKey = "025ce74922313fb7d611103b2b3064404acfd456ae82cbc2";
#if UNITY_ANDROID
        appKey = "fd7c17501ed8fc63e77339a934884d0f876469d71a987ade";
#endif

#if UNITY_IOS
        appKey = "8464001a6d509297f53727082148cf8d31ea55ac496b0530";
#endif
        
        //Appodeal.setTesting(true);
        Appodeal.disableLocationPermissionCheck();
        Appodeal.setAutoCache(Appodeal.REWARDED_VIDEO, true);
        Appodeal.initialize(appKey, Appodeal.REWARDED_VIDEO);
        Appodeal.setRewardedVideoCallbacks(this);
    }

    public override void Start()
    {  
        _pdata = BalanceManager.PlayerData;
    }



    private Action _rewardAction;
    
    public void WatchAd(string contentType, Action a)
    {
#if UNITY_EDITOR
        _rewardAction = () => {
            Services.GetService<IFacebookService>()?.AddTotalAdsCount(contentType, 1);
            a?.Invoke();
        };
        _rewardAction?.Invoke();
#else
        Debug.Log("Entered Watch Add");
        if (_watchAdTriggered)
            return;

        if (!Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
        {
            NoAdsScreen.GetComponent<ADLoadingScreen>().Fill(contentType, a);
            return;
        }

        //_rewardAction = a;
         _rewardAction = () => {
            Services.GetService<IFacebookService>()?.AddTotalAdsCount(contentType, 1);
            a?.Invoke();
        };

        Appodeal.show(Appodeal.REWARDED_VIDEO);
        _watchAdTriggered = true;
#endif
    }

    public void WatchBoostAd()
    {
#if UNITY_EDITOR
        Services.GetService<IX2ProfitService>().OnAdWatched();
        FacebookEventUtils.LogADEvent("BoostProfit");
#else
        WatchAd("BoostProfit", () =>
        {
            Services.GetService<IX2ProfitService>().OnAdWatched();
            FacebookEventUtils.LogADEvent("BoostProfit");
        });
#endif
    }

    public void onRewardedVideoLoaded(bool precache)
    {
        Debug.Log("onRewardedVideoLoaded");
    }

    public void onRewardedVideoFailedToLoad()
    {
        Debug.Log("onRewardedVideoFailedToLoad");
        _watchAdTriggered = false;
    }

    public void onRewardedVideoShown()
    {
        Debug.Log("onRewardedVideoShown");
    }

    public void onRewardedVideoFinished(double amount, string name)
    {
        Debug.Log("onRewardedVideoFinished");
        
        _rewardAction?.Invoke();
        //StatsCollector.Instance[Stats.ADS_WATCHED]++;
        _watchAdTriggered = false;
        //GameEvents.OnRewardedVideoFinished((int)StatsCollector.Instance[Stats.ADS_WATCHED]);
    }

    public void onRewardedVideoClosed(bool finished)
    {
        Debug.Log("onRewardedVideoClosed");
        _watchAdTriggered = false;
    }

    public void onRewardedVideoExpired()
    {
    }
}
*/
