namespace Bos.UI {
    using UnityEngine;
    using UnityEngine.UI;

    public class TransferWarningView : TypedView {

        public Text costText;
        public Button closeButton;
        public RectTransform backgroundTransform;

        public override void Setup(ViewData data) {
            base.Setup(data);
            double cost = (double)data.UserData;
            costText.text = new CurrencyNumber(cost).AbbreviationColored("#FFFFFF", "#FFE565");
            backgroundTransform.MessageBoxAnimateIn();
            closeButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                closeButton.interactable = false;
                backgroundTransform.MessageBoxAnimateOut(() => ViewService.Remove(Type));
            });
        }

        #region TypedView overrides
        public override ViewType Type => ViewType.TransferWarningView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 50;
        #endregion
    }

}