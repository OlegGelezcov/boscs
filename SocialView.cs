namespace Bos.UI {
    using Bos.Services;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;


    public class SocialView : TypedViewWithCloseButton {

        public Button closeBigButton;
        public Button promoButton;

        #region TypedView overrides
        public override ViewType Type => ViewType.SocialView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 5;

        private bool IsInitialized { get; set; }

        public override void Setup(ViewData data) {
            base.Setup(data);
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.SocialView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            closeBigButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.SocialView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });

            UpdatePromoButton();

            promoButton.SetListener(() => {
                ViewService.Show(ViewType.PromoInputView, new ViewData { ViewDepth = ViewService.NextViewDepth });
                Sounds.PlayOneShot(SoundName.click);
            });

            if(!IsInitialized) {

                GameEvents.PromoReceived.Subscribe(info => {
                    UpdatePromoButton();
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }
        #endregion

        private void UpdatePromoButton() {
            IPromoService promoService = Services.GetService<IPromoService>();
            //promoButton.interactable = promoService.IsAllowPromo();
            if(promoService.IsAllowPromo()) {
                promoButton.Activate();
            } else {
                promoButton.Deactivate();
            }
        }

    }

}