using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ManagerInfoScreen : GameBehaviour {

    public GameManager GameManager;
    public GameUI uiManager;
    public Text RemainingTime;
    public Image Back;
    public Text Title, Desc, CostText;
    
    public Text EfficiencyText, KickBackText, CashOnHandText, LifeTimeEarningText, KickBacksPayed;
    
    public Button KickBackButton;
    public Button KickBackButtonAd;
    public Button HireManager;

    public GameObject NextKickBackTime;

    public Button mechanicButton;


    private PlayerData _pData;

    //private ManagerStat _manager;

    public static int ManagerChoosen;
    
    public override void Awake()
    {
        KickBackButton.onClick.AddListener(InternalKickBack);
        KickBackButtonAd.onClick.AddListener(() =>
        {
            Services.AdService.WatchAd("KickBack", InternalKickBack);
            FacebookEventUtils.LogADEvent("KickBack");
        });
    }

    private ManagerInfo manager;

    
    private float _frameTime = 0;
    private DateTime _now;

    

    public void Fill(int managerId)
    {       
        ManagerChoosen = managerId;
        manager = Services.GetService<IManagerService>().GetManager(managerId); //BalanceManager.PlayerData.managers.FirstOrDefault(val => val.Id == managerId);
        Title.text = manager.Name;
        Desc.text = manager.Description;
        
        EfficiencyText.text = $"{(int) (manager.Efficiency(Services) * 100)}%";
        KickBackText.text = $"{(int) (manager.MaxRollback * 100)}%";
        var spriteName = "kickback_" + managerId;
        Back.sprite = SpriteDB.SpriteRefs.FirstOrDefault(val => val.Key == spriteName).Value;
        
        //uiManager.ShowManagerInfo();

        HireManager.onClick.RemoveAllListeners();
        HireManager.onClick.AddListener(() =>
        {
            Services.ManagerService.HireManager(manager.Id);
            _frameTime = 1;
        });
    }

    public override void Update()
    {
        if (manager != null) {
            if (_frameTime < 1) {
                _frameTime += Time.deltaTime;
                return;
            }
            _frameTime = 0;
            _now = _now.AddSeconds(1);

            var isCanClaim = manager.NextKickBackTime < Services.TimeService.Now;

            var isHire = Services.ManagerService.IsHired(manager.Id);

            KickBackButton.gameObject.SetActive(isCanClaim && isHire);
            KickBackButtonAd.gameObject.SetActive(!isCanClaim && isHire);

            KickBackButton.interactable = manager.CashOnHand > 0;
            KickBackButtonAd.interactable = manager.CashOnHand > 0;

            HireManager.gameObject.SetActive(!isHire);
            HireManager.interactable = Services.PlayerService.CompanyCash.Value >= manager.Cost;
            CostText.text = Services.PlayerService.CompanyCash.Value >= manager.Cost
                ? "HIRE NOW"
                : BosUtils.GetCurrencyStringSimple(new CurrencyNumber(manager.Cost));

            Back.color = isHire ? Color.white : new Color(1, 1, 1, 0.5f);

            var diff = isCanClaim ? TimeSpan.Zero : manager.NextKickBackTime - _now;
            NextKickBackTime.SetActive(diff.TotalSeconds > 0 && isHire);
            RemainingTime.text = diff.TotalSeconds < 0 ? "00:00:00" : $"{diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";

            UpdateTexts();
        }
    }

    private void UpdateTexts()
    {
        CashOnHandText.text = Services.Currency.CreatePriceString(manager.CashOnHand, false, " ");
        LifeTimeEarningText.text = Services.Currency.CreatePriceString(manager.CashLifeTime, false, " ");
        KickBacksPayed.text = Services.Currency.CreatePriceString(manager.KickBacksPayed, false, " ");
        
    }

    private void InternalKickBack()
    {
        //uiManager.PlaySlotManagerGame();
    }

    public override void OnEnable()
    {
        
        _now = Services.TimeService.Now;
        _frameTime = 1;

        mechanicButton?.SetListener(() => {
            //GameServices.Instance.ViewService.Show(Bos.UI.ViewType.MechanicView, ManagerChoosen);
        });
    }
}