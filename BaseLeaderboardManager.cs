using System;
using Bos;
using UnityEngine;

public abstract class BaseLeaderboardManager : ILeaderboardManager
{
    #region Events

    public event EventHandler<LeaderboardScoreEventArgs> Top3Response;
    public event EventHandler<LeaderboardScoreEventArgs> Get5Response;

    #endregion

    #region Fields

    protected IValueConverter _scoreConverter;
    protected IValueConverter _identityConverter;
    protected IValueConverter _timeConverter;

    protected IGameService _gameService;

    #endregion

    #region Initialization

    public BaseLeaderboardManager()
    {
        _gameService = ServiceLocator.Instance.Resolve<IGameService>();

        _scoreConverter = new LogarithmicScoreConverter();
        _identityConverter = new IdentityConverter();
        _timeConverter = new TimePlayedConverter();
    }

    #endregion

    #region ILeaderboardManager Members

    public abstract void GetTop3(string leaderboardId);

    public abstract void Get5AroundPlayer(string leaderboardId);

    public virtual void SubmitLevel()
    {
        
#if UNITY_EDITOR
        return;
#endif
        Social.ReportScore(GameServices.Instance.PlayerService.Level, LeaderboardConstants.leaderboard_level, OnReportScoreResponse);
    }
    
    public virtual void SubmitTimeSpent(TimeSpan timeSpent)
    {
#if UNITY_EDITOR
        return;
#endif
        var dateTime = _gameService.GetServerDateTime();

        var diff = dateTime - TransientData.AppStarted;
        StatsCollector.Instance[Stats.TIME_PLAYED] += diff.TotalMilliseconds;

        TransientData.AppStarted = dateTime;

        Social.ReportScore((long)StatsCollector.Instance[Stats.TIME_PLAYED], LeaderboardConstants.leaderboard_time_played, OnReportScoreResponse);
    }
   
    #endregion

    #region Helpers

    protected IValueConverter GetConverter(string leaderboardId)
    {
        IValueConverter conv = null;

        switch (leaderboardId)
        {
            case LeaderboardConstants.leaderboard_level:
                conv = _identityConverter;
                break;
            case LeaderboardConstants.leaderboard_time_played:
                conv = _timeConverter;
                break;
            default:
                break;
        }

        return conv;
    }
   
    protected void OnReportScoreResponse(bool success)
    {

    }

    #endregion

    #region Event Initiators

    protected void OnTop3Response(LeaderboardScoreEventArgs args)
    {
        Top3Response?.Invoke(this, args);
    }
    protected void OnGet5Response(LeaderboardScoreEventArgs args)
    {
        Get5Response?.Invoke(this, args);
    }


    #endregion

}
