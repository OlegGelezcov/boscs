using Bos;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : GameBehaviour
{
	public override void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			PlayerPrefs.DeleteAll();
		});

	}
}
