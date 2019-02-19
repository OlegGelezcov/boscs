namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class X2ProfitView : TypedView {

        public Button closeButton;

        public override ViewType Type => ViewType.X2ProfitView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 5;

        public override void Setup(ViewData data) {
            base.Setup(data);
            closeButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                ViewService.Remove(ViewType.X2ProfitView, BosUISettings.Instance.ViewCloseDelay);
            });
        }
    }

}