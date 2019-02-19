using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AchievementNotificationHandler : GameBehaviour
{
    public GameObject NotificationContainer;

    public Text Name;
    public Text RewardText;
    public Text Points;
    public Image Image;

    private readonly List<ExtendedAchievmentInfo> currentAchievments = new List<ExtendedAchievmentInfo>();

    private bool inProcess = false;

    //private int processCounter = 0;
    

    public override void Start()
    {
        NotificationContainer.SetActive(false);
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.GeneratorAchievmentsReceived += OnGeneratorAchievmentsReceived;
    }

    public override void OnDisable() {
        GameEvents.GeneratorAchievmentsReceived -= OnGeneratorAchievmentsReceived;
        base.OnDisable();
    }
    private void OnGeneratorAchievmentsReceived(int generatorId, List<ExtendedAchievmentInfo> achievments) {
        StartCoroutine(ShowAchievmentsImpl(achievments));
    }

    private IEnumerator ShowAchievmentsImpl(List<ExtendedAchievmentInfo> achievments) {

        yield return new WaitUntil(() => inProcess == false);
        inProcess = true;
        //processCounter += achievments.Count();

        currentAchievments.AddRange(achievments);
        foreach(var achievment in achievments) {
            
            yield return StartCoroutine(ProcNotification_Internal(achievment));
            //processCounter--;
        }
        Debug.Log($"current achievemnt count: {currentAchievments.Count}");

        inProcess = false;
    }


    //public void ProcNotification(Achievement achi)
    //{
    //    StartCoroutine(ProcNotification_Internal(achi));
    //}



    private IEnumerator ProcNotification_Internal(ExtendedAchievmentInfo achi)
    {
        if(currentAchievments.Count >= 5 ) {
            currentAchievments.Remove(achi);
            Debug.Log($"break overheaded achievment, count => {currentAchievments.Count}");
            yield break;
        } else {
            currentAchievments.Remove(achi);
            Debug.Log($"processed achievment, count => {currentAchievments.Count}");
        }

        NotificationContainer.SetActive(false);
        yield return new WaitForEndOfFrame();

        Name.text = achi.Name;
        RewardText.text = achi.RewardText;
        Points.text = achi.Points.ToString();
        Image.sprite = achi.Icon;

        NotificationContainer.SetActive(true);
        yield return new WaitForSeconds(4.5f);
    }

    public void NotificationTap()
    {
        NotificationContainer.SetActive(false);
    }
}
