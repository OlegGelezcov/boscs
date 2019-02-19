using Bos;
using Bos.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using Bos.UI;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class InvestorView : GameBehaviour
{
    private InvestorBuyMode _mode;

    public GameObject ManagersContainer;

    public int CoinAmount = 20;

    public Text CurrentInvestorsText;
    public Text TargetInvestorsText;
    public Text AllInvestors;
    public Text RemainingTime;

    public Button ClaimButton;
    public Button ClaimButtonAd;

    public GameObject ConfirmPanel;
    public GameObject NotEnoughCoinsPopup;
    public Image ButtonImage;
    public Sprite ActiveSprite, NoActiveSprite;

    public GameObject NextCliamTime;
    public Text statusText;

    private float _frameTime = 0;
    private DateTime _now;

    private readonly UpdateTimer updateTimer = new UpdateTimer();


    public override void Awake()
    {

        _now = Services.TimeService.Now;
        updateTimer.Setup(1, (deltaTime) => {
            _now = _now.AddSeconds(1);
            UpdateView();
        }, invokeImmediatly: true);
    }

   

    private void UpdateView() {
        string securitiesText = NumberMinifier.PrettyAbbreviatedValue(Math.Floor(Services.PlayerService.Securities.Value));
        CurrentInvestorsText.text = securitiesText;

        AllInvestors.text = NumberMinifier.PrettyAbbreviatedValue(Math.Floor(Services.InvestorService.CalcInvestoreByMoney()));
        double targetInvestors = Services.InvestorService.GetSecuritiesCountFromInvestors();
        string investorString = NumberMinifier.PrettyAbbreviatedValue(Math.Floor(targetInvestors));

        ClaimButton.interactable = targetInvestors >= 1;
        ClaimButtonAd.interactable = targetInvestors >= 1;
        TargetInvestorsText.text = investorString;
        ButtonImage.sprite = targetInvestors >= 1 ? ActiveSprite : NoActiveSprite;
        var isCanClaim = ViewService.Utils.IsInvestorSellStateOk(Services.InvestorService.SellState);

        ClaimButton.gameObject.SetActive(isCanClaim);
        ClaimButtonAd.gameObject.SetActive(!isCanClaim);
        NextCliamTime.Deactivate();
        RemainingTime.Deactivate();

        if(Services.InvestorService.TriesCount <= 0 ) {
            statusText.Activate();
        } else {
            statusText.Deactivate();
        }
        Debug.Log($"target investors => {targetInvestors}".BoldItalic().Colored(ConsoleTextColor.navy));
    }


    public override void Update() {
        base.Update();
        updateTimer.Update();

    }

    public void Claim()
    {
        switch (_mode)
        {
            case InvestorBuyMode.Normal:
                InternalClaim();
                break;
            case InvestorBuyMode.Ad:

                Services.AdService.WatchAd("Investor", () =>
                {
                    InternalClaim();
                    FacebookEventUtils.LogADEvent("Investor");
                });
                break;
        }
    }

    private void InternalClaim(double multiplier = 1.0)
    {
        Services.InvestorService.SellToInvestors(multiplier);
    }

    public void BuyWithCoins()
    {
        if (Services.PlayerService.Coins < 20)
        {
            Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                UserData = 20
            });
            return;
        }

        SetCoinBuyMode();
        ConfirmPanel.SetActive(true);
    }

    public void SetNormalBuyMode()
    {
        _mode = InvestorBuyMode.Normal;
    }

    public void SetCoinBuyMode()
    {
        _mode = InvestorBuyMode.Coins;
    }

    public void SetAdBuyMode()
    {
        _mode = InvestorBuyMode.Ad;
    }

    public override void OnEnable()
    {
        _frameTime = 1;
        UpdateView();
    }
}

public enum InvestorBuyMode
{
    Normal,
    Coins,
    Ad
}
