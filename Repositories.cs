namespace Bos.Data {
    using Bos.Debug;
    using Newtonsoft.Json;
    using Ozh.Tools.Functional;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public interface IGeneratorLocalDataRepository : IRepository {
        GeneratorLocalData GetLocalData(int generatorId);
        IEnumerable<GeneratorLocalData> LocalDatas { get; }
    }

    public class GeneratorLocalDataRepository : IGeneratorLocalDataRepository {

        public Dictionary<int, GeneratorLocalData> LocalData { get; } = new Dictionary<int, GeneratorLocalData>();


        public bool IsLoaded { get; private set; }

        public GeneratorLocalData GetLocalData(int generatorId) {
            return LocalData.ContainsKey(generatorId) ? LocalData[generatorId] : null;
        }

        public void Load(string file) {
            if(!IsLoaded) {
                LocalData.Clear();
                var listItems = JsonConvert.DeserializeObject<List<GeneratorLocalData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(li => LocalData.Add(li.id, li));

                listItems.ForEach(li => {
                    //Debug.Log($"generator local data {li.id}, names count => {li.names.Count}");
                });
                IsLoaded = true;
            }
        }

        public IEnumerable<GeneratorLocalData> LocalDatas
            => LocalData.Values;
    }


    public interface IGeneratorDataRepository : IRepository {
        void ReplaceValues(GeneratorJsonData data);
        void Append(IEnumerable<GeneratorData> generators, GeneratorType type);
        GeneratorData GetGeneratorData(int id);
        List<GeneratorData> GeneratorCollection { get; }
        int[] NormalIds { get; }
        void AppendFromFile(string file, GeneratorType type);
        Dictionary<int, GeneratorData> Generators { get; }
    }

    public class GeneratorDataRepository : IGeneratorDataRepository {

        public Dictionary<int, GeneratorData> Generators { get; } = new Dictionary<int, GeneratorData>();

        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            if(!IsLoaded) {
                Generators.Clear();
                var listItems = JsonConvert.DeserializeObject<List<GeneratorJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Generators.Add(item.id, new GeneratorData(item, GeneratorType.Normal)));
                IsLoaded = true;
            }
        }

        public void ReplaceValues(GeneratorJsonData data) {
            var generator = GetGeneratorData(data.id);
            generator?.Replace(
                data.baseCost, 
                data.incrementFactor, 
                data.baseGeneration, 
                data.timeToGenerate, 
                data.enhancePrice, 
                data.profitIncrementFactor);
            if(generator == null ) {
                Debug.LogError($"generator null when replaced => {data.id}");
            } else {
                Debug.Log($"generator => {data.id} replaced with data increment factor => {data.incrementFactor} on generator increment factor {generator.IncrementFactor}".Colored(ConsoleTextColor.green));
                GameEvents.GeneratorDataReplacedSubject.OnNext(generator);
            }
        }

        public void AppendFromFile(string file, GeneratorType type) {
            var listItems = JsonConvert.DeserializeObject<List<GeneratorJsonData>>(Resources.Load<TextAsset>(file).text);
            var generators = listItems.Select(li => new GeneratorData(li, type)).ToList();
            Append(generators, type);
        }

        public void Append(IEnumerable<GeneratorData> generators, GeneratorType type) {
            if(IsLoaded) {
                foreach(var gen in generators) {
                    //GeneratorData newData = new GeneratorData(jsonData, type);
                    if(!Generators.ContainsKey(gen.Id)) {
                        Generators.Add(gen.Id, gen);
                    } else {
                        throw new UnityException($"Repository already contains generator => {gen.Id}");
                    }
                }
            }
        }

        public GeneratorData GetGeneratorData(int id)
            => Generators.ContainsKey(id) ? Generators[id] : null;

        public List<GeneratorData> GeneratorCollection
            => Generators.Values.OrderBy(g => g.Type).ThenBy(g => g.Id).ToList();

        public int[] NormalIds
            => GeneratorCollection.Where(g => g.Type == GeneratorType.Normal).Select(g => g.Id).ToArray();

    }


    public interface IManagerRepository : IRepository {
        ManagerData GetManagerData(int id);
        List<ManagerData> ManagerCollections { get; }
        void UpdateValues(int managerId, double baseCost, double coef);
    }

    public class ManagerDataRepository : IManagerRepository {
        public Dictionary<int, ManagerData> Managers { get; } = new Dictionary<int, ManagerData>();
        public bool IsLoaded { get; private set; }

        public ManagerData GetManagerData(int id) {
            return Managers.ContainsKey(id) ? Managers[id] : null;
        }

        public List<ManagerData> ManagerCollections {
            get {
                return Managers.Values.OrderBy(m => m.Id).ToList();
            }
        }

        public void Load(string file) {
            if(!IsLoaded) {
                Managers.Clear();
                var listItems = JsonConvert.DeserializeObject<List<ManagerJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Managers.Add(item.id, new ManagerData(item)));
                IsLoaded = true;
            }
        }

        public void UpdateValues(int managerId, double baseCost, double coef) {
            var managerData = GetManagerData(managerId);
            if(managerData != null ) {
                managerData.UpdateValues(baseCost, coef);
                GameEvents.ManagerDataReplacedSubject.OnNext(managerData);
            } else {
                Debug.LogWarning($"not found manager for replacing, id: {managerId}");
            }
            //GetManagerData(managerId)?.UpdateValues(baseCost, coef);
        }
    }

    [System.Serializable]
    public class PlanetNameData {
        public int id;
        public string name;
        public string icon;
        public string research_bg;
        public SpritePathData bg_image_path;
        public SpritePathData ui_icon;
        public SpritePathData history_back;
        public int module_id;

        public bool IsModuleRequired
            => module_id >= 0;

    }


    public class ModuleNameData {
        public int id;
        public string name;
        public string icon;
    }

    public interface IModuleNameRepository : IRepository {
        string GetModuleNameId(int moduleId);
        ModuleNameData GetModuleNameData(int moduleId);
        IEnumerable<ModuleNameData> ModuleNameCollection { get; }
    }

    public class ModuleNameRepository : IModuleNameRepository {
        public Dictionary<int, ModuleNameData> ModuleNames { get; } = new Dictionary<int, ModuleNameData>();
        public bool IsLoaded { get; private set; }

        public ModuleNameData GetModuleNameData(int moduleId)
            => ModuleNames.ContainsKey(moduleId) ? ModuleNames[moduleId] : null;

        public string GetModuleNameId(int moduleId)
            => GetModuleNameData(moduleId)?.name ?? string.Empty;

        public IEnumerable<ModuleNameData> ModuleNameCollection
            => new List<ModuleNameData>(ModuleNames.Values);

        public void Load(string file) {
            if(!IsLoaded) {
                ModuleNames.Clear();
                var listItems = JsonConvert.DeserializeObject<List<ModuleNameData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => ModuleNames.Add(item.id, item));
                IsLoaded = true;
            }
        }
    }

    public interface IPlanetNameRepository: IRepository {
        string GetPlanetNameId(int planetId);
        PlanetNameData GetPlanetNameData(int planetId);
        IEnumerable<PlanetNameData> Items { get; }
    }



    public class PlanetNameRepository : IPlanetNameRepository {

        public Dictionary<int, PlanetNameData> PlanetNames { get; } = new Dictionary<int, PlanetNameData>();
        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            if(!IsLoaded) {
                PlanetNames.Clear();
                var listItems = JsonConvert.DeserializeObject<List<PlanetNameData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => PlanetNames.Add(item.id, item));
                IsLoaded = true;
            }
        }

        public string GetPlanetNameId(int planetId) 
            => PlanetNames.ContainsKey(planetId) ? PlanetNames[planetId].name : string.Empty;

        public PlanetNameData GetPlanetNameData(int planetId)
            => PlanetNames.ContainsKey(planetId) ? PlanetNames[planetId] : null;

        public IEnumerable<PlanetNameData> Items
            => PlanetNames.Values.OrderBy(p => p.id).ToList();


    }

    public class PlanetDataRepository : IPlanetDataRepository {
        public Dictionary<int, PlanetServerData> Planets { get; } = new Dictionary<int, PlanetServerData>();
        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            if(!IsLoaded) {
                Planets.Clear();
                var listItems = JsonConvert.DeserializeObject<List<PlanetJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Planets.Add(item.id, new PlanetServerData(item)));
                IsLoaded = true;
            }
        }

        public void SetFromExternalDataSource(IEnumerable<PlanetServerData> planets) {
            Planets.Clear();
            planets.ToList().ForEach(item => Planets.Add(item.Id, item));
            IsLoaded = true;

            foreach(var p in Planets.Values) {
                //Debug.Log($"{p.ToString()}".Colored(ConsoleTextColor.orange));
            }
        }

        public PlanetServerData GetPlanet(int id) {
            return Planets.ContainsKey(id) ? Planets[id] : null;
        }

        public IEnumerable<PlanetServerData> PlanetCollection
            => new List<PlanetServerData>(Planets.Values);

        public PlanetServerData GetPlanetForPlanetId(int planetId) {
            return GetPlanet(planetId);
        }
    }

    public class UnitStrengthRepository : IUnitStrengthRepository {
        public Dictionary<int, UnitStrengthData> Strengts { get; } 
            = new Dictionary<int, UnitStrengthData>();

        public IEnumerable<UnitStrengthData> StrengthCollection 
            => Strengts.Values.OrderBy(s => s.Id).ToList();

        public int Count => Strengts.Count;

        public bool IsLoaded { get; private set; } = false;

        public UnitStrengthData GetStrengthData(int generatorId) {
            return Strengts.ContainsKey(generatorId) ? Strengts[generatorId] : null;
        }

        public void Load(string file) {
            if(!IsLoaded) {
                Strengts.Clear();
                var listItems = JsonConvert.DeserializeObject<List<UnitStrengthJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Strengts.Add(item.id, new UnitStrengthData(item)));
                IsLoaded = true;
            }
        }

        public void SetFromExternalSource(IEnumerable<UnitStrengthData> strengths) {
            Strengts.Clear();
            strengths.ToList().ForEach(item => Strengts.Add(item.Id, item));
            IsLoaded = true;
        }
    }

    public class SecretaryDataRepository : ISecretaryDataRepository {

        public Dictionary<int, SecretaryData> Secretaries { get; } 
            = new Dictionary<int, SecretaryData>();

        public IEnumerable<SecretaryData> SecretaryCollection 
            => Secretaries.Values.OrderBy(secretary => secretary.PlanetId).ToList();

        public int Count
            => Secretaries.Count;

        public bool IsLoaded { get; private set; } = false;

        public SecretaryData GetSecretaryData(PlanetId planetId) 
            => Secretaries.ContainsKey(planetId.Id) ? Secretaries[planetId.Id] : null;

        public void Load(string file) {
            if(!IsLoaded) {
                Secretaries.Clear();
                var listItems = JsonConvert.DeserializeObject<List<SecretaryJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Secretaries.Add(item.planetId, new SecretaryData(item)));
                IsLoaded = true;
            }
        }

        public void SetFromExternalDataSource(IEnumerable<SecretaryData> secretaries) {
            Secretaries.Clear();
            secretaries.ToList().ForEach(item => Secretaries.Add(item.PlanetId, item));
            IsLoaded = true;
        }
    }

    public class MechanicDataRepository : IMechanicDataRepository {
        public Dictionary<int, MechanicData> Mechanics { get; } = new Dictionary<int, MechanicData>();
        public bool IsLoaded { get; private set; }

        public IEnumerable<MechanicData> MechanicCollection => Mechanics.Values.OrderBy(m => m.PlanetId).ToList();

        public MechanicData GetMechanicData(int planetId) {
            return Mechanics.ContainsKey(planetId) ? Mechanics[planetId] : null;
        }

        public void Load(string file) {
            if(!IsLoaded) {
                Mechanics.Clear();
                var listItems = JsonConvert.DeserializeObject<List<MechanicJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Mechanics.Add(item.planetId, new MechanicData(item)));
                IsLoaded = true;
            }
        }

        public void SetFromExternalDataSource(IEnumerable<MechanicData> mechanics) {
            Mechanics.Clear();
            mechanics.ToList().ForEach(item => Mechanics.Add(item.PlanetId, item));
            IsLoaded = true;
        }

        public int Count => Mechanics.Count;
    }

    public class BankLevelRepository : IBankLevelReporitory {
        public Dictionary<int, BankLevelData> BankLevels { get; } = new Dictionary<int, BankLevelData>();
        public bool IsLoaded { get; private set; } = false;

        public IEnumerable<BankLevelData> BankLevelCollection
            => BankLevels.Values.OrderBy(bl => bl.Level).ToList();

        public BankLevelData GetBankLevelData(int level) {
            return BankLevels.ContainsKey(level) ? BankLevels[level] : null;
        }

        public void Load(string file) {
            if(!IsLoaded) {
                BankLevels.Clear();
                var listItems = JsonConvert.DeserializeObject<List<BankLevelJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => BankLevels.Add(item.level, new BankLevelData(item)));
                IsLoaded = true;
            }
        }

        public void SetFromExternalDataSource(IEnumerable<BankLevelData> bankLevels) {
            BankLevels.Clear();
            bankLevels.ToList().ForEach(item => BankLevels.Add(item.Level, item));
            IsLoaded = true;
        }

    }

    public class ShipModuleDataRepository : IShipModuleRepository {
        public Dictionary<int, ShipModuleData> Modules { get; } = new Dictionary<int, ShipModuleData>();
        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            if(false == IsLoaded) {
                Modules.Clear();
                var listItems = JsonConvert.DeserializeObject<List<ShipModuleJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(item => Modules.Add(item.id, new ShipModuleData(item)));
                IsLoaded = true;
            }
        }

        public void SetFromExternalDataSource(IEnumerable<ShipModuleData> modules) {
            Modules.Clear();
            modules.ToList().ForEach(item => Modules.Add(item.Id, item));
            IsLoaded = true;
        }

        public ShipModuleData GetModule(int moduleId) {
            return Modules.ContainsKey(moduleId) ? Modules[moduleId] : null;
        }

        public IEnumerable<ShipModuleData> ModuleCollection
            => new List<ShipModuleData>(Modules.Values);
    }

    public class LocalizationStringRepository : ILocalizationRepository {

        private readonly Dictionary<uint, string> strings = new Dictionary<uint, string>();

        private readonly ObjectCache<uint, string> frequentSubcache = new ObjectCache<uint, string>();


        public SystemLanguage Language { get; private set; } = SystemLanguage.English;

        private string[] files = new string[] { };

        public void Setup(SystemLanguage language) {
            Language = language;
        }

        public bool IsLoaded { get; private set; }

    //Before call Reload need once call Load(file) for file cache
       public bool Reload(SystemLanguage targetLanguage) {
            if(files.Length > 0) {
                Setup(targetLanguage);
                IsLoaded = false;
                Load(files);
                return true;
            }
            return false;
       }

        public void Load(string file) {
            Load(new string[] { file });
        }

        public void Load(string[] files) {
            if(!IsLoaded) {
                this.files = files;
                strings.Clear();

                foreach (string file in files) {
                    Debug.Log($"parse file => {file.Colored(ConsoleTextColor.yellow)}");
                    var listItems = JsonConvert.DeserializeObject<List<LocalizationStringData>>(Resources.Load<TextAsset>(file).text);

                    foreach (var item in listItems) {
                        uint jenkinsID = BosUtils.JenkinsOneAtATimeHash(item.id);
                        if (strings.ContainsKey(jenkinsID)) {
                            Debug.LogWarning($"string => {item.id} has repeated jenkins id => {jenkinsID} at file => {file}");
                        } else {
                            strings.Add(jenkinsID, item.GetString(Language));
                        }
                    }
                }
                IsLoaded = true;
            }
        }

        public string GetString(uint id) {
            if (strings.ContainsKey(id)) {
                return strings[id];
            }
            return string.Empty;
        }

        public string GetFrequentString(string key) {
            uint id = BosUtils.JenkinsOneAtATimeHash(key);
            if(!frequentSubcache.Contains(id)) {
                frequentSubcache.Add(id, GetString(id));
            }
            return frequentSubcache.GetObject(id);
        }
 
        public string GetString(string key) {
            uint id = BosUtils.JenkinsOneAtATimeHash(key);
            if(strings.ContainsKey(id)) {
                return strings[id];
            }
            Debug.LogError($"not found string with key => {key}");
            return string.Empty;
        }
    }

    public interface ILocalizationRepository : IRepository {
        void Setup(SystemLanguage language);
        string GetString(uint id);
        string GetString(string key);
        string GetFrequentString(string key);
        void Load(string[] files);
    }


    public interface IPersonalProductRepository : IRepository {
        Dictionary<ProductType, List<ProductData>> Products { get; }
        IEnumerable<ProductData> ProductCollection { get; }
        List<ProductData> GetProducts(ProductType type);
        ProductData GetProduct(int id);
    }

    public class PersonalProductRepository : IPersonalProductRepository {
        public Dictionary<ProductType, List<ProductData>> Products { get; } = new Dictionary<ProductType, List<ProductData>>();

        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            List<ProductJsonData> products = JsonConvert.DeserializeObject<List<ProductJsonData>>(Resources.Load<TextAsset>(file).text);
            Products.Clear();

            foreach (var prod in products) {
                if (Products.ContainsKey(prod.Type)) {
                    Products[prod.Type].Add(new ProductData(prod));
                } else {
                    Products.Add(prod.Type, new List<ProductData> { new ProductData(prod) });
                }
            }
            IsLoaded = true;
        }

        public List<ProductData> GetProducts(ProductType type)
            => Products.ContainsKey(type) ? Products[type] : new List<ProductData>();

        public IEnumerable<ProductData> ProductCollection
            => Products.Values.SelectMany(s => s).OrderBy(p => p.id);

        public ProductData GetProduct(int id) {
            foreach (var kvp in Products) {
                foreach (var product in kvp.Value) {
                    if (product.id == id) {
                        return product;
                    }
                }
            }
            return null;
        }

    }



    public interface IPersonalImprovementsRepository : IRepository {
        void SetFromExternalSource(PersonalImprovementsData data);

        long GetPointsForStatusLevel(int level);
        int MaxStatusLevel { get; }
        int MinStatusLevel { get; }
        int  GetStatusLevel(long points);
        float GetStatusLevelProgress(long points);
        PersonalConvertData ConvertData { get; }
        
        int GetPlanetForStatus(int status);
        StatusPointData GetStatus(int status);
    }

    public class PersonalImprovementsRepository : IPersonalImprovementsRepository {

        public PersonalImprovementsData Data { get; } = new PersonalImprovementsData();

        public bool IsLoaded { get; private set; } = false;

        private int maxStatusLevel = -1;
        private int minStatusLevel = -1;


        public int MaxStatusLevel {
            get {
                if(maxStatusLevel < 0) {
                    maxStatusLevel = Data.StatusPoints.Max(sp => sp.StatusLevel);
                }
                return maxStatusLevel;
            }
        }

        public int MinStatusLevel {
            get {
                if(minStatusLevel < 0 ) {
                    minStatusLevel = Data.StatusPoints.Min(sp => sp.StatusLevel);
                }
                return minStatusLevel;
            }
        }

        public int GetPlanetForStatus(int status)
            => GetStatus(status)?.Planet ?? 0;

        public StatusPointData GetStatus(int status)
            => Data.StatusPoints.FirstOrDefault(st => st.StatusLevel == status);

        public PersonalConvertData ConvertData => Data.ConvertData;

        public long GetPointsForStatusLevel(int level) {
            if(level < MinStatusLevel) {
                return 0;
            }
            int maxLevel = MaxStatusLevel;
            if(level > maxLevel ) {
                return Data.StatusPoints[maxLevel].Points;
            }
            return Data.StatusPoints.FirstOrDefault(sp => sp.StatusLevel == level).Points;
        }

        //public ProductData GetProduct(int id) {
        //    foreach(var kvp in Data.Products) {
        //        foreach(var product in kvp.Value) {
        //            if(product.id == id ) {
        //                return product;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //public List<ProductData> GetProducts(ProductType type) {
        //    //return Data.Products.ContainsKey(type) ? Data.Products[type] : new List<ProductData>();
        //    //return 
        //}

        //public IEnumerable<ProductData> ProductCollection {
        //    get {
        //        return Data.Products.Values.SelectMany(s => s).OrderBy(p => p.id);
        //    }
        //}

        public int GetStatusLevel(long points) {
            int minLevel = MinStatusLevel;
            int maxLevel = MaxStatusLevel;

            for(int i = minLevel; i <= maxLevel; i++   ) {
                if(points < GetPointsForStatusLevel(i)) {
                    return i - 1;
                }
            }

            return maxLevel;
        }

        public float GetStatusLevelProgress(long points) {
            int minLevel = MinStatusLevel;
            int maxLevel = MaxStatusLevel;
            int level = GetStatusLevel(points);
            long nextPoints = GetPointsForStatusLevel(level + 1);
            long currentPoints = GetPointsForStatusLevel(level);
            if(nextPoints == currentPoints) {
                return 0f;
            } else {
                return Mathf.Clamp01((float)(points - currentPoints) / (nextPoints - currentPoints));
            }
        }

        public void Load(string file) {
            if(!IsLoaded) {
                PersonalImprovementJsonData jsonData = JsonConvert.DeserializeObject<PersonalImprovementJsonData>(Resources.Load<TextAsset>(file).text);
                Data.Copy(jsonData);
                IsLoaded = true;
            }
        }

        public void SetFromExternalSource(PersonalImprovementsData data) {
            Data.Copy(data);
            IsLoaded = true;
        }
    }

    public interface ILocalProductDataRepository : IRepository {
        LocalProductJsonData GetLocalProductData(int id);
        IEnumerable<LocalProductJsonData> ProductCollection { get; }
    }

    public class LocalProductDataRepository : ILocalProductDataRepository {
        public Dictionary<int, LocalProductJsonData> Products { get; } = new Dictionary<int, LocalProductJsonData>();

        public bool IsLoaded { get; private set; }

        public LocalProductJsonData GetLocalProductData(int id)
            => Products.ContainsKey(id) ? Products[id] : null;

        public void Load(string file) {
            if(!IsLoaded) {
                var listItems = JsonConvert.DeserializeObject<List<LocalProductJsonData>>(Resources.Load<TextAsset>(file).text);
                Products.Clear();
                listItems.ForEach(li => Products.Add(li.id, li));
                IsLoaded = true;
            }
        }

        public IEnumerable<LocalProductJsonData> ProductCollection
            => Products.Values.OrderBy(p => p.id);
    }

    public interface IBankLevelReporitory : IRepository {
        void SetFromExternalDataSource(IEnumerable<BankLevelData> planets);
        BankLevelData GetBankLevelData(int level);
        IEnumerable<BankLevelData> BankLevelCollection { get; }
    }

    public interface IPlanetDataRepository : IRepository {
        void SetFromExternalDataSource(IEnumerable<PlanetServerData> planets);
        PlanetServerData GetPlanet(int id);
        IEnumerable<PlanetServerData> PlanetCollection { get; }
        PlanetServerData GetPlanetForPlanetId(int planetId);
    }

    public interface IShipModuleRepository : IRepository {
        void SetFromExternalDataSource(IEnumerable<ShipModuleData> modules);
        ShipModuleData GetModule(int id);
        IEnumerable<ShipModuleData> ModuleCollection { get; }
    }

    public interface IMechanicDataRepository : IRepository {
        void SetFromExternalDataSource(IEnumerable<MechanicData> mechanics);
        MechanicData GetMechanicData(int planetId);

        IEnumerable<MechanicData> MechanicCollection { get; }
        int Count { get; }
    }

    public interface IUnitStrengthRepository : IRepository {
        void SetFromExternalSource(IEnumerable<UnitStrengthData> strengths);
        UnitStrengthData GetStrengthData(int generatorId);
        IEnumerable<UnitStrengthData> StrengthCollection { get; }
        int Count { get; }
    }

    public interface ISecretaryDataRepository : IRepository {
        void SetFromExternalDataSource(IEnumerable<SecretaryData> secretaries);
        SecretaryData GetSecretaryData(PlanetId planetId);
        IEnumerable<SecretaryData> SecretaryCollection { get; }
        int Count { get; }
    }

    public class PlanetId {
        public int Id { get; }

        public PlanetId(int id) {
            Id = id;
        }
    }

    public interface ISpritePathRepository : IRepository {
        Dictionary<string, SpritePathData> SpritePathMap { get; }
        void AddItems(List<SpritePathData> items);
        Option<SpritePathData> GetPath(string key);
    }

    public class SpritePathRepository : ISpritePathRepository {
        public Dictionary<string, SpritePathData> SpritePathMap { get; } = new Dictionary<string, SpritePathData>();

        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            if(!IsLoaded) {
                SpritePathMap.Clear();
                var listItems = JsonConvert.DeserializeObject<List<SpritePathData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(li => SpritePathMap.Add(li.id, li));
                IsLoaded = true;
            }
        }

        public void AddItems(List<SpritePathData> items ) {
            foreach(var item in items) {
                if(!SpritePathMap.ContainsKey(item.id)) {
                    SpritePathMap.Add(item.id, item);
                }
            }
        }

        public Option<SpritePathData> GetPath(string key) {
            if(SpritePathMap.ContainsKey(key)) {
                return F.Some(SpritePathMap[key]);
            }
            return F.None;
        }
    }



    public interface IRepository {

        bool IsLoaded { get; }
        void Load(string file);
    }

    public enum CurrencyType {
        Coins,
        CompanyCash,
        PlayerCash,
        Securities
    }

    public interface IManagerLocalJsonDataRepository : IRepository {
        ManagerLocalJsonData GetManagerLocalData(int managerId);
        List<ManagerLocalJsonData> ManagerCollection { get; }
        SpritePathData GetIconData(int managerId, int planetId, bool isActive);
    }

    public class ManagerLocalJsonDataRepository : IManagerLocalJsonDataRepository {

        public bool IsLoaded { get; private set; }

        public Dictionary<int, ManagerLocalJsonData> Managers { get; } = new Dictionary<int, ManagerLocalJsonData>();


        public void Load(string file) {
            if(!IsLoaded) {
                Managers.Clear();
                List<ManagerLocalJsonData> listItems = JsonConvert.DeserializeObject<List<ManagerLocalJsonData>>(UnityEngine.Resources.Load<TextAsset>(file).text);
                listItems.ForEach(li => Managers.Add(li.id, li));
                IsLoaded = true;
            }
        }

        public ManagerLocalJsonData GetManagerLocalData(int managerId)
            => Managers.ContainsKey(managerId) ? Managers[managerId] : null;

        public List<ManagerLocalJsonData> ManagerCollection
            => Managers.Values.OrderBy(m => m.id).ToList();

        public SpritePathData GetIconData(int managerId, int planetId, bool isActive) {
            var managerData = GetManagerLocalData(managerId);
            if(managerData != null ) {
                foreach(var iconData in managerData.icons ) {
                    if(iconData.planet_id == planetId ) {
                        return (isActive) ? iconData.active : iconData.disabled;
                    }
                }
            }
            return null;
        }
    }

    public interface IManagerImprovementRepository : IRepository {
        ManagerImproveData Improvements { get; }
        void SetFromExternalSource(ManagerImproveData other);
        ManagerEfficiencyImproveData GetEfficiencyImproveData(int level);
        ManagerRollbackImproveData GetRollbackImproveData(int level);
        MegaManagerImproveData MegaImprovement { get; }
        int MinLevel { get; }
        int MaxLevel { get; }
    }

    public class ManagerImprovementRepository : IManagerImprovementRepository {

        public ManagerImproveData Improvements { get; private set; }

        public bool IsLoaded { get; private set; }

        public void Load(string file) {
            if(!IsLoaded) {
                ManagerImproveJsonData jsonData = JsonConvert.DeserializeObject<ManagerImproveJsonData>(Resources.Load<TextAsset>(file).text);
                Improvements = new ManagerImproveData(jsonData);
                IsLoaded = true;
            }
        }

        public void SetFromExternalSource(ManagerImproveData other) {
            Improvements = new ManagerImproveData(other);
            IsLoaded = true;
        }

        public ManagerEfficiencyImproveData GetEfficiencyImproveData(int level)
            => Improvements.EfficiencyImprovements.ContainsKey(level) ? Improvements.EfficiencyImprovements[level] : null;

        public ManagerRollbackImproveData GetRollbackImproveData(int level)
            => Improvements.RollbackImprovements.ContainsKey(level) ? Improvements.RollbackImprovements[level] : null;

        public MegaManagerImproveData MegaImprovement
            => Improvements.MegaImprovement;

        private int minLevel = -1;
        private int maxLevel = -1;

        public int MinLevel {
            get {
                if(minLevel < 0 ) {
                    minLevel = Improvements.EfficiencyImprovements.Keys.Min();
                }
                return minLevel;
            }
        }

        public int MaxLevel {
            get {
                if(maxLevel < 0 ) {
                    maxLevel = Improvements.EfficiencyImprovements.Keys.Max();
                }
                return maxLevel;
            }
        }
    }

    public interface IUpgradeRepository : IRepository {
        Dictionary<int, UpgradeData> Upgrades { get; }
        List<UpgradeData> UpgradeList { get; }

        int Count {get;}

        UpgradeData GetData(int id);

        bool Contains(int id);

    }

    public class UpgradeRepository : IUpgradeRepository {


        public Dictionary<int, UpgradeData> Upgrades { get; } = new Dictionary<int, UpgradeData>();


        public List<UpgradeData> UpgradeList => Upgrades.Values.OrderBy(u => u.Id).ToList();

        public int Count
            => Upgrades.Count;


        public bool IsLoaded { get; private set; }

        public UpgradeData GetData(int id)
            => Upgrades.ContainsKey(id) ? Upgrades[id] : null;

        public void Load(string file) {
            if(!IsLoaded) {
                Upgrades.Clear();
                var listItems = JsonConvert.DeserializeObject<List<UpgradeJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(li => Upgrades.Add(li.id, new UpgradeData(li)));
                IsLoaded = true;
            }
        }

        public bool Contains(int id)
            => Upgrades.ContainsKey(id);

    }

    public interface ICoinUpgradeRepository : IRepository {
        Dictionary<int, BosCoinUpgradeData> Upgrades { get; }
        List<BosCoinUpgradeData> UpgradeList { get; }

        BosCoinUpgradeData GetData(int id);
    }

    public class CoinUpgradeRepository : ICoinUpgradeRepository {
        public Dictionary<int, BosCoinUpgradeData> Upgrades { get; } = new Dictionary<int, BosCoinUpgradeData>();

        public List<BosCoinUpgradeData> UpgradeList => Upgrades.Values.OrderBy(u => u.Id).ToList();

        public bool IsLoaded { get; private set; }

        public BosCoinUpgradeData GetData(int id)
            => Upgrades.ContainsKey(id) ? Upgrades[id] : null;

        public void Load(string file) {
            if (!IsLoaded) {
                Upgrades.Clear();
                var listItems = JsonConvert.DeserializeObject<List<CoinUpgradeJsonData>>(Resources.Load<TextAsset>(file).text);
                listItems.ForEach(li => Upgrades.Add(li.id, new BosCoinUpgradeData(li)));
                IsLoaded = true;
            }
        }
    }

    public interface IPlayerIconRepository : IRepository {
        SpritePathData GetLarge(int planetId, Gender gender);
        SpritePathData GetSmall(int planetId, Gender gender);
        IEnumerable<SpritePathData> IconPaths { get; }
    }

    public class PlayerIconRepository : IPlayerIconRepository {

        public Dictionary<int, PlayerIconData> PlayerIcons { get; } = new Dictionary<int, PlayerIconData>();

        public IEnumerable<SpritePathData> IconPaths {
            get {
                return PlayerIcons.Values.SelectMany(ic => new List<SpritePathData> { ic.LargeMale, ic.LargeFemale, ic.SmallMale, ic.SmallFemale });
            }
        }

        public bool IsLoaded { get; private set; }

        public SpritePathData GetLarge(int planetId, Gender gender) {
            if(PlayerIcons.ContainsKey(planetId)) {
                return PlayerIcons[planetId].GetLarge(gender);
            }
            return null;
        }

        public SpritePathData GetSmall(int planetId, Gender gender) {
            if(PlayerIcons.ContainsKey(planetId)) {
                return PlayerIcons[planetId].GetSmall(gender);
            }
            return null;
        }

        public void Load(string file) {
            if(!IsLoaded) {
                var listItems = JsonConvert.DeserializeObject<List<PlayerIconJsonData>>(Resources.Load<TextAsset>(file).text);
                PlayerIcons.Clear();
                listItems.ForEach(li => {
                    PlayerIcons.Add(li.planet_id, new PlayerIconData(li));
                });
                IsLoaded = true;
            }
        }
    }

    public interface IStatusNameRepository : IRepository {
        StatusNameData GetStatusName(int status);
    }

    public class StatusNameRepository : IStatusNameRepository
    {
        public Dictionary<int, StatusNameData> StatusNames {get;} = new Dictionary<int, StatusNameData>();
        public bool IsLoaded { get; private set;}

        public StatusNameData GetStatusName(int status)
        {
            return StatusNames.ContainsKey(status) ? StatusNames[status] : null;
        }

        public void Load(string file)
        {
            if(!IsLoaded) {
                StatusNames.Clear();
                JsonConvert.DeserializeObject<List<StatusNameJsonData>>(Resources.Load<TextAsset>(file).text)
                    .ForEach(li => StatusNames.Add(li.status, new StatusNameData(li)));
                IsLoaded = true;
            }
        }
    }


}