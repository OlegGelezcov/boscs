namespace Bos {
    using UnityEngine;
    using UnityEngine.UI;

    public class HighlightRegion : GameBehaviour {

        public Image image;

        public void Setup(float x, float y, float width, float height) {
            Setup(new Vector4(x, y, width, height));
        }

        public void Setup(Vector4 centerSize ) {
            if (image.material != null) {
                image.material.SetVector("_CenterSize", centerSize);
            }
        }
    }

}