namespace Bos.UI {
    using UniRx;
    using UnityEngine;

    public class PlayerPortrait : GameBehaviour {

        public GameObject alert;


        private bool isAvailableForCurrentCash = false;
        private bool isAvailableForTotalCash = false;


        public override void Start() {
            base.Start();
            Player.ProductNotifier.ProductAvailability.Subscribe(info => {
                if(info.IsAvailableForCurrentCash || info.IsAvailableForTotalCash ) {
                    if(alert != null ) {
                        alert.ToggleActivity(true);
                    }
                } else {
                    if(alert != null ) {
                        alert.ToggleActivity(false);
                    }
                }
            }).AddTo(gameObject);
        }
    }

}