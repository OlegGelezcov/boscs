using Bos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstInvestorScreen : GameBehaviour
{
	public Button Continue;
	public override void Awake()
	{
		Continue.onClick.RemoveAllListeners();
		Continue.onClick.AddListener(() =>
		{
            Services.ViewService.ShowDelayed(Bos.UI.ViewType.InvestorsView, Bos.UI.BosUISettings.Instance.ViewShowDelay, new Bos.UI.ViewData {
                 ViewDepth = ViewService.NextViewDepth
            });

            Services.SoundService.PlayOneShot(SoundName.click);
            ViewService.Remove(Bos.UI.ViewType.FirstInvestorView);
		});
	}

	
}
