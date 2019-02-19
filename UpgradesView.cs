namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class UpgradesView : GameBehaviour {

        public GameObject itemPrefab;
        public Transform layout;
        public CurrencyType currencyType;


        private readonly BosItemList<UpgradeData, UpgradeItemView> itemList = new BosItemList<UpgradeData, UpgradeItemView>();

        public List<UpgradeData> DataList
            => itemList.DataList;

        public override void OnEnable() {
            base.OnEnable();
            Setup();
            GameEvents.UpgradeAdded += OnUpgradeAdded;

        }

        public override void OnDisable() {
            GameEvents.UpgradeAdded -= OnUpgradeAdded;
            base.OnDisable();
        }

        private void OnUpgradeAdded(UpgradeData data ) {
            if(data.CurrencyType == currencyType ) {
                itemList.UpdateViews(DataSource);
            }
        }

        private void Setup() {
            itemList.Setup(itemPrefab, layout, (data, view) => view.Setup(data), (data1, data2) => data1.Id == data2.Id, 
                (data1, data2) => {
                    IUpgradeService serv = Services.UpgradeService;
                    bool isAllowBuyFirst = serv.IsAllowBuy(data1);
                    bool isAllowBuySecond = serv.IsAllowBuy(data2);

                    if(isAllowBuyFirst && !isAllowBuySecond) {
                        return -1;
                    }
                    if(!isAllowBuyFirst && isAllowBuySecond) {
                        return 1;
                    }
                    return data1.Price(() => {
                        return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data1);
                    }).CompareTo(data2.Price(() => {
                        return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data2);
                    }));
                });
            itemList.UpdateViews(DataSource);
            
        }

        private bool IsTeleportIncluded(UpgradeData data ) {
            return UpgradeUtils.IsNotTeleportWhenEarthOrMoon(data);
            /*
            if(data.GeneratorId == Bos.GenerationService.TELEPORT_ID ) {
                if(Planets.IsMarsOpened ) {
                    return true;
                } else {
                    return false;
                }
            }
            return true;*/
        }

        private List<UpgradeData> DataSource {
            get {
                IUpgradeRepository repo = null;
                if(currencyType == CurrencyType.CompanyCash) {
                    repo = Services.ResourceService.CashUpgrades;
                } else {
                    repo = Services.ResourceService.SecuritiesUpgrades;
                }

                IUpgradeService upgradeService = Services.UpgradeService;
                List<UpgradeData> items = repo.UpgradeList
                    .Where(d => !upgradeService.IsBought(d) && upgradeService.IsAllowBuy(d) && IsTeleportIncluded(d))
                    .Take(10)
                    .ToList();

                if(items.Count < 10 ) {
                    int addCount = 10 - items.Count;
                    items.AddRange(repo.UpgradeList
                        .Where(d => !upgradeService.IsBought(d) && !upgradeService.IsAllowBuy(d))
                        .Take(addCount));
                }
                return items;
            }
        }

        
    }

}