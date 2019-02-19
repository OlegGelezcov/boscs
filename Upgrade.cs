using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public int GeneratorIdToUpgrade;
    public double[] Cost;

    public string[] Names;
    public string OverflowName;

    public UpgradeType UpgradeType;
    public CostType CostType = CostType.Balance;
    public int ProfitMultiplier;
    public int TimeMultiplier;

    //public double ExponentBase = 1.14;

    public double CalculateCost(int count)
    {
        if (count > Cost.Length)
            return Cost[Cost.Length - 1];

        return Cost[count];
        //switch (CostType)
        //{
        //    case CostType.Balance:
        //        return Cost * Math.Pow(ExponentBase, count);
        //    case CostType.Investors:
        //    case CostType.Coins:
        //        return Cost;
        //    default:
        //        break;
        //}

        //return 0;
    }
}

public enum UpgradeType
{
    Profit = 0,
    Time = 1,
    InvestorEffectiveness = 2,
    FreeGenerators = 3,
    FutureBalance,
    AdvertisementBoost,
    Reward,
    Micromanager,
    Enhance
}

public enum CostType
{
    Balance,
    Investors,
    Coins
}
