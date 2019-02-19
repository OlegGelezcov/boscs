using Bos;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class BalanceView : GameBehaviour
{

    public Text NumberTextView;
    public Text WordTextView;

    private float frameTime = 0;
    
    public override void Start()
    {
        //BalanceManager.Balance.Subscribe(val =>
        //{
        //    var a = BalanceManager.Currency.CreatePriceStringSeparated(val);
        //    NumberTextView.text = a[0];
        //    WordTextView.text = a.Length > 1 ? a[1].ToUpper() : string.Empty;
        //});
        OnCompanyCashChanged(new CurrencyNumber(), Services.PlayerService.CompanyCash);
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.CompanyCashChanged += OnCompanyCashChanged;
    }

    public override void OnDisable() {
        base.OnDisable();
        GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
    }

    private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) {
        NumberTextView.text = newValue.PrefixedAbbreviation();
        if(WordTextView != null ) {
            WordTextView.text = string.Empty;
        }
        //WordTextView.text = string.Empty;
    }
}
