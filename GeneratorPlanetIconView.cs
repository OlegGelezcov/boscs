namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class GeneratorPlanetIconView : GameBehaviour {

        public int generatorId;

        public override void OnEnable() {
            base.OnEnable();
            UpdateIcon();
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
        }

        public override void OnDisable() {
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            base.OnDisable();
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo info) {
            if(newState == PlanetState.Opened && info.Id > PlanetConst.EARTH_ID) {
                UpdateIcon();
            }
        }

        private void UpdateIcon() {
            Image image = GetComponent<Image>();
            var currentPlanetId = Services.GetService<IPlanetService>().CurrentPlanet.Id;
            if(currentPlanetId > PlanetConst.EARTH_ID) {
                var localData = Services.ResourceService.GeneratorLocalData.GetLocalData(generatorId).GetIconData(currentPlanetId);
                if(localData != null ) {
                    image.overrideSprite = Services.ResourceService.GetSpriteByKey(localData.icon_id);
                }
            }
        }
    }

}