using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour {
	
	public Text text;
	float deltaTime;
	
	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		var fps = 1.0f / deltaTime;
		text.text = fps.ToString();
	}
}