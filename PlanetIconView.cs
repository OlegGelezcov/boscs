namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlanetIconView : GameBehaviour {

        public Image iconImage;

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
            UpdateIcon();
        }

        public override void OnDisable() {
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            base.OnDisable();
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if(newState == PlanetState.Opened) {
                UpdateIcon();
            }
        }

        private void UpdateIcon() {
            PlanetNameData planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(
                Services.GetService<IPlanetService>().CurrentPlanet.Id
                );
            iconImage.overrideSprite = Services.ResourceService.GetSprite(planetNameData.ui_icon);
        }
    }

}