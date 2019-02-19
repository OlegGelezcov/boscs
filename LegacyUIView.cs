namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LegacyUIView : NamedView {

        public override string Name => "legacy";

        public override int ViewDepth => -100;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => false;
    }

}