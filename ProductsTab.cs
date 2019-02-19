namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    public class ProductsTab : GameBehaviour {

        public ProductType productType;
        public Color selectedColor;
        public Color unselectedColor;
        public GameObject selectLine;
        public GameObject alert;

        private bool IsInitialized { get; set; }

        public override void OnEnable() {
            base.OnEnable();
            UpdateAlert();
            if(!IsInitialized ) {
                GameEvents.PlayerCashChangedObservable.Subscribe(args => {
                    UpdateAlert();
                }).AddTo(gameObject);
                GameEvents.ProductPurchasedObservable.Subscribe(args => {
                    UpdateAlert();
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        public void Select() {
            GetComponent<Text>().color = selectedColor;
            selectLine?.Activate();
        }

        public void Unselect() {
            GetComponent<Text>().color = unselectedColor;
            selectLine?.Deactivate();
        }


        private void UpdateAlert() {
            var products = ResourceService.PersonalProducts.GetProducts(productType);
            alert?.ToggleActivity(products.Where(p => Player.PlayerCash.Value >= p.price && (!Player.IsProductPurchased(p.id))).Count() > 0);
        }
    }

}