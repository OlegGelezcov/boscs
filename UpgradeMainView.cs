using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Bos.UI {
    using UnityEngine;
    using UnityEngine.UI;

    public class UpgradeMainView : TypedViewWithCloseButton {
        public Toggle useCashToggle;
        public Toggle useSecuritiesToggle;
        public Toggle useCoinsToggle;
        public Toggle shopToggle;
        
        //public Button shopButton;
        public RectTransform shopButtonIcon;

        public GameObject cashListParent;
        public GameObject securitiesListParent;
        public CoinsList coinList;
        public GameObject shopObjects;
        

        #region TypedView overrides
        public override ViewType Type => ViewType.UpgradesView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 10;

        private UpgradeViewData GetUpgradeViewData()
            => (ViewData.UserData as UpgradeViewData);

        public override void Setup(ViewData data) {
            base.Setup(data);
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.UpgradesView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });

            UpgradeTabName tabName = UpgradeTabName.CashUpgrades;
            if (ViewData != null && ViewData.UserData != null) {
                try {
                    tabName = GetUpgradeViewData().TabName;
                }
                catch (System.Exception exception) {
                    Debug.LogException(exception);
                    tabName = UpgradeTabName.CashUpgrades;
                }
            }
            ActivateTab(tabName);
            SetToggleListeners();
        }
        #endregion

        private Dictionary<UpgradeTabName, GameObject> GetListObjects() {
            return new Dictionary<UpgradeTabName, GameObject> {
                [UpgradeTabName.CashUpgrades] = cashListParent,
                [UpgradeTabName.SecuritiesUpgrades] = securitiesListParent,
                [UpgradeTabName.CoinsUpgrades] = coinList.gameObject,
                [UpgradeTabName.Shop] = shopObjects
            };
        }

        private Dictionary<UpgradeTabName, Toggle> GetToggles() {
            return new Dictionary<UpgradeTabName, Toggle> {
                [UpgradeTabName.CashUpgrades] = useCashToggle,
                [UpgradeTabName.SecuritiesUpgrades] = useSecuritiesToggle,
                [UpgradeTabName.CoinsUpgrades] = useCoinsToggle,
                [UpgradeTabName.Shop] = shopToggle
            };
        }
        private void ActivateTab(UpgradeTabName tabName) {
            useCashToggle.onValueChanged.RemoveAllListeners();
            useSecuritiesToggle.onValueChanged.RemoveAllListeners();
            useCoinsToggle.onValueChanged.RemoveAllListeners();
            shopToggle.onValueChanged.RemoveAllListeners();

            var tabLists = GetListObjects();
            foreach (var kvp in tabLists) {
                if (kvp.Key == tabName) {
                    kvp.Value.Activate();
                    if (tabName == UpgradeTabName.CoinsUpgrades) {
                        coinList.Setup(new CoinListData(GetUpgradeViewData()?.CoinUpgradeId ?? 0, Services.StoreService.GetNotPurchasedCoinUpgradeDataList()));
                    }
                    if(tabName == UpgradeTabName.Shop) {
                        shopObjects.GetComponent<StoreItemsList>()?.Setup(GetUpgradeViewData()?.StoreSection ?? StoreItemSection.CompanyCash);
                    }
                }
                else {
                    kvp.Value.Deactivate();
                }
            }

            var toggles = GetToggles();
            toggles[tabName].isOn = true;
            toggles[tabName].OnPointerClick(new PointerEventData(EventSystem.current));

            if(tabName == UpgradeTabName.CashUpgrades ) {
                GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.CashUpgradesOpened));
            }
        }

        private void SetToggleListeners() {
            useCashToggle.SetListener(isOn => {
                if(isOn ) {
                    cashListParent.Activate();
                    Services.SoundService.PlayOneShot(SoundName.click);
                } else {
                    cashListParent.Deactivate();
                }
                
            });
            useSecuritiesToggle.SetListener(isOn => {
                if(isOn) {
                    securitiesListParent.Activate();
                    Services.SoundService.PlayOneShot(SoundName.click);
                } else {
                    securitiesListParent.Deactivate();
                }
            });
            useCoinsToggle.SetListener(isOn => {
                Debug.Log($"use coins toggle => {isOn}");
                if (isOn) {                   
                    Services.SoundService.PlayOneShot(SoundName.click);
                    coinList.Activate();
                    coinList.Setup(new CoinListData(0, Services.StoreService.GetNotPurchasedCoinUpgradeDataList()));
                    GameEvents.ScreenOpenedObservable.OnNext(ScreenType.CoinUpgrades);
                } else {
                    coinList.Deactivate();
                }
            });
            
            shopToggle.SetListener(isOn => {
                if (isOn) {
                    shopObjects.Activate();
                    shopObjects.GetComponent<StoreItemsList>()?.Setup(GetUpgradeViewData()?.StoreSection ?? StoreItemSection.CompanyCash);
                    Services.SoundService.PlayOneShot(SoundName.click);
                }
                else {
                    shopObjects.Deactivate();
                }
            });       
        }
    }

    public enum UpgradeTabName : byte {
        CashUpgrades,
        SecuritiesUpgrades,
        CoinsUpgrades,
        Shop
    }

    public class UpgradeViewData {
        public UpgradeTabName TabName { get; set; }
        public StoreItemSection StoreSection { get; set; }
        public int CoinUpgradeId { get; set; }
    }
}