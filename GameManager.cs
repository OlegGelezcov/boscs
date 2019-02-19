using System.Collections.Generic;
using UnityEngine;

public class GameManager : ReactiveMonoBehaivor
{
    public int BuyMultiplier = 1; // 0 = MAX
    public AllManagers Managers;

    //public GameObject ManagerContainer;
    //public UpgradeScreen UpgradeScreen;

    private double _lastInvestorsCount;
    public static bool InvestorAlreadyShown;

    private List<Manager> _managers;

    private void HandleBackButton()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!TransientData.CanExit)
            {
                Managers.UIManager.ShowPressBackToExit();
            }
            else
            {
                Application.Quit();
            }
        }
    }

    public void CollectCustomStat(string stat)
    {
        StatsCollector.Instance.CollectCustom(stat);
    }


}
