namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class UniversalCurrencyView : GameBehaviour {

        public GameObject coinsParent;
        public GameObject companyCashParent;
        public GameObject playerCashParent;
        public GameObject securitiesParent;

        public Text coinsText;
        public Text companyCashText;
        public Text playerCashText;
        public Text securitiesText;

        public void Setup(Bos.Data.Currency currency) {
            ActivateOnlyCurrencyType(currency.Type);
            GetTextForCurrencyType(currency.Type).text = currency.DisplayString;
        }

        private void ActivateOnlyParent(GameObject toActivate) {
            GameObject[] allParents = new GameObject[] {
                coinsParent,
                companyCashParent,
                playerCashParent,
                securitiesParent
            };
            foreach(GameObject obj in allParents) {
                if(obj == toActivate) {
                    obj.Activate();
                } else {
                    obj.Deactivate();
                }
            }
        }

        private void ActivateOnlyCurrencyType(CurrencyType type) {
            Dictionary<CurrencyType, GameObject> parents = new Dictionary<CurrencyType, GameObject> {
                [CurrencyType.Coins] = coinsParent,
                [CurrencyType.CompanyCash] = companyCashParent,
                [CurrencyType.PlayerCash] = playerCashParent,
                [CurrencyType.Securities] = securitiesParent
            };
            ActivateOnlyParent(parents[type]);
        }

        private Text GetTextForCurrencyType(CurrencyType type) {
            Dictionary<CurrencyType, Text> allTexts = new Dictionary<CurrencyType, Text> {
                [CurrencyType.Coins] = coinsText,
                [CurrencyType.CompanyCash] = companyCashText,
                [CurrencyType.PlayerCash] = playerCashText,
                [CurrencyType.Securities] = securitiesText
            };
            return allTexts[type];
        }
    }

}