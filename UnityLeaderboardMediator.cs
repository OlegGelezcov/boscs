using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityLeaderboardMediator : MonoBehaviour
{
    private ILeaderboardManager _leaderboardManager;

    private void Start()
    {
        _leaderboardManager = ServiceLocator.Instance.Resolve<ILeaderboardManager>();
    }

    public void ReportScore()
    {
        _leaderboardManager.SubmitLevel();
    }

    public void ShowLeaderboardUI_Score()
    {
        if (!Social.localUser.authenticated)
        {
            Social.localUser.Authenticate(success =>
            {
                Debug.Log("Auth = " + success);
                Social.ShowLeaderboardUI();
            });
        } else
        {
            Social.ShowLeaderboardUI();
        }

        
    }
}
