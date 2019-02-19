using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AnimationWindow : MonoBehaviour
{

	private Animator anim;
	private CanvasGroup canvas;

	private bool _init; 
	private void Awake()
	{
		anim = GetComponent<Animator>();
		canvas = GetComponent<CanvasGroup>();
		if (anim != null && canvas != null)
		{
			_init = true;
		}
	}

	private void OnEnable()
	{
		if (!_init) return;
		canvas.alpha = 0;
		StartCoroutine(AnimationEnable());
	}

	private IEnumerator AnimationEnable()
	{
		anim.SetTrigger("EnterTrigger");
		yield break;
	}

	private IEnumerator AnimationDisable()
	{
		anim.SetTrigger("ExitTrigger");
		yield break;
	}

	private void OnDisable()
	{
		if (!_init) return;
		canvas.alpha = 0;
	}
}
