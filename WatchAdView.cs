using Bos;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatchAdView : GameBehaviour
{
    public Text RemainingAdsLabel;
    public Text RemainingTime;
    public Button WatchAddButon;
    public Text boostAllprofits;
    public Image watchButtonImage;

    private readonly UpdateTimer updateTimer = new UpdateTimer();

    public override void Start() {
        base.Start();
        var x2Service = Services.X2ProfitService;

        updateTimer.Setup(.5f, dt => {
            int adsRemaining = x2Service.FreeSlotsCount;
            RemainingAdsLabel.text = adsRemaining.ToString();
            int interval = x2Service.AvailableAfterInterval; //x2Service.ResetTime - TimeService.UnixTimeInt;
            if(interval < 0 ) { interval = 0; }
            UpdateWatchButtonState(adsRemaining);
            if(interval == 0) {
                RemainingTime.text = "00:00:00";
            } else {
                TimeSpan ts = TimeSpan.FromSeconds(interval);
                RemainingTime.text = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
            boostAllprofits.text = "AD.BOOST.DESC".GetLocalizedString();
        }, true);
        WatchAddButon.SetListener(() => {
            Services.AdService.WatchX2Ad();
        });
        int ads = x2Service.FreeSlotsCount;
        UpdateWatchButtonState(ads);
        UpdateInteractability(WatchAddButon.interactable);
    }

    private void UpdateWatchButtonState(int adsRemaining) {
        bool oldInteractable = WatchAddButon.interactable;
        WatchAddButon.interactable = (adsRemaining > 0);
        if (WatchAddButon.interactable != oldInteractable) {
            UpdateInteractability(WatchAddButon.interactable);
        }
    }

    private void UpdateInteractability(bool interactable) {
        if(interactable) {
            watchButtonImage.material.SetFloat("_Enabled", 0);
        } else {
            watchButtonImage.material.SetFloat("_Enabled", 1);
        }
    }

    public override void Update()
    {
        updateTimer.Update();
    }
}
