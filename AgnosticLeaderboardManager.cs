using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// dummy class for ios build
public class AgnosticLeaderboardManager : ILeaderboardManager {
    
    public event EventHandler<LeaderboardScoreEventArgs> Top3Response;
    public event EventHandler<LeaderboardScoreEventArgs> Get5Response;
    public void SubmitLevel()
    {
        //throw new NotImplementedException();
    }

    public void SubmitTimeSpent(TimeSpan timeSpent)
    {
        //throw new NotImplementedException();
    }

    public void GetTop3(string leaderboardId)
    {
        //throw new NotImplementedException();
    }

    public void Get5AroundPlayer(string leaderboardId)
    {
        //throw new NotImplementedException();
    }
}
