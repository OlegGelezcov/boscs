namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class FirstTimeView : TypedView {

        public Button showTutorialButton;
        public Button skipButton;

        public override ViewType Type => ViewType.FirstTimeView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => false;

        public override int ViewDepth => 3;

        public override void Setup(ViewData data) {
            base.Setup(data);
            Services.GameModeService.SetIsFirstLaunchGame(false);
            showTutorialButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.FirstTimeView);
                Services.ViewService.Show(ViewType.HelpView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            skipButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.FirstTimeView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        }
    }

}