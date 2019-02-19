namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProductCategoryView : GameBehaviour {

        public ProductType productType;
        public Text categoryNameText;
        public GameObject productViewPrefab;
        public Transform layout;

        private readonly BosItemList<ProductData, ProductView> itemList = new BosItemList<ProductData, ProductView>();

        private void Setup() {
            var products = Services.ResourceService.PersonalProducts.GetProducts(productType);
            if(itemList.Count != products.Count ) {
                itemList.Setup(productViewPrefab, layout, (prod, view) => view.Setup(prod),
                    (prod1, prod2) => prod1.id == prod2.id, (prod1, prod2) => prod1.id.CompareTo(prod2.id), Services);
                itemList.Fill(products, 0.2f);
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            Setup();
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }
    
}