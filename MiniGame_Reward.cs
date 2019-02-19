using I2.MiniGames;
using UnityEngine.Events;
using UnityEngine;
using Boo;
using Bos;

public class MiniGame_Reward : MonoBehaviour 
{
	public float Probability = 1;
	public Reward Reward;

	public void Execute(MiniGame game, Transform parent )
	{
		GameEvents.OnPrizeWheelRewardClaimed(Reward);

		if (Reward != null)
		{
			//Debug.Log($"Wheel Reward: {Reward.Name}" );
		}
	}
}