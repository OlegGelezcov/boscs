using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyBonusButton : MonoBehaviour
{
    public GameObject DailyBonusScreen;
    public float DailyBonusDelay = 200;

    private void Start()
    {
        //if (LocalData.IsFirstTimeLaunch)
        //{
        //    //Destroy(gameObject);
        //   //StartCoroutine(ShowDailyBonusDelayed());
        //}
    }

    private IEnumerator ShowDailyBonusDelayed()
    {
        yield return new WaitForSeconds(DailyBonusDelay);
        gameObject.SetActive(true);
    }

   /* private void Update()
    {
        if (GlobalRefs.PlayerData.DailyBonusGathered)
        {
            enabled = false;
            gameObject.SetActive(false);
        }
    }*/

    public void Click()
    {
        DailyBonusScreen.SetActive(true);
    }
}
