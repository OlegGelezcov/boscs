namespace Bos.UI {
    using Bos.Data;
    using Ozh.Tools.Functional;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProductTab : GameBehaviour {
        public ProductList currentProductList;
        public GameObject statics;
        public List<Button> Tabs;

        private ProductType selectedType = ProductType.Transport;


        public void Setup(Option<ProductType> targetTab) {

            targetTab.Match(() => selectedType = ProductType.Transport, tab => selectedType = tab);

            for(int i = 0; i < Tabs.Count; i++ ) {

                ProductsTab tabComponent = Tabs[i].GetComponent<ProductsTab>();
                Tabs[i].SetListener(() => {
                    UnselectAll();
                    Services.SoundService.PlayOneShot(SoundName.click);
                    if (selectedType != tabComponent.productType) {
                        LoadCatgeory(tabComponent.productType);
                    }
                    selectedType = tabComponent.productType;
                    tabComponent.Select();
                });
            }
            LoadCatgeory(selectedType);

        }

        private void UnselectAll()
            => Tabs.ForEach(t => t.GetComponent<ProductsTab>().Unselect());

        private void LoadCatgeory(ProductType type) {
            currentProductList.Setup(new ProductListViewData {
                Products = Services.ResourceService.PersonalProducts.GetProducts(type),
                ProductType = type
            });
            Tabs.Select(t => t.GetComponent<ProductsTab>()).ToList().ForEach(t => {
                if(t.productType != type) {
                    t.Unselect();
                } else {
                    t.Select();
                }
            });
        }

    }

}