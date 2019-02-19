namespace Bos.Data {
    using Bos.Debug;
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class ResourceService : GameBehaviour, IResourceService {

        public bool isWaitKeySpaceEvent = true;
        public bool isLoadFromLocalFiles = false;
        public Transform spriteContainersParent;

        private bool isWasSpaceKey = false;


        private const string kGeneratorFile = "Data/generator";
        private const string kManagerFile = "Data/manager";
        private readonly string[] kLocalizationFiles = new string[] { "Data/localization", "Data/localization2" };

        private const string kPlanetsFile = "Data/planets";
        private const string kDefaultsFile = "Data/defaults";
        private const string kPlanetNameFile = "Data/planet_name";
        private const string kShipModuleNameFile = "Data/ship_modules";
        private const string kModuleNameFile = "Data/ship_module_names";
        private const string kMechanicsFile = "Data/mechanics";
        private const string kSecretariesFile = "Data/secretaries";
        private const string kStrengthFile = "Data/strengths";
        private const string kBankFile = "Data/bank";
        private const string kPlanetGeneratorFile = "Data/planet_generator";
        private const string kGeneratorLocalDataFile = "Data/generator_local_data";
        private const string kSprites = "Data/sprites";
        private const string kPersonalImprovements = "Data/personal_improvements";
        private const string kLocalProducts = "Data/local_products";
        private const string kManagerLocalData = "Data/manager_local_data";
        private const string kManagerImprovements = "Data/manager_improvements";
        private const string kCashUpgrades = "Data/cash_upgrade";
        private const string kSecurityUpgrades = "Data/investor_upgrade";
        private const string kCoinUpgrades = "Data/coin_upgrade";
        private const string kPlayerIcons = "Data/player_icons";
        private const string kProducts = "Data/products";
        private const string kPersonalProducts = "Data/personal_purchases";

        private const string kStatusNames = "Data/status_names";
        private const string kRocketProb = "Data/rocket_prob";
        private const string kRocketUpgarde = "Data/rocket_upgarde";
        private const string kProfiles = "Data/profiles";




        public IGeneratorDataRepository Generators { get; } = new GeneratorDataRepository();
        public IManagerRepository Managers { get; } = new ManagerDataRepository();

        public ILocalizationRepository Localization { get;  } = new LocalizationStringRepository();
        public IPrefabRepository<string> Prefabs { get; } = new PrefabRepository<string>();
        public IPlanetDataRepository PlanetRepository { get; } = new PlanetDataRepository();
        public IPlanetNameRepository PlanetNameRepository { get; } = new PlanetNameRepository();
        public IShipModuleRepository ShipModuleRepository { get; } = new ShipModuleDataRepository();
        public IModuleNameRepository ModuleNameRepository { get; } = new ModuleNameRepository();
        public IMechanicDataRepository MechanicDataRepository { get; } = new MechanicDataRepository();
        public ISecretaryDataRepository SecretaryDataRepository { get; } = new SecretaryDataRepository();
        public IUnitStrengthRepository UnitStrengthDataRepository { get; } = new UnitStrengthRepository();
        public IBankLevelReporitory BankLevelRepository { get; } = new BankLevelRepository();
        public IGeneratorLocalDataRepository GeneratorLocalData { get; } = new GeneratorLocalDataRepository();
        public IPersonalImprovementsRepository PersonalImprovements { get; } = new PersonalImprovementsRepository();
        public ILocalProductDataRepository LocalProducts { get; } = new LocalProductDataRepository();

        private readonly ISpritePathRepository spritePathRepository = new SpritePathRepository();
        public IManagerLocalJsonDataRepository ManagerLocalDataRepository { get; } = new ManagerLocalJsonDataRepository();
        public IManagerImprovementRepository ManagerImprovements { get; } = new ManagerImprovementRepository();

        public IUpgradeRepository CashUpgrades { get; } = new UpgradeRepository();
        public IUpgradeRepository SecuritiesUpgrades { get; } = new UpgradeRepository();
        public ICoinUpgradeRepository CoinUpgrades { get; } = new CoinUpgradeRepository();
        public IPlayerIconRepository PlayerIcons { get; } = new PlayerIconRepository();

        public IStatusNameRepository StatusNames {get;} = new StatusNameRepository();
        public IStoreProductRepository Products { get; } = new StoreProductRepository();
        public IPersonalProductRepository PersonalProducts { get; } = new PersonalProductRepository();
        public IRocketUpgradeRepository RocketUpgradeRepository { get; } = new RocketUpgradeRepository();
        
        public AudioCache Audio { get; } = new AudioCache();


        //public SpriteCache Sprites { get; } = new SpriteCache();
        public ISpriteCache Sprites { get; } = new SpriteContainerManager();

        private readonly List<IRepository> repositories = new List<IRepository>();

        public DefaultSettings Defaults { get; private set; }
        public MaterialCache Materials { get; } = new MaterialCache();
        public IRocketData RocketData { get; } = new RocketData();
        public IProfileRepository Profiles { get; } = new GameProfileManager();


        private bool isPlanetsLoaded = false;
        private bool isShipModulesLoaded = false;
        private bool isMechanicsLoaded = false;
        private bool isSecretariesLoaded = false;
        private bool isUnitStrengthLoaded = false;
        private bool isBankLoaded = false;
        private bool isGeneratorsLoaded = false;
        private bool isManagersLoaded = false;
        private bool isPlanetGeneratorsLoaded = false;
        private bool isPersonalImprovementsLoaded = false;
        private bool isManagerImprovementsLoaded = false;

        public void SetGeneratorsLoaded() {
            isGeneratorsLoaded = true;
            GameEvents.OnNetResourceLoaded("generators");
        }

        public void SetManagersLoaded() {
            isManagersLoaded = true;
            GameEvents.OnNetResourceLoaded("managers");
        }


        private void LoadDefaults() {
            Defaults = JsonConvert.DeserializeObject<DefaultSettings>(Resources.Load<TextAsset>(kDefaultsFile).text);
        }

        private readonly Dictionary<string, SpritePathData> spriteTempIcons = new Dictionary<string, SpritePathData> {
            ["earth_icon"] = new SpritePathData { container = "planets", name = "earth", id = "earth_icon" },
            ["moon_icon"] = new SpritePathData { container = "planets", name = "moon", id = "moon_icon" },
            ["mars_icon"] = new SpritePathData { container = "planets", name = "mars", id = "mars_icon" },
            ["asteroid_icon"] = new SpritePathData { container = "planets", name = "asteroid", id = "asteroid_icon" },
            ["europe_icon"] = new SpritePathData { container = "planets", name = "europe", id = "europe_icon" },
            ["titan_icon"] = new SpritePathData { container = "planets", name = "titan", id = "titan_icon" },
            ["level_progress_back"] = new SpritePathData { container = "misc", name = "progress_bg", id = "level_progress_back" },
            ["level_progress_moon"] = new SpritePathData { container = "moon", name = "progress_full_moon", id = "level_progress_moon" },
            ["level_progress_mars"] = new SpritePathData { container = "mars", name = "progress_full_mars", id = "level_progress_mars" },
            ["level_progress_asteroid"] = new SpritePathData { container = "belt", name = "progress_full_asteroid", id = "level_progress_asteroid" },
            ["level_progress_europe"] = new SpritePathData { container = "europe", name = "progress_full_europe", id = "level_progress_europe" },
            ["level_progress_titan"] = new SpritePathData { container = "titan", name = "progress_full_titan", id = "level_progress_titan" },
            ["coins"] = new SpritePathData { container = "misc", name = "coins", id = "coins" },
            ["company_cash"] = new SpritePathData { container = "misc", name = "company_cash", id = "company_cash" },
            ["player_cash"] = new SpritePathData { container = "misc", name = "player_cash", id = "player_cash" },
            ["securities"] = new SpritePathData { container = "misc", name = "securities", id = "securities" },
            ["but_plus"] = new SpritePathData { container = "misc", name = "but_plus", id = "but_plus" },
            ["alert_kickback"] = new SpritePathData { container = "misc", name = "alert_KickBack", id = "alert_kickback" },
            ["transparent"] = new SpritePathData { container = "misc", name = "transparent", id = "transparent" }
        };

        public void Setup(object data = null) {
            LoadDefaults();

            (Sprites as SpriteContainerManager).Setup(spriteContainersParent, new Dictionary<string, string> {
                ["badges"] = "Prefabs/SpriteContainers/Badges",
                ["misc"] = "Prefabs/SpriteContainers/Misc",
                ["planets"] = "Prefabs/SpriteContainers/Planets",
                ["earth"] = "Prefabs/SpriteContainers/Earth",
                ["moon"] = "Prefabs/SpriteContainers/Moon",
                ["mars"] = "Prefabs/SpriteContainers/Mars",
                ["belt"] = "Prefabs/SpriteContainers/Belt",
                ["europe"] = "Prefabs/SpriteContainers/Europe",
                ["titan"] = "Prefabs/SpriteContainers/Titan"
            });

            repositories.Add(Generators);
            repositories.Add(Managers);
            repositories.Add(Localization);
            repositories.Add(PlanetRepository);
            repositories.Add(PlanetNameRepository);
            repositories.Add(ShipModuleRepository);
            repositories.Add(ModuleNameRepository);
            repositories.Add(MechanicDataRepository);
            repositories.Add(SecretaryDataRepository);
            repositories.Add(UnitStrengthDataRepository);
            repositories.Add(BankLevelRepository);
            repositories.Add(GeneratorLocalData);
            repositories.Add(PersonalImprovements);
            repositories.Add(LocalProducts);
            repositories.Add(ManagerLocalDataRepository);
            repositories.Add(ManagerImprovements);
            repositories.Add(CashUpgrades);
            repositories.Add(SecuritiesUpgrades);
            repositories.Add(CoinUpgrades);
            repositories.Add(PlayerIcons);
            repositories.Add(StatusNames);
            repositories.Add(Products);
            repositories.Add(PersonalProducts);
            repositories.Add(RocketData);
            repositories.Add(RocketUpgradeRepository);
            repositories.Add(Profiles);

            Localization.Setup(SystemLanguage.English);
            Load();
            Prefabs.AddPath("coins", "Prefabs/Effects/Coins");
            Prefabs.AddPath("movetext", "Prefabs/Effects/MoveText");
            Prefabs.AddPath("wincoin", "Prefabs/Effects/wincoin");
            Prefabs.AddPath("lastsibling", "Prefabs/lastsibling");
            Prefabs.AddPath("finger", "Prefabs/UI/TutorialFingerView");
            Prefabs.AddPath("accum_text", "Prefabs/Effects/AccumulatedText");
            Prefabs.AddPath("buy_er", "Prefabs/Effects/buy_er");
            Prefabs.AddPath("highlightregion", "Prefabs/Effects/HighlightRegion");
            Prefabs.AddPath("highlightarea", "Prefabs/Effects/HighlightArea");

            Audio.AddPath(SoundName.GameBackMusic, "Audio/GameBackMusic");
            Audio.AddPath(SoundName.SplitBackMusic, "Audio/SplitBackMusic");
        }

        public void UpdateResume(bool pause) { }



        public Sprite GetSpriteByKey(string key) {

            var optPath = spritePathRepository.GetPath(key);
            return optPath.Match(() => {
                Debug.LogError($"No sprite path for key => {key}");
                return Sprites.FallbackSprite;
            }, (path) => {
                return GetSprite(path);
            });
        }

        public Sprite GetCurrencySprite(Currency currency) {
            return GetCurrencySprite(currency.Type);
        }



        public Sprite GetCurrencySprite(CurrencyType type) {
            switch (type) {
                case CurrencyType.Coins:
                    return GetSpriteByKey("coins");
                case CurrencyType.CompanyCash:
                    return GetSpriteByKey("company_cash");
                case CurrencyType.PlayerCash:
                    return GetSpriteByKey("player_cash");
                case CurrencyType.Securities:
                    return GetSpriteByKey("securities");
                default:
                    throw new ArgumentException($"unsupported currency type => {type}");
            }
        }

        public Sprite GetSprite(SpritePathData data) {

            return Sprites.GetSprite(data);
        }

        //private IEnumerator WaitKeySpaceImpl() {
        //    if(isWaitKeySpaceEvent) {
        //        yield return new WaitUntil(() => isWasSpaceKey);
        //        isWasSpaceKey = false;
        //    }
        //}

        private void LoadPlanetsLocal() {
            PlanetRepository.Load(kPlanetsFile);
            Debug.Log("planets setted fallback from file".Colored(ConsoleTextColor.yellow));
            GameEvents.OnPlanetsReceivedFromServer();
            isPlanetsLoaded = true;
        }

        private void LoadShipModulesLocal() {
            ShipModuleRepository.Load(kShipModuleNameFile);
            Debug.Log("modules setted from fallback file".Colored(ConsoleTextColor.yellow));
            isShipModulesLoaded = true;
        }

        private void LoadMechanicsLocal() {
            MechanicDataRepository.Load(kMechanicsFile);
            Debug.Log($"mechanics setted from local file => {MechanicDataRepository.Count}".Colored(ConsoleTextColor.yellow));
            isMechanicsLoaded = true;
        }

        private void LoadSecretariesLocal() {
            SecretaryDataRepository.Load(kSecretariesFile);
            Debug.Log($"secretaries setted from local file, count => {SecretaryDataRepository.Count}".Colored(ConsoleTextColor.yellow).Bold());
            isSecretariesLoaded = true;
        }

        private void LoadTransportStrengthLocal() {
            UnitStrengthDataRepository.Load(kStrengthFile);
            Debug.Log($"strengths setted from local file, count => {UnitStrengthDataRepository.Count}".Colored(ConsoleTextColor.yellow).Bold());
            isUnitStrengthLoaded = true;
        }

        private void LoadBankLocal() {
            BankLevelRepository.Load(kBankFile);
            Debug.Log($"bank setted from local file");
            isBankLoaded = true;
        }

        private void LoadPlanetTransportLocal() {
            Generators.AppendFromFile(kPlanetGeneratorFile, GeneratorType.Planet);
            Debug.Log($"planet generators setted from local file");
            isPlanetGeneratorsLoaded = true;
        }

        private void LoadPersonalImprovementsLocal() {
            PersonalImprovements.Load(kPersonalImprovements);
            Debug.Log($"personal improvements setted from local file".BoldItalic().Colored(ConsoleTextColor.navy));
            isPersonalImprovementsLoaded = true;
        }

        private void LoadManagerImprovementsLocal() {
            ManagerImprovements.Load(kManagerImprovements);
            Debug.Log($"manager improvements setted from local file".BoldItalic().Colored(ConsoleTextColor.navy));
            isManagerImprovementsLoaded = true;
        }

        private IEnumerator LoadNetResourcesImpl() {

            if(isLoadFromLocalFiles ) {
                LoadPlanetsLocal();
                LoadShipModulesLocal();
                LoadMechanicsLocal();
                LoadSecretariesLocal();
                LoadTransportStrengthLocal();
                LoadBankLocal();
                LoadPlanetTransportLocal();
                LoadPersonalImprovementsLocal();
                LoadManagerImprovementsLocal();
                isManagersLoaded = isGeneratorsLoaded = true;
                Debug.Log($"Resources is loade => {IsLoaded}".Colored(ConsoleTextColor.orange));
                Debug.Log(GetResourceLoadingStateString());

            } else {
                Services.GetService<IServerBalanceService>().GetBalanceFromServer();
                yield return new WaitUntil(() => isManagersLoaded && isGeneratorsLoaded);

                INetService netService = Services.GetService<INetService>();
                netService.GetBalance(planets => {
                    PlanetRepository.SetFromExternalDataSource(planets);
                    Debug.Log("planets setted from external data source".Colored(ConsoleTextColor.yellow));
                    GameEvents.OnPlanetsReceivedFromServer();
                    GameEvents.OnNetResourceLoaded("planets");
                    isPlanetsLoaded = true;
                }, error => {
                    Debug.Log(error.Colored(ConsoleTextColor.red));
                    LoadPlanetsLocal();
                });
                yield return new WaitUntil(() => isPlanetsLoaded);



                netService.GetBalanceShip(modules => {
                    ShipModuleRepository.SetFromExternalDataSource(modules);
                    //Debug.Log("modules setted from external data source".Colored(ConsoleTextColor.yellow));
                    isShipModulesLoaded = true;
                    GameEvents.OnNetResourceLoaded("space ship modules");
                }, error => {
                    LoadShipModulesLocal();
                });
                yield return new WaitUntil(() => isShipModulesLoaded);



                netService.GetBalanceMechanic(mechanics => {
                    MechanicDataRepository.SetFromExternalDataSource(mechanics);
                    isMechanicsLoaded = true;
                    GameEvents.OnNetResourceLoaded("mechanics");
                }, error => {
                    LoadMechanicsLocal();
                });
                yield return new WaitUntil(() => isMechanicsLoaded);



                netService.GetSecretaryBalance(secretaries => {
                    SecretaryDataRepository.SetFromExternalDataSource(secretaries);
                    isSecretariesLoaded = true;
                    GameEvents.OnNetResourceLoaded("secretaries");
                }, error => {
                    LoadSecretariesLocal();
                });
                yield return new WaitUntil(() => isSecretariesLoaded);


                netService.GetTransportStrength(strengs => {
                    UnitStrengthDataRepository.SetFromExternalSource(strengs);
                    isUnitStrengthLoaded = true;
                    GameEvents.OnNetResourceLoaded("transport strength");
                }, error => {
                    LoadTransportStrengthLocal();
                });
                yield return new WaitUntil(() => isUnitStrengthLoaded);


                netService.GetBank(bankLevels => {
                    BankLevelRepository.SetFromExternalDataSource(bankLevels);
                    isBankLoaded = true;
                    GameEvents.OnNetResourceLoaded("bank");
                }, error => {
                    LoadBankLocal();
                });
                yield return new WaitUntil(() => isBankLoaded);


                netService.GetPlanetsTransport(generators => {
                    Generators.Append(generators, GeneratorType.Planet);
                    isPlanetGeneratorsLoaded = true;
                    GameEvents.OnNetResourceLoaded("planet generators");
                }, (error) => {
                    LoadPlanetTransportLocal();
                });
                yield return new WaitUntil(() => isPlanetGeneratorsLoaded);

                /*
                netService.GetPersonalImprovements(personalImprovements => {
                    PersonalImprovements.SetFromExternalSource(personalImprovements);
                    isPersonalImprovementsLoaded = true;
                    GameEvents.OnNetResourceLoaded("personal_improvements");

                }, error => {
                    LoadPersonalImprovementsLocal();
                });*/
                LoadPersonalImprovementsLocal();

                netService.GetManagerImprovements(improvements => {
                    ManagerImprovements.SetFromExternalSource(improvements);
                    isManagerImprovementsLoaded = true;
                    GameEvents.OnNetResourceLoaded("manager improvements");
                }, error => {
                    LoadManagerImprovementsLocal();
                });
                yield break;
            }
            
        }

        public override void Update() {
            base.Update();
#if UNITY_EDITOR
            if (isWaitKeySpaceEvent) {
                if(Input.GetKeyUp(KeyCode.Space)) {
                    isWasSpaceKey = true;
                }
            }
#endif
        }

        public void Load() {
            Profiles.Load(kProfiles);
            Localization.Load(kLocalizationFiles);
            Generators.Load(kGeneratorFile);
            Managers.Load(kManagerFile);

            

            PlanetNameRepository.Load(kPlanetNameFile);
            ModuleNameRepository.Load(kModuleNameFile);
            GeneratorLocalData.Load(kGeneratorLocalDataFile);

            spritePathRepository.Load(kSprites);
            spritePathRepository.AddItems(spriteTempIcons.Values.ToList());

            /*
            foreach (var planet in PlanetNameRepository.Items) {
                Sprites.AddPath(planet.GetBgImageSpriteKey(), planet.bg_image_path);
                if(planet.ui_icon != null ) {
                    Sprites.AddPath(planet.ui_icon.id, planet.ui_icon.path);
                }
                if(planet.history_back != null ) {
                    Sprites.AddPath(planet.history_back.id, planet.history_back.path);
                }
            }*/

            LocalProducts.Load(kLocalProducts);

            /*
            foreach(var prod in LocalProducts.ProductCollection) {
                if(prod.icon != null ) {
                    Sprites.AddPath(prod.icon.id, prod.icon.path);
                }
            }*/

            ManagerLocalDataRepository.Load(kManagerLocalData);

            /*
            foreach(var manager in ManagerLocalDataRepository.ManagerCollection ) {
                foreach(var iconData in manager.icons ) {
                    if(iconData.active != null && !string.IsNullOrEmpty(iconData.active.path)) {
                        Sprites.AddPath(iconData.active.id, iconData.active.path);
                    }
                    if(iconData.disabled != null && !string.IsNullOrEmpty(iconData.disabled.path)) {
                        Sprites.AddPath(iconData.disabled.id, iconData.disabled.path);
                    }
                }
            }*/


            CashUpgrades.Load(kCashUpgrades);
            SecuritiesUpgrades.Load(kSecurityUpgrades);
            CoinUpgrades.Load(kCoinUpgrades);
            PlayerIcons.Load(kPlayerIcons);
            /*
            foreach(var spritePathData in PlayerIcons.IconPaths) {
                Sprites.AddPath(spritePathData.id, spritePathData.path);
            }*/

            StatusNames.Load(kStatusNames);
            Products.Load(kProducts);
            PersonalProducts.Load(kPersonalProducts);
            RocketData.Load(kRocketProb);
            RocketUpgradeRepository.Load(kRocketUpgarde);

            StartCoroutine(LoadNetResourcesImpl());
            //Debug.Log($"Count of module names => {ModuleNameRepository.ModuleNameCollection.Count()}".Colored(ConsoleTextColor.orange));
        }

        public void LoadPlanets(IEnumerable<PlanetServerData> planets = null) {
            if(planets == null ) {
                PlanetRepository.Load(kPlanetsFile);
            } else {
                PlanetRepository.SetFromExternalDataSource(planets);
            }
        }

        public bool IsLoaded {
            get {
                bool isLoaded = true;
                foreach(IRepository repo in repositories) {
                    if(false == repo.IsLoaded) {
                        isLoaded = false;
                        break;
                    }
                }
                return isLoaded &&
                    isPlanetsLoaded &&
                    isShipModulesLoaded &&
                    isMechanicsLoaded &&
                    isSecretariesLoaded &&
                    isUnitStrengthLoaded && 
                    isBankLoaded &&
                    isGeneratorsLoaded &&
                    isManagersLoaded && 
                    isPlanetGeneratorsLoaded &&
                    isPersonalImprovementsLoaded && 
                    isManagerImprovementsLoaded;
            }
        }

        private string GetResourceLoadingStateString() {
                bool isLoaded = true;
                foreach(IRepository repo in repositories) {
                    if(false == repo.IsLoaded) {
                        isLoaded = false;
                        break;
                    }
                }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"LOCAL => {isLoaded}");
            sb.AppendLine($"PLANETS => {isPlanetsLoaded}");
            sb.AppendLine($"SHIP MODULES => {isShipModulesLoaded}");
            sb.AppendLine($"MECHANICS => {isMechanicsLoaded}");
            sb.AppendLine($"SECRETARIES => {isSecretariesLoaded}");
            sb.AppendLine($"UNIT STRENGTH => {isUnitStrengthLoaded}");
            sb.AppendLine($"BANK => {isBankLoaded}");
            sb.AppendLine($"GENERATORS => {isGeneratorsLoaded}");
            sb.AppendLine($"MANAGERS => {isManagersLoaded}");
            sb.AppendLine($"PLANET GENERATORS => {isPlanetGeneratorsLoaded}");
            sb.AppendLine($"PERSONAL IMPROVEMENTS => {isPersonalImprovementsLoaded}");
            sb.AppendLine($"MANAGER IMPROVEMENTS => {isManagerImprovementsLoaded}");
            return sb.ToString();
        }

        public IPlanetDataRepository Planets
            => PlanetRepository;
    }

    public interface IResourceService : IGameService {
        void Load();
        ILocalizationRepository Localization { get; }
        IPrefabRepository<string> Prefabs { get; }
        IPlanetDataRepository Planets { get; }
        IPlanetNameRepository PlanetNameRepository { get; }
        IShipModuleRepository ShipModuleRepository { get; }
        IModuleNameRepository ModuleNameRepository { get; }
        IMechanicDataRepository MechanicDataRepository { get; }
        ISecretaryDataRepository SecretaryDataRepository { get; }
        IUnitStrengthRepository UnitStrengthDataRepository { get; }
        IBankLevelReporitory BankLevelRepository { get; }
        IGeneratorDataRepository Generators { get; }
        IGeneratorLocalDataRepository GeneratorLocalData { get; }
        IManagerRepository Managers { get; }
        IPersonalImprovementsRepository PersonalImprovements { get; }
        ILocalProductDataRepository LocalProducts { get; }
        IManagerLocalJsonDataRepository ManagerLocalDataRepository { get; }
        IManagerImprovementRepository ManagerImprovements { get; }
        IStoreProductRepository Products { get; }
        IPersonalProductRepository PersonalProducts { get; }
        IRocketUpgradeRepository RocketUpgradeRepository { get; } 
        
        void LoadPlanets(IEnumerable<PlanetServerData> planets = null);
        bool IsLoaded { get; }
        DefaultSettings Defaults { get; }
        ISpriteCache Sprites { get; }
        void SetGeneratorsLoaded();
        void SetManagersLoaded();

        Sprite GetCurrencySprite(Currency currency);
        Sprite GetCurrencySprite(CurrencyType type);
        Sprite GetSprite(SpritePathData data);
        Sprite GetSpriteByKey(string key);

        IUpgradeRepository CashUpgrades { get; }

        IUpgradeRepository SecuritiesUpgrades { get; }

        ICoinUpgradeRepository CoinUpgrades { get; }
        IPlayerIconRepository PlayerIcons { get; }

        IStatusNameRepository StatusNames {get;}
        AudioCache Audio { get; }
        MaterialCache Materials { get; }
        IRocketData RocketData { get; }
        IProfileRepository Profiles { get; }
    }

    public static class ResourceUtils {

        public static readonly Dictionary<PlanetType, string> LevelProgressPlanetMap = new Dictionary<PlanetType, string> {
            [PlanetType.Moon] = "level_progress_moon",
            [PlanetType.Mars] = "level_progress_mars",
            [PlanetType.Asteroid] = "level_progress_asteroid",
            [PlanetType.Europe] = "level_progress_europe",
            [PlanetType.Titan] = "level_progress_titan"
        };

        public static string LevelEmptyProgressForPlanets
            => "level_progress_back";
    }

    public interface ISpriteCache {
        Sprite GetSprite(SpritePathData pathData);
        Sprite GetSprite(string container, string name);
        Sprite FallbackSprite { get; }
        void RemovePlanetContainersExcept(int planetId);
    }

    public class SpriteContainerManager : ISpriteCache {

        private readonly Dictionary<string, string> containerPathMap = new Dictionary<string, string>();

        private Dictionary<string, SpriteContainer> Containers { get; } = new Dictionary<string, SpriteContainer>();
        private Transform Parent { get; set; }

        private readonly Dictionary<int, string> planetContainers = new Dictionary<int, string> {
            [0] = "earth",
            [1] = "moon",
            [2] = "mars",
            [3] = "belt",
            [4] = "europe",
            [5] = "titan"
        };

        public Sprite FallbackSprite
            => GetContainer("misc").GetSprite("transparent");

        public void Setup(Transform parent, Dictionary<string, string> containerMap ) {
            this.Parent = parent;
            containerPathMap.Clear();
            foreach(var kvp in containerMap) {
                containerPathMap.Add(kvp.Key, kvp.Value);
            }
        }



        public void RemovePlanetContainersExcept(int planetId) {
            foreach(var kvp in planetContainers) {
                if(kvp.Key != planetId ) {
                    RemoveContainer(kvp.Value);
                }
            }
        }

        private void RemoveContainer(string name) {
            if (Containers.ContainsKey(name)) {
                var target = Containers[name];
                if (Containers.Remove(name)) {
                    if (target && target.gameObject) {
                        GameObject.Destroy(target.gameObject);
                    }
                }
            }
        }

        public Sprite GetSprite(string container, string name)
            => GetSprite(new SpritePathData { container = container, name = name });

        public Sprite GetSprite(SpritePathData pathData ) {
            var container = GetContainer(pathData.container);
            if(container == null ) {
                Debug.LogError($"container {pathData.container} not founde with sprite name {pathData.name}");
                return FallbackSprite;
            } else {
                var sprite = container.GetSprite(pathData.name);
                if(sprite == null ) {
                    Debug.LogError($"Not found sprite {pathData.name} in container {pathData.container}");
                    return FallbackSprite;
                }
                return sprite;
            }
        }

        private SpriteContainer GetContainer(string containerName) {
            if (Containers.ContainsKey(containerName)) {
                return Containers[containerName];
            } else {
                if (containerPathMap.ContainsKey(containerName)) {
                    GameObject prefab = Resources.Load<GameObject>(containerPathMap[containerName]);
                    if (prefab != null) {
                        GameObject instance = GameObject.Instantiate(prefab, Parent);
                        SpriteContainer spriteContainer = instance.GetComponent<SpriteContainer>();
                        Containers.Add(spriteContainer.containerName, spriteContainer);
                        return spriteContainer;
                    }
                }
                return null;
            }
        }
    }
}