using System;
using UnityEngine;

public class LifetimeBalanceReward : Reward
{
    public double PercentGain;

    public double Result()
    {
        var bonus = PercentGain * Services.PlayerService.CompanyCash.Value;
        if (bonus < 20) bonus = 20;
        return bonus;
    }

    public override void Apply(AllManagers bman)
    {
        Player.AddGenerationCompanyCash(Result());
        //Debug.Log("AddBalance from LifetimeBalanceReward::Apply -> " + PercentGain * Services.PlayerService.CompanyCash.Value);
        StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
    }
}
