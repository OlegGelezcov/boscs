namespace Bos {
#if UNITY_ANDROID
    using GooglePlayGames.BasicApi;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SocialService : GameBehaviour, ISocialService{

        private bool isSocialAuthenticated = false;

        public override void OnEnable() {
            base.OnEnable();
        }

        public override void OnDisable() {
            base.OnDisable();
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode ) {
            if(newGameMode == GameModeName.Game ) {
                if(!isSocialAuthenticated ) {
                    StartCoroutine(AuthImpl());
                }
            }
        }

        private IEnumerator AuthImpl() {
            yield return new WaitForSeconds(1f);
            Social.localUser.Authenticate(success => {
                UnityEngine.Debug.Log("Auth = " + success);

#if UNITY_ANDROID
                ((GooglePlayGames.PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.TOP);
#endif
                isSocialAuthenticated = true;
            });
        }

        public void Setup(object data = null) {
            
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(SocialService)}.{nameof(UpdateResume)}() => {pause}");

    }


    public interface ISocialService : IGameService {

    }
}