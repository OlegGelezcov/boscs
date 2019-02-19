using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Bos;
using Bos.UI;
using UnityEngine.SceneManagement;

public class BreakLinerContentView : GameBehaviour {

    public GameObject timerParent;
    public Text TimeDisplay;
    public Text triesCountText;
    public Text getFreeSpins;
    public Button PlayGame, WatchAd;
    public Button buyCoinsButton;
    public Button buyTryButton;
    public Text descriptionText;

    public Text currentCoinBySplt;
    public Text currentCashBySplt;
    public Text nextCoinBySplt;
    public Text nextCashBySplt;
    public Text upgradeCost;
    public GameObject upgradeContent;
    public Button upgradeButton;

    private float _pauseTS;
    private bool _canPlay = true;
    private bool _wasDisabled;
    private DateTime _lastDisabled;    
    
    private readonly UpdateTimer updateTimer = new UpdateTimer();


    public override void OnEnable() {
        base.OnEnable();
        descriptionText.text = string.Format("lbl_break_liner_desc".GetLocalizedString(), Services.SplitService.MaxTries);
        GameEvents.SplitTriesChanged += OnSplitTriesChanged;
        GameEvents.SplitMaxTriesChanged += OnSplitMaxTriesChanged;
        GameEvents.SplitLevelChanged += OnLevelChanged;
        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.BreakLinesOpened));
        UpdateControls();
        UpdateRewardView();
    }

    public override void OnDisable() {
        GameEvents.SplitTriesChanged -= OnSplitTriesChanged;
        GameEvents.SplitMaxTriesChanged -= OnSplitMaxTriesChanged;
        GameEvents.SplitLevelChanged -= OnLevelChanged;
        base.OnDisable();
    }

    private void OnSplitTriesChanged(int oldCount, int newCount ) {
        UpdateControls();
    }

    private void OnSplitMaxTriesChanged(int oldCount, int newCount ) {
        UpdateControls();
    }


    private void OnApplicationFocus(bool hasFocus)
    {
        if(hasFocus)
            UpdateControls();
    }

    private void OnLevelChanged(int oldLevel, int newLevel)
    {
        UpdateRewardView();
    }

    private void UpdateRewardView()
    {
        if (Services.SplitService.IsMaxLevel())
        {
            upgradeContent.SetActive(false);
        }
        else
        {
            upgradeContent.SetActive(true);
            var nextLevel = Services.SplitService.GetLevel() + 1;
            var nextUpgradeData = ResourceService.RocketUpgradeRepository.GetUpgrade(nextLevel);
            nextCoinBySplt.text = $"+{nextUpgradeData.coin}";
            nextCashBySplt.text = $"+{(int)(nextUpgradeData.cash * 100)}%";
            upgradeCost.text = $"{nextUpgradeData.cost}";
        }
        var currentUpgrade = Services.SplitService.GetCurrentUpgrade();
        currentCoinBySplt.text = $"+{currentUpgrade.coin}";
        currentCashBySplt.text =  $"+{(int)(currentUpgrade.cash * 100)}%";
       
    }

    public override void Start()
    {
        PlayGame.SetListener(PlayNormalF);

        WatchAd.SetListener(GetFreeSpins);
        buyTryButton.SetListener(BuyTries);
        
        buyCoinsButton.SetListener(() => {
            Services.SoundService.PlayOneShot(SoundName.buyGenerator);
            Services.ViewService.Show(ViewType.UpgradesView, new ViewData {UserData = new UpgradeViewData {  TabName = UpgradeTabName.Shop, StoreSection = StoreItemSection.Coins } });
        });
        
        upgradeButton.SetListener(() =>
        {
            var isMax = Services.SplitService.IsMaxLevel();
            if (!isMax)
            {
                var nextLevel = Services.SplitService.GetLevel() + 1;
                var nextUpgradeData = ResourceService.RocketUpgradeRepository.GetUpgrade(nextLevel);
                if (Services.PlayerService.IsEnoughCoins(nextUpgradeData.cost))
                {
                    Services.SoundService.PlayOneShot(SoundName.buyGenerator);
                    Services.SplitService.UpgradeLevel();
                }
                else
                {
                    Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                        UserData = nextUpgradeData.cost
                    });
                }
            }
        });

        UpdateControls();

        updateTimer.Setup(1, (dt) => {
            var splitService = Services.SplitService;
            var isNoTries = !splitService.HasTries;

            if (splitService.NextTriesUpdateTime > Services.TimeService.UnixTimeInt) {
                var tDisplay =
                    System.TimeSpan.FromSeconds(splitService.NextTriesUpdateTime - Services.TimeService.UnixTimeInt);
                var answer = $"{tDisplay.Minutes:D2}:{tDisplay.Seconds:D2}";
                TimeDisplay.text = answer;
            } else {
                if (isNoTries) {
                    Services.SplitService.SetTries(Services.SplitService.MaxTries);
                    TimeDisplay.text = "00:00";
                }
            }
        }, true);
    }

    public void BuyTries() {
        var productData = ResourceService.Products.GetProduct(18);
        if (productData != null) {
            Services.Inap.PurchaseProduct(productData);
        } else {
            UnityEngine.Debug.LogError($"not found product id {18}");
        }
    }
    
    public void IncreaseTriesPaid()
    {
        Services.SplitService.AddMaxTries(1);
        Services.SplitService.AddTries(1);
    }

    private void UpdateControls() {
        var splitService = Services.SplitService;
        var isNoTries = !splitService.HasTries;
        var hasTries = splitService.HasTries;

        WatchAd.gameObject.SetActive(isNoTries);
        PlayGame.gameObject.SetActive(hasTries);

        triesCountText.text = splitService.TriesCount.ToString();
        triesCountText.text = splitService.TriesCount > 0 ? splitService.TriesCount.ToString() : splitService.MaxTries.ToString();
        getFreeSpins.text = string.Format(Services.ResourceService.Localization.GetString("btn_get_fmt_tries"), splitService.MaxTries);
        timerParent.SetActive(isNoTries);
    }

    public override void Update() {
        updateTimer.Update();
    }

   
    
    public void PlayNormalF()
    {
        if (!_canPlay)
            return;

        _canPlay = false;

        PlayGame.interactable = false;
        Services.ViewService.Show(Bos.UI.ViewType.LoadingView, new ViewData {
            UserData = new LoadSceneData {
                BuildIndex = 6,
                Mode = LoadSceneMode.Additive,
                LoadAction = () => {
                    
                    Services.SplitService.RemoveTries(1);                  
                    _canPlay = true;
                    Debug.Log("TEST!");
                    var splitLinerUI = FindObjectOfType<SplitLinerUI>();
                    splitLinerUI?.SetupWithDelay(Services.PlayerService.CompanyCash.Value, 0.5f);
                    PlayGame.interactable = true;
                }
            }
        });
    }


    public void GetFreeSpins()
    {
        Services.AdService.WatchAd("LineBreaker", () =>
        {
            Services.SplitService.SetTries(Services.SplitService.MaxTries);
            Services.SplitService.ResetNextTriesTime();
            TimeDisplay.text = "00:00";
            FacebookEventUtils.LogADEvent("LineBreaker");
        });
    }

    
}
