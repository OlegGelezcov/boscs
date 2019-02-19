namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ParallaxObjectPositionFix : GameBehaviour {

        public int planetId;
        public Vector2 fixAnchoredPosition;
        
        private IPlanetService planetService;
        
        public override void OnEnable() {
            base.OnEnable();
            planetService = Services.PlanetService;
            if (planetService.IsOpened(planetId)) {
                GetComponent<RectTransform>().anchoredPosition = fixAnchoredPosition;
            }
        }
    }


}