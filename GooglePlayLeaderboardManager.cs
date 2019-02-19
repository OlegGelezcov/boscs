using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GooglePlayLeaderboardManager : BaseLeaderboardManager
{
    public GooglePlayLeaderboardManager() : base()
    {
#if UNITY_ANDROID
        var config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
#endif
    }

    public override void Get5AroundPlayer(string leaderboardId)
    {
        var conv = GetConverter(leaderboardId);

        Social.LoadScores(leaderboardId, scores =>
        {
            var args = new LeaderboardScoreEventArgs();

            if (scores.Length > 0)
            {
                int qindex = 0;
                for (int i = 0; i < scores.Length; i++)
                {
                    if (scores[i].userID == Social.localUser.id)
                    {
                        qindex = i;
                        break;
                    }
                }
                args.Success = true;

                var l = new List<LeaderboardScore>();

                if (qindex - 2 < 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        l.Add(new LeaderboardScore(scores[i], conv));
                    }
                }
                if (qindex + 2 > scores.Length)
                {
                    for (int i = scores.Length - 5; i < scores.Length; i++)
                    {
                        l.Add(new LeaderboardScore(scores[i], conv));
                    }
                }
                else
                {
                    for (int i = qindex - 2; i < qindex + 2; i++)
                    {
                        l.Add(new LeaderboardScore(scores[i], conv));
                    }
                }

                args.Data = l.ToArray();
            }
            else
            {
                args.Success = false;
                args.Reason = "Returned score len is 0. Maybe not authenticated";
            }
            OnGet5Response(args);
        });
    }

    public override void GetTop3(string leaderboardId)
    {
        var conv = GetConverter(leaderboardId);

        
        Social.LoadScores(leaderboardId, scores =>
        {
            var args = new LeaderboardScoreEventArgs();

            if (scores.Length > 0)
            {
                var q = (from x in scores
                         orderby x.rank ascending
                         select new LeaderboardScore(x, conv)).Take(3);

                args.Success = true;
                args.Data = q.ToArray();
            }
            else
            {
                args.Success = false;
                args.Reason = "Returned score len is 0. Maybe not authenticated";
            }
            OnTop3Response(args);
        });
    }

    public void TestSocial() {
        
    }
}