namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class CoinRequiredView : TypedViewWithCloseButton {

        public Button buyCoinsButton;
        public Text coinCountText;
        
        
        public override CanvasType CanvasType => CanvasType.UI;
        public override bool IsModal => true;

        public override ViewType Type => ViewType.CoinRequiredView;

        public override int ViewDepth => 90;

        public override void Setup(ViewData data) {
            base.Setup(data);

            coinCountText.text = ((int) data.UserData).ToString();
            
            buyCoinsButton.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.buyGenerator);
                Services.ViewService.Remove(ViewType.CoinRequiredView);
                //Services.ViewService.ShowDelayed(ViewType.StoreView, BosUISettings.Instance.ViewShowDelay);
                Services.ViewService.Show(ViewType.UpgradesView, new ViewData {
                    UserData = new UpgradeViewData {
                        TabName = UpgradeTabName.Shop,
                        StoreSection = StoreItemSection.Coins,
                        
                    },
                    ViewDepth = Mathf.Max(91, ViewDepth + 1)
                });
            });
            
            closeButton.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.Remove(ViewType.CoinRequiredView);             
            });
        }
    }


}