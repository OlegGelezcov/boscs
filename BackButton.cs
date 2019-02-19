using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
	private Button _button;

	private void Awake()
	{
		_button = GetComponent<Button>();
	}

	void Update () {

#if !UNITY_EDITOR
		if (Input.GetKeyUp(KeyCode.Escape))
#else
		if (Input.GetKeyUp(KeyCode.Backspace))
#endif
		{
			if (_button != null)
			{
				_button.onClick.Invoke();
			}
		}

	}
}
