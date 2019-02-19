using UnityEngine.UI;

namespace Bos.UI {
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZTHADView : TypedViewWithCloseButton
{

	public Button intallButton;
	public Text lowReward, bigReward;


	private void Start()
	{
		Services.SoundService.PlayOneShot(SoundName.click);
		intallButton.SetListener(() => {
			Sounds.PlayOneShot(SoundName.click);
			Services.ZTHADService.OpenZthPage();
			intallButton.SetInteractable(false);
			ViewService.Remove(ViewType.ZTHAdView, BosUISettings.Instance.ViewCloseDelay);
		});
		
		closeButton.SetListener(() => {
			Sounds.PlayOneShot(SoundName.click);
			closeButton.SetInteractable(false);
			ViewService.Remove(ViewType.ZTHAdView, BosUISettings.Instance.ViewCloseDelay);
		});
		lowReward.text = ZTHADService.SmallReward.ToString();
		bigReward.text = ZTHADService.BigReward.ToString();
	}


	#region BaseView overrides
	public override ViewType Type => ViewType.ZTHAdView;

	public override CanvasType CanvasType => CanvasType.UI;

	public override bool IsModal => true;

	public override int ViewDepth => 100;
	#endregion
}}
