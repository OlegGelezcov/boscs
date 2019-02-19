using UnityEngine.UI;

public class StockMarketEventView : EventView
{
    public Text LostBalanceView;


    public override void Show(LocalEvent evt)
    {
        base.Show(evt);

        var hackEvt = (OneTimeEvent)evt;
        var lost = hackEvt.LostBalance;
        LostBalanceView.text = Services.Currency.CreatePriceString(lost, false, " ");
    }
}
