using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureHuntLoseView : MonoBehaviour
{
	public Image Chest;
	public void Show(Sprite sprite)
	{
		Chest.overrideSprite = sprite;
		gameObject.SetActive(true);
	}
}
