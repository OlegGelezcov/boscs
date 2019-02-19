namespace Bos.Data {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Ozh.Tools.Functional;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public enum GeneratorType : byte {
        Normal = 0,
        Planet = 1
    }

    public class GeneratorData {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public double BaseCost { get; private set; }
        public double IncrementFactor { get; private set; }
        public double BaseGeneration { get; private set; }
        public float TimeToGenerate { get; private set; }
        //public int CoinPrice { get; private set; }
        public int EnhancePrice { get; private set; }
        public double ProfitIncrementFactor { get; private set; }
        public string ManagerIconId { get; private set; }
        public GeneratorType Type { get; private set; }

        public void UpdateFrom(GeneratorData otherData ) {
            BaseGeneration = otherData.BaseGeneration;
            ProfitIncrementFactor = otherData.ProfitIncrementFactor;
            TimeToGenerate = otherData.TimeToGenerate;
            IncrementFactor = otherData.IncrementFactor;
            BaseCost = otherData.BaseCost;
        }

        public GeneratorData(GeneratorJsonData data, GeneratorType type) {
            this.Id = data.id;
            this.Name = data.name;
            this.BaseCost = data.baseCost;
            this.IncrementFactor = data.incrementFactor;
            this.BaseGeneration = data.baseGeneration;
            this.TimeToGenerate = data.timeToGenerate;
            //this.CoinPrice = data.coinPrice;
            this.EnhancePrice = data.enhancePrice;
            this.ProfitIncrementFactor = data.profitIncrementFactor;
            this.ManagerIconId = data.managerIcon;
            this.Type = type;
        }

        public void Replace(double baseCost, double incrementFactor, double baseGeneration, float timeToGenerate, 
            int enhancePrice, double profitIncrementFactor) {
            this.BaseCost = baseCost;
            this.IncrementFactor = incrementFactor;
            this.BaseGeneration = baseGeneration;
            this.TimeToGenerate = timeToGenerate;
            //this.CoinPrice = coinPrice;
            this.EnhancePrice = enhancePrice;
            this.ProfitIncrementFactor = profitIncrementFactor;
        }

        public override string ToString() {
            return $"id => {Id}, name => {Name}, base cost => {BaseCost}, increment factor => {IncrementFactor}{System.Environment.NewLine}"
                + $"base generation => {BaseGeneration}, time to generate => {TimeToGenerate}, enhance price => {EnhancePrice}{System.Environment.NewLine}"
                + $"profit increment factor => {ProfitIncrementFactor}, manager icon id => {ManagerIconId}, type => {Type}";
        }
    }

    public class GeneratorJsonData {

        public int id;
        public string name;
        public double baseCost;
        public double incrementFactor;
        public double baseGeneration;
        public float timeToGenerate;
        //public int coinPrice;
        public int enhancePrice;
        public double profitIncrementFactor;
        public string managerIcon;
    }

    [System.Serializable]
    public class GeneratorLocalData {
        public int id;
        public bool dependent;
        public int required_id;
        public int planet_id;
        public List<ObjectName> names;
        public List<GeneratorPlanetIconData> planet_icons;
        public List<ObjectCoinPrice> research_price;


        public GeneratorPlanetIconData GetIconData(int planetId) {
            if(planet_icons == null ) {
                planet_icons = new List<GeneratorPlanetIconData>();
            }
            return planet_icons.FirstOrDefault(data => data.planet_id == planetId);
        }

        public ObjectName GetName(int planetId) {
            if(names == null ) { names = new List<ObjectName>(); }
            return names.FirstOrDefault(nm => nm.planet_id == planetId);
        }

        public Option<ObjectCoinPrice> GetResearchPrice(int planetId ) {
            if(research_price == null ) { research_price = new List<ObjectCoinPrice>(); }
            var objPrice = research_price.FirstOrDefault(p => p.planet_id == planetId);
            if(objPrice != null ) {
                return F.Some(objPrice);
            }
            return F.None;
        }

    }

    [System.Serializable]
    public class ObjectCoinPrice {
        public int planet_id;
        public int price;
    }

    [System.Serializable]
    public class ObjectName {
        public int planet_id;
        public string name;
    }

    [System.Serializable]
    public class GeneratorPlanetIconData {
        public int planet_id;
        public string icon_id;
        public string big_icon_id;
        public string lock_icon;
    }

    public class ManagerData {
        public int Id { get; private set; }
        public double BaseCost { get; private set; }
        public double Coef { get; private set; }
        public double KickBackCoef { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public ManagerData(ManagerJsonData data) {
            this.Id = data.id;
            this.BaseCost = data.baseCost;
            this.Coef = data.coef;
            this.KickBackCoef = data.kickBackCoef;
            this.Name = data.name;
            this.Description = data.description;
        }

        public void UpdateValues(double baseCost, double coef) {
            this.BaseCost = baseCost;
            this.Coef = coef;
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

    public class CashUpgradeData {
        public int id;
        public string name;
        public double cost;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public UpgradeType upgradeType;

        public int amount;
    }

    public class InvestorUpgradeData {
        public int id;
        public string name;
        public double cost;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public UpgradeType upgradeType;

        public int amount;
    
    }

    public class CoinUpgradeData {
        public int id;
        public int targetId;
        public int price;
        public string name;
        public string description;
        public int oneTimePurchase;
        public int permanent;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public UpgradeType upgradeType;

        public int profitMultiplier;
        public int timeMultiplier;
        public int daysOfFutureBalance;
    }

    public class LocalizationStringData {
        public string id;
        public string en;
        public string ru;

        public string GetString(SystemLanguage language) {
            if(language == SystemLanguage.Russian) {
                return ru;
            } else {
                return en;
            }
        }
    }
}