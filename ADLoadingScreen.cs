using System;
using System.Collections;
using System.Collections.Generic;
using AppodealAds.Unity.Api;
using UnityEngine;

public class ADLoadingScreen : MonoBehaviour
{

	//public AdManager admanager;

	private float frameTime;

	private Action _rewardAction;
    private string contentType = string.Empty;
	
	public void Fill(string contentType, Action a)
	{
        this.contentType = contentType;
		_rewardAction = a;
		gameObject.SetActive(true);
	}

	private void Update()
	{
		if (frameTime < 0.2f)
		{
			frameTime += Time.deltaTime;
			return;
		}
		frameTime = 0;
		
		if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
		{
			//admanager.WatchAd(contentType, _rewardAction);
			gameObject.SetActive(false);
		}
	}
}
