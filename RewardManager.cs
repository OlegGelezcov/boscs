using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : RewardManagerBase
{
    [Header("Cateories SORTED by weight")]
    public RewardCategory[] Categories;

    public override Reward PickReward()
    {
        var len = Categories.Length;

        var categ = Categories[Random.Range(0, len)];

        len = categ.Rewards.Length;

        return categ.Rewards[Random.Range(0, len)];
    }

    public override Reward CreateReward()
    {
        var reward = PickReward();
        return reward;
    }
}

public abstract class RewardManagerBase : MonoBehaviour
{
    public abstract Reward PickReward();
    public abstract Reward CreateReward();
}
