namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class MainScreenUpdater : GameBehaviour {

        public Image image;

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.CurrentPlanetChanged += OnPlanetChanged;
            UpdateBg();
        }

        public override void OnDisable() {
            GameEvents.CurrentPlanetChanged -= OnPlanetChanged;
            base.OnDisable();
        }

        private void OnPlanetChanged(PlanetInfo planetOld, PlanetInfo planetNew) {
            UpdateBg();
        }

        private void UpdateBg() {
            int currentPlanetId = Services.GetService<IPlanetService>().CurrentPlanet.Id;
            var planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(currentPlanetId);
            Sprite bgSprite = Services.ResourceService.Sprites.GetSprite(planetNameData.bg_image_path);
            image.overrideSprite = bgSprite;
        }
    }

}