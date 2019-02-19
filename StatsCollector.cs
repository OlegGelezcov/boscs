using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsCollector
{
    public const string KEY = "STATS_COLLECTOR";

    #region Singleton

    private static volatile StatsCollector _instance;
    private static object _syncRoot = new object();


    public static StatsCollector Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                        _instance = new StatsCollector();
                }
            }

            return _instance;
        }
    }
    private StatsCollector()
    {
    }

    #endregion

    public Dictionary<string, double> Stats { get; set; }

    public double this[string key]
    {
        get
        {
            if (Stats.ContainsKey(key))
                return Stats[key];

            return 0;
        }
        set
        {
            if (Stats.ContainsKey(key))
                Stats[key] = value;
            else
                Stats.Add(key, value);

            Save();
        }
    }

    public void CollectCustom(string statName)
    {
        this[statName]++;
    }

    public void Load()
    {
        var q = PlayerPrefs.GetString(KEY, null);
        if (!string.IsNullOrEmpty(q))
        {
            var d = JsonConvert.DeserializeObject<StatsCollector>(q);
            Stats = d.Stats;
        }
        else
        {
            Stats = new Dictionary<string, double>();
        }
    }

    public void Save()
    {
        var json = JsonConvert.SerializeObject(this);
        PlayerPrefs.SetString(KEY, json);
    }
}

public static class Stats
{
    public const string COINS_BOUGHT = "coinsBought";
    public const string COINS_AQUIRED = "coinsAQ";
    //public const string ADS_WATCHED = "watchedAds";
    //public const string SLOTS_WON = "slotsWon";
    public const string SLOTS_LOST = "slotsLost";
    public const string MONEY_SPENT = "moneySpent";
    public const string REWARDS_UNLOCKED = "rewardsUNlocked";
    public const string TIME_PLAYED = "timePlayed";
    public const string UNITS_BOUGHT = "unitsBought";
}