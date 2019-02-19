namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ScreenCanvas : GameBehaviour {

        public Canvas canvas;

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GameModeChanged += OnGameModeChanged;
        }

        public override void OnDisable() {
            GameEvents.GameModeChanged -= OnGameModeChanged;
            base.OnDisable();
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode) {
            if (oldGameMode == GameModeName.Game && newGameMode == GameModeName.SplitLiner) {
                canvas.enabled = false;
            } else if (oldGameMode == GameModeName.SplitLiner && newGameMode == GameModeName.Game) {
                canvas.enabled = true;
            }
        }
    }

}