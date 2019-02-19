using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalData
{
    public const string BalancePrefKey = "BIGINT_BALANCE";
    public const string CurrencyPrefKey = "STRING_CURRENCY";
    public const string LastUpdateTimeKey = "DATE_LAST_UPDATE_BALLANCE";
    public const string LifetimeSavingsKey = "LIFETIME_SAVINGS";
    public const string LastPauseKey = "LAST_PAUSE";
    public const string LastServerPauseKey = "LAST_SERVER_PAUSE";
    public const string LastServerPingKey = "LAST_SERVER_PING";

    //public static string StoredBalanceString
    //{
    //    get { return PlayerPrefs.GetString(BalancePrefKey, "4"); }
    //    set { PlayerPrefs.SetString(BalancePrefKey, value); }
    //}

    //public static double StoredBalance
    //{
    //    get { return GlobalRefs.PlayerData.StoredBalance; }
    //    set { GlobalRefs.PlayerData.StoredBalance = value; }
    //}
    
    //public static double StoreMaxBalance
    //{
    //    get { return GlobalRefs.PlayerData.StoredMaxBalance; }
    //    set { GlobalRefs.PlayerData.StoredMaxBalance = value; }
    //}

        /*
    public static string StoredCurrencyString
    {
        get { return PlayerPrefs.GetString(CurrencyPrefKey, "$"); }
        set { PlayerPrefs.SetString(CurrencyPrefKey, value); }
    }

    public static Currency StoredCurrency
    {
        get { return Currencies.GetFromString(StoredCurrencyString); }
        set { StoredCurrencyString = value.CurrencySign; }
    }*/


    //public static DateTime LastUpdateBalance
    //{
    //    get { return DateTime.Parse(PlayerPrefs.GetString(LastUpdateTimeKey, "1337/3/3")); }
    //    set { PlayerPrefs.SetString(LastUpdateTimeKey, value.ToString()); }
    //}

    //public static DateTime LastPauseTime
    //{
    //    get { return DateTime.Parse(PlayerPrefs.GetString(LastPauseKey, "1337/3/3")); }
    //    set { PlayerPrefs.SetString(LastPauseKey, value.ToString()); }
    //}

    //public static double LifetimeSavings
    //{
    //    get { return GlobalRefs.PlayerData.LifetimeEarnings; }
    //    set { GlobalRefs.PlayerData.LifetimeEarnings = value; }
    //}

    //public static bool IsFirstTimeLaunch
    //{
    //    get { return Convert.ToBoolean(PlayerPrefs.GetInt("FirstTimeRun", 1)); }
    //    set { PlayerPrefs.SetInt("FirstTimeRun", Convert.ToInt32(value)); }
    //}

    /*
    public static int Resets
    {
        get { return PlayerPrefs.GetInt("resets", 0); }
        set { PlayerPrefs.SetInt("resets", value); }
    }*/


    //public static int CurrentTutorialStep
    //{
    //    get { return PlayerPrefs.GetInt("CurrentTutorialStep", 0); }
    //    set { PlayerPrefs.SetInt("CurrentTutorialStep", value); }
    //}

    public static bool TutorialIntroComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialIntroComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialIntroComplete", Convert.ToInt32(value)); }
    }

    public static bool TutorialManagersComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialManagersComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialManagersComplete", Convert.ToInt32(value)); }
    }

    public static bool TutorialUpgradesComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialUpgradesComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialUpgradesComplete", Convert.ToInt32(value)); }
    }

    public static bool TutorialInvestorsComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialInvestorsComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialInvestorsComplete", Convert.ToInt32(value)); }
    }
    
    //public static bool FirstInvestorsComplete
    //{
    //    get { return Convert.ToBoolean(PlayerPrefs.GetInt("FirstInvestorsComplete", 0)); }
    //    set { PlayerPrefs.SetInt("FirstInvestorsComplete", Convert.ToInt32(value)); }
    //}

    public static double LastPauseBalance
    {
        get { return GameServices.Instance.PlayerService.LegacyPlayerData.LastPauseBalance; }
        set { GameServices.Instance.PlayerService.LegacyPlayerData.LastPauseBalance = value; }
    }

    /*
    public static int FreeGameTriesForCasino
    {
        get { return PlayerPrefs.GetInt("FreeGameTriesForCasino", 0); }
        set { PlayerPrefs.SetInt("FreeGameTriesForCasino", value); }
    }*/
    
    
    public static int FreeGameTriesForRace
    {
        get { return PlayerPrefs.GetInt("FreeGameTriesForRace", 0); }
        set { PlayerPrefs.SetInt("FreeGameTriesForRace", value); }
    }
    
    /*
    public static DateTime TriesRefundDateCasino
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("TriesRefundDateCasino", "1337/3/3")); }
        set { PlayerPrefs.SetString("TriesRefundDateCasino", value.ToString()); }
    }*/
    
    /*
    public static DateTime TriesRefundDateRace
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("TriesRefundDateRace", "1337/3/3")); }
        set { PlayerPrefs.SetString("TriesRefundDateRace", value.ToString()); }
    }*/
    

    public static DateTime NextEventTime
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("NextEventTime", "1337/3/3")); }
        set { PlayerPrefs.SetString("NextEventTime", value.ToString()); }
    }

    public static int AdvertisementMultiplier
    {
        get { return PlayerPrefs.GetInt("AdvertisementMultiplier", 2); }
        set { PlayerPrefs.SetInt("AdvertisementMultiplier", value); }
    }

/* 
    public static int Gender
    {
        get { return PlayerPrefs.GetInt("Gender", 1); }
        set { PlayerPrefs.SetInt("Gender", value); }
    }
*/

    /* 
    public static DateTime LastUpdateServerDateTime
    {
        get { return DateTime.Parse(PlayerPrefs.GetString(LastServerPingKey, "1337/3/3")); }
        set { PlayerPrefs.SetString(LastServerPingKey, value.ToString()); }
    }*/

    
    /*
    public static DateTime LastPauseServerDateTime
    {
        get { return DateTime.Parse(PlayerPrefs.GetString(LastServerPauseKey, DateTime.MinValue.ToString())); }
        set { PlayerPrefs.SetString(LastServerPauseKey, value.ToString()); }
    }*/
    
    
    public static DateTime FirstOpenDate
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("FirstOpenDate", "1337/3/3")); }
        set { PlayerPrefs.SetString("FirstOpenDate", value.ToString()); }
    }
    
    public static DateTime SessionStart
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("SessionStart", "1337/3/3")); }
        set { PlayerPrefs.SetString("SessionStart", value.ToString()); }
    }
    
    public static int LastCardChoosen
    {
        get { return PlayerPrefs.GetInt("LastCardChoosen", 1); }
        set { PlayerPrefs.SetInt("LastCardChoosen", value); }
    }
    
    public static int LastCardChoosenTempCount
    {
        get { return PlayerPrefs.GetInt("LastCardChoosenTempCount", 1); }
        set { PlayerPrefs.SetInt("LastCardChoosenTempCount", value); }
    }
}

public class OLD_LocalData
{
    public const string BalancePrefKey = "BIGINT_BALANCE";
    public const string CurrencyPrefKey = "STRING_CURRENCY";
    public const string LastUpdateTimeKey = "DATE_LAST_UPDATE_BALLANCE";
    public const string LifetimeSavingsKey = "LIFETIME_SAVINGS";
    public const string LastPauseKey = "LAST_PAUSE";

    public static string StoredBalanceString
    {
        get { return PlayerPrefs.GetString(BalancePrefKey, "4"); }
        set { PlayerPrefs.SetString(BalancePrefKey, value); }
    }

    public static double StoredBalance
    {
        get { return double.Parse(StoredBalanceString); }
        set { StoredBalanceString = value.ToString(); }
    }

    public static string StoredCurrencyString
    {
        get { return PlayerPrefs.GetString(CurrencyPrefKey, "$"); }
        set { PlayerPrefs.SetString(CurrencyPrefKey, value); }
    }

    public static Currency StoredCurrency
    {
        get { return Currencies.GetFromString(StoredCurrencyString); }
        set { StoredCurrencyString = value.CurrencySign; }
    }

    public static DateTime LastUpdateBalance
    {
        get { return DateTime.Parse(PlayerPrefs.GetString(LastUpdateTimeKey, "1337/3/3")); }
        set { PlayerPrefs.SetString(LastUpdateTimeKey, value.ToString()); }
    }

    public static DateTime LastPauseTime
    {
        get { return DateTime.Parse(PlayerPrefs.GetString(LastPauseKey, "1337/3/3")); }
        set { PlayerPrefs.SetString(LastPauseKey, value.ToString()); }
    }

    public static double LifetimeSavings
    {
        get { return double.Parse(PlayerPrefs.GetString(LifetimeSavingsKey, "0")); }
        set { PlayerPrefs.SetString(LifetimeSavingsKey, value.ToString()); }
    }

    public static bool IsFirstTimeLaunch
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("FirstTimeRun", 1)); }
        set { PlayerPrefs.SetInt("FirstTimeRun", Convert.ToInt32(value)); }
    }

    public static int Resets
    {
        get { return PlayerPrefs.GetInt("resets", 0); }
        set { PlayerPrefs.SetInt("resets", value); }
    }

    public static int CurrentTutorialStep
    {
        get { return PlayerPrefs.GetInt("CurrentTutorialStep", 0); }
        set { PlayerPrefs.SetInt("CurrentTutorialStep", value); }
    }

    public static bool TutorialIntroComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialIntroComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialIntroComplete", Convert.ToInt32(value)); }
    }

    public static bool TutorialManagersComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialManagersComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialManagersComplete", Convert.ToInt32(value)); }
    }

    public static bool TutorialUpgradesComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialUpgradesComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialUpgradesComplete", Convert.ToInt32(value)); }
    }

    public static bool TutorialInvestorsComplete
    {
        get { return Convert.ToBoolean(PlayerPrefs.GetInt("TutorialInvestorsComplete", 0)); }
        set { PlayerPrefs.SetInt("TutorialInvestorsComplete", Convert.ToInt32(value)); }
    }

    public static double LastPauseBalance
    {
        get { return double.Parse(PlayerPrefs.GetString("LastPauseBalance", "0")); }
        set { PlayerPrefs.SetString("LastPauseBalance", value.ToString()); }
    }

    public static int FreeGameTries
    {
        get { return PlayerPrefs.GetInt("FreeGameTries", 0); }
        set { PlayerPrefs.SetInt("FreeGameTries", value); }
    }
    public static DateTime TriesRefundDate
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("TriesRefundDate", "1337/3/3")); }
        set { PlayerPrefs.SetString("TriesRefundDate", value.ToString()); }
    }


    public static DateTime NextEventTime
    {
        get { return DateTime.Parse(PlayerPrefs.GetString("NextEventTime", "1337/3/3")); }
        set { PlayerPrefs.SetString("NextEventTime", value.ToString()); }
    }

    public static int AdvertisementMultiplier
    {
        get { return PlayerPrefs.GetInt("AdvertisementMultiplier", 2); }
        set { PlayerPrefs.SetInt("AdvertisementMultiplier", value); }
    }

    public static int Gender
    {
        get { return PlayerPrefs.GetInt("Gender", 1); }
        set { PlayerPrefs.SetInt("Gender", value); }
    }
}