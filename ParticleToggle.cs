namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleToggle : GameBehaviour {

        private ParticleSystem particleSystem;

        public override void OnEnable() {
            base.OnEnable();
            particleSystem = GetComponent<ParticleSystem>();
            GameEvents.GameModeChanged += OnGameModeChanged;
        }

        public override void OnDisable() {
            base.OnDisable();
            GameEvents.GameModeChanged -= OnGameModeChanged;
        }

        private void OnGameModeChanged(GameModeName oldName, GameModeName newName) {
            if(newName == GameModeName.Game) {
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