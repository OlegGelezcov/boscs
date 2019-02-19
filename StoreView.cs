namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StoreView : TypedViewWithCloseButton {

        public Button coinButton;
        public Transform coinParent;
        public GameObject[] coinObjects;

        public Button upgradeButton;
        public GameObject[] upgradeObjects;
        public CoinsList coinList;



        public override ViewType Type => ViewType.None;

        public override CanvasType CanvasType => CanvasType.UI;

        public override int ViewDepth => 20;

        public override bool IsModal => true;

        public override void Setup(ViewData data) {
            base.Setup(data);

            coinButton.SetListener(() => {
                coinObjects.Activate();
                coinParent.gameObject.Activate();

                upgradeObjects.Deactivate();
                coinList.Deactivate();

                Services.SoundService.PlayOneShot(SoundName.click);
            });

            upgradeButton.SetListener(() => {
                coinObjects.Deactivate();
                coinParent.gameObject.Deactivate();

                upgradeObjects.Activate();
                coinList.Activate();
                coinList.Setup(new CoinListData(0, GetCoinUpgradeDataList()));
                Services.SoundService.PlayOneShot(SoundName.click);
            });

            closeButton.SetListener(() => {
                //Services.ViewService.Remove(ViewType.StoreView, BosUISettings.Instance.ViewCloseDelay);
                //Services.SoundService.PlayOneShot(SoundName.click);
            });
        }

        private List<BosCoinUpgradeData> GetCoinUpgradeDataList() {
            List<BosCoinUpgradeData> result = new List<BosCoinUpgradeData>();
            foreach(var data in Services.ResourceService.CoinUpgrades.UpgradeList) {
                if(!data.IsOneTime) {
                    result.Add(data);
                } else {
                    if(!Services.StoreService.IsCoinUpgradePurchased(data)) {
                        result.Add(data);
                    }
                }
            }
            return result;
        }
    }


}