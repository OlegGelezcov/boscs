using Bos.Data;
using Bos;

namespace Bos.UI {
    using Ozh.Tools.Functional;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StoreItemView : GameBehaviour {

        public int productId;

        public Text priceText;
        public Text descriptionText;
        public Text longDescriptionText;

        public Button buyButton;
        public RectTransform iconTransform;
        public GameObject particles;

        public override void OnEnable() {
            base.OnEnable();
            //var product = IAPManager.instance.GetProduct(productId);
            var resourceData = Services.ResourceService.Products.GetProduct(productId);

            Services.Inap.GetProductByResourceId(productId).Match(() => {
                //Clear();
                buyButton.Activate();
                buyButton.SetListener(() => Services.Inap.PurchaseProduct(resourceData));
                priceText.text = "0.00";
                
                return F.None;
            }, product => {
                buyButton.Activate();
                buyButton.SetListener(() => Services.Inap.PurchaseProduct(resourceData));

                if (longDescriptionText != null) {
                    /*
                    if(resourceData.UseCash || resourceData.UseSecurities || resourceData.UsePlayerCash) {
                        var value = Services.Currency.CreatePriceStringSeparated(GetSoldCurrencyValue(resourceData));
                        if(value.Length > 1) {
                            longDescriptionText.text = $"{value[0]} {value[1]}";
                        } else {
                            longDescriptionText.text = $"{value[0]}";
                        }
                    } else {
                        longDescriptionText.text = string.Empty;
                    }*/
                    longDescriptionText.text = string.Empty;
                }

                if (descriptionText != null) {
                    if (resourceData.UseCash || resourceData.UseSecurities || resourceData.UsePlayerCash) {
                        var cost = GetSoldCurrencyValue(resourceData);
                        CurrencyNumber num = new CurrencyNumber(cost);
                        string costShortText = "$ " + BosUtils.GetCurrencyString(num, "#f0b03c", "#f9f7bc");
                        descriptionText.text = costShortText;
                    } else {
                        descriptionText.text = string.Empty;
                    }
                }

                priceText.text = product.metadata.localizedPriceString;
                return F.Some(product);
            });

            AnimateIcon();
        }

        private void OnBecameVisible() {
            if(particles != null ) {
                particles.Activate();
            }
        }

        private void OnBecameInvisible() {
            if(particles != null ) {
                particles.Deactivate();
            }
        }

        private void AnimateIcon() {
            var animator = iconTransform.gameObject.GetOrAdd<Vector3Animator>();
            var thirdData = iconTransform.ConstructScaleAnimationData(0.95f * Vector3.one, Vector3.one, 0.03f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => { });
            var secondData = iconTransform.ConstructScaleAnimationData(Vector3.one * 1.15f, Vector3.one * 0.95f, 0.1f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                animator.StartAnimation(thirdData);
            });
            var firstData = iconTransform.ConstructScaleAnimationData(Vector3.one, Vector3.one * 1.15f, 0.1f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                animator.StartAnimation(secondData);
            });
            animator.StartAnimation(firstData);
        }

        private double GetSoldCurrencyValue(StoreProductData data) {
            return GetMaxCurrencyValue(data) * data.BonusValue;
        }

        private double GetMaxCurrencyValue(StoreProductData data) {
            return Services.PlayerService.GetCurrencyMaxValue(data.CurrencyType);
        }

        private void Clear() {
            priceText.text = string.Empty;
            if(descriptionText != null) {
                descriptionText.text = string.Empty;
            }
            buyButton.Deactivate();
        }
    }

}