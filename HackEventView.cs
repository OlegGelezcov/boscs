using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HackEventView : EventView
{
    public Text LostBalanceView;

    public override void Show(LocalEvent evt)
    {
        base.Show(evt);

        var hackEvt = (OneTimeEvent)evt;
        var lost = hackEvt.LostBalance;
        if (evt.IsPositive)
            LostBalanceView.text = Services.Currency.CreatePriceString(lost, false, " ");
        else
            LostBalanceView.text = Services.Currency.CreatePriceString(lost, false, " ");
    }
}
