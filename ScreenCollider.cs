namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ScreenCollider : GameBehaviour {

        private bool isInitialized = false;

        public void Setup() {
            if (!isInitialized) {
                RectTransform rectTransform = GetComponent<RectTransform>();
                rectTransform.SetAsLastSibling();
                isInitialized = true;
            }
        }

        public override void Update() {
            base.Update();
            Setup();
        }
    }

}