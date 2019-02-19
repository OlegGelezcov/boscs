using System;
using System.Collections;
using System.Collections.Generic;
using Bos;
using Bos.Debug;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeBackView : GameBehaviour
{
    public Text OfflineTimeTextView;
    public Text OfflineBalanceTextView;
    public Text OfflineBalanceWordTextView;
    public Text DoubleBalanceTextView;
    public Text DoubleBalanceWordTextView;

    public GameObject Content;

    public Button continueButton;
    public Button watchAdButton;

    public float MinimumDisplayTime;
    private IGameService _gameSvc;


    public override void OnEnable() {
        base.OnEnable();
        GameEvents.Resume += OnResume;
    }

    public override void OnDisable() {
        GameEvents.Resume -= OnResume;
        base.OnDisable();
    }

    public override void Start()
    {
        continueButton.SetListener(Continue);
        watchAdButton.SetListener(WatchAd);

        StartCoroutine(ProcWelcomeBack());
    }


    private IEnumerator ProcWelcomeBack(bool fromPause = false)
    {
        Content.SetActive(false);
        yield return new WaitUntil(() => Services.GenerationService.IsResumed);
        TimeSpan sleepInterval = System.TimeSpan.FromSeconds(Services.SleepService.SleepInterval);
        if(sleepInterval < System.TimeSpan.FromSeconds(MinimumDisplayTime)) {
            Content.Deactivate();
            yield break;
        }
        Content.Activate();
        var balanceStrings = Services.Currency.CreatePriceStringSeparated(Services.GenerationService.TotalOfflineBalance);
        var doubleBalanceStrings = Services.Currency.CreatePriceStringSeparated(Services.GenerationService.TotalOfflineBalance * 2);

        OfflineBalanceTextView.text = balanceStrings[0];
        if (balanceStrings.Length > 1)
            OfflineBalanceWordTextView.text = balanceStrings[1];
        else
            OfflineBalanceWordTextView.text = string.Empty;

        DoubleBalanceTextView.text = doubleBalanceStrings[0];

        if (doubleBalanceStrings.Length > 1)
            DoubleBalanceWordTextView.text = doubleBalanceStrings[1];
        else
            DoubleBalanceWordTextView.text = string.Empty;

        OfflineTimeTextView.text = $"{(int)sleepInterval.TotalHours:D2}:{sleepInterval.Minutes:D2}:{sleepInterval.Seconds:D2}";

    }
    
    public void Continue()
    {
        Content.SetActive(false);
    }

    private double _OfflineBalanceForAD = 0; // cause watch ad paused game -_-
    public void WatchAd()
    {
        _OfflineBalanceForAD = Services.GenerationService.TotalOfflineBalance;

        Services.AdService.WatchAd("WellcomeBack", () =>
           {
               Player.AddGenerationCompanyCash(_OfflineBalanceForAD);
               Debug.Log("AddBalance from SimpleGeneratorView::WatchAd::BalanceManager.AdManager.WatchAd -> " + _OfflineBalanceForAD);
               Content.SetActive(false);
               FacebookEventUtils.LogADEvent("WellcomeBack");
           });
    }

    private void OnResume()
    {
        StopAllCoroutines();
        StartCoroutine(ProcWelcomeBack(true));
        Debug.Log($"ON RESUME ====!!!====".Colored(ConsoleTextColor.navy));
    }

}
