namespace Bos.Data {
    using Bos.Debug;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;



    public class PlanetServerData
    {
        public int Id;
        public double ProfitMultiplier;
        public double TimeFactor;
        public double GlobalUpgradeMult;
        public double CompanyCashPrice;
        public double SecuritiesPrice;
        public double OpeningTime;
        public double TransportUnityPriceMult;
        public List<double> GeneratorsMult;
        public List<double> ManagersMult; 
        public List<double> UpgradeMult; 
        
        public void SetProfitMultiplier(double value) {
            ProfitMultiplier = value;
        }

        public PlanetServerData(int id, IEnumerable<double> serverValues) {
            Id = id;
            double[] serverArr = serverValues.ToArray();
            ProfitMultiplier = serverArr[0];
            TimeFactor = serverArr[1];
           // MaxUpgradeLevel = (int)serverArr[2];
            CompanyCashPrice = serverArr[3];
            SecuritiesPrice = serverArr[4];
            OpeningTime = serverArr[5];
            if (serverArr.Length >= 7) {
                Debug.Log($"PlanetServerData() set unit trans mult set from server to value => {serverArr[6]}".Colored(ConsoleTextColor.cyan));
                TransportUnityPriceMult = serverArr[6];
            } else {
                Debug.Log($"PlanetServerData() set unit trans mult set from fallback to value => {1}".Colored(ConsoleTextColor.cyan));
                TransportUnityPriceMult = 1.0;
            }
        }

        public PlanetServerData()
        {
            
        }
        
        public PlanetServerData(PlanetJsonData data) {
            Id = data.id;
            ProfitMultiplier = data.profitMultiplier;
            TimeFactor = data.timeFactor;
            GlobalUpgradeMult = data.globalUpgradeMult;
            CompanyCashPrice = data.companyCashPrice;
            SecuritiesPrice = data.securitiesPrice;
            OpeningTime = data.openingTime;
            TransportUnityPriceMult = data.transportUnitPriceMult;
            if(TransportUnityPriceMult == 0.0 ) {
                TransportUnityPriceMult = 1.0;
            }

            GeneratorsMult = data.GeneratorsMult;
            ManagersMult = data.ManagersMult;
            UpgradeMult = data.UpgradeMult;

            /*
            Debug.Log($"planet: {Id}, generator mults: {GeneratorsMult.AsString()}");
            Debug.Log($"planet: {Id}, managers mult: {ManagersMult.AsString()}");
            Debug.Log($"planet: {Id}, upgrade mult: {UpgradeMult.AsString()}");*/

        }

        public override string ToString() {
            return $"ID: {Id}, PROFIT MULT: {ProfitMultiplier}, TIME FACT: {TimeFactor}, CASH PRICE: {CompanyCashPrice}, SECURITY PRICE: {SecuritiesPrice}, OPEN TIME: {OpeningTime}";
        }

        public Currency[] Prices =>
            new Currency[] {
                Currency.CreateCompanyCash(CompanyCashPrice),
                Currency.CreateSecurities(SecuritiesPrice)
            };
    }

    public class PlanetJsonData {
        public int id;
        public double profitMultiplier;
        public double timeFactor;
        public double globalUpgradeMult;
        public double companyCashPrice;
        public double securitiesPrice;
        public double openingTime;
        public double transportUnitPriceMult;
        public List<double> GeneratorsMult;
        public List<double> ManagersMult; 
        public List<double> UpgradeMult; 
    }

    public class ShipModuleData {
        public int Id { get; private set; }
        public int PlanetLevel { get; private set; }
        public Currency Currency { get; private set; }

        public int PlanetId
            => PlanetLevel - 1;


        public ShipModuleData(int id, int planetLevel, Currency currency) {
            Id = id;
            PlanetLevel = planetLevel;
            Currency = currency;
        }

        public ShipModuleData(ShipModuleJsonData jsonData) {
            Id = jsonData.id;
            PlanetLevel = jsonData.planetLevel;
            switch (jsonData.currencyType) {
                case CurrencyType.Coins: {
                        Currency = Currency.CreateCoins((int)jsonData.price);
                    }
                    break;
                case CurrencyType.CompanyCash: {
                        Currency = Currency.CreateCompanyCash(jsonData.price);
                    }
                    break;
                case CurrencyType.PlayerCash: {
                        Currency = Currency.CreatePlayerCash(jsonData.price);
                    }
                    break;
                case CurrencyType.Securities: {
                        Currency = Currency.CreateSecurities(jsonData.price);
                    }
                    break;
            }
        }

        public override string ToString() {
            return $"Id: {Id}, Planet Level: {PlanetLevel}, Price: {Currency.ToString()}";
        }
    }

    public class ShipModuleJsonData {
        public int id;
        public int planetLevel;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public CurrencyType currencyType;

        public double price;

    }

    public class BankLevelData {
        public int Level { get; private set; }
        public int LevelPriceCoins { get; private set; }
        public float Profit { get; private set; }
        public float ProfitInterval { get; private set; }

        public float ProfitPerSecond
            => Profit / ProfitInterval;

        public float ProfitPerHour
            => ProfitPerSecond * 3600.0f;

        public float ProfitPerInterval(TimeSpan interval)
            => (float)(ProfitPerSecond * interval.TotalSeconds);

        public BankLevelData(int level, int levelPriceCoins, float profit, float profitInterval) {
            this.Level = level;
            this.LevelPriceCoins = levelPriceCoins;
            this.Profit = profit;
            this.ProfitInterval = profitInterval;
        }

        public BankLevelData(BankLevelJsonData data) {
            this.Level = data.level;
            this.LevelPriceCoins = data.levelPriceCoins;
            this.Profit = data.profit;
            this.ProfitInterval = data.profitInterval;
        }

        public override string ToString() {
            return $"level => {Level}, level price => {LevelPriceCoins}, profit => {Profit}, profit interval => {ProfitInterval}";
        }

        public float TimeOfProfitOneCoin
            => ProfitInterval / Profit;
    }

    public class BankLevelJsonData {
        public int level;
        public int levelPriceCoins;
        public float profit;
        public float profitInterval;

        public BankLevelDelta Diff(BankLevelJsonData other ) {
            return new BankLevelDelta(level,
                other.levelPriceCoins - levelPriceCoins,
                other.profit - profit,
                other.profitInterval - profitInterval);
        }

    }

    public class BankLevelDelta {
        public int Level { get; private set; }
        public int PriceCoinsDelta { get; private set; }
        public float ProfitDelta { get; private set; }
        public float IntervalDelta { get; private set; }

        public bool IsValid
            => PriceCoinsDelta != 0 ||
                Mathf.Abs(ProfitDelta) > BosUtils.Epsilon ||
                Mathf.Abs(IntervalDelta) > BosUtils.Epsilon;


        public BankLevelDelta(int level, int priceCoinsDelta, float profitDelta, float intervalDelta) {
            Level = level;
            PriceCoinsDelta = priceCoinsDelta;
            ProfitDelta = profitDelta;
            IntervalDelta = intervalDelta;
        }

        public override string ToString() {
            return $"Level: {Level}, Price Coins Delta: {PriceCoinsDelta}, Profit Delta: {ProfitDelta}, Interval Delta: {IntervalDelta}";
        }
    }

    public class UnitStrengthData {
        public int Id { get; private set; }
        public float Strength { get; private set; }

        public UnitStrengthData(int id, float strength) {
            this.Id = id;
            this.Strength = strength;
        }

        public UnitStrengthData(UnitStrengthJsonData data) {
            Id = data.id;
            Strength = data.strength;
        }

        public override string ToString() {
            return $"id => {Id}, strength => {Strength}";
        }
    }

    public class UnitStrengthJsonData {
        public int id;
        public float strength;
    }

    public class SecretaryData {
        public int PlanetId { get; private set; }
        public int PriceForFirstSecretary { get; private set; }
        public int PriceIncreasingForNextSecretary { get; private set; }
        public int ReportCountPerSecretary { get; private set; }
        public float FatigueOfEfficiency { get; private set; }
        public int ReportCountProcessedPer10Seconds { get; private set; }
        public double AuditCashPrice { get; private set; }

        public float AuditorSpeed
            => (float)ReportCountProcessedPer10Seconds / 10f;


        public SecretaryData(int planetId, int priceForFirst, int priceIncreasing, int reportCountPerSecretary, float fatigueEfficiency, int reportProcessed, double cashPrice) {
            this.PlanetId = planetId;
            this.PriceForFirstSecretary = priceForFirst;
            this.PriceIncreasingForNextSecretary = priceIncreasing;
            this.ReportCountPerSecretary = reportCountPerSecretary;
            this.FatigueOfEfficiency = fatigueEfficiency;
            this.ReportCountProcessedPer10Seconds = reportProcessed;
            this.AuditCashPrice = cashPrice;
        }

        public SecretaryData(SecretaryJsonData data) {
            this.PlanetId = data.planetId;
            this.PriceForFirstSecretary = data.priceForFirstSecretary;
            this.PriceIncreasingForNextSecretary = data.priceIncreasingForNextSecretary;
            this.ReportCountPerSecretary = data.reportCountPerSecretary;
            this.FatigueOfEfficiency = data.fatigueOfEfficiency;
            this.ReportCountProcessedPer10Seconds = data.reportCountProcessedPer10Seconds;
            this.AuditCashPrice = data.auditCashPrice;
        }

        public override string ToString() {
            return $"planet id => {PlanetId}, price for first secretary => {PriceForFirstSecretary}, price increasing => {PriceIncreasingForNextSecretary}"
                + $"report count per secretary => {ReportCountPerSecretary}, fatigue of efficiency % => {FatigueOfEfficiency}"
                + $"report count processed per 10 seconds => {ReportCountProcessedPer10Seconds}, audit cash price => {AuditCashPrice}";
        }
    }

    [System.Serializable]
    public class SecretaryJsonData {
        public int planetId;
        public int priceForFirstSecretary;
        public int priceIncreasingForNextSecretary;
        public int reportCountPerSecretary;
        public float fatigueOfEfficiency;
        public int reportCountProcessedPer10Seconds;
        public double auditCashPrice;

        public IDataDifference GetDifference(SecretaryJsonData other)
            => new SecretaryDataDiff(planetId, this, other);
    }

    public class SecretaryDataDiff : IDataDifference {
        public int Id { get; }
        public int PriceForFirstSecretaryDiff { get; }
        public int PriceIncreasingForNextSecretaryDiff { get; }
        public int ReportCountPerSecretaryDiff { get; }
        public float FatigueOfEfficiencyDiff { get; }
        public int ReportCountProcessedPer10SecondsDiff { get; }
        public double AuditCashPriceDiff { get; }



        public SecretaryDataDiff(int id, SecretaryJsonData first, SecretaryJsonData second ) {
            Id = id;
            PriceForFirstSecretaryDiff = second.priceForFirstSecretary - first.priceForFirstSecretary;
            PriceIncreasingForNextSecretaryDiff = second.priceIncreasingForNextSecretary - first.priceIncreasingForNextSecretary;
            ReportCountPerSecretaryDiff = second.reportCountPerSecretary - first.reportCountPerSecretary;
            FatigueOfEfficiencyDiff = second.fatigueOfEfficiency - first.fatigueOfEfficiency;
            ReportCountProcessedPer10SecondsDiff = second.reportCountProcessedPer10Seconds - first.reportCountProcessedPer10Seconds;
            AuditCashPriceDiff = second.auditCashPrice - first.auditCashPrice;
        }

        public bool IsSame
            => PriceForFirstSecretaryDiff == 0 &&
                PriceIncreasingForNextSecretaryDiff == 0 &&
                ReportCountPerSecretaryDiff == 0 &&
                FatigueOfEfficiencyDiff.Approximately(0f) &&
                ReportCountProcessedPer10SecondsDiff == 0 &&
                AuditCashPriceDiff.Approximately(0.0);


        public Dictionary<string, object> Difference {
            get {
                Dictionary<string, object> difference = new Dictionary<string, object>();
                if (PriceForFirstSecretaryDiff != 0) {
                    difference.Add("priceForFirstSecretary", PriceForFirstSecretaryDiff);
                }
                if (PriceIncreasingForNextSecretaryDiff != 0) {
                    difference.Add("priceIncreasingForNextSecretary", PriceIncreasingForNextSecretaryDiff);
                }
                if (ReportCountPerSecretaryDiff != 0) {
                    difference.Add("reportCountPerSecretary", ReportCountPerSecretaryDiff);
                }
                if (!FatigueOfEfficiencyDiff.Approximately(0f)) {
                    difference.Add("fatigueOfEfficiency", FatigueOfEfficiencyDiff);
                }
                if (ReportCountProcessedPer10SecondsDiff != 0) {
                    difference.Add("reportCountProcessedPer10Seconds", ReportCountProcessedPer10SecondsDiff);
                }
                if (!AuditCashPriceDiff.Approximately(0.0)) {
                    difference.Add("auditCashPrice", AuditCashPriceDiff);
                }
                return difference;
            }
        }

        public override string ToString() {
            var difference = Difference;
            return DataHelper.DifferenceToString(Id, difference);
        }
    }


    public class MechanicData {
        public int PlanetId { get; private set; }
        public int PriceForFirstMechanic { get; private set; }
        public int PriceIncreasingForNextMechanic { get; private set; }
        public int UnitCountService { get; private set; }
        public float FatigueUntisPercentPerHour { get; private set; }
        public int ServiceUnitsRestoredPer10Seconds { get; private set; }
        public double ServiceCashPrice { get; private set; }


        public float TempMechanicSpeed
            => (float)ServiceUnitsRestoredPer10Seconds / 10f;


        public MechanicData(int planetId, int priceForFirstMechanic, int priceIncreasingForNetMechanic, int unitCountService, float fatigueUnitsPercentPerHour,
            int serviceUnitsRestoresPer10Seconds, double serviceCashPrice ) {
            this.PlanetId = planetId;
            this.PriceForFirstMechanic = priceForFirstMechanic;
            this.PriceIncreasingForNextMechanic = priceIncreasingForNetMechanic;
            this.UnitCountService = unitCountService;
            this.FatigueUntisPercentPerHour = fatigueUnitsPercentPerHour;
            this.ServiceUnitsRestoredPer10Seconds = serviceUnitsRestoresPer10Seconds;
            this.ServiceCashPrice = serviceCashPrice;
        }

        public MechanicData(MechanicJsonData jsonData) {
            Load(jsonData);
        }

        public void Load(MechanicJsonData data) {
            this.PlanetId = data.planetId;
            this.PriceForFirstMechanic = data.priceForFirstMechanic;
            this.PriceIncreasingForNextMechanic = data.priceIncreasingForNextMechanic;
            this.UnitCountService = data.unitCountService;
            this.FatigueUntisPercentPerHour = data.fatigueUnitsPercentPerHour;
            this.ServiceUnitsRestoredPer10Seconds = data.serviceUnitsRestoredPer10Seconds;
            this.ServiceCashPrice = data.serviceCashPrice;
        }

        public override string ToString() {
            return $"planet: {PlanetId}, price for first mech: {PriceForFirstMechanic}, price increasing for next mechanic: {PriceIncreasingForNextMechanic}, unit count serviced by mechanic: {UnitCountService}" +
                $" fatigue per hour %: {FatigueUntisPercentPerHour} units restored by service per 10 secs: {ServiceUnitsRestoredPer10Seconds}, cash price: {ServiceCashPrice}";
        }
    }

    [System.Serializable]
    public class MechanicJsonData {
        public int planetId;
        public int priceForFirstMechanic;
        public int priceIncreasingForNextMechanic;
        public int unitCountService;
        public float fatigueUnitsPercentPerHour;
        public int serviceUnitsRestoredPer10Seconds;
        public double serviceCashPrice;

        public MechanicDataDiff GetDifference(MechanicJsonData other ) {
            return new MechanicDataDiff(planetId, this, other);
        }
    }

    public class MechanicDataDiff : IDataDifference {
        public int Id { get; }

        public int PriceForFirstMechanicDiff { get; private set; }
        public int PriceIncreasingForNextMechanicDiff { get; private set; }
        public int UnitCountServiceDiff { get; private set; }
        public float FatigueUnitsPercentPerHourDiff { get; private set; }
        public int ServiceUnitsRestoredPer10SecondsDiff { get; private set; }
        public double ServiceCashPriceDiff { get; private set; }

        public MechanicDataDiff(int id, MechanicJsonData first, MechanicJsonData second ) {
            Id = id;
            PriceForFirstMechanicDiff = first.priceForFirstMechanic - second.priceForFirstMechanic;
            PriceIncreasingForNextMechanicDiff = first.priceIncreasingForNextMechanic - second.priceIncreasingForNextMechanic;
            UnitCountServiceDiff = first.unitCountService - second.unitCountService;
            FatigueUnitsPercentPerHourDiff = first.fatigueUnitsPercentPerHour - second.fatigueUnitsPercentPerHour;
            ServiceUnitsRestoredPer10SecondsDiff = first.serviceUnitsRestoredPer10Seconds - second.serviceUnitsRestoredPer10Seconds;
            ServiceCashPriceDiff = first.serviceCashPrice - second.serviceCashPrice;
        }

        #region IDataDifference
        public bool IsSame
            => PriceForFirstMechanicDiff == 0 &&
            PriceIncreasingForNextMechanicDiff == 0 &&
            UnitCountServiceDiff == 0 &&
            FatigueUnitsPercentPerHourDiff.Approximately(0f) &&
            ServiceUnitsRestoredPer10SecondsDiff == 0 &&
            ServiceCashPriceDiff.Approximately(0.0);

        public Dictionary<string, object> Difference {
            get {
                Dictionary<string, object> diff = new Dictionary<string, object>();
                if(PriceForFirstMechanicDiff != 0 ) {
                    diff.Add("priceForFirstMechanic", PriceForFirstMechanicDiff);
                }
                if(PriceIncreasingForNextMechanicDiff != 0 ) {
                    diff.Add("priceIncreasingForNextMechanic", PriceIncreasingForNextMechanicDiff);
                }
                if(UnitCountServiceDiff != 0 ) {
                    diff.Add("unitCountService", UnitCountServiceDiff);
                }
                if(!FatigueUnitsPercentPerHourDiff.Approximately(0f)) {
                    diff.Add("fatigueUnitsPercentPerHour", FatigueUnitsPercentPerHourDiff);
                }
                if(ServiceUnitsRestoredPer10SecondsDiff != 0 ) {
                    diff.Add("serviceUnitsRestoredPer10Seconds", ServiceUnitsRestoredPer10SecondsDiff);
                }
                if(!ServiceCashPriceDiff.Approximately(0.0)) {
                    diff.Add("serviceCashPrice", ServiceCashPriceDiff);
                }
                return diff;
            }
        }
        #endregion

        public override string ToString() {
            var difference = Difference;
            return DataHelper.DifferenceToString(Id, difference);
        }


    }

    public static class DataHelper {
        public static string DifferenceToString(int id, Dictionary<string, object> difference) {
            if (difference.Count == 0) {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"id: {id}");

            foreach (var key in difference.Keys.OrderBy(k => k)) {
                sb.AppendLine($"{key}: {difference[key]}");
            }
            return sb.ToString();
        }
    }

    public class Currency {
        public double Value { get; private set; }
        public CurrencyType Type { get; private set; }

        private Currency(double value, CurrencyType type) {
            Value = value;
            Type = type;
        }

        public override string ToString() {
            return $"{Type}: {Value}";
        }

        public static Currency Create(CurrencyType type, double value) {
            switch(type) {
                case CurrencyType.Coins:
                    return CreateCoins((int)value);
                case CurrencyType.CompanyCash:
                    return CreateCompanyCash(value);
                case CurrencyType.PlayerCash:
                    return CreatePlayerCash(value);
                case CurrencyType.Securities:
                    return CreateSecurities(value);
                default:
                    throw new UnityException($"unsupported currency type {type}");
            }
        }

        public static Currency CreateCoins(int count) {
            return new Currency(count, CurrencyType.Coins);
        }

        public static Currency CreateCompanyCash(double count) {
            return new Currency(count, CurrencyType.CompanyCash);
        }

        public static Currency CreatePlayerCash(double count) {
            return new Currency(count, CurrencyType.PlayerCash);
        }

        public static Currency CreateSecurities(double count) {
            return new Currency(count, CurrencyType.Securities);
        }

        public string DisplayString {
            get {
                if(Type == CurrencyType.Coins) {
                    return ((int)Value).ToString();
                } else {
                    return new CurrencyNumber(Value).Abbreviation;
                }
            }
        }

        public static Currency operator+(Currency left, Currency right) {
            return new Currency(left.Value + right.Value, left.Type);
        }
    }

    [System.Serializable]
    public class SpritePathData {
        public string id;
        public string path;
        public string container;
        public string name;

        public bool IsValid
            => container.IsValid() && name.IsValid(); //id.IsValid() && path.IsValid();

        public override string ToString() {
            Validate();
            return $"{container}:{name}";
        }

        private void Validate() {
            if(container == null) {
                container = string.Empty;
            }
            if(name == null ) {
                name = string.Empty;
            }
        }

        
    }

    public class PersonalConvertData {
        public float OfficialConvertPercent {get; private set;}
        public float UnofficialConvertPercent { get; private set;}
        public float LoosePercent {get; private set;}

        public PersonalConvertData(float officialPercent, float unofficialPercent, float loosePercent) {
            this.OfficialConvertPercent = officialPercent;
            this.UnofficialConvertPercent = unofficialPercent;
            this.LoosePercent = loosePercent;
        }

        public PersonalConvertData(PersonalConvertJsonData jsonData) {
            this.OfficialConvertPercent = jsonData.officialPercent;
            this.UnofficialConvertPercent = jsonData.unofficialPercent;
            this.LoosePercent = jsonData.loosePercent;
        }

        public PersonalConvertJsonData GetJsonData()
            => new PersonalConvertJsonData {
                officialPercent = OfficialConvertPercent,
                unofficialPercent = UnofficialConvertPercent,
                loosePercent = LoosePercent
            };

        public override string ToString() {
            return $"official percent => {OfficialConvertPercent}, unofficial percent => {UnofficialConvertPercent}, loose percent => {LoosePercent}";
        }
    }



    [System.Serializable]
    public class PersonalConvertJsonData {
        public float officialPercent;
        public float unofficialPercent;
        public float loosePercent;
    }


    public class ProductData {
        public ProductData(string nameId, ProductType type, int id, int statusPoints, double price)
        {
            name_id = nameId;
            Type = type;
            this.id = id;
            status_points = statusPoints;
            this.price = price;
        }

        public ProductType Type {get; private set;}
        public int id {get; private set;}
        public string name_id;
        public long status_points {get; private set; }
        public double price;


        public ProductData(ProductJsonData jsonData ) {
            this.Type = jsonData.Type;
            this.id = jsonData.id;
            this.name_id = jsonData.name_id;
            this.status_points = jsonData.status_points;
            this.price = jsonData.price;
        }

        public ProductJsonData GetJsonData()
            => new ProductJsonData {
                Type = Type,
                id = id,
                name_id = name_id,
                status_points = status_points,
                price = price,
            };

        public override string ToString() {
            return $"id => {id}, type => {Type}, status points => {status_points}";
        }
    }

    [System.Serializable]
    public class ProductJsonData
    {
        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public ProductType Type;
        public int id;
        public string name_id;
        public long status_points;
        public double price;
    }

    public class StatusPointData{
        public int StatusLevel {get; private set;}
        public long Points { get; private set;}
        public int Planet { get; private set; }


        public StatusPointData(int level, long points, int planet) {
            this.StatusLevel = level;
            this.Points = points;
        }

        public StatusPointData(StatusPointJsonData jsonData) {
            this.StatusLevel = jsonData.level;
            this.Points = jsonData.points;
            this.Planet = jsonData.planet;
        }

        public StatusPointJsonData GetJsonData()
            => new StatusPointJsonData {
                level = StatusLevel,
                points = Points,
                planet = Planet
            };

        public override string ToString() {
            return $"status level => {StatusLevel}, points => {Points}, planet => {Planet}";
        }
    }

    [System.Serializable]
    public class StatusPointJsonData {
        public int level;
        public long points;
        public int planet;
    }




    public class PersonalImprovementsData {
        public PersonalConvertData ConvertData { get; private set; }
        //public Dictionary<ProductType, List<ProductData>> Products { get; private set; }
        public List<StatusPointData> StatusPoints { get; private set; }

        public int GetMaxSellsCountToInvestors(int planetId)
            => StatusPoints.Count(sp => sp.Planet == planetId);


        public void Copy(PersonalImprovementsData other ) {
            this.ConvertData = other.ConvertData;
            //this.Products = other.Products;
            this.StatusPoints = other.StatusPoints;
        }

        public void Copy(PersonalImprovementJsonData data) {
            ConvertData = new PersonalConvertData(data.convertData);
            //Products = new Dictionary<ProductType, List<ProductData>>();
            //foreach (var kvp in data.products) {
            //    Products.Add((ProductType)kvp.Key, kvp.Value.Select(o => new ProductData(o)).ToList());
            //}
            StatusPoints = data.statusPoints.Select(sp => new StatusPointData(sp)).ToList();
        }

        public PersonalImprovementsData() { }

        public PersonalImprovementsData(PersonalImprovementJsonData data) {
            Copy(data);
        }

        public PersonalImprovementsData(PersonalConvertData convertData, List<StatusPointData> statusPoints) {
            this.ConvertData = convertData;
            //this.Products = products;
            this.StatusPoints = statusPoints;
        }

        public PersonalImprovementJsonData GetJsonData() {
            PersonalConvertJsonData convertData = ConvertData.GetJsonData();
            Dictionary<int, List<ProductJsonData>> products = new Dictionary<int, List<ProductJsonData>>();
            //foreach(var kvp in Products ) {
            //    products.Add((int)kvp.Key, kvp.Value.Select(v => v.GetJsonData()).ToList());
            //}
            List<StatusPointJsonData> points = StatusPoints.Select(sp => sp.GetJsonData()).ToList();
            return new PersonalImprovementJsonData {
                convertData = convertData,
                //products = products,
                statusPoints = points
            };
        }
    }

    public class PersonalImprovementJsonData {
        public PersonalConvertJsonData convertData;
        //public Dictionary<int, List<ProductJsonData>> products;
        public List<StatusPointJsonData> statusPoints;
    }

    public enum ProductType : int {
        Transport = 0,
        Clothing = 1,
        RealEstate = 2,
        Assistants = 3,
        Entertainment = 4,
        Equipment = 5,
        Health = 6
    }

    public class LocalProductJsonData {
        public int id;
        public string name;
        public SpritePathData icon;
    }

    [System.Serializable]
    public class ManagerIconJsonData {
        public int planet_id;
        public SpritePathData active;
        public SpritePathData disabled;
    }

    [System.Serializable]
    public class ManagerLocalJsonData {
        public int id;
        public List<ManagerIconJsonData> icons;
        public List<ObjectName> names;

        public Option<ObjectName> GetName(int planetId ) {
            var nmObj = names.FirstOrDefault(nm => nm.planet_id == planetId);
            if(nmObj == null ) {
                return F.None;
            } else {
                return F.Some(nmObj);
            }
        }
    }

    public class ManagerEfficiencyImproveData {
        public int Level { get; private set; }
        public float EfficiencyIncrement { get; private set; }
        public int CoinsPrice { get; private set; }

        public ManagerEfficiencyImproveData(int level, float effImprove, int price) {
            this.Level = level;
            this.EfficiencyIncrement = effImprove;
            this.CoinsPrice = price;
        }

        public ManagerEfficiencyImproveData(ManagerEfficiencyImproveJsonData jsonData ) {
            this.Level = jsonData.level;
            this.EfficiencyIncrement = jsonData.efficiencyIncrement;
            this.CoinsPrice = jsonData.coinsPrice;
        }

        public ManagerEfficiencyImproveJsonData GetJsonData() {
            return new ManagerEfficiencyImproveJsonData {
                level = Level,
                efficiencyIncrement = EfficiencyIncrement,
                coinsPrice = CoinsPrice
            };
        }

        public override string ToString() {
            return $"level => {Level}, Increment => {EfficiencyIncrement}, Price => {CoinsPrice}";
        }
    }

    public class ManagerEfficiencyImproveJsonData {
        public int level;
        public float efficiencyIncrement;
        public int coinsPrice;
    }



    public class ManagerRollbackImproveData {
        public int Level { get; private set; }
        public float RollbackIncrement { get; private set; }
        public int CoinsPrice { get; private set; }

        public ManagerRollbackImproveData(int level, float rollbackIncrement, int coinsPrice ) {
            this.Level = level;
            this.RollbackIncrement = rollbackIncrement;
            this.CoinsPrice = coinsPrice;
        }

        public ManagerRollbackImproveData(ManagerRollbackImproveJsonData jsonData ) {
            this.Level = jsonData.level;
            this.RollbackIncrement = jsonData.rollbackIncrement;
            this.CoinsPrice = jsonData.coinsPrice;
        }

        public ManagerRollbackImproveJsonData GetJsonData()
            => new ManagerRollbackImproveJsonData {
                level = Level,
                rollbackIncrement = RollbackIncrement,
                coinsPrice = CoinsPrice
            };

        public override string ToString() {
            return $"Level => {Level}, Increment => {RollbackIncrement}, Price => {CoinsPrice}";
        }
    }

    public class ManagerRollbackImproveJsonData {
        public int level;
        public float rollbackIncrement;
        public int coinsPrice;
    }

    public class MegaManagerImproveData {
        public float EfficiencyIncrement { get; private set; }
        public float RollbackIncrement { get; private set; }
        public int CoinPrice { get; private set; }

        public MegaManagerImproveData(float efficiencyIncrement, float rollbackIncrement, int coinPrice) {
            this.EfficiencyIncrement = efficiencyIncrement;
            this.RollbackIncrement = rollbackIncrement;
            this.CoinPrice = coinPrice;
        }

        public MegaManagerImproveData(MegaManagerImproveJsonData jsonData ) {
            this.EfficiencyIncrement = jsonData.efficiencyIncrement;
            this.RollbackIncrement = jsonData.rollbackIncrement;
            this.CoinPrice = jsonData.coinPrice;
        }

        public MegaManagerImproveJsonData GetJsonData()
            => new MegaManagerImproveJsonData {
                efficiencyIncrement = EfficiencyIncrement,
                rollbackIncrement = RollbackIncrement,
                coinPrice = CoinPrice
            };

        public override string ToString() {
            return $"Efficiency Increment => {EfficiencyIncrement}, Rollback Increment => {RollbackIncrement}";
        }
    }

    public class MegaManagerImproveJsonData {
        public float efficiencyIncrement;
        public float rollbackIncrement;
        public int coinPrice;
    }

    public class ManagerImproveData {
        public Dictionary<int, ManagerEfficiencyImproveData> EfficiencyImprovements { get; private set; }
        public Dictionary<int, ManagerRollbackImproveData> RollbackImprovements { get; private set; }
        public MegaManagerImproveData MegaImprovement { get; private set; }

        public ManagerImproveData(Dictionary<int, ManagerEfficiencyImproveData> efficiencyImrovements,
            Dictionary<int, ManagerRollbackImproveData> rollbackImrovements,
            MegaManagerImproveData megaImprovement ) {
            this.EfficiencyImprovements = efficiencyImrovements;
            this.RollbackImprovements = rollbackImrovements;
            this.MegaImprovement = megaImprovement;
        }

        public ManagerImproveData(ManagerImproveData other ) {
            EfficiencyImprovements = new Dictionary<int, ManagerEfficiencyImproveData>();
            foreach(var kvp in other.EfficiencyImprovements ) {
                EfficiencyImprovements.Add(kvp.Key, kvp.Value);
            }

            RollbackImprovements = new Dictionary<int, ManagerRollbackImproveData>();
            foreach(var kvp in other.RollbackImprovements ) {
                RollbackImprovements.Add(kvp.Key, kvp.Value);
            }

            MegaImprovement = new MegaManagerImproveData(other.MegaImprovement.EfficiencyIncrement, other.MegaImprovement.RollbackIncrement, other.MegaImprovement.CoinPrice);
        }

        public ManagerImproveData(ManagerImproveJsonData jsonData) {
            EfficiencyImprovements = new Dictionary<int, ManagerEfficiencyImproveData>();
            if(jsonData.efficiencyImprovements != null ) {
                foreach(var kvp in jsonData.efficiencyImprovements) {
                    EfficiencyImprovements.Add(kvp.Key, new ManagerEfficiencyImproveData(kvp.Value));
                }
            }

            RollbackImprovements = new Dictionary<int, ManagerRollbackImproveData>();
            if(jsonData.rollbackImprovements != null ) {
                foreach(var kvp in jsonData.rollbackImprovements) {
                    RollbackImprovements.Add(kvp.Key, new ManagerRollbackImproveData(kvp.Value));
                }
            }

            MegaImprovement = new MegaManagerImproveData(jsonData.megaImprovement);
        }

        public ManagerImproveJsonData GetJsonData() {
            Dictionary<int, ManagerEfficiencyImproveJsonData> efficiencyImprovements = new Dictionary<int, ManagerEfficiencyImproveJsonData>();
            foreach(var kvp in EfficiencyImprovements) {
                efficiencyImprovements.Add(kvp.Key, kvp.Value.GetJsonData());
            }

            Dictionary<int, ManagerRollbackImproveJsonData> rollbackImprovements = new Dictionary<int, ManagerRollbackImproveJsonData>();
            foreach(var kvp in RollbackImprovements) {
                rollbackImprovements.Add(kvp.Key, kvp.Value.GetJsonData());
            }
            return new ManagerImproveJsonData {
                efficiencyImprovements = efficiencyImprovements,
                rollbackImprovements = rollbackImprovements,
                megaImprovement = MegaImprovement.GetJsonData()
            };
        }

        public override string ToString() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Efficiency Improvements => ");
            foreach(int level in EfficiencyImprovements.Keys.OrderBy(k => k)) {
                sb.AppendLine(EfficiencyImprovements[level].ToString());
            }

            sb.AppendLine("Rollback Imrpovements => ");
            foreach(int level in RollbackImprovements.Keys.OrderBy(k => k)) {
                sb.AppendLine(RollbackImprovements[level].ToString());
            }

            sb.AppendLine("MEGA Improvement => ");
            sb.AppendLine(MegaImprovement.ToString());
            return sb.ToString();
        }
    }

    public class ManagerImproveJsonData {
        public Dictionary<int, ManagerEfficiencyImproveJsonData> efficiencyImprovements;
        public Dictionary<int, ManagerRollbackImproveJsonData> rollbackImprovements;
        public MegaManagerImproveJsonData megaImprovement;
    }

    public interface IGeneratorUpgradeData {
        int GeneratorId { get; }
    }

    public class UpgradeJsonData {
        public int id;
        public int generatorId;
        public string name;
        public double price;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public UpgradeType upgradeType;

        public double value;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public CurrencyType currencyType;
    }

    public class UpgradeData : IGeneratorUpgradeData {
        public int Id { get;}
        public int GeneratorId { get;  }
        public string Name { get; }

        public double Price(Func<double> priceMult) {
            return BasePrice * priceMult();
        }

        public UpgradeType UpgradeType { get; }
        public double Value { get; }
        public CurrencyType CurrencyType { get; }

        private double BasePrice { get; set; }

        public UpgradeData(UpgradeJsonData jsonData ) {
            Id = jsonData.id;
            GeneratorId = jsonData.generatorId;
            Name = jsonData.name;
            BasePrice = jsonData.price;
            UpgradeType = jsonData.upgradeType;
            Value = jsonData.value;
            CurrencyType = jsonData.currencyType;
        }

        public override string ToString() {
            return $"Id=>{Id}, GeneratorId=>{GeneratorId}, Name=>{Name}, Price=>{BasePrice}, UpgradeType=>{UpgradeType}, Value=>{Value}, CurrencyType=>{CurrencyType}";
        }
    }

    public class CoinUpgradeJsonData {
        public int id;
        public int generatorId;
        public int price;
        public bool isOneTime;
        public bool isPermanent;
        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public UpgradeType upgradeType;
        public int profitMultiplier;
        public int timeMultiplier;
        public int daysOfFutureBalance;
        public string name;
        public string description;
        public double baseCost;
        public double coef;
        public double rollbackCoef;
        public SpritePathData icon;
        public int order;


    }

    public class BosCoinUpgradeData : IGeneratorUpgradeData  {
        public int Id { get; }
        public int GeneratorId { get; }
        public int Price { get; }
        public bool IsOneTime { get; }
        public bool IsPermanent { get; }
        public UpgradeType UpgradeType { get; }
        public int ProfitMutlitplier { get; }
        public int TimeMultiplier { get; }
        public int DaysOfFutureBalance { get; }
        public string Name { get; }
        public string Description { get; }
        public double BaseCost { get; }
        public double Coef { get; }
        public double RollbackCoef { get; }
        public SpritePathData Icon { get; }
        public int Order { get; }
        
        public BosCoinUpgradeData(CoinUpgradeJsonData jsonData ) {
            Id = jsonData.id;
            GeneratorId = jsonData.generatorId;
            Price = jsonData.price;
            IsOneTime = jsonData.isOneTime;
            IsPermanent = jsonData.isPermanent;
            UpgradeType = jsonData.upgradeType;
            ProfitMutlitplier = jsonData.profitMultiplier;
            TimeMultiplier = jsonData.timeMultiplier;
            DaysOfFutureBalance = jsonData.daysOfFutureBalance;
            Name = jsonData.name;
            Description = jsonData.description;
            BaseCost = jsonData.baseCost;
            Coef = jsonData.coef;
            RollbackCoef = jsonData.rollbackCoef;
            Icon = jsonData.icon;
            Order = jsonData.order;
        }

        public override string ToString() {
            return $"Id=>{Id}, GeneratorId=>{GeneratorId}, Price=>{Price}, IsOneTime=>{IsOneTime}, IsPermanent=>{IsPermanent}, UpgradeType=>{UpgradeType}, " +
                $"ProfitMultiplier=>{ProfitMutlitplier}, TimeMultiplier=>{TimeMultiplier}, DaysOfFutureBalance=>{DaysOfFutureBalance}, Name=>{Name}, Description=>{Description}, " +
                $"BaseCost=>{BaseCost}, Coef=>{Coef}, RollbackCoef=>{RollbackCoef}, IconId=>{Icon?.ToString() ?? ""}";
        }
    }

    public class PlayerIconJsonData {
        public int planet_id;
        public SpritePathData large_male;
        public SpritePathData large_female;
        public SpritePathData small_male;
        public SpritePathData small_female;
    }

    public class PlayerIconData {
        public int PlanetId { get;  }
        public SpritePathData LargeMale { get; }
        public SpritePathData LargeFemale { get; }
        public SpritePathData SmallMale { get; }
        public SpritePathData SmallFemale { get; }

        public PlayerIconData(PlayerIconJsonData jsonData ) {
            PlanetId = jsonData.planet_id;
            LargeMale = jsonData.large_male;
            LargeFemale = jsonData.large_female;
            SmallMale = jsonData.small_male;
            SmallFemale = jsonData.small_female;
        }

        public SpritePathData GetLarge(Gender gender) {
            if(gender == Gender.Male) {
                return LargeMale;
            } else {
                return LargeFemale;
            }
        } 

        public SpritePathData GetSmall(Gender gender ) {
            if(gender == Gender.Male) {
                return SmallMale;
            } else {
                return SmallFemale;
            }
        }
    }

    public class StatusNameJsonData {
        public int status;
        public string name;
    }

    public class StatusNameData {
        public int Status {get;}
        public string Name {get;}

        public StatusNameData(StatusNameJsonData jsonData) {
            this.Status = jsonData.status;
            this.Name = jsonData.name;
        }
    }
}