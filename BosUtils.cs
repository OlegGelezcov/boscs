using System;
using Unit = System.ValueTuple;

namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Ozh.Tools.Functional;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public static class BosUtils {

        public const double Epsilon = 1e-6;

        public static uint JenkinsOneAtATimeHash(string str) {
            uint hash = 0;
            for (int i = 0; i < str.Length; ++i) {
                hash += (uint)str[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        public static double Clamp01(double val) {
            if(val < 0) {
                val = 0.0;
            }
            if(val > 1.0) {
                val = 1.0;
            }
            return val;
        }

        public static double Clamp(double val, double min, double max) {
            if(val < min ) {
                val = min;
            }
            if(val > max ) {
                val = max;
            }
            return val;
        }

        public static readonly System.DateTime startDate = new System.DateTime(1970, 1, 1);
        
        public static double GetUnixTimeFor(System.DateTime dt) {
            return (dt - startDate).TotalSeconds;
        }

        public static double UnixTimestamp {
            get {
                ITimeService timeService = GameServices.Instance.TimeService;
                if(timeService != null && timeService.IsValid) {
                    return timeService.UnixTime;
                }
                UnityEngine.Debug.Log($"request time when time service is not valid".Colored(ConsoleTextColor.red).Bold());
                return 0.0;
            }
        }
            //=> (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;

        public static int UnixTimestampInt
            => (int)UnixTimestamp;

        public static System.DateTime UnixTimestampToDateTime(float timeStamp) {
            System.DateTime result = new System.DateTime(1970, 1, 1).AddSeconds(timeStamp);
            return result;
        }

        public static string FormatTimeWithColon(double seconds ) {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);
            if(timeSpan.Days == 0  && timeSpan.Hours == 0 ) {
                return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            } else if(timeSpan.Days == 0 ) {
                return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            } else {
                return $"{timeSpan.Days:00}:{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
        }

        public static string FormatTime(double seconds ) {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);

            if(timeSpan.TotalDays >= 1.0f  ) {
                return $"{timeSpan.Days:00}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m {timeSpan.Seconds:00}s";
            } else {
                return $"{timeSpan.Hours:00}h {timeSpan.Minutes:00}m {timeSpan.Seconds:00}s";
            }
        }
        
        public static bool IsWholeNumber(float value) {
            return Mathf.Approximately(value - Mathf.FloorToInt(value), 0f);
        }


        public static string GetCurrencyString(CurrencyNumber num, string numberComponentColor = "", string suffixColor = "#FFE565") {
            string[] prettyArr = num.AbbreviationColoredComponents(numberComponentColor, suffixColor);
            string result = prettyArr[0];
            if (!string.IsNullOrEmpty(prettyArr[1])) {
                result += " " + prettyArr[1];
            }
            return result;
        }

        public static string GetStandardCurrencyString(double value) {
            return "$ " + GetCurrencyString(new CurrencyNumber(value), "", "");
        }

        public static string GetCurrencyString(Bos.Data.Currency currency, string numberComponentColor = "", string suffixColor = "#FFE565") {
            switch(currency.Type) {
                case Data.CurrencyType.Coins: {
                        string result =  ((int)currency.Value).ToString();
                        if(!string.IsNullOrEmpty(numberComponentColor)) {
                            result = result.Colored(numberComponentColor);
                        }
                        return result;
                    }
                default: {
                        return GetCurrencyString(new CurrencyNumber(currency.Value), numberComponentColor, suffixColor);
                    }

            }
        }
        
        public static string GetCurrencyStringSimple(Bos.Data.Currency currency) {
            switch(currency.Type) {
                case Data.CurrencyType.Coins: {
                    string result =  ((int)currency.Value).ToString();
                    return result;
                }
                default: {
                    return GetCurrencyStringSimple(new CurrencyNumber(currency.Value));
                }
            }
        }
        
        public static string GetCurrencyStringSimple(CurrencyNumber num)
        {
            string[] prettyArr = num.AbbreviationComponents();
            string result = prettyArr[0];
            if (!string.IsNullOrEmpty(prettyArr[1])) {
                result += " " + prettyArr[1];
            }
            return result;
        }
        

        public static bool Approximately(double first, double second)
        {
            double diff = Math.Abs(first - second);
            return (diff < Epsilon);
        }

        public static void CopyDictionary<K, V>(Dictionary<K, V> source, Dictionary<K, V> destination ) {
            destination.Clear();
            foreach(var kvp in source) {
                destination.Add(kvp.Key, kvp.Value);
            }
        }

        public static readonly Unit Unit = new Unit();



        public static Unit If(System.Func<bool> predicate, System.Action trueAction, System.Action falseAction)
            => predicate() ? trueAction.ToFunc().Invoke() : falseAction.ToFunc().Invoke();

        public static List<T> MakeList<T>(params T[] objs)
            => objs.ToList();

        
        public static Option<int> GetBadgeObjectiveValue(string statName ) {
            if(statName.ToLower() == "slotswon") {
                return F.Some(GameServices.Instance.GameModeService.SlotGameWonCount);
            }  else if(statName.ToLower() == "ads_watched") {
                return F.Some(GameServices.Instance.AdService.TotalAdsWatched);
            }
            return F.None;
        }

        public static string PrefixedId(string prefix) {
            return prefix + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
        }

        public static double GetUpgradePriceMult(PlanetServerData planet, UpgradeData upgrade) {

            if (upgrade.GeneratorId != -1 && upgrade.GeneratorId < 10)
            {
                return planet.UpgradeMult[upgrade.GeneratorId];
            }
            else
            {
                return planet.GlobalUpgradeMult;
            }
        }

        public static IEnumerable<Currency> CombineCurrencies(IEnumerable<Currency> firstSeq, IEnumerable<Currency> secondSeq) {
            IEnumerable<Currency> flat = firstSeq.Concat(secondSeq);
            Dictionary<CurrencyType, Currency> result = new Dictionary<CurrencyType, Currency>();
            foreach(Currency c in flat ) {
                if(result.ContainsKey(c.Type)) {
                    result[c.Type] = result[c.Type] + c;
                } else {
                    result.Add(c.Type, c);
                }
            }
            return result.Values;
        } 
    }
}