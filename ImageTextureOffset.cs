namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageTextureOffset : GameBehaviour {

        public Image image;

        public override void Update() {
            base.Update();
            image.materialForRendering.SetTextureOffset("_MainTex", new Vector2(Time.time, 0));
        }
    }

}