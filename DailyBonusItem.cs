using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DailyBonusItem : MonoBehaviour
{
    //public event EventHandler Claim;

    [Header("Data")]
    public string DayName;
    public DailyBonusStatus Status;
    public Sprite ButtonBG;
    public int RewardCount = 3;

    [Header("UI Elements")]
    public Text DayTitle;
    public Text StatusText;
    public Image ButtonImage;
    public Image DayBG;
    public Text NoItemsLabel;
    public GameObject Claimed;

    [Header("Colors")]
    public Color ClaimedColor;
    public Color AvailableColor;
    public Color FutureColor;
    public float OpacityForFuture = 0.25f;

    private void FixedUpdate()
    {
        DayTitle.text = DayName;
        ButtonImage.sprite = ButtonBG;
        if (RewardCount == 1)
            NoItemsLabel.text = $"{RewardCount} Item";
        else
            NoItemsLabel.text = $"{RewardCount} Items";

        switch (Status)
        {
            case DailyBonusStatus.Claimed:
                StatusText.text = "Claimed";
                StatusText.color = ClaimedColor;
                Claimed.SetActive(true);

                ButtonImage.color = new Color(1, 1, 1, 1);
                DayBG.color = new Color(1, 1, 1, 1);
                DayTitle.color = new Color(DayTitle.color.r, DayTitle.color.g, DayTitle.color.b, 1);
                break;
            case DailyBonusStatus.Available:
                StatusText.text = "Claim today";
                StatusText.color = AvailableColor;


                DayTitle.color = new Color(DayTitle.color.r, DayTitle.color.g, DayTitle.color.b, 1);
                ButtonImage.color = new Color(1, 1, 1, 1);
                DayBG.color = new Color(1, 1, 1, 1);
                Claimed.SetActive(false);
                break;
            case DailyBonusStatus.Future:
                StatusText.text = "Future";
                StatusText.color = FutureColor;

                DayTitle.color = new Color(DayTitle.color.r, DayTitle.color.g, DayTitle.color.b, OpacityForFuture);
                ButtonImage.color = new Color(1, 1, 1, OpacityForFuture);
                DayBG.color = new Color(1, 1, 1, OpacityForFuture);
                Claimed.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void Click()
    {
        //Claim?.Invoke(this, EventArgs.Empty);
        GameEvents.OnDailyBonusClaimed(this);
        Analytics.CustomEvent(AnalyticsStrings.DAILY_BONUS_COLLECTED);

    }
}

public enum DailyBonusStatus
{
    Claimed,
    Available,
    Future
}