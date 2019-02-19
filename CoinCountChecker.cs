using Bos;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CoinCountChecker : GameBehaviour
{
    public int RequiredCoins;

    private Button _btn;

    public override void Start()
    {
        _btn = GetComponent<Button>();

        //GlobalRefs.IAP.Coins.Subscribe(val => 
        //{
        //    _btn.interactable = val >= RequiredCoins; 
        //});
        OnCoinsChanged(0, Services.PlayerService.Coins);
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.CoinsChanged += OnCoinsChanged;
    }
    public override void OnDisable() {
        base.OnDisable();
        GameEvents.CoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int oldCount, int newCount) {
        _btn.interactable = (newCount >= RequiredCoins);
    }
}
