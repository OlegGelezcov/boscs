using System;
using System.Linq;
using UnityEngine.UI;

namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CoinsList : GameBehaviour {

        public GameObject itemPrefab;
        public Transform layout;
        public ScrollRect scrollRect;
        public FloatAnimator animator;

        private bool isInitialized = false;

        private readonly BosItemList<BosCoinUpgradeData, ShopCoinUpgradeView> itemList = new BosItemList<BosCoinUpgradeData, ShopCoinUpgradeView>();

        public void Setup(CoinListData coinListData)
        {
            var orderedItems = coinListData.Items.Where(data => IsNotTeleportWhenEarthOrMoon(data)).OrderBy(val => val.Order).ThenBy(val => val.Id).ToList();

            if(!isInitialized) {
                itemList.IsCorrectScale = true;
                itemList.CorrectedScaleValue = Vector3.one;
                itemList.Setup(itemPrefab, 
                    layout, 
                    (data, view) => view.Setup(data, itemList), 
                    (d1, d2) => d1.Id == d2.Id, 
                    (d1, d2) => d1.Id.CompareTo(d2.Id), 
                    Services);
                itemList.Fill(orderedItems, 0.05f);
                isInitialized = true;
            }
            if (coinListData.CoinUpgradeId != 0)
                StartCoroutine(SetupImpl(coinListData.CoinUpgradeId));
        }

        private IEnumerator SetupImpl(int elementId)
        {
            yield return new WaitUntil(() => itemList.IsLoaded);
            var endValue = GetEndValue(elementId);
            
            var scrollData = new FloatAnimationData {
                StartValue = scrollRect.content.anchoredPosition.y,
                EndValue = endValue,
                Duration = 1,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = scrollRect.gameObject,
                OnStart = (v, o) => scrollRect.content.anchoredPosition = new Vector2(0, v),
                OnUpdate = (v, t, o) => scrollRect.content.anchoredPosition = new Vector2(0, v),
                OnEnd = (v, o) => scrollRect.content.anchoredPosition = new Vector2(0, v),
            };
            animator.StartAnimation(scrollData);
        }

        private bool IsNotTeleportWhenEarthOrMoon(BosCoinUpgradeData data) {
            return UpgradeUtils.IsNotTeleportWhenEarthOrMoon(data);
        }

        private float GetEndValue(int elementId)
        {
            var data = itemList.DataList.FirstOrDefault(val => val.Id == elementId);
            if (data != null)
            {
                var view = itemList.FindView(data);
                if (view != null)
                {
                    var posY = Math.Abs(view.GetComponent<RectTransform>().anchoredPosition.y);
                    return Mathf.Clamp(posY, 0 ,scrollRect.content.sizeDelta.y  - (view.GetComponent<RectTransform>().sizeDelta.y * 4));
                }
            }
            return 0;
        }
    }
    
    public class CoinListData
    {
        public int CoinUpgradeId;
        public List<BosCoinUpgradeData> Items;

        public CoinListData(int coinUpgradeId, List<BosCoinUpgradeData> items)
        {
            CoinUpgradeId = coinUpgradeId;
            Items = items;
        }
    }
}