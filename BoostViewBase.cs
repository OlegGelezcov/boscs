using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;

public abstract class BoostViewBase : GameBehaviour
{
	public CanvasGroup CanvasGroup;
	public Button Button;

	protected void SetState(bool active)
	{
		CanvasGroup.alpha = active ? 1 : 0.5f;
	}

	public void Fill(GeneratorInfo generatorInfo)
	{
		FillInner(generatorInfo);
		SetState(CheckState(generatorInfo));
		Button.onClick.RemoveAllListeners();
		Button.onClick.AddListener(() => Invoke(generatorInfo));
	}

	protected abstract void FillInner(GeneratorInfo generatorInfo);
	protected abstract void Invoke(GeneratorInfo generatorInfo);
	protected abstract bool CheckState(GeneratorInfo generatorInfo);
}
