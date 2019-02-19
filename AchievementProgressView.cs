using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementProgressView : MonoBehaviour
{
    public AchievementTracker Tracker;

    public Image ProgressBar;

    private void FixedUpdate()
    {
        ProgressBar.fillAmount = Tracker.PercentCompletion;
    }
}
