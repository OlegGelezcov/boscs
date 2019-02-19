namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UICanvas : GameBehaviour {

        private static bool isCreated = false;

        public override void Awake() {
            base.Awake();

            if(!isCreated) {
                DontDestroyOnLoad(gameObject);
                isCreated = true;
            } else {
                Destroy(gameObject);
                return;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GameModeChanged += OnGameModeChanged;      
        }

        public override void OnDisable() {
            GameEvents.GameModeChanged -= OnGameModeChanged;
            base.OnDisable();
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode ) {
            if(newGameMode == GameModeName.Game) {
                var gameCamera = FindObjectOfType<GameCamera>();
                if(gameCamera != null ) {
                    GetComponent<Canvas>().worldCamera = gameCamera.selfCamera;
                }
            }
        }
    }

}