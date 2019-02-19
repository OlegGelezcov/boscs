namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using UDebug = UnityEngine.Debug;

    public class SoundService : SaveableGameBehaviour, ISoundService {

        private SoundManager soundManager;
        private bool isBackgroundMusicInChanging = false;
        public bool IsMute { get; private set; } = false;

        private bool isInitialized = false;
        private bool isMutedByAd = false;

        public void SetMute(bool value) {
            IsMute = value;
            if(IsMute) {
                AudioListener.volume = 0;
            } else {
                AudioListener.volume = 1;
            }
        }

        private void UpdateVolume() {
            if(!IsMute) {
                if(isMutedByAd) {
                    AudioListener.volume = 0;
                } else {
                    AudioListener.volume = 1;
                }
            }
        }

       

        public SoundManager LegacyManager
            => (soundManager != null) ? soundManager : (soundManager = FindObjectOfType<SoundManager>());

        public void PlayOneShot(SoundName soundName) 
            => LegacyManager?.PlayOneShot(soundName.ToString());

        public void Setup(object data = null) {
            UDebug.Log($"Sound manager is initialized...");
            if(!isInitialized) {
                #if UNITY_IOS
                GameEvents.AdWillBePlayed.SubscribeOnMainThread().Subscribe(isPlayed => {
                    bool oldValue = isMutedByAd;
                    isMutedByAd = isPlayed;
                    if(oldValue != isMutedByAd) {
                        UpdateVolume();
                    }
                }).AddTo(gameObject);
                #endif
                isInitialized = true;
            }
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(SoundService)}.{nameof(UpdateResume)}() => {pause}");


        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GameModeChanged += OnGameModeChanged;
        }

        public override void OnDisable() {
            GameEvents.GameModeChanged -= OnGameModeChanged;
            base.OnDisable();
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode ) {
            if(oldGameMode == GameModeName.Game && newGameMode == GameModeName.SplitLiner) {
                ChangeBackgroundMusic(SoundName.SplitBackMusic);
            } else if(oldGameMode == GameModeName.SplitLiner && newGameMode == GameModeName.Game) {
                ChangeBackgroundMusic(SoundName.GameBackMusic);
            }
            SetMute(IsMute);
        }


        public void ChangeBackgroundMusic(SoundName name) {
            StartCoroutine(ChangeBackgroundMusicImpl(name));
        }

        private IEnumerator ChangeBackgroundMusicImpl(SoundName name) {
            yield return new WaitUntil(() => isBackgroundMusicInChanging == false);
            ChangeBackgroundMusicInner(name);
        }

        private void ChangeBackgroundMusicInner(SoundName name) {
            FloatAnimator animator = LegacyManager.backgroundMusicSource.gameObject.GetOrAdd<FloatAnimator>();

            var unmuteData = new FloatAnimationData {
                StartValue = 0,
                EndValue = 1,
                Duration = 1,
                EaseType = EaseType.Linear,
                OnStart = (v, o) => LegacyManager.backgroundMusicSource.volume = v,
                OnUpdate = (v, t, o) => LegacyManager.backgroundMusicSource.volume = v,
                OnEnd = (v, o) => {
                    LegacyManager.backgroundMusicSource.volume = v;
                    isBackgroundMusicInChanging = false;
                }
            };

            var muteData = new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = 1,
                EaseType = EaseType.Linear,
                Target = animator.gameObject,
                OnStart = (v, o) => LegacyManager.backgroundMusicSource.volume = v,
                OnUpdate = (v, t, o) => LegacyManager.backgroundMusicSource.volume = v,
                OnEnd = (v, o) => {
                    LegacyManager.backgroundMusicSource.volume = v;
                    LegacyManager.backgroundMusicSource.clip = Services.ResourceService.Audio.GetObject(name);
                    LegacyManager.backgroundMusicSource.Play();
                    animator.StartAnimation(unmuteData);
                }
            };
            animator.StartAnimation(muteData);
            isBackgroundMusicInChanging = true;
        }

        #region SaveableGameBehaviour overrides
        public override object GetSave() {
            return new SoundServiceSave {
                isMute = IsMute
            };
        }

        public override void LoadDefaults() {
            IsMute = false;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            SoundServiceSave save = obj as SoundServiceSave;
            if(save != null ) {
                SetMute(save.isMute);
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override string SaveKey => "sound_service";

        public override Type SaveType => typeof(SoundServiceSave);

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetFull() {
            LoadDefaults();
        }


        #endregion
    }

    public enum SoundName {
        buyCoins,
        buyCoinUpgrade,
        buyGenerator,
        buyUpgrade,
        slotFail,
        slotSpin,
        slotWin,
        badgeUnlock,
        Poof,
        click,
        race,
        race_fail,
        race_ready,
        race_win,
        GameBackMusic,
        SplitBackMusic,
        panel_slide
    }

    public interface ISoundService : IGameService {
        void PlayOneShot(SoundName soundName);
        bool IsMute { get; }
        void SetMute(bool value);
    }

    public class SoundServiceSave {
        public bool isMute;
    }

}