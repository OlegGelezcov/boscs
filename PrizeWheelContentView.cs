using Bos;
using Bos.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using I2.MiniGames;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UniRx;

public class PrizeWheelContentView : GameBehaviour {

    public int CoinsPerGame = 5;
    public GameObject timerParent;
    public Text TimeDisplay;
    public Text Tries;
    public Text getFreeSpins;
    public Text Desc;
    public GameObject NormalPlayButton;
    public GameObject NoTriesPanel;
    public Button PlayGame, WatchAd;
    public PrizeWheel _Game;		
    
    public Button buyTryButton;
    private float _pauseTS;
    private bool _canPlay = true;
    private bool _wasDisabled;
    private DateTime _lastDisabled;

    public Sprite ActiveWheel, DiactiveWheel;
    public Image WheelImage;

    public RewardView RewardsView;
    public GameObject LooseObject;

    private bool IsInitialized { get; set; }
    
    public override void Start() {

        var slotsService = Services.GetService<IPrizeWheelGameService>();
        if (slotsService.NextTriesUpdateTime < Services.TimeService.UnixTimeInt) {
            slotsService.SetTries(slotsService.MaxTries);
        }

        PlayGame.SetListener(PlayNormal);
        WatchAd.SetListener(GetFreeSpins);
        
        buyTryButton.SetListener(BuyTries);

        if(!IsInitialized ) {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                timerParent.SetActive(!slotsService.HasTries);
            }).AddTo(gameObject);
            IsInitialized = true;
        }
    }


    public override void OnEnable()
    {
        GameEvents.PrizeWheelRewardClaimed += OnRewardClaimed;
        UpdateView();
    }

    public override void OnDisable()
    {
        GameEvents.PrizeWheelRewardClaimed -= OnRewardClaimed;
    }


    private void OnRewardClaimed(Reward reward)
    {
        if (reward == null)
        {
            LooseObject.SetActive(true);
        }
        else
        {
            RewardsView.Reset();
            RewardsView.Activate(reward);
        }
    }

    public void IncreaseTriesPaid() {
        var slotsService = Services.GetService<IPrizeWheelGameService>();
        slotsService.AddMaxTries(1);
        slotsService.AddTries(1);
    }

    public void BuyTries() {
        var productData = ResourceService.Products.GetProduct(8);
        if (productData != null) {
            Services.Inap.PurchaseProduct(productData);
        } else {
            UnityEngine.Debug.LogError($"not found product id {8}");
        }
    }
    
    private void FixedUpdate()
    {
        var service = Services.GetService<IPrizeWheelGameService>();
        var isNoTries = !service.HasTries;
        NoTriesPanel.SetActive(isNoTries);

        var hasTries = service.HasTries;
        if (hasTries)
        {
            WheelImage.overrideSprite = ActiveWheel;
        }
        NormalPlayButton.SetActive(hasTries);
        Desc.text = string.Format("lbl_prize_wheel_desc".GetLocalizedString(), service.MaxTries);
        if (service.NextTriesUpdateTime > Services.TimeService.UnixTimeInt) {
            var tDisplay = System.TimeSpan.FromSeconds(service.NextTriesUpdateTime - Services.TimeService.UnixTimeInt);
            var answer = $"{tDisplay.Minutes:D2}:{tDisplay.Seconds:D2}";
            TimeDisplay.text = answer;
        }
        else
        {
            if (isNoTries)
            {
                service.SetTries(service.MaxTries);
                TimeDisplay.text = "00:00";
            }
        }

        getFreeSpins.text = $"Get +{service.MaxTries}\nspins now";
    }

    public void PlayNormal()
    {
        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.PlayCasinoClicked));
        if (!_canPlay)
            return;

        _canPlay = false;
        StartCoroutine(Playing());
    }


    public void OnReadyForNextRound()
    {
        _Game.StopPlay();
        _canPlay = true;
        UpdateView();
    }

    private void UpdateView()
    {
        var service = Services.GetService<IPrizeWheelGameService>();
        WheelImage.overrideSprite = Services.GetService<IPrizeWheelGameService>().HasTries ? ActiveWheel : DiactiveWheel;
        Tries.text = service.TriesCount > 0 ? service.TriesCount.ToString() : service.MaxTries.ToString();
        timerParent.SetActive(!service.HasTries);
    }

    IEnumerator Playing()
    {
        EventSystemController.OffEventSystemForSeconds(5);
        var service = Services.GetService<IPrizeWheelGameService>();
        if (service.HasTries) {
            service.RemoveTries(1);
        }
        
        _Game.SetupGame();
        _Game.StartRound();
        yield break;
    }

    public void GetFreeSpins()
    {
        Services.AdService.WatchAd("Prize Wheel", () =>
        {
            var slotsService = Services.GetService<IPrizeWheelGameService>();
            slotsService.SetTries(slotsService.MaxTries);
            slotsService.ResetNextTriesTime();
            TimeDisplay.text = "00:00";
            FacebookEventUtils.LogADEvent("Prize Wheel");
        });
    }
}
