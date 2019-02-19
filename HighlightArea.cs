namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class HighlightArea : MonoBehaviour {

        public RectTransform pointer;

        public void Setup(HighlightParams highlightParams ) {
            pointer.anchorMin = highlightParams.anchorMin;
            pointer.anchorMax = highlightParams.anchorMax;
            pointer.anchoredPosition = highlightParams.anchoredPosition;
            pointer.sizeDelta = highlightParams.size;
        }

    }

    public class HighlightParams {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchoredPosition;
        public Vector2 size;

        private HighlightParams() { }

        public static HighlightParams CreateDefaultAnchored(Vector2 position, Vector2 size) {
            return Create(position, size, Vector2.one * .5f, Vector2.one * .5f);
        }

        public static HighlightParams Create(Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax ) {
            return new HighlightParams {
                anchoredPosition = pos,
                size = size,
                anchorMin = anchorMin,
                anchorMax = anchorMax
            };
        }
    }
}