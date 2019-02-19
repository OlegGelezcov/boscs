namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ReconnectView : TypedView {

        public Image background;

        #region TypedView Overrides
        public override ViewType Type => ViewType.ReconnectView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public void ScheduleRemove() {
            Services.Execute(() => ViewService.Remove(ViewType.ReconnectView), 3);
        }

        public override int ViewDepth => 1001;

        public override void Setup(ViewData data) {
            base.Setup(data);

            ColorAnimator colorAnimator = background.gameObject.GetOrAdd<ColorAnimator>();
            colorAnimator.StartAnimation(
                AnimUtils.GetColorAnimData(
                    new Color(0, 0, 0, 0), 
                    new Color(0, 0, 0, 0.83f),
                    0.3f, 
                    EaseType.EaseInOutQuartic, 
                    background.GetComponent<RectTransform>(), 
                    BosAnimationMode.Single));
        }
        #endregion
    }

}