using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;

/*
[JsonObject(MemberSerialization.OptOut), Serializable]
public class LocalizationManager
{
    public static string[] availableLocales = {"ru_RU", "en_US"};
    public static string defaultLocale = "en_US"; 
    
    public static LocalizationData instance;

    static public LocalizationData getLocalization(string locale,bool forceLocal )
    {
        LocalizationData localization = null;


        
        var bin = File.ReadAllBytes(GameData.GetFilePath(locale + ".bytes"));

        var stream = new MemoryStream(bin);
        IFormatter formatter = new BinaryFormatter();
        localization = formatter.Deserialize(stream) as LocalizationData;
        return localization;
    }

    public static void saveLocalization(LocalizationData localization, string locale)
    {
        var bf = new BinaryFormatter();
        var file = File.Create(GameData.GetFilePath(locale + ".bytes"));

        try
        {
            bf.Serialize(file, localization);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            file.Close();
        }

        file.Close();
    }
}
public static class Localization
{
     public static void Localize(this UnityEngine.UI.Text text, string id, LocalizationDataType dataType)
    {
        var data = LocalizationManager.instance;

        var list = data.GetData(dataType);
        var localization = list.Find(val => val != null && val.ID == id);
        if (localization == null)
        {
            localization = BaseLocalization.empty;
            Debug.Log("Couldn't find the locale id: " + id);
        }
        if (text != null)
        {
            text.text = localization.GetValue();
        }
        else
        {
            Debug.Log("text component is null");
        }
    }

    public static string GetLocale(this string id, LocalizationDataType dataType)
    {
        var data = LocalizationManager.instance;
        var list = data.GetData(dataType);
        var localization = list.Find(val => val != null && val.ID == id);
        if (localization == null)
        {
            localization = BaseLocalization.empty;
            Debug.Log("Couldn't find the locale id: " + id);
        }
        return localization.GetValue();
    }
}


public enum LocalizationDataType
{
    ui,
}

[Serializable]
public class LocalizationData
{
    public string locale;
    public List<BaseLocalization> ui;
    public List<BaseLocalization> GetData(LocalizationDataType dataType)
    {
        switch (dataType)
        {
            case LocalizationDataType.ui: return ui;

            default:
                throw new ArgumentOutOfRangeException("dataType", dataType, null);
        }
    }
}


[Serializable]
public class BaseLocalization
{
    public static BaseLocalization empty = new BaseLocalization();
    public const string emptyKeyword = "!empty";
    public const string placeholder = "[Not localized yet]";

    public string ID;

    private Dictionary<string, string> additionalRows = new Dictionary<string, string>();

    public void SetValue(string key, string value)
    {
        additionalRows[key] = value;
    }

    public string GetValue(string key = null)
    {
        var result = (string) null;
        if (key == null)
        {
            foreach (var row in additionalRows)
            {
                result = row.Value;
            }
        } else
        {
            additionalRows.TryGetValue(key, out result);
        }
        return result == emptyKeyword ? "" : result ?? placeholder;
    }
}*/
