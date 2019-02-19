namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Bos.Ioc;
    using Bos.Services;
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UDebug = UnityEngine.Debug;

    public class GameServices : Singleton<GameServices>, IBosServiceCollection {

        public const string kLoadingPlanetKey = "loading_planet";

        private static bool created = false;
        private string previousSceneName = string.Empty;
        private readonly Dictionary<System.Type, object> services = new Dictionary<System.Type, object>();
        public bool IsInitialized { get; private set; }


        private global::Currency currentCurrency = Currencies.DefaultCurrency;
        public global::Currency Currency {get; private set;}
        private GameManager legacyGameManager = null;
        private GameUI legacyGameUI = null;
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public GameManager LegacyGameManager
            => (legacyGameManager != null) ? legacyGameManager : (legacyGameManager = FindObjectOfType<GameManager>());

        public GameUI LegacyGameUI
            => (legacyGameUI != null) ? legacyGameUI : (legacyGameUI = FindObjectOfType<GameUI>());

        public static void ResetCreated()
            => created = false;

        private void Awake() {
            if(!created) {
                DontDestroyOnLoad(gameObject);
                created = true;
            } else {
                Destroy(gameObject);
                return;
            }
            
            UDebug.Log($"Device resolution {Screen.width}x{Screen.height}".Colored(ConsoleTextColor.navy));
            Currency = Currencies.DefaultCurrency;
            LevelDb.Load();
            
            RegisterServices();
            SetupServices();
            StartCoroutine(WaitForTimeServiceImpl());            

            previousSceneName = SceneManager.GetActiveScene().name;
            SceneManager.activeSceneChanged += (oldScene, newScene) => {
                if(newScene.name == "LoadingScene" || newScene.name == "Restart") {
                    GameModeService.SetGameMode(GameModeName.Loading);
                } else if(newScene.name == "GameScene") {
                    GameModeService.SetGameMode(GameModeName.Game);
                } else if(newScene.name == "ManagerSlot") {
                    GameModeService.SetGameMode(GameModeName.ManagerSlot);
                } else if(newScene.name == "MoneySink_SlotMachine") {
                    GameModeService.SetGameMode(GameModeName.SlotGame);
                } else if(newScene.name == "MoneySink_RacingGame") {
                    GameModeService.SetGameMode(GameModeName.RaceGame);
                } else if(newScene.name == "SplitLiner") {
                    GameModeService.SetGameMode(GameModeName.SplitLiner);
                }
                else {
                    GameModeService.SetGameMode(GameModeName.None);
                }
            };
            IsInitialized = true;
        }

        private IEnumerator WaitForTimeServiceImpl() {
            (TimeService as SaveableGameBehaviour).Register();
            TimeService.StartService();
            
            yield return new WaitUntil(() => TimeService.IsValid );
            RegisterSaveables();
        }

        private void OnApplicationPause(bool pause) {
            if(pause) {
                SaveService?.SaveAll();
            }
        }

        private void OnApplicationFocus(bool focus) {
            if(!focus) {
                SaveService?.SaveAll();
            }
        }

        private void OnApplicationQuit() {
            SaveService?.SaveAll();
        }

        private void OnDestroy() {
            if(!Disposables.IsDisposed) {
                Disposables.Dispose();
            }
        }


        private void RegisterServices() {
            ResourceService =  Register<IResourceService>(GetComponent<ResourceService>());
            SaveService = Register<ISaveService>(GetComponent<SaveService>());
            ViewService = Register<IViewService>(GetComponent<ViewService>());
            GameModeService = Register<IGameModeService>(GetComponent<GameModeService>());
            Register<IConsoleService>(GetComponent<Console>());
            //Register<IPlayerDataUpgrader>(new PlayerDataUpgrader());
            Register<IShopUpgrader>(new ShopItemUpgrader());
            Register<IFacebookService>(GetComponent<FacebookService>());
            PlayerService = Register<IPlayerService>(GetComponent<PlayerService>());
            Register<INetService>(GetComponent<NetService>());
            PlanetService = Register<IPlanetService>(GetComponent<PlanetService>());
            GenerationService = Register<IGenerationService>(GetComponent<GenerationService>());
            SleepService = Register<ISleepService>(GetComponent<SleepService>());
            Register<IServerBalanceService>(GetComponent<ServerCheats>());
            Register<IX20BoostService>(GetComponent<X20BoostAlternativeService>());
            Modules = Register<IShipModuleService>(GetComponent<ShipModuleService>());
            TransportService = Register<ITransportUnitsService>(GetComponent<TransportUnitsService>());
            AchievmentService =  Register<IAchievmentServcie>(GetComponent<AchievmentService>());
            ManagerService = Register<IManagerService>(GetComponent<ManagerService>());
            MechanicService = Register<IMechanicService>(GetComponent<MechanicService>());
            InvestorService = Register<IInvestorService>(GetComponent<InvestorService>());
            SecretaryService = Register<ISecretaryService>(GetComponent<SecretaryService>());
            BankService = Register<IBankService>(GetComponent<BankService>());
            SoundService = Register<ISoundService>(GetComponent<SoundService>());
            TimeService = Register<ITimeService>(GetComponent<TimeManager>());
            UpgradeService = Register<IUpgradeService>(GetComponent<UpgradeService>());
            BadgeService = Register<IBadgeService>(GetComponent<BadgeService>());
            Register<ISocialService>(GetComponent<SocialService>());
            TutorialService = Register<ITutorialService>(GetComponent<TutorialService>());
            PrizeWheelService = Register<IPrizeWheelGameService>(GetComponent<PrizeWheelGameService>());
            TreasureHuntService = Register<ITreasureHuntService>(GetComponent<TreasureHuntService>());
            SplitService = Register<ISplitGameService>(GetComponent<SplitGameService>());
            RewardsService = Register<IRewardsService>(GetComponent<RewardsService>());
            StoreService = Register<IStoreService>(GetComponent<StoreService>());
            TimeChangeService = Register<ITimeChangeService>(GetComponent<TimeChangeService>());
            TempMechanicService = Register<ITempMechanicService>(GetComponent<TempMechanicService>());
            AuditorService = Register<IAuditorService>(GetComponent<AuditorService>());
            VersionUpdateService = Register<IVersionUpdateService>(GetComponent<VersionUpdateService>());
            X2ProfitService = Register<IX2ProfitService>(GetComponent<X2ProfitService>());
            Register<IRateService>(GetComponent<RateService>());
            AdService = Register<IAdService>(GetComponent<AdService>());
            ZTHADService = Register<IZTHADService>(GetComponent<ZTHADService>());
            Register<ILocalPushService>(GetComponent<LocalPushService>());
            Register<ISpecialOfferService>(GetComponent<SpecialOfferService>());
            Inap = Register<IInapService>(GetComponent<InapService>());
            Register<IPromoService>(new PromoService());
        }

        private void SetupServices() {
            foreach(var pair in services ) {
                if (pair.Value is ISaveService) {
                    (pair.Value as IGameService).Setup(new PlayerPrefsStorage());
                } else {
                    (pair.Value as IGameService).Setup();
                }
            }
        }

        private void RegisterSaveables() {
            StartCoroutine(RegisterSaveablesImpl());
        }

        private IEnumerator RegisterSaveablesImpl() {
            yield return new WaitUntil(() => ResourceService.IsLoaded);

            foreach (var pair in services) {
                if ((pair.Value is SaveableGameBehaviour)) {
                    ((SaveableGameBehaviour)pair.Value).Register();
                } else if (pair.Value is SaveableGameElement) {
                    ((SaveableGameElement)pair.Value).Register();
                } else if (pair.Value is ISaveable) {
                    SaveService.Register(((ISaveable)pair.Value));
                }
            }

            StartUpdateResume();
        }

        /// <summary>
        /// Explicit call UpdateResume on Start, its required for iOS, where is problem with call OnApplicationPause(), OnApplicationFocus() when application first starts
        /// </summary>
        private void StartUpdateResume() {
            UDebug.Log($"{nameof(GameServices)}.{nameof(StartUpdateResume)}()");
            foreach(var service in services) {
                if(service.Value is IGameService ) {
                    IGameService gameService = service.Value as IGameService;
                    //UDebug.Log($"Update resume on service: {gameService.GetType().Name}");
                    gameService.UpdateResume(false);
                }
            }
            //UDebug.Log("==================================================");
        }
        

        private T Register<T>(object service) where T : class, IGameService {
            if (service is T) {
                services[typeof(T)] = service;
            } else {
                throw new UnityException($"Invalid service instance, expect => {typeof(T).Name}, actual => {service.GetType().Name}");
            }
            return (service as T);
        }

        #region IBosServiceCollection
        public T GetService<T>() where T : class {
            if (services.ContainsKey(typeof(T))) {
                return services[typeof(T)] as T;
            }
            //UDebug.LogError($"Missing service => {typeof(T).Name}");
            return default(T);
        }

        public IResourceService ResourceService { get; private set; }
        public ISaveService SaveService { get; private set; }
        public IViewService ViewService { get; private set; }
        public IGameModeService GameModeService { get; private set; }
        public IPlayerService PlayerService { get; private set; }
        public IGenerationService GenerationService { get; private set; }
        public ITransportUnitsService TransportService { get; private set; }
        public IManagerService ManagerService { get; private set; }
        public IInvestorService InvestorService { get; private set; }
        public IMechanicService MechanicService { get; private set; }
        public IPlanetService PlanetService { get; private set; }
        public ISecretaryService SecretaryService { get; private set; }
        public ISoundService SoundService { get; private set; }
        public IUpgradeService UpgradeService { get; private set; }
        public IAchievmentServcie AchievmentService { get; private set; }
        public IBadgeService BadgeService { get; private set; }
        public ISleepService SleepService { get; private set; }
        public IBankService BankService { get; private set; }

        public ITimeService TimeService { get; private set; }
        public ITutorialService TutorialService { get; private set; }
        public IPrizeWheelGameService PrizeWheelService { get; private set; }
        public ITreasureHuntService TreasureHuntService { get;  private set; }
        public IRewardsService RewardsService { get; private set; }
        public IStoreService StoreService { get; private set; }
        public IShipModuleService Modules { get; private set; }
        public ITempMechanicService TempMechanicService { get; private set; }
        public IAuditorService AuditorService { get; private set; }
        public IVersionUpdateService VersionUpdateService { get; private set; }
        public IAdService AdService { get; private set; }
        public IZTHADService ZTHADService { get; private set; }
        public IX2ProfitService X2ProfitService { get; private set; }
        public IInapService Inap { get; private set; }

        public CompositeDisposable Disposables
            => disposables;

        public ISplitGameService SplitService {
            get;
            private set;
        }
        public ITimeChangeService TimeChangeService { get; private set; }

        public void RunCoroutine(IEnumerator coroutine) 
            => StartCoroutine(coroutine);

        public void Execute(System.Action action, float delay ) {
            StartCoroutine(ExecuteImpl(action, delay));
        }

        private IEnumerator ExecuteImpl(System.Action action, float delay) {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        public void ExecuteWhen(System.Action action, System.Func<bool> predicate) {
            StartCoroutine(ExecuteWhenImpl(action, predicate));
        }

        private IEnumerator ExecuteWhenImpl(System.Action action, System.Func<bool> predicate) {
            yield return new WaitUntil(predicate);
            action?.Invoke();
        }

        public void SetLoadingPlanet(int planetId)
            => PlayerPrefs.SetInt(kLoadingPlanetKey, planetId);

        public int LoadingPlanet
            => PlayerPrefs.GetInt(kLoadingPlanetKey, 0);

        #endregion
    }

    public interface IServiceCollection {
        T GetService<T>() where T : class;
    }

    public interface IBosServiceCollection : IServiceCollection, ICoroutineContext {
        IResourceService ResourceService { get; }
        ISaveService SaveService { get; }
        IViewService ViewService { get; }
        IGameModeService GameModeService { get; }
        IPlayerService PlayerService { get; }
        //IContainer Container { get; }
        IGenerationService GenerationService { get; }
        ITransportUnitsService TransportService { get; }
        IManagerService ManagerService { get; }
        IInvestorService InvestorService { get; }
        IMechanicService MechanicService { get; }
        IPlanetService PlanetService { get; }
        ISecretaryService SecretaryService { get; }
        ISoundService SoundService { get; }
        IUpgradeService UpgradeService { get; }
        IAchievmentServcie AchievmentService { get; }
        IBadgeService BadgeService { get; }
        ISleepService SleepService { get; }
        ITutorialService TutorialService { get; }
        IBankService BankService { get; }
        IPrizeWheelGameService PrizeWheelService { get; }
        ISplitGameService SplitService { get; }
        ITreasureHuntService TreasureHuntService { get; }
        IRewardsService RewardsService { get; }
        IStoreService StoreService { get; }
        IShipModuleService Modules { get; }
        ITimeChangeService TimeChangeService { get; }
        ITempMechanicService TempMechanicService { get; }
        IAuditorService AuditorService { get; }

        ITimeService TimeService{get;}
        IVersionUpdateService VersionUpdateService {get;}
        IAdService AdService { get; }
        IZTHADService ZTHADService { get; }
        IX2ProfitService X2ProfitService { get; }
        bool IsInitialized { get; }
        
        
        

        global::Currency Currency {get;}
        GameManager LegacyGameManager { get; }
        GameUI LegacyGameUI { get; }
        //AdManager LegacyAdManager { get; }

        void SetLoadingPlanet(int id);
        int LoadingPlanet { get; }
        CompositeDisposable Disposables { get; }
        IInapService Inap { get; }
    }

    public interface ICoroutineContext {
        void RunCoroutine(IEnumerator coroutine);
        void Execute(System.Action action, float delay);
        void ExecuteWhen(System.Action action, System.Func<bool> predicate);
    }

    public interface IGameService {
        void Setup(object data = null);
        void UpdateResume(bool isPause);
    }

}