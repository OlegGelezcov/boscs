namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Bos.Debug;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UDebug = UnityEngine.Debug;

    public class ViewService : GameBehaviour, IViewService {

        public GameObject startViewPrefab;

        private PrefabRepository<ViewType> typedViewPrefabs = new PrefabRepository<ViewType>();
        private PrefabRepository<string> namedViewPrefabs = new PrefabRepository<string>();

        public ViewType CurrentOpen;
        public Stack<ViewType> OpenedStack = new Stack<ViewType>();
        
        
        private readonly Dictionary<ViewType, TypedView> typedViewInstances = new Dictionary<ViewType, TypedView>();
        private readonly Dictionary<string, NamedView> namedViewInstances = new Dictionary<string, NamedView>();

        private Transform uiCanvasTransform;
        private Transform gameCanvasTransform;

        public GUISkin skin;

        private bool isNeedUpdateOnResume = true;

        private GUIStyle textStyle = null;
        private GUIStyle TextStyle
            => (textStyle != null) ? textStyle : (textStyle = skin.GetStyle("nettext"));

        private readonly List<string> legacyViews = new List<string>();

        private CanvasOptimizer canvasOptimizer = null;
        private MainScreenUpdater mainScreenUpdater = null;


        private CanvasOptimizer CanvasOptimizer
            => (canvasOptimizer != null) ? canvasOptimizer : (canvasOptimizer = FindObjectOfType<CanvasOptimizer>());

        private MainScreenUpdater MainScreenUpdater
            => (mainScreenUpdater != null) ? mainScreenUpdater : (mainScreenUpdater = FindObjectOfType<MainScreenUpdater>());

        public int ModalCount
            => typedViewInstances.Count(kvp => kvp.Value.IsModal) +
                namedViewInstances.Count(kvp => kvp.Value.IsModal);

        public void PushLegacy(string name) {
            if(!legacyViews.Contains(name)) {
                legacyViews.Add(name);
                GameEvents.OnLegacyViewAdded(name);
            }
        }

        public void PopLegacy(string name) {
            bool wasRemoved = legacyViews.Remove(name);
            if(wasRemoved) {
                GameEvents.OnLegacyViewRemoved(name);
            }
        }

        public int LegacyCount
            => legacyViews.Count;




        public Transform UICanvasTransform {
            get {
                if (!uiCanvasTransform) {
                    uiCanvasTransform = FindObjectOfType<UICanvasPanel>()?.transform;
                }
                return uiCanvasTransform;
            }
        }

        public Transform GameCanvasTransform {
            get {
                if (!gameCanvasTransform) {
                    gameCanvasTransform = FindObjectOfType<GameCanvas>()?.transform;
                }
                return gameCanvasTransform;
            }
        }

        public Transform GetCanvasTransform(CanvasType canvasType) {
            if (canvasType == CanvasType.UI) {
                return UICanvasTransform;
            }
            return GameCanvasTransform;
        }

        public ViewType GetCurrentViewType()
        {
            return CurrentOpen;
        }

        private void OnApplicationPause(bool pause) {
            UpdateResume(pause);
        }

        private void OnApplicationFocus(bool focus) {
            UpdateResume(!focus);
        }

        public void UpdateResume(bool pause) {
            //UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");
            UpdateOnResume(pause);
        }

        private void UpdateOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;


                    StartCoroutine(UpdateOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateOnResumeImpl() {
            yield return new WaitUntil(() => Services.ResourceService.IsLoaded  && Services.GameModeService.IsGame);
            ISleepService sleepService = Services.SleepService;
            yield return new WaitUntil(() => sleepService.IsRunning);
            yield return new WaitUntil(() => Services.GenerationService.IsResumed);

            if(sleepService.SleepInterval >= 1800 ) {
                if(Services.GenerationService.TotalOfflineBalance > 0 ) {
                    Show(ViewType.BackToGameView, () => {
                        return ModalCount == 0 && GameMode.GameModeName == GameModeName.Game && LegacyCount == 0;
                    }, (go) => { }, new ViewData {
                        UserData = new BackToGameData { Interval = sleepService.SleepInterval, Cash = Services.GenerationService.TotalOfflineBalance },
                        ViewDepth = NextViewDepth
                    });
                    Debug.Log($"show back to game: sleep interval => {sleepService.SleepInterval}, total balance => {Services.GenerationService.TotalOfflineBalance}".Colored(ConsoleTextColor.fuchsia).BoldItalic());
                }
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GameModeChanged += OnGameModeChanged;
            GameEvents.LegacyViewAdded += OnLegacyViewAdded;
            GameEvents.LegacyViewRemoved += OnLegacyViewRemoved;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
        }

        public override void OnDisable() {
            GameEvents.GameModeChanged -= OnGameModeChanged;
            GameEvents.LegacyViewAdded -= OnLegacyViewAdded;
            GameEvents.LegacyViewRemoved -= OnLegacyViewRemoved;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            base.OnDisable();
        }


        public override void Update() {
            base.Update();
            if(Input.GetKeyUp(KeyCode.Alpha1)) {
                //Show(ViewType.InvestorConfirmMessageBox, new ViewData {
                //    UserData = Services.PlayerService.Securities.Value
                //});
            }else 
#if !UNITY_EDITOR
		    if (Input.GetKeyUp(KeyCode.Escape))
#else
            if (Input.GetKeyUp(KeyCode.Backspace))
#endif
            {
                Debug.Log("Try back event in: " + CurrentOpen);
                if (Exists(CurrentOpen) && typedViewInstances[CurrentOpen] is TypedViewWithCloseButton)
                {
                    var screen = typedViewInstances[CurrentOpen] as TypedViewWithCloseButton;
                    screen.closeButton.onClick.Invoke();
                    Debug.Log("Invoked back event in: " + CurrentOpen);
                }
            }
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode) {

            if (newGameMode == GameModeName.Game) {
                StartCoroutine(ShowBankButtonImpl());
                if(oldGameMode == GameModeName.ManagerSlot || oldGameMode == GameModeName.RaceGame || oldGameMode == GameModeName.RaceGame || oldGameMode == GameModeName.SplitLiner) {
                    RestoreGenerators();
                }
            } else if (newGameMode == GameModeName.Loading) {

                RemoveAllViews();
                RemoveFooter();
                ShowLoading();
            } else {
                RemoveFooter();
            } 
        }

        private void RemoveFooter() {
            Remove(ViewType.MenuFooterView, 0.3f);
        }

        private void RemoveAllViews() {
            var typedViews = typedViewInstances.Keys.ToList();
            foreach (ViewType type in typedViews) {
                if (type != ViewType.LoadingView && type != ViewType.StartView ) {
                    Remove(type);
                }
            }

            var namedViews = namedViewInstances.Keys.ToList();
            foreach (string name in namedViews) {
                Remove(name);
            }
        }

        private void OnLegacyViewRemoved(string name) {}

        private void OnLegacyViewAdded(string name) {}

        private void HideGenerators() {
            var gameCanvas = FindObjectOfType<GameCanvas>();
            if(gameCanvas != null ) {
                gameCanvas.GetComponent<Canvas>().enabled = false;
            }
        }

        private void RestoreGenerators() {
            var gameCanvas = FindObjectOfType<GameCanvas>();
            if(gameCanvas != null ) {
                gameCanvas.GetComponent<Canvas>().enabled = true;
            }
        }

        private void OnViewShowed(ViewType viewType) {
            if(ModalCount > 0 ) {
                //RemoveFooter();
                HideGenerators();
            }
        }

        private void OnViewHided(ViewType viewType) {
            if(Services.GameModeService.GameModeName == GameModeName.Game ) {
                if(LegacyCount == 0 && ModalCount == 0 ) {
                    //StartCoroutine(ShowBankButtonImpl());
                }
                //Debug.Log($"View hided legacy count => {LegacyCount}, modal count => {ModalCount}".Colored(ConsoleTextColor.green));

                if(ModalCount == 0) {
                    RestoreGenerators();
                }
             }
        }

        private IEnumerator ShowBankButtonImpl() {
            //yield return new WaitForSeconds(.5f);
            yield return new WaitUntil(() => LegacyCount == 0);
            yield return new WaitUntil(() => !legacyViews.Contains("AutoCloseBg"));
            Show(ViewType.MenuFooterView);
        }

        public void Setup(object data = null) {
            typedViewPrefabs.AddPath(ViewType.HeaderGameView, "Prefabs/UI/HeaderView");
            typedViewPrefabs.AddPath(ViewType.PlanetsView, "Prefabs/UI/PlanetsView");
            typedViewPrefabs.AddPath(ViewType.BuyModuleView, "Prefabs/UI/ModulesView");
            typedViewPrefabs.AddPath(ViewType.ManagementView, "Prefabs/UI/ManagementView");
            typedViewPrefabs.AddPath(ViewType.BankView, "Prefabs/UI/BankView");
            typedViewPrefabs.AddPath(ViewType.InvestorsView, "Prefabs/UI/InvestorsView");
            typedViewPrefabs.AddPath(ViewType.InvestorConfirmMessageBox, "Prefabs/UI/InvestorConfirmMessageBox");
            typedViewPrefabs.AddPath(ViewType.HistoryView, "Prefabs/UI/HistoryView");
            typedViewPrefabs.AddPath(ViewType.LoadingView, "Prefabs/UI/LoadingView");
            typedViewPrefabs.AddPath(ViewType.StatsView, "Prefabs/UI/StatsView");
            typedViewPrefabs.AddPath(ViewType.ProfileView, "Prefabs/UI/ProfileView");
            typedViewPrefabs.AddPath(ViewType.BadgeInfoView, "Prefabs/UI/BadgeInfoView");
            typedViewPrefabs.AddPath(ViewType.MenuFooterView, "Prefabs/UI/MenuFooterView");
            typedViewPrefabs.AddPath(ViewType.CoinRequiredView, "Prefabs/UI/CoinRequiredView");
            typedViewPrefabs.AddPath(ViewType.MainView, "Prefabs/UI/MainView");
            typedViewPrefabs.AddPath(ViewType.HelpView, "Prefabs/UI/HelpView");
            typedViewPrefabs.AddPath(ViewType.FirstTimeView, "Prefabs/UI/FirstTimeView");
            typedViewPrefabs.AddPath(ViewType.ManagersView, "Prefabs/UI/ManagersView");
            typedViewPrefabs.AddPath(ViewType.RewardsView, "Prefabs/UI/RewardsView");
            typedViewPrefabs.AddPath(ViewType.UpgradesView, "Prefabs/UI/UpgradesView");
            typedViewPrefabs.AddPath(ViewType.SocialView, "Prefabs/UI/SocialView");
            typedViewPrefabs.AddPath(ViewType.EndGameView, "Prefabs/UI/EndGameView");
            typedViewPrefabs.AddPath(ViewType.TutorialDialogView, "Prefabs/UI/TutorialDialogView");
            typedViewPrefabs.AddPath(ViewType.EnahnceManagerView, "Prefabs/UI/EnhanceManagerView");
            typedViewPrefabs.AddPath(ViewType.StartView, "Prefabs/UI/StartView");
            typedViewPrefabs.AddPath(ViewType.ReconnectView, "Prefabs/UI/ReconnectView");
            typedViewPrefabs.AddPath(ViewType.X2ProfitView, "Prefabs/UI/X2ProfitView");
            typedViewPrefabs.AddPath(ViewType.FirstInvestorView, "Prefabs/UI/FirstInvestorView");
            typedViewPrefabs.AddPath(ViewType.RateAppView, "Prefabs/UI/RateAppView");
            typedViewPrefabs.AddPath(ViewType.WaitAdView, "Prefabs/UI/WaitAdView");
            typedViewPrefabs.AddPath(ViewType.ZTHAdView, "Prefabs/UI/ZTHADView");
            typedViewPrefabs.AddPath(ViewType.TransferWarningView, "Prefabs/UI/TransferWarningView");
            typedViewPrefabs.AddPath(ViewType.BackToGameView, "Prefabs/UI/BackToGameView");
            typedViewPrefabs.AddPath(ViewType.BankNotify, "Prefabs/UI/BankFullView");
            typedViewPrefabs.AddPath(ViewType.DebugView, "Prefabs/UI/DebugView");
            typedViewPrefabs.AddPath(ViewType.DependGeneratorView, "Prefabs/UI/DependGeneratorView");
            typedViewPrefabs.AddPath(ViewType.SpecialOfferView, "Prefabs/UI/SpecialOfferView");
            typedViewPrefabs.AddPath(ViewType.IAPLoadingView, "Prefabs/UI/IAPLoadingView");
            typedViewPrefabs.AddPath(ViewType.BosEventView, "Prefabs/UI/BosEventView");
            typedViewPrefabs.AddPath(ViewType.MiniGameView, "Prefabs/UI/MiniGameView");
            typedViewPrefabs.AddPath(ViewType.TransportInfoView, "Prefabs/UI/TransportInfoView");
            typedViewPrefabs.AddPath(ViewType.PromoInputView, "Prefabs/UI/PromoInputView");

            typedViewPrefabs.AddPrefab(ViewType.StartView, startViewPrefab);
            ShowLoading();
        }

        private void ShowLoading() {
            if(GameMode.GameModeName == GameModeName.Loading ) {
                if(SceneManager.GetActiveScene().name == "LoadingScene") {
                    Show(ViewType.StartView);
                } else {
                    StartCoroutine(WaitForLoadingSceneImpl());
                }
            }
        }

        private IEnumerator WaitForLoadingSceneImpl() {
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "LoadingScene");
            Show(ViewType.StartView);
        }

        private void SortByDepth(CanvasType type) {
            List<BaseView> list = new List<BaseView>(typedViewInstances.Count + namedViewInstances.Count);
            list.AddRange(typedViewInstances.Values.Where(v => v.CanvasType == type).ToList());
            list.AddRange(namedViewInstances.Values.Where(v => v.CanvasType == type).ToList());
            Transform canvasTransform = GetCanvasTransform(type);
            LegacyUIView[] legacyViews = canvasTransform.GetComponentsInChildren<LegacyUIView>();
            if (legacyViews.Length > 0) {
                list.AddRange(legacyViews);
            }

            list.Sort((v1, v2) => {
                return v1.SortDepth.CompareTo(v2.SortDepth);
            });
            for (int i = 0; i < list.Count; i++) {
                if (list[i] && list[i].gameObject) {
                    list[i].GetComponent<RectTransform>().SetSiblingIndex(i);
                }
            }
        }

        private bool IsModal(ViewType viewType ) {
            if(typedViewInstances.ContainsKey(viewType)) {
                TypedView instance = typedViewInstances[viewType];
                if(instance) {
                    return instance.IsModal;
                }
            }
            return false;
        }

        #region IViewService

        public bool IsNoModalAndLegacyViews
            => ModalCount == 0 && LegacyCount == 0;

        public int NextViewDepth
            => MaxSortDepth + 1;

        public int MaxSortDepth {
            get {
                int maxValue = int.MinValue;
                foreach(var kvp in typedViewInstances) {
                    if(kvp.Value.SortDepth > maxValue ) {
                        maxValue = kvp.Value.SortDepth;
                    }
                }
                foreach(var kvp in namedViewInstances ) {
                    if(kvp.Value.SortDepth > maxValue ) {
                        maxValue = kvp.Value.SortDepth;
                    }
                }
                return maxValue;
            }
        }

        public int MaxModalSiblingIndex {
            get {
                int maxValue = int.MinValue;
                foreach(var kvp in typedViewInstances) {
                    if (kvp.Value.IsModal) {
                        int sibIndex = kvp.Value.transform.GetSiblingIndex();
                        if (sibIndex > maxValue) {
                            maxValue = sibIndex;
                        }
                    }
                }
                return maxValue;
            }
        }
        
        public void ShowDelayed(ViewType viewType, float delay, ViewData data = null) {
            StartCoroutine(ShowDelayedImpl(viewType, delay, data));
        }

        private IEnumerator ShowDelayedImpl(ViewType viewType, float delay, ViewData data) {
            yield return new WaitForSeconds(delay);
            Show(viewType, data);
        }
        public GameObject Show(ViewType viewType, ViewData data = null) {
            TypedView typedView = null;
            if (typedViewInstances.ContainsKey(viewType)) {
                var instance = typedViewInstances[viewType];
                instance.Setup(data);
                typedView = instance;
            } else {
                var prefab = typedViewPrefabs.GetPrefab(viewType);
                if (prefab != null) {
                    GameObject instance = GameObject.Instantiate<GameObject>(prefab);
                    typedView = instance.GetComponentInChildren<TypedView>();
                    if (typedView != null) {
                        typedViewInstances[viewType] = typedView;
                        instance.transform.SetParent(GetCanvasTransform(typedView.CanvasType), false);
                        if (data != null && data.ViewDepth.HasValue) {
                            typedView.SetViewDepth(data.ViewDepth.Value);
                        }
                        SortByDepth(typedView.CanvasType);
                        typedView.AnimIn();
                        typedView.Setup(data);
                        GameEvents.OnViewShowed(viewType);
                    } else {
                        throw new UnityException($"Not found TypedView on view => {viewType}");
                    }
                } else {
                    throw new UnityException($"Prefab for view type => {viewType} not founded");
                }
            }
            
            OpenedStack.Push(CurrentOpen);
            CurrentOpen = viewType;
            return typedView?.gameObject ?? null;
        }

        public GameObject Show(string name, ViewData data = null) {
            NamedView namedView = null;
            if (namedViewInstances.ContainsKey(name)) {
                var instance = namedViewInstances[name];
                instance.Setup(data);
                namedView = instance;
            } else {
                var prefab = namedViewPrefabs.GetPrefab(name);
                if (prefab != null) {
                    GameObject instance = GameObject.Instantiate<GameObject>(prefab);
                    namedView = instance.GetComponentInChildren<NamedView>();
                    if (namedView != null) {
                        namedViewInstances[name] = namedView;
                        instance.transform.SetParent(GetCanvasTransform(namedView.CanvasType), false);
                        if (data != null && data.ViewDepth.HasValue) {
                            namedView.SetViewDepth(data.ViewDepth.Value);
                        }
                        SortByDepth(namedView.CanvasType);
                        namedView.AnimIn();
                        namedView.Setup(data);
                    } else {
                        throw new UnityException($"Not found NamedView on view => {name}");
                    }
                } else {
                    throw new UnityException($"Prefab for view name => {name} not founded");
                }
            }
            return namedView?.gameObject ?? null;
        }

        public void Show(ViewType viewType, System.Func<bool> whenShow, System.Action<GameObject> onShowed = null, ViewData data = null) {
            StartCoroutine(ShowImpl(viewType, whenShow, onShowed, data));
        }

        private IEnumerator ShowImpl(ViewType viewType, System.Func<bool> whenShow, System.Action<GameObject> onShowed = null, ViewData data = null) {
            WaitUntil waitUntil = new WaitUntil(whenShow);
            yield return waitUntil;
            if (typedViewInstances.ContainsKey(viewType)) {
                var instance = typedViewInstances[viewType];
                instance.Setup(data);
                if (instance.gameObject) {
                    onShowed?.Invoke(instance.gameObject);
                }
            } else {
                var prefab = typedViewPrefabs.GetPrefab(viewType);
                if (prefab != null) {
                    GameObject instance = GameObject.Instantiate<GameObject>(prefab);
                    TypedView typedView = instance.GetComponentInChildren<TypedView>();
                    if (typedView != null) {
                        typedViewInstances[viewType] = typedView;
                        instance.transform.SetParent(GetCanvasTransform(typedView.CanvasType), false);
                        if (data != null && data.ViewDepth.HasValue) {
                            typedView.SetViewDepth(data.ViewDepth.Value);
                        }
                        SortByDepth(typedView.CanvasType);
                        typedView.AnimIn();
                        typedView.Setup(data);
                        onShowed?.Invoke(typedView.gameObject);
                        GameEvents.OnViewShowed(viewType);
                    } else {
                        throw new UnityException($"Not found TypedView on view => {viewType}");
                    }
                } else {
                    throw new UnityException($"Prefab for view type => {viewType} not founded");
                }
            }
        }

        public bool Remove(ViewType viewType, float delay = 0.0f) {
            if (typedViewInstances.ContainsKey(viewType)) {
                TypedView view = typedViewInstances[viewType];
                typedViewInstances.Remove(viewType);
           
                if (OpenedStack.Any()) 
                    CurrentOpen = OpenedStack.Pop();
                
                if (view && view.gameObject) {
                    if (Mathf.Approximately(delay, 0.0f)) {

                        view.OnViewRemove();
                        Destroy(view.gameObject);
                        GameEvents.OnViewHided(viewType);
                    } else {
                        StartCoroutine(RemoveImpl(view, delay));
                    }
                    return true;
                }
            }
            return false;
        }

        private IEnumerator RemoveImpl(BaseView view, float delay) {
            view.AnimOut();
            
            yield return new WaitForSeconds(delay);
            if (view && view.gameObject) {
                view.OnViewRemove();
                if(view is TypedView ) {
                    GameEvents.OnViewHided((view as TypedView).Type);
                }
                Destroy(view.gameObject);
            }
        }

        public bool Remove(string name, float delay = 0.0f) {
            if (namedViewInstances.ContainsKey(name)) {
                NamedView view = namedViewInstances[name];
                namedViewInstances.Remove(name);
                if (view && view.gameObject) {
                    if (Mathf.Approximately(delay, 0.0f)) {
                        view.OnViewRemove();
                        Destroy(view.gameObject);
                    } else {
                        StartCoroutine(RemoveImpl(view, delay));
                    }
                    return true;
                }
            }
            return false;
        }

        public bool Exists(ViewType viewType) {
            return typedViewInstances.ContainsKey(viewType);
        }

        public bool Exists(string viewName) {
            return namedViewInstances.ContainsKey(viewName);
        }

        public BosUIUtils Utils {get;} = new BosUIUtils();

        public T FindView<T>(ViewType type) where T : TypedView {
            if(typedViewInstances.ContainsKey(type)) {
                TypedView inst = typedViewInstances[type];
                return inst as T;
            }
            return null;
        }
        #endregion
    }

    public class ViewData {
        public object UserData { get; set; }
        public int? ViewDepth { get; set; } = null;
        public ViewType ParentView { get; set; } = ViewType.None;
        public ViewData ParentViewData { get; set; } = null;
    }

    public interface IViewService : IGameService, ILegacyViewCounter {
        GameObject Show(ViewType viewType, ViewData data = null);
        GameObject Show(string name, ViewData data = null);
        void Show(ViewType viewType, System.Func<bool> whenShow, System.Action<GameObject> onShowed = null, ViewData data = null);
        void ShowDelayed(ViewType viewType, float delay, ViewData data = null);

        bool Remove(ViewType viewType, float delay = 0.0f);
        bool Remove(string name, float delay = 0.0f);
        bool Exists(ViewType viewType);
        bool Exists(string viewName);

        BosUIUtils Utils {get;}
        int ModalCount { get; }
        int NextViewDepth { get; }
        Transform GetCanvasTransform(CanvasType canvasType);

        ViewType GetCurrentViewType();
        T FindView<T>(ViewType type) where T : TypedView;

        bool IsNoModalAndLegacyViews { get; }

        /// <summary>
        /// Return Max Sort Depth (TOP View) between existings views
        /// </summary>
        int MaxSortDepth { get; }
        int MaxModalSiblingIndex { get;  }
    }

    public interface ILegacyViewCounter {
        void PushLegacy(string name);
        void PopLegacy(string name);
        int LegacyCount { get; }
    }

    public enum ViewType : int {
        None = 0,
        HeaderGameView = 1,
        PlanetsView = 2,
        BuyModuleView = 4,
        ManagementView = 7,
        BankView = 8,
        InvestorsView = 12,
        InvestorConfirmMessageBox = 13,
        HistoryView = 14,
        LoadingView = 15,
        StatsView = 16,
        ProfileView = 17,
        BadgeInfoView = 18,
        MenuFooterView = 19,
        //StoreView = 20,
        CoinRequiredView = 21,
        MainView = 22,
        HelpView = 23,
        FirstTimeView = 24,
        ManagersView = 25,
        RewardsView = 26,
        UpgradesView = 27,
        SocialView = 28,
        EndGameView = 29,
        TutorialDialogView = 30,
        EnahnceManagerView = 31,
        StartView = 32,
        ReconnectView = 33,
        X2ProfitView = 34,
        FirstInvestorView = 35,
        RateAppView = 36,
        WaitAdView = 37,
        ZTHAdView = 38,
        TransferWarningView = 39,
        BackToGameView = 40,
        BankNotify = 41,
        DebugView = 42,
        DependGeneratorView = 43,
        SpecialOfferView = 44,
        IAPLoadingView = 45,
        BosEventView = 46,
        MiniGameView = 47,
        TransportInfoView = 48,
        PromoInputView = 49
        //SpecialOfferButton = 45
    }

    public enum CanvasType {
        UI,
        Game
    }
}