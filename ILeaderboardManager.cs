using System;
using UnityEngine.SocialPlatforms;

public interface ILeaderboardManager
{
    event EventHandler<LeaderboardScoreEventArgs> Top3Response;
    event EventHandler<LeaderboardScoreEventArgs> Get5Response;

    void SubmitLevel();
    //void SubmitGames();
    void SubmitTimeSpent(TimeSpan timeSpent);
    //void SubmitUnits();

    void GetTop3(string leaderboardId);
    void Get5AroundPlayer(string leaderboardId);
}

public class LeaderboardScore
{
    public string FormattedValue;
    public DateTime Date;
    public string UserId;
    public int Rank;

    public LeaderboardScore()
    {

    }

    public LeaderboardScore(IScore score, IValueConverter converter)
    {
        Date = score.date;
        Rank = score.rank;
        UserId = score.userID;
        FormattedValue = converter.ConvertFrom((int)score.value);
    }
}

public class LeaderboardScoreEventArgs : EventArgs
{
    public bool Success { get; set; }
    public LeaderboardScore[] Data { get; set; }

    public string Reason { get; set; }
}