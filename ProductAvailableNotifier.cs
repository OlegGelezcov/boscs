namespace Bos {
    using Bos.Data;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;

    public class ProductAvailableNotifier {

        public class ProductAvailableInfo {
            public bool IsAvailableForCurrentCash { get; set; }
            public bool IsAvailableForTotalCash { get; set; }
    
        }

        private IBosServiceCollection services;
        private readonly Subject<ProductAvailableInfo> availabilitySubject = new Subject<ProductAvailableInfo>();


        public IReadOnlyReactiveProperty<ProductAvailableInfo> ProductAvailability { get; private set; }

        public void Setup(IBosServiceCollection services) {
            this.services = services;
            ProductAvailability = availabilitySubject.ToReadOnlyReactiveProperty(new ProductAvailableInfo { IsAvailableForCurrentCash = false, IsAvailableForTotalCash = false });
            Observable.Interval(TimeSpan.FromSeconds(4)).Subscribe(_ => {
                UpdateState();
            }).AddTo(services.Disposables);
            //GameEvents.ProductPurchasedObservable.Subscribe(prod => UpdateState()).AddTo(services.Disposables);
            //GameEvents.OfficialTransferObservable.Value.Subscribe(info => UpdateState()).AddTo(services.Disposables);
            //GameEvents.UnofficialTransferObservable.Value.Subscribe(info => UpdateState()).AddTo(services.Disposables);
        }

        public void OnOfficialTransfer(TransferCashInfo info ) {
            UpdateState();
        }
        public void OnUnofficialTransfer(UnofficialTransferCashInfo info ) {
            UpdateState();
        }

        private bool IsServicesLoaded
            => services.ResourceService.IsLoaded && services.PlayerService.IsLoaded;

        
        private void UpdateState() {
            bool isAvailableForCurrentCash;
            bool isAvailableForTotalCash;
            UpdateStateForCash(services.PlayerService.PlayerCash.Value, out isAvailableForCurrentCash);
            UpdateStateForCash(TargetCash, out isAvailableForTotalCash);
            availabilitySubject.OnNext(new ProductAvailableInfo {
                IsAvailableForCurrentCash = isAvailableForCurrentCash,
                IsAvailableForTotalCash = isAvailableForTotalCash
            });
        }

        private void UpdateStateForCash(double cash, out bool isAvailable) {
            isAvailable = false;
            if (IsServicesLoaded) {
                foreach (var product in services.ResourceService.PersonalProducts.ProductCollection) {
                    if (false == services.PlayerService.IsProductPurchased(product.id)) {
                        double price = product.price;
                        if (price <= cash) {
                            isAvailable = true;
                            return;
                        }
                    }
                }
            }

        }

        public Option<ProductData> AvailableProduct {
            get {
                double cash = TargetCash;
                List<ProductData> products = services.ResourceService.PersonalProducts.ProductCollection.
                    Where(p => p.price <= cash && (false == services.PlayerService.IsProductPurchased(p.id))).
                    OrderBy(p => p.price).
                    ToList();
                return (products.Count > 0) ? F.Some(products[0]) : F.None;
            }
        }

        public ProductData NotPurchasedMinPriceProduct {
            get {
                return services.ResourceService.PersonalProducts.ProductCollection
                    .Where(p => !services.PlayerService.IsProductPurchased(p.id))
                    .OrderBy(p => p.price)
                    .FirstOrDefault();
            }
        }


        /// <summary>
        /// Personal cash what will be if player make official transfer
        /// </summary>
        public double TargetCash
            => services.PlayerService.PlayerCash.Value + 
            services.ResourceService.PersonalImprovements.ConvertData.OfficialConvertCashValue(services.PlayerService.CompanyCash.Value);


        public void OnProductPurchased() {
            UpdateState();
        }
    }

}