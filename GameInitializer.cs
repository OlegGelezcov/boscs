using Bos;
using Facebook.Unity;
using System;
using TwitterKit.Unity;
using UnityEngine.Analytics;

public class GameInitializer : GameBehaviour
{

    private IGameService _gameService;
    private ILeaderboardManager _leaderboardManager;

    public override void Awake()
    {
        Analytics.CustomEvent("GAME_START");
        
        StatsCollector.Instance.Load();

        _gameService = ServiceLocator.Instance.Resolve<IGameService>();
        _leaderboardManager = ServiceLocator.Instance.Resolve<ILeaderboardManager>();

        Player.LegacyPlayerData.Load();
        Player.LegacyPlayerData.SessionCount++;
        StoreAchievementDB.Load();
        TransientData.AppStarted = _gameService.GetServerDateTime();
        
        if (Services.GameModeService.IsFirstTimeLaunch)
            LocalData.FirstOpenDate = DateTime.Now;
        
        LocalData.SessionStart = DateTime.Now;
        
        Twitter.Init();
        FB.Init();
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.Pause += OnPause;
        GameEvents.Quit += OnQuit;
    }

    public override void OnDisable() {
        GameEvents.Pause -= OnPause;
        GameEvents.Quit -= OnQuit;
        base.OnDisable();
    }

    private void OnQuit()
    {
        if (Cheats.NO_PREF_SAVE)
            return;

        var dateTime = _gameService.GetServerDateTime();
        Player.LegacyPlayerData.Save();
    }

    private void OnPause()
    {
        var dateTime = _gameService.GetServerDateTime();

        var diff = dateTime - TransientData.AppStarted;
        StatsCollector.Instance[Stats.TIME_PLAYED] += diff.TotalMilliseconds;

        _leaderboardManager.SubmitLevel();
        _leaderboardManager.SubmitTimeSpent(diff);
        StatsCollector.Instance.Save();
        Player.LegacyPlayerData.Save();
    }

    public void SubmitAll()
    {
        var dateTime = _gameService.GetServerDateTime();

        var diff = dateTime - TransientData.AppStarted;
        StatsCollector.Instance[Stats.TIME_PLAYED] += diff.TotalMilliseconds;

        _leaderboardManager.SubmitLevel();
        _leaderboardManager.SubmitTimeSpent(diff);
    }
}
