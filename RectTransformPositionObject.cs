namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class RectTransformPositionObject : GameBehaviour, IPositionObject {

        private RectTransform rectTransform;

        private RectTransform RectTransform
            => (rectTransform != null) ? rectTransform : (rectTransform = GetComponent<RectTransform>());


        public Vector3 Position
            => RectTransform.anchoredPosition;

        public GameObject GameObject => gameObject;

        public void SetPosition(Vector3 pos) {
            RectTransform.anchoredPosition = pos;
        }
    }

}