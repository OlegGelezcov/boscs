using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureHuntChestView : MonoBehaviour
{
	public int id;
	public Button ChestButton;
	public Animator Animator;

	public Image FullIcon;
	public Image EmptyIcon;
	
	public void SetAnim(TreasureHuntAnimType type)
	{
		switch (type)
		{
			case TreasureHuntAnimType.Gray: Animator.SetTrigger("Gray");
				break;
			case TreasureHuntAnimType.Lock:  Animator.SetTrigger("Available");
				break;
			case TreasureHuntAnimType.OpenEmpty: Animator.SetTrigger("Empty");
				break;
			case TreasureHuntAnimType.OpenReward: Animator.SetTrigger("Full");
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}
	}

}

public enum TreasureHuntAnimType
{
	Gray,
	Lock,
	OpenEmpty,
	OpenReward
}
