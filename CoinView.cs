using Bos;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CoinView : ReactiveMonoBehaivor
{
    //public IAPManager IAPManager;

    public Text CoinsTextView;


    private float frameTime = 0;

    private void Start()
    {
        //Collerctor.add = IAPManager.Coins.Subscribe(val =>
        //{
        //    CoinsTextView.text = val.ToString();
        //});
        OnCoinsChanged(0, GameServices.Instance.PlayerService.Coins);
    }

    private void OnEnable() {
        GameEvents.CoinsChanged += OnCoinsChanged;
    }

    private void OnDisable() {
        GameEvents.CoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int oldCount, int newCount)
        => CoinsTextView.text = newCount.ToString();
}
