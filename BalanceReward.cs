using System;
using UnityEngine;

public class BalanceReward : Reward
{
    public double BalanceGain;

    public override void Apply(AllManagers bman)
    {
        Player.AddGenerationCompanyCash(BalanceGain);
        Debug.Log("AddBalance from BalanceReward::Apply -> " + BalanceGain);

        StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
    }
}
