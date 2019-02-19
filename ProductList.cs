namespace Bos.UI {
    using Bos.Data;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProductList : GameBehaviour {
		public GameObject productViewPrefab;
		public Transform layout;
        public ScrollRect scrollRect;
        private readonly BosItemList<ProductData, ProductView> viewList = new BosItemList<ProductData, ProductView>();
        private ProductListViewData viewData;

        public void Setup(ProductListViewData viewData ) {
            this.viewData = viewData;

            foreach(Transform trs in layout) {
                if(trs && trs.gameObject) {
                    Destroy(trs.gameObject);
                }
            }

            viewList.Clear();
            viewList.Setup(productViewPrefab, layout, (prod, view) => view.Setup(prod), (p1, p2) => p1.id == p2.id, CompareProducts, Services);
            viewList.UpdateViews(viewData.Products);
            Debug.Log($"load products => {viewData.ProductType}, count => {viewData.Products.Count}");
        }

        private int CompareProducts(ProductData p1, ProductData p2) {
            bool firstPurchased = IsPurchased(p1);
            bool secondPurchased = IsPurchased(p2);
            if(firstPurchased && secondPurchased) {
                return p1.id.CompareTo(p2.id);
            }else  if(!firstPurchased && secondPurchased) {
                return -1;
            } else if(firstPurchased && !secondPurchased) {
                return 1;
            } else {
                return GetProductPrice(p1).CompareTo(GetProductPrice(p2));
            }
        }

        private double GetProductPrice(ProductData p) => p.price;
            //=> Services.GenerationService.
            //    CalculateProfit20Minute(p.transport_Id, p.transport_count, p.planet_Id);

        private bool IsPurchased(ProductData p)
            => Services.PlayerService.IsProductPurchased(p.id);


        public override void OnDisable() {
            base.OnDisable();
            viewList.Clear();
        }

        public ProductType? Category
            => viewData?.ProductType ?? null;
    }

    public class ProductListViewData {
        public ProductType ProductType { get; set; }
        public List<ProductData> Products { get; set; }
    }

}

