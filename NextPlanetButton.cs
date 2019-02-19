namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DBG = UnityEngine.Debug;

    public class NextPlanetButton : GameBehaviour {

        public Button button;

        public override void Start() {
            base.Start();
#if !BOSDEBUG
            gameObject.Deactivate();
#endif
        }

        public override void OnEnable() {
            base.OnEnable();

            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
            button.SetListener(() => {
                /* 
                if(Services.PlanetService.HasNextPlanet) {
                    Services.PlanetService.ForceSetOpened(Services.PlanetService.NextPlanetId);
                    Services.SoundService.PlayOneShot(SoundName.click);
                } else {
                    DBG.Log("Unable to go at next planet...");
                    DBG.Log($"has next planet => {Services.PlanetService.HasNextPlanet}, next planet id => {Services.PlanetService.NextPlanetId}, current planet id => {Services.PlanetService.CurrentPlanet.Id}");
                }
                */
                DebugUtils.MoveOnNextPlanet();
            });
        }

        public override void OnDisable() {
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            base.OnDisable();
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo info ) {
            UpdateButtonState();
        }

        private void UpdateButtonState() {
            button.interactable = Services.PlanetService.HasNextPlanet;
        }
    }

}