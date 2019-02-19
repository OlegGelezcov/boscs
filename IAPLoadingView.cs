namespace Bos.UI {

    public class IAPLoadingView : TypedView {

        public override ViewType Type => ViewType.IAPLoadingView;


        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 700;
    }

}