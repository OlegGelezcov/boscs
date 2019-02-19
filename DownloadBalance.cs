using Bos;
using Bos.Data;
using Bos.Debug;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BosUtils : MonoBehaviour {

    [MenuItem("Tools/DOWNLOAD ALL BALANCE(EXCLUDE PERSONAL)")]
    private static void DownloadBalance() {
        DownloadManagers();
        DownloadGenerators();
        DownloadManagerImprovements();
        //DownloadPersonalImprovements();
        DownloadPlanetTransport();
        DownloadBank();
        DownloadTransportStrength();
        DownloadSecretaryBalance();
        DownloadMechanicBalance();
        DownloadShipModulesBalance();
        DownloadPlanetBalance();
    }
    

    [MenuItem("Tools/Download Managers")]
    private static void DownloadManagers() {
        var items = JsonConvert.DeserializeObject<List<ManagerJsonData>>(Resources.Load<TextAsset>("Data/manager").text);
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl("http://bos.heatherglade.com/_dev/get_managers_prices"));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        var obj = JObject.Parse(json);
        var managers = obj["response"]["data"];
        var index = 0;
        foreach(var manager in managers ) {
            double baseCost = manager[0].ToString().ToDouble();
            double coef = manager[1].ToString().ToDouble();
            var item = GetWithId(items, index);
            index++;
            item.baseCost = baseCost;
            item.coef = coef;
            Debug.Log($"manager => {index} loaded");
        }

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/manager.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, items);
        Debug.Log("managerss saved...".Colored(ConsoleTextColor.orange).BoldItalic());
        EditorUtility.DisplayDialog("Managers loaded", $"data saved to {"Resources/Data/manager.json"}", "Ok");
    }

    private static ManagerJsonData GetWithId(List<ManagerJsonData> items, int id)
        => items.FirstOrDefault(it => it.id == id);

    [MenuItem("Tools/Download Generators")]
    private static void DownloadGenerators() {

        var items = JsonConvert.DeserializeObject<List<GeneratorJsonData>>(Resources.Load<TextAsset>("Data/generator").text);
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl("http://bos.heatherglade.com/_dev/get_balance"));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        var obj = JObject.Parse(json);

        for(int i = 0; i < 10; i++ ) {
            if(obj["response"]["data"][i] != null ) {
                var m = obj["response"]["data"][i];
                items[i].baseCost = m[1].ToString().ToDouble();
                items[i].incrementFactor = m[2].ToString().ToDouble();
                items[i].baseGeneration = m[3].ToString().ToDouble();
                items[i].timeToGenerate = m[4].ToString().ToFloat();
                //items[i].coinPrice = m[5].ToString().ToInt();
                items[i].enhancePrice = m[6].ToString().ToInt();
                items[i].profitIncrementFactor = m[7].ToString().ToDouble();
                Debug.Log($"item => {i} handled");
                if(i == 2) {
                    print("COIN FOR BUS: " + m[5]);
                }
            }
        }

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/generator.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, items);
        Debug.Log("generators saved...".Colored(ConsoleTextColor.orange).BoldItalic());
        EditorUtility.DisplayDialog("Generators loaded", $"data saved to {"Resources/Data/generator.json"}", "Ok");
    }

    [MenuItem("Tools/Download Manager Improvement")]
    private static void DownloadManagerImprovements() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kManagerImprovements));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        var efficiencyImrpovements = NetService.ParseManagerEfficiencyImprovements(arr);
        var rollbackImprovements = NetService.ParseManagerRollbackImprovements(arr);
        var megaImprovement = NetService.ParseMegaManagerImprovement(arr);
        ManagerImproveJsonData jsonData = new ManagerImproveJsonData {
            efficiencyImprovements = efficiencyImrpovements,
            rollbackImprovements = rollbackImprovements,
            megaImprovement = megaImprovement
        };

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/manager_improvements.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, jsonData);
        Debug.Log("manager improvements saved...".Colored(ConsoleTextColor.orange).BoldItalic());
        EditorUtility.DisplayDialog("Manager improvements loaded", $"data saved to {"Resources/Data/manager_improvements.json"}", "Ok");
    }

    [MenuItem("Tools/Download Personal Improvements")]
    private static void DownloadPersonalImprovements() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kPersonalImprovements));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        PersonalConvertData personalConvertData = NetService.ParsePersonalConvertData(arr["c"]);
        List<StatusPointData> statusPoints = NetService.ParseStatusPoints(arr["s"]);
        Dictionary<ProductType, List<ProductData>> products = NetService.ParseProducts(arr["i"]);
        PersonalImprovementJsonData jsonData = new PersonalImprovementsData(personalConvertData,  statusPoints).GetJsonData();

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/personal_improvements.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, jsonData);
        Debug.Log("personal improvements saved...".Colored(ConsoleTextColor.orange).BoldItalic());
        EditorUtility.DisplayDialog("Personal improvements loaded", $"data saved to {"Resources/Data/personal_improvements.json"}", "Ok");
    }

    [MenuItem("Tools/Download Planet Transport")]
    private static void DownloadPlanetTransport() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kPlanetsTransportUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        List<GeneratorJsonData> generators = new List<GeneratorJsonData>();
        int generatorId = 10;
        foreach(JToken token in arr ) {
            GeneratorJsonData jsonData = new GeneratorJsonData {
                id = generatorId,
                baseCost = token.Value<double>(1),
                incrementFactor = token.Value<double>(2),
                baseGeneration = token.Value<double>(3),
                timeToGenerate = token.Value<float>(4),
                //coinPrice = token.Value<int>(5),
                enhancePrice = token.Value<int>(6),
                profitIncrementFactor = token.Value<double>(7),
                name = token.Value<string>(0).Trim(),
                managerIcon = string.Empty
            };
            generatorId++;
            generators.Add(jsonData);
        }


        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/planet_generator.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, generators);
        Debug.Log("planet generators saved...");
        EditorUtility.DisplayDialog("Planet generators loaded", $"data saved to {"Resources/Data/planet_generator.json"}", "Ok");
    }

    private static List<BankLevelJsonData> GetBankLevelDataList() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kBankUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        List<BankLevelJsonData> bankLevels = new List<BankLevelJsonData>();
        int level = 1;
        foreach (JToken token in arr) {
            int price = token.Value<int>(0);
            float profit = token.Value<float>(1);
            float interval = token.Value<float>(2);
            BankLevelJsonData data = new BankLevelJsonData {
                level = level,
                levelPriceCoins = price,
                profit = profit,
                profitInterval = interval
            };
            level++;
            bankLevels.Add(data);
        }
        return bankLevels;
    }

    private static List<BankLevelJsonData> GetBankLevelDataListLocal() {
        string jsonText = Resources.Load<TextAsset>("Data/bank").text;
        //print(jsonText);
        return JsonConvert.DeserializeObject<List<BankLevelJsonData>>(jsonText);
    }

    [MenuItem("Tools/Print Bank Level Difference")]
    private static void PrintBankDifference() {
        var serverLevels = GetBankLevelDataList();
        var localLevels = GetBankLevelDataListLocal();
        print($"server level count: {serverLevels.Count}, local level count: {localLevels.Count}");
        var diffs = localLevels.Zip(serverLevels, (data, second) => data.Diff(second)).Where(d => d.IsValid).ToList();
        StringBuilder sb = new StringBuilder();
        diffs.ForEach(d => sb.AppendLine(d.ToString()));
        print(sb.ToString());
    }


    [MenuItem("Tools/Download bank")]
    private static void DownloadBank() {

        /*
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kBankUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        List<BankLevelJsonData> bankLevels = new List<BankLevelJsonData>();
        int level = 1;
        foreach(JToken token in arr) {
            int price = token.Value<int>(0);
            float profit = token.Value<float>(1);
            float interval = token.Value<float>(2);
            BankLevelJsonData data = new BankLevelJsonData {
                level = level,
                levelPriceCoins = price,
                profit = profit,
                profitInterval = interval
            };
            level++;
            bankLevels.Add(data);
        }*/

        var bankLevels = GetBankLevelDataList();

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/bank.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, bankLevels);
        Debug.Log("bank saved...");
        EditorUtility.DisplayDialog("Bank loaded", $"data saved to {"Resources/Data/bank.json"}", "Ok");
    }

    [MenuItem("Tools/Download transport strength")]
    private static void DownloadTransportStrength() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kTransportStrengthUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        int generatorId = 0;
        List<UnitStrengthJsonData> strengths = new List<UnitStrengthJsonData>();
        foreach(JToken token in arr) {
            float strength = token.Value<float>();
            strengths.Add(new UnitStrengthJsonData {
                id = generatorId,
                strength = strength
            });
            generatorId++;
        }

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/strengths.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, strengths);
        Debug.Log("strengths saved...");
        EditorUtility.DisplayDialog("Strengths loaded", $"data saved to {"Resources/Data/strengths.json"}", "Ok");
    }

    public static List<SecretaryJsonData> GetSecretariesRemote() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kSecretaryBalanceUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        List<SecretaryJsonData> secretaries = new List<SecretaryJsonData>();
        int planetId = 0;
        foreach (JToken token in arr) {
            int priceForFirst = token.Value<int>(0);
            int priceIncreasing = token.Value<int>(1);
            int reportCountPerSecretary = token.Value<int>(2);
            float fatigue = token.Value<float>(3);
            int reportCountProcessedPer10Seconds = token.Value<int>(4);
            double cashPrice = token.Value<double>(5);
            secretaries.Add(new SecretaryJsonData {
                planetId = planetId,
                priceForFirstSecretary = priceForFirst,
                priceIncreasingForNextSecretary = priceIncreasing,
                reportCountPerSecretary = reportCountPerSecretary,
                fatigueOfEfficiency = fatigue,
                reportCountProcessedPer10Seconds = reportCountProcessedPer10Seconds,
                auditCashPrice = cashPrice
            });
            planetId++;

        }
        return secretaries;
    }

    public static void SaveSecretariesLocal(List<SecretaryJsonData> secretaries ) {
        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/secretaries.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, secretaries);
        Debug.Log("secretaries saved...");
        EditorUtility.DisplayDialog("Secretaries loaded", $"data saved to {"Resources/Data/secretaries.json"}", "Ok");
    }

    [MenuItem("Tools/Download Secretary Balance")]
    private static void DownloadSecretaryBalance() {

        var secretaries = GetSecretariesRemote();
        SaveSecretariesLocal(secretaries);

    }

    public static List<MechanicJsonData> LoadMechanicsRemote() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kBalanceMechanicUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        List<MechanicJsonData> mechanics = new List<MechanicJsonData>();
        int planetId = 0;
        foreach (JToken token in arr) {
            int priceForFirstMechanic = token.Value<int>(0);
            int priceIncreasing = token.Value<int>(1);
            int unitCountService = token.Value<int>(2);
            float fatigue = token.Value<float>(3);
            int restoredPer10Seconds = token.Value<int>(4);
            double cashPrice = token.Value<double>(5);
            mechanics.Add(new MechanicJsonData {
                planetId = planetId,
                fatigueUnitsPercentPerHour = fatigue,
                priceForFirstMechanic = priceForFirstMechanic,
                priceIncreasingForNextMechanic = priceIncreasing,
                serviceCashPrice = cashPrice,
                serviceUnitsRestoredPer10Seconds = restoredPer10Seconds,
                unitCountService = unitCountService
            });
            planetId++;
        }
        return mechanics;
    }

    [MenuItem("Tools/Download Mechanic Balance")]
    private static void DownloadMechanicBalance() {
        /*
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.kBalanceMechanicUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];

        List<MechanicJsonData> mechanics = new List<MechanicJsonData>();
        int planetId = 0;
        foreach(JToken token in arr ) {
            int priceForFirstMechanic = token.Value<int>(0);
            int priceIncreasing = token.Value<int>(1);
            int unitCountService = token.Value<int>(2);
            float fatigue = token.Value<float>(3);
            int restoredPer10Seconds = token.Value<int>(4);
            double cashPrice = token.Value<double>(5);
            mechanics.Add(new MechanicJsonData {
                planetId = planetId,
                fatigueUnitsPercentPerHour = fatigue,
                priceForFirstMechanic = priceForFirstMechanic,
                priceIncreasingForNextMechanic = priceIncreasing,
                serviceCashPrice = cashPrice,
                serviceUnitsRestoredPer10Seconds = restoredPer10Seconds,
                unitCountService = unitCountService
            });
            planetId++;
        }*/

        var mechanics = LoadMechanicsRemote();
        SaveMechanicsToLocal(mechanics);
    }

    public static void SaveMechanicsToLocal(List<MechanicJsonData> mechanics) {
        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/mechanics.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, mechanics);
        Debug.Log("mechanics saved...");
    }

    [MenuItem("Tools/Download Module Balance")]
    private static void DownloadShipModulesBalance() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.balanceShipUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];
        List<ShipModuleJsonData> modules = new List<ShipModuleJsonData>();
        int moduleId = 0;
        foreach(JToken token in arr) {
            int planetLevel = token.Value<int>(0);
            double companyCash = token.Value<double>(1);
            double securities = token.Value<double>(2);
            int coins = token.Value<int>(3);
            if (companyCash != 0.0) {
                modules.Add(new ShipModuleJsonData {
                    currencyType = CurrencyType.CompanyCash,
                    id = moduleId,
                    planetLevel = planetLevel,
                    price = companyCash
                });
            } else if (securities != 0.0) {
                modules.Add(new ShipModuleJsonData {
                    currencyType = CurrencyType.Securities,
                    id = moduleId,
                    planetLevel = planetLevel,
                    price = securities
                });
            } else {
                modules.Add(new ShipModuleJsonData {
                    currencyType = CurrencyType.Coins,
                    id = moduleId,
                    planetLevel = planetLevel,
                    price = coins
                });
            }
            moduleId++;
        }

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/ship_modules.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, modules);
        Debug.Log("modules saved...");
    }

    [MenuItem("Tools/Download Planet Balance")]
    private static void DownloadPlanetBalance() {
        WebClient webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.Authorization, NetService.AuthHeader);
        string downloadedString = webClient.DownloadString(NetService.FullUrl(NetService.balanceUrl));
        string json = JsonWebToken.Decode(downloadedString, NetService.secretKey);
        JObject parent = JObject.Parse(json);
        JToken arr = parent["response"]["data"];
        List<PlanetJsonData> planets = new List<PlanetJsonData>();
        int planetId = NetService.kEarthId;
        foreach (JToken token in arr)
        {
            // IEnumerable<double> values = token.Values<double>();
            //if (values.Count() >= kCountPlanetParameters) {
            //    throw new UnityException($"invalid count of planet parameters PLANET: {planetId}, EXPECTED: {kCountPlanetParameters}, ACTUAL: {values.Count()}");
            //}
            PlanetJsonData planet = new PlanetJsonData();
            planet.id = planetId;
            planet.profitMultiplier = token.Value<double>(0);
            planet.timeFactor = token.Value<double>(1);
            planet.globalUpgradeMult = token.Value<double>(2);
            planet.companyCashPrice = token.Value<double>(3);
            planet.securitiesPrice = token.Value<double>(4);
            planet.openingTime = token.Value<double>(5);
            planet.transportUnitPriceMult = token.Value<double>(6);
            planet.GeneratorsMult = token[7].ToObject<List<double>>();
            planet.ManagersMult = token[8].ToObject<List<double>>();
            planet.UpgradeMult = token[9].ToObject<List<double>>();
            planets.Add(planet);
            planetId++;
        }

        string serializePath = Path.Combine(Application.dataPath, "Resources/Data/planets.json");
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        Serialize(serializePath, planets);
        Debug.Log("planets saving DONE");
    }

    public static void Serialize(string path, object items) {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;

        using (StreamWriter streamWriter = new StreamWriter(path)) {
            using (JsonWriter writer = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(writer, items);
            }
        }
    }

    public class ManagerJsonData {
        public int id;
        public double baseCost;
        public double coef;
        public double kickBackCoef;
        public string name;
        public string description;
    }
}
