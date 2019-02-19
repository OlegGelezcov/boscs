using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TabHeader))]
public class LeaderboardView : MonoBehaviour
{
    private ILeaderboardManager _manager;

    #region UI Objects

    public GameObject Top3LoadingIndicator;
    public GameObject RankLoadingIndicator;

    public GameObject Top3Container;
    public GameObject ListContainer;

    public Text FirstPlaceName;
    public Text SecondPlaceName;
    public Text ThirdPlaceName;

    public Text FirstPlaceValue;
    public Text SecondPlaceValue;
    public Text ThirdPlaceValue;

    public GameObject Top3ErrorLabel;
    public GameObject PlayerAreaError;

    #endregion

    private void Start()
    {
        _manager = ServiceLocator.Instance.Resolve<ILeaderboardManager>();
        _manager.Top3Response += manager_Top3Response;
        _manager.Get5Response += manager_Get5Response;


        PlayerAreaError.SetActive(false);
        Top3ErrorLabel.SetActive(false);
        Top3Container.SetActive(false);

    }

    public void ProcessTabChange(int q)
    {
        //switch (q)
        //{
        //    case 0:
        //        LoadLeaderboard(LeaderboardConstants.leaderboard_units);
        //        break;
        //    case 1:
        //        LoadLeaderboard(LeaderboardConstants.leaderboard_money);
        //        break;
        //    case 2:
        //        LoadLeaderboard(LeaderboardConstants.leaderboard_time_played);
        //        break;
        //    case 3:
        //        LoadLeaderboard(LeaderboardConstants.leaderboard_games);
        //        break;
        //    default:
        //        break;
        //}
    }

    public void LoadLeaderboard(string leaderboardId)
    {
        if (_manager == null)
            return;


        ShowLoadingTop3();
        ShowLoadingPlayerArea();

        Top3ErrorLabel.SetActive(true);
        Top3Container.SetActive(false);


        _manager.GetTop3(leaderboardId);
        _manager.Get5AroundPlayer(leaderboardId);
    }

    #region UI Helpers

    private void ShowLoadingTop3()
    {
        Top3LoadingIndicator.SetActive(true);
    }

    private void ShowLoadingPlayerArea()
    {
        RankLoadingIndicator.SetActive(true);
    }

    private void HideLoadingTop3()
    {
        Top3LoadingIndicator.SetActive(false);
    }

    private void HideLoadingPlayerArea()
    {
        RankLoadingIndicator.SetActive(false);
    }

    #endregion

    private void manager_Get5Response(object sender, LeaderboardScoreEventArgs e)
    {
        HideLoadingPlayerArea();

        if (e.Success)
        {
            var t = ListContainer.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var x = t.GetChild(i).gameObject;
                var q = x.GetComponent<RankItem>();

                var r = e.Data[i];

                if (r != null)
                    q.Set(r.Rank, r.UserId, r.FormattedValue);
            }

            PlayerAreaError.SetActive(false);
            ListContainer.SetActive(true);
        }
        else
        {
            PlayerAreaError.SetActive(true);
            ListContainer.SetActive(false);
        }
    }

    private void manager_Top3Response(object sender, LeaderboardScoreEventArgs e)
    {
        HideLoadingTop3();

        if (e.Success)
        {
            var p = e.Data[0];

            FirstPlaceName.text = p.UserId;
            FirstPlaceValue.text = p.FormattedValue;

            p = e.Data[1];

            SecondPlaceName.text = p.UserId;
            SecondPlaceValue.text = p.FormattedValue;

            p = e.Data[2];

            ThirdPlaceName.text = p.UserId;
            ThirdPlaceValue.text = p.FormattedValue;

            Top3ErrorLabel.SetActive(false);
            Top3Container.SetActive(true);
        }
        else
        {
            Top3Container.SetActive(false);
            Top3ErrorLabel.SetActive(true);
            Top3ErrorLabel.GetComponent<Text>().text = e.Reason;
        }
    }

}
