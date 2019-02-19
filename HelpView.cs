namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class HelpView : TypedViewWithCloseButton {


        #region TypedView overrides
        public override ViewType Type => ViewType.HelpView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 5;

        public override void Setup(ViewData data) {
            base.Setup(data);
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.HelpView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        } 
        #endregion
    }

}