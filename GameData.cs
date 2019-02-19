using Bos;
using Bos.Data;
using Bos.Debug;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Purchasing;

/*
[System.Serializable]
public class GameData
{
    private static GameData _instance;
    public static GameData instance => _instance ?? (_instance = ReadGameDataFromDisk());

    //public List<GeneratorPrototype> generators;
    //public List<SerializedUpgrade> cashUpgrades;
    //public List<SerializedUpgrade> investorsUpgrades;
    //public List<CoinUpgrade> coinUpgrades;
    
    //public List<ManagerPrototype> managers = new List<ManagerPrototype>();
    
    
    public Language language;
    public static GameData ReadGameDataFromDisk()
    {
        GameData game = null;
        var data =File.ReadAllBytes(GetFilePath("StaticData.bytes"));
        {
            var stream = new MemoryStream(data);
            IFormatter formatter = new BinaryFormatter();
            game = formatter.Deserialize(stream) as GameData;

        }
        return game;
    }


    
    public Dictionary<string, string> en = new Dictionary<string, string>();
    public Dictionary<string, string> ru = new Dictionary<string, string>();
    public string GetLocalization(string id)
    {
        var result = "";
        switch (language)
        {
            case Language.en: en.TryGetValue(id, out result);
                break;
            case Language.ru: ru.TryGetValue(id, out result);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return result;
    }

    public static string GetFilePath(string fileName)
    {
        string dbPath = "";
           
        if (Application.platform == RuntimePlatform.Android)
        {
            // Android
            string oriPath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
             
            // Android only use WWW to read file
            WWW reader = new WWW(oriPath);
            while ( ! reader.isDone) {}
             
            var realPath = Application.persistentDataPath + "/" + fileName;
            System.IO.File.WriteAllBytes(realPath, reader.bytes);
             
            return realPath;
        }
        else
        {
            // iOS
            return System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        }
    }
}
*/
/*
[System.Serializable]
 public class GeneratorPrototype
 {
     public int Id{ get; set; }
     public double BaseCost { get; set; }
     public int CoinPrice { get; set; }
     public int EnhancePrice { get; set; }
     public double IncrementFactor { get; set; }
     public double BaseGeneration { get; set; }
     public float TimeToGenerate { get; set; }
     public string Name { get; set; }
     public double IncrementFactorTime { get; set; }
     public double ProfitIncrementFactor { get; set; }
     public string ManagerIcon { get; set; }
 }*/

    /*
[System.Serializable]
public class ManagerPrototype
{
    public int Id{ get; set; }
    public double BaseCost { get; set; }
    public double Coef { get; set; }
    public double KickBackCoef { get; set; }
    public string Name{ get; set; }
    public string Desc{ get; set; }

    public override string ToString() {
        return $"Id: {Id}, BaseCost: {BaseCost}, Coef: {Coef}, KickBackCoef: {KickBackCoef}, Name: {Name}, Desc: {Desc}";
    }
}*/

/* 
[System.Serializable]
public class CoinUpgrade
{
    public int Id{ get; set; }
    public int TargetId{ get; set; }
    public int Price{ get; set; }
    public bool OneTimePurchase{ get; set; }
    public bool Permanent{ get; set; }
    public UpgradeType UpgradeType{ get; set; }
    public int ProfitMultiplier { get; set; }
    public int TimeMultiplier { get; set; }
    public int DaysOfFutureBalance { get; set; }
    
    public string Name{ get; set; }
    public string Desc{ get; set; }
    
    public double BaseCost { get; set; }
    public double Coef { get; set; }
    public double KickBackCoef { get; set; }

    public void Transform(int id, out CoinUpgradeJsonData newUpgrade, out LocalizationStringData nameData, out LocalizationStringData descriptionData ) {
        string nameId = $"coin_upgrade_name_{id}";
        string descriptionId = $"coin_upgrade_desc_{id}";

        newUpgrade = new CoinUpgradeJsonData {
            baseCost = BaseCost,
            coef = Coef,
            daysOfFutureBalance = DaysOfFutureBalance,
            description = descriptionId,
            generatorId = TargetId,
            id = id,
            isOneTime = OneTimePurchase,
            isPermanent = Permanent,
            name = nameId,
            price = Price,
            profitMultiplier = ProfitMultiplier,
            rollbackCoef = KickBackCoef,
            timeMultiplier = TimeMultiplier,
            upgradeType = this.UpgradeType
        };

        nameData = new LocalizationStringData {
            id = nameId,
            en = Name,
            ru = string.Empty
        };

        descriptionData = new LocalizationStringData {
            id = descriptionId,
            en = Desc,
            ru = string.Empty
        };
    }

}*/

/*
public enum Language
{
    en,
    ru
}*/