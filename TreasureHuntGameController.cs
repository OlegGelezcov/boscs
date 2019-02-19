using System;
using System.Collections;
using Bos;
using UnityEngine;

public class TreasureHuntGameController : GameBehaviour {

	public RewardManager2 RewardManager;
	public TreasureHuntLoseView LoseView;
	public RewardViewTreasureHunt WinView;
	
	private bool _gameEnd;
	public float WinChance;

	private int _openedChest;
	private int _winCount;
	
	public void Setup()
	{
		_openedChest = 0;
		_winCount = 0;
	}


	public void OpenChest(TreasureHuntChestView chest, Action finishAction)
	{
		StartCoroutine(OpenChestInner(chest, finishAction));
	}

	public bool HasAnyReward()
	{
		return _winCount < 3;
	}


	private bool _inProcess = false;
	private IEnumerator OpenChestInner(TreasureHuntChestView chest, Action finishAction)
	{
		if (_inProcess) yield break;
		_inProcess = true;
		
		var winner = false;
		
		if (_winCount >= 3)
		{
			winner = false;
			
		} else if (_openedChest <= 3)
		{
			winner = UnityEngine.Random.Range(0, 1f) < WinChance;
		}
		else
		{
			winner = true;
		}

		chest.SetAnim(winner ? TreasureHuntAnimType.OpenReward : TreasureHuntAnimType.OpenEmpty);
		
		yield return new WaitForSeconds(0.8f);
		Services.SoundService.PlayOneShot(SoundName.Poof);
		yield return new WaitForSeconds(0.4f);
		
		if (winner)
		{
			var reward = RewardManager.CreateReward();
			WinView.Reset();
			WinView.Activate(reward, chest.FullIcon.sprite);
			//Debug.Log($"TreasureHunt Reward {reward.name}");
			_winCount++;
		}
		else
		{
			LoseView.Show(chest.EmptyIcon.sprite);
		}

		_openedChest++;
		_inProcess = false;
		
		yield return new WaitForSeconds(1);
		finishAction.Invoke();
	}
}
