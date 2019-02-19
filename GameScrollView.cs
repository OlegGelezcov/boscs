namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class GameScrollView : GameBehaviour {

        public ScrollRect scroll;

        public ScrollRect Scroll
            => scroll;

        public float VerticalNormalizedPosition
            => Scroll.verticalNormalizedPosition;

        public void SetOnTop() => 
            Scroll.verticalNormalizedPosition = 1;


    }

}