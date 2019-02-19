namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UDBG = UnityEngine.Debug;

    public class NavigateToGame : GameBehaviour {

        private global::IGameService _gameService;
        private ILeaderboardManager _leaderboardManager;
        private readonly List<string> loadedNetResources = new List<string>();
        private float progress = 0f;
        public static event Action<float> ReportLoadProgress;

        private static void OnReportProgress(float p) {

            ReportLoadProgress?.Invoke(p);
            //UDBG.Log($"progress reported => {p}");
        }


        public override void Start() {
            base.Start();
            _gameService = new MockGameService();
            ServiceLocator.Instance.Register<global::IGameService>(ServiceLocator.NoFactory, _gameService);
#if UNITY_ANDROID
            _leaderboardManager = new GooglePlayLeaderboardManager();
#endif

#if UNITY_IOS
        _leaderboardManager = new AgnosticLeaderboardManager();
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ServiceLocator.Instance.Register<ILeaderboardManager>(ServiceLocator.NoFactory, _leaderboardManager);
            StartCoroutine(NavigateToGameImpl());
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.NetResourceLoaded += OnNetResourceLoaded;
        }

        public override void OnDisable() {
            GameEvents.NetResourceLoaded -= OnNetResourceLoaded;
            base.OnDisable();
        }

        private void OnNetResourceLoaded(string resourceName ) {
            loadedNetResources.Add(resourceName);
            progress += 0.05f;
            progress = Mathf.Clamp(progress, 0, 0.8f);
            OnReportProgress(progress);
        }

        private IEnumerator NavigateToGameImpl() {
            yield return new WaitUntil(() => ResourceService.IsLoaded);

            //print("start wait for SleepService");
            yield return new WaitUntil(() => Services.SleepService.IsRunning);
            //print("Ok, sleep service is running");

            float limit = Mathf.Max(progress, .8f);
            while(progress < limit ) {
                progress += 0.03f;
                OnReportProgress(progress);
                yield return new WaitForSeconds(.1f);
            }

            StartCoroutine(LoadGameImpl());
        }

        private IEnumerator LoadGameImpl() {
            var asyncOp = SceneManager.LoadSceneAsync(1);
            asyncOp.allowSceneActivation = false;
            progress = Mathf.Clamp01(progress);
            float scaling = 0.9f - progress;
            scaling = Mathf.Clamp01(scaling);
            while (asyncOp.progress < 0.9f) {
                OnReportProgress(progress + asyncOp.progress * scaling);
                yield return new WaitForEndOfFrame();
            }
            OnReportProgress(1);
            yield return new WaitForSeconds(1);
            asyncOp.allowSceneActivation = true;
            while(!asyncOp.isDone) {
                yield return null;
            }
        }
    }

}