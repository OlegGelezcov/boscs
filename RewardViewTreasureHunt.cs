using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;

public class RewardViewTreasureHunt : RewardView
{
	public Image ChestIcon;
	public GameObject Particle;
	public void Activate(Reward reward, Sprite sprite)
	{
		ChestIcon.overrideSprite = sprite;
		base.Activate(reward);
		
		var isBalanceReward = reward is LifetimeBalanceReward;
		Particle.SetActive(isBalanceReward);
		if (isBalanceReward)
		{
			var balanceReward = reward as LifetimeBalanceReward;
			Name.text = BosUtils.GetCurrencyString(new CurrencyNumber(balanceReward.Result()), "", "#FFDF5F");
		}
	}
}
