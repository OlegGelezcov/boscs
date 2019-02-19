namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class AutoCloseBg : GameBehaviour {

        public Image image;

        public LoadingScreenSprite[] loadingSprites;
        public override void OnEnable() {
            base.OnEnable();
            BosUIUtils.ApplyLoadingSprite(loadingSprites, Services.LoadingPlanet, image);
        }


    }

}