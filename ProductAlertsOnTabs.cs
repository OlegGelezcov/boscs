namespace Bos.UI {
    using UniRx;
    using UnityEngine;

    public class ProductAlertsOnTabs : GameBehaviour {

        public GameObject transferAlert;
        public GameObject productAlert;

        public override void Start() {
            base.Start();

            Player.ProductNotifier.ProductAvailability.Subscribe(info => {
                if(info.IsAvailableForCurrentCash) {
                    ToggleProductAlert(true);
                    ToggleTransferAlert(false);
                } else {
                    if(info.IsAvailableForTotalCash ) {
                        ToggleTransferAlert(true);
                        ToggleProductAlert(false);
                    } else {
                        ToggleTransferAlert(false);
                        ToggleProductAlert(false);
                    }
                }
            }).AddTo(gameObject);
        }

        private void ToggleProductAlert(bool available ) {
            ToggleAlert(productAlert, available);
        }

        private void ToggleTransferAlert(bool available ) {
            ToggleAlert(transferAlert, available);
        }
        private void ToggleAlert(GameObject target, bool available ) {
            if(target != null ) {
                target.ToggleActivity(available);
            }
        }

    }

}