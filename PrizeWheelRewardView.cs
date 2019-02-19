using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;

public class PrizeWheelRewardView : RewardView
{
	public GameObject Particle;
	public override void Activate(Reward reward)
	{
		base.Activate(reward);

		var isBalanceReward = reward is LifetimeBalanceReward;
		Particle.SetActive(isBalanceReward);
		if (isBalanceReward)
		{
			var balanceReward = reward as LifetimeBalanceReward;
			Name.text = BosUtils.GetCurrencyString(new CurrencyNumber(balanceReward.Result()), "", "#FFDF5F");
		}
	}


    public override void OnDisable() {
        base.OnDisable();
        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.WheelCompleted));
    }
}
