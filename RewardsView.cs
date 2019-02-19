namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class RewardsView : TypedViewWithCloseButton {

        public LootboxOpenView lootboxOpenView;
        public Button continueButton;

        #region TypedView overrides
        public override ViewType Type => ViewType.RewardsView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 5;

        public override void Setup(ViewData data) {
            base.Setup(data);
            lootboxOpenView.Prepare();
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.RewardsView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            continueButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.RewardsView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        }
        #endregion
    }

}