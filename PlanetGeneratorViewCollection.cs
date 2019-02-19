namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlanetGeneratorViewCollection : GameBehaviour {

        public PlanetGeneratorView[] planetViews;
        public PlanetGeneratorView[] normalViews;

        public override void Start(){
            for(int i = 0; i < normalViews.Length; i++ ) {
                normalViews[i].Activate();
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.PlanetStateChanged += OnPlanetState;
            UpdateViews();
        }

        public override void OnDisable() {
            GameEvents.PlanetStateChanged -= OnPlanetState;
            base.OnDisable();
        }

        private void OnPlanetState(PlanetState oldState, PlanetState newState, PlanetInfo planet ) {
            if(newState == PlanetState.Opened) {
                UpdateViews();
            }
        }

        private int PlanetId2GeneratorId(int planetId)
            => 10 + planetId;

        private int GeneratorId2PlanetId(int generatorId)
            => generatorId - 10;

        private void UpdateViews() {
            IPlanetService planetService = Services.PlanetService;
            int currenPlanetId = planetService.CurrentPlanet.Id;

            foreach(var view in planetViews) {
                int targetPlanetId = GeneratorId2PlanetId(view.generatorId);
                bool isActivate = false;
                if( IsNotTitanTransportActive(targetPlanetId) || IsTitanTransportActive(targetPlanetId)) {
                    isActivate = true;
                }
                if(isActivate) {
                    view.Activate();
                } else {
                    view.Deactivate();
                }
            }
        }

        private bool IsNotTitanTransportActive(int targetPlanetId ) {
            int currentPlanetId = Services.PlanetService.CurrentPlanet.Id;
            return targetPlanetId < currentPlanetId && Services.PlanetService.IsOpened(targetPlanetId);
        }

        private bool IsTitanTransportActive(int targetPlanetId) {
            int currentPlanetId = Services.PlanetService.CurrentPlanet.Id;
            if(currentPlanetId == PlanetConst.TITAN_ID && targetPlanetId == PlanetConst.TITAN_ID) {
                return true;
            }
            return false;
        }
    }

}