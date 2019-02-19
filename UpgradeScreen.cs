using Bos;
using Bos.Data;
using Bos.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeScreen : GameBehaviour
{

    public UpgradesView cashViews;
    public UpgradesView securitiesViews;

    //public BalanceManager BalanceManager;
    //public GameObject QuickBuyResearchContainer;
    //public GameObject QuickBuyButton;
    //public GameObject NotEnoughCoinsPopup;
    //public Button quickBuyButton;
    //public Button researchQuickBuyButton;


    public override void OnEnable()
    {
        base.OnEnable();
        GameEvents.UpgradeQuickBuyResearched += OnQuickBuyResearched;
        Reload();
    }

    public override void OnDisable() {
        GameEvents.UpgradeQuickBuyResearched -= OnQuickBuyResearched;
        base.OnDisable();
    }

    private void OnQuickBuyResearched(bool isResearched ) {
        if(isResearched) {
            Reload();
        }
    }

    private void Reload()
    {
        /*
        if (Services.UpgradeService.IsQuickBuyResearched)
        {
            QuickBuyResearchContainer.SetActive(false);
            QuickBuyButton.SetActive(true);
        }
        else
        {
            QuickBuyResearchContainer.SetActive(true);
            QuickBuyButton.SetActive(false);
        }
        quickBuyButton.SetListener(QuickBuy);
        researchQuickBuyButton.SetListener(ResearchQuickBuy);*/

    }

    public bool CanShowUpgradeAlert()
    {
        List<UpgradeData> allUpgrades = new List<UpgradeData>();
        allUpgrades.AddRange(Services.ResourceService.CashUpgrades.UpgradeList);
        allUpgrades.AddRange(Services.ResourceService.SecuritiesUpgrades.UpgradeList);

        foreach(var upgrade in allUpgrades) {
            double price = upgrade.Price(() => {
                return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, upgrade);
            });
            if(!Services.UpgradeService.IsBought(upgrade) &&
                Services.PlayerService.IsEnough(Bos.Data.Currency.Create(upgrade.CurrencyType, upgrade.Price(() => {
                    return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, upgrade);
                }))) &&
                (Services.TransportService.HasUnits(upgrade.GeneratorId) || upgrade.GeneratorId == -1)) {
                return true;
            }
        }
        return false;
    }



    public void QuickBuy()
    {
        if(Services.UpgradeService.IsQuickBuyResearched) {
            List<UpgradeData> dataList = cashViews.DataList.Select(d => d).ToList();
            foreach(var data in dataList) {
                if(Services.PlayerService.IsEnough(Bos.Data.Currency.Create(data.CurrencyType, data.Price(() => {
                    return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                })))) {
                    Services.UpgradeService.BuyUpgrade(data);

                }
            }
            Services.SoundService.PlayOneShot(SoundName.buyUpgrade);
        }

    }

    public void ResearchQuickBuy()
    {

        if (!Services.UpgradeService.IsQuickBuyResearched) {
            if (Services.PlayerService.Coins < 100) {
                //NotEnoughCoinsPopup.GetComponent<NotEnoughCoinsScreen>().Show(100);
                Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                    UserData = 100
                });
                return;
            }

            Services.PlayerService.RemoveCoins(100);
            Services.UpgradeService.SetQuickBuyResearched(true);
            //QuickBuyResearchContainer.SetActive(false);
            //QuickBuyButton.SetActive(true);
        }
    }
}
