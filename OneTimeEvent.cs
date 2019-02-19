using System;
using Bos.UI;
using UnityEngine;

public class OneTimeEvent : LocalEvent
{
    public float PercentLoss = 0.1f;
    public double LostBalance;

    public override void EventEnter()
    {
        LostBalance = PercentLoss * Services.PlayerService.CompanyCash.Value;

        if (!IsPositive) {
            Player.RemoveCompanyCash(LostBalance);
        } else {
            Player.AddGenerationCompanyCash(LostBalance);
            Debug.Log("AddBalance from OneTimeEvent::EventEnter -> " + LostBalance);
        }
        StartCoroutine(ShowImpl());
    }

    private System.Collections.IEnumerator ShowImpl() {
        yield return new WaitUntil(() => GameObject.Find("AutoCloseBg") == null &&
            ViewService.ModalCount == 0);
        //EventView.Show(this);
        ViewService.Show(ViewType.BosEventView, new ViewData {
            UserData = new BosEventViewModel {
                EventType = this.eventType,
                Model = this
            }
        });
    }

    public override void EventExit()
    {
        // do nothing, is one time event
    }

    public override void EventStay()
    {
        // do nothing, is one time event
    }
}
