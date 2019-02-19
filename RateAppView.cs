namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class RateAppView : TypedView {

        public Button closeButton;
        public Button supportButton;
        public Button rateButton;

        public override void Setup(ViewData data) {
            base.Setup(data);

            rateButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                Services.GetService<IRateService>().Rate();
                ViewService.Remove(ViewType.RateAppView);
            });

            supportButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                Application.OpenURL(Services.ResourceService.Defaults.supportLink);
            });

            closeButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                closeButton.SetInteractable(false);
                ViewService.Remove(ViewType.RateAppView, BosUISettings.Instance.ViewCloseDelay);
            });
        }


        #region BaseView overrides
        public override ViewType Type => ViewType.RateAppView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 100;
        #endregion

    }

}