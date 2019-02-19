using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager2 : RewardManagerBase
{
    private Reward[] _rewardTable;
    public RewardWithChance[] Rewards;
   
    private void Start()
    {
        var q = new List<Reward>();
        NormalizeWieght();
        foreach (var x in Rewards)
        {
            if (x.Chance < 0)
                continue;

            for (int d = 0; d < x.Chance; d++)
            {
                q.Add(x.Reward);
            }
        }
        _rewardTable = q.ToArray();
        RandomizeArray(_rewardTable);
    }


    public override Reward PickReward()
    {
        var d = UnityEngine.Random.Range(0, _rewardTable.Length);
        return _rewardTable[d];
    }


    private void NormalizeWieght()
    {
        if (Rewards.Any(val => val.Chance < 1))
        {
            foreach (var reward in Rewards)
            {
                reward.Chance *= 10;
            }
        }
    }

    public override Reward CreateReward()
    {
        var reward = PickReward();
        return reward;
    }

    private void RandomizeArray(object[] arr)
    {
        for (var i = arr.Length - 1; i > 0; i--)
        {
            var r = UnityEngine.Random.Range(0, i);
            var tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }
}

[Serializable]
public class RewardWithChance
{
    public float Chance;
    public Reward Reward;
}
