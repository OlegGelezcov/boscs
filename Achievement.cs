using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Bos;

[ExecuteInEditMode]
public class Achievement : MonoBehaviour
{
    public int Id;
    public string Name;
    public string CustomText;
    public int Points;
    public Sprite Icon;

    public int TargetGeneratorId;
    public int TargetCount;

    public bool IsCompleted;

    public RewardType RewardType;
    public int RewardValue;

    public string RewardText
    {
        get
        {
            switch (RewardType)
            {
                case RewardType.SpeedUpgrade:
                    return string.Format("X.SPEED".GetLocalizedString(), RewardValue);
                case RewardType.ProfitUpgrade:
                    return string.Format("X.PROFIT".GetLocalizedString(), RewardValue);
                default:
                    return CustomText;
            }
        }
    }
}


public enum RewardType
{
    SpeedUpgrade,
    ProfitUpgrade,
    None,
    MiniGameTry,
    Lootbox,
    MiniGameRaceTry,
    MiniGameTreasureHuntTry,
    CashReward,
    CoinReward,
    SecuritiesReward,
    PlayerCashReward,
    SpecialOfferBundle,
    Custom
}

