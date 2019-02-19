namespace Bos {
    using Bos.UI;
    using UniRx;
    using UnityEngine;

    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleModalViewDisabler : GameBehaviour {

        private ParticleSystem particleSystem;

        private bool isInitialized = false;

        public override void OnEnable() {
            base.OnEnable();
            particleSystem = GetComponent<ParticleSystem>();
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
            if(!isInitialized ) {
                isInitialized = true;
            }
        }

        public override void OnDisable() {
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            base.OnDisable();
        }

        private void OnViewHided(ViewType type) {
            ChangeVisibility();
        }

        private void OnViewShowed(ViewType type) {
            ChangeVisibility();
        }

        private void ChangeVisibility() {
            if(ViewService.IsNoModalAndLegacyViews ) {
                if(!particleSystem.isPlaying) {
                    particleSystem.Play();
                }
            } else {
                if(!particleSystem.isStopped) {
                    particleSystem.Stop();
                }
            }
        }
    }

}