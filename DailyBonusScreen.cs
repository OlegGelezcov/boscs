using Bos;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonusScreen : GameBehaviour
{
    public Text TimerLabel;
    public GameObject TimeRemainingPanel;

    public override void Start()
    {
    }

    public override void OnEnable() {
        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.DailyBonusOpened));
    }

    public override void Update() {
        base.Update();
        var now = DateTime.Now;

        var timeLeft = Player.LegacyPlayerData.CurrentOfferExpires - now;
        TimerLabel.text = timeLeft.ToString(@"hh\:mm\:ss");

        if (Player.LegacyPlayerData.DailyBonusGathered) {
            TimeRemainingPanel.SetActive(false);
            enabled = false;
        }
    }
}
