using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelDb
{
    public static List<Level> Levels;

    public static void Load()
    {
        Levels = new List<Level>();
        Levels.Add(new Level(0, RewardType.SpeedUpgrade, 25, 2));
        Levels.Add(new Level(0, RewardType.SpeedUpgrade, 50, 2));
        Levels.Add(new Level(0, RewardType.SpeedUpgrade, 75, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 100, 2));        
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 200, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 300, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 400, 4));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 500, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 600, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 700, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 800, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 900, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1000, 4));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1100, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1200, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1300, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1400, 3));        
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1500, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1600, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1700, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1800, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 1900, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 2000, 5));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 2250, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 2500, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 2750, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 3000, 5));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 3250, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 3500, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 3750, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 4000, 5));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 4250, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 4500, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 4750, 2));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 5000, 5));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 5250, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 5500, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 5750, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 6000, 5));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 6250, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 6500, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 6750, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 7000, 5));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 7000, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 7250, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 7500, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 7777, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 8000, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 8200, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 8400, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 8600, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 8800, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9000, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9100, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9200, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9300, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9400, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9500, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9600, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9700, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9800, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 9999, 3));
        Levels.Add(new Level(0, RewardType.ProfitUpgrade, 10000, 5));

        for (int i = 0; i < Levels.Count; i++)
        {
            Levels[i].Id = i;
        }

        
    }

    public static int Count
        => Levels.Count;

    public static Level GetLevel(int id) {
        if(id < Count) {
            return Levels[id];
        }
        return null;
    }
    public static IEnumerable<Level> GetLevelsForCount(int count) {
        return Levels.Where(level => level.NextLevelCount <= count);
    }

    /*
    public static Level CalculateLevel(int count)
    {
        for(int i = 0; i < Levels.Count; i++ ) {
            int nextIndex = i + 1;
            if(Levels[i].NextLevelCount > count) {
                return Levels[i];
            } else if(Levels[i].NextLevelCount == count && nextIndex < Levels.Count) {
                return Levels[nextIndex];
            }
        }
        return Levels[Levels.Count - 1];
    }

    public static Level CalculatePreviousLevel(int count)
    {
        for(int i = 0; i < Levels.Count; i++ ) {
            int prevIndex = i - 1;
            if(Levels[i].NextLevelCount > count ) {
                if(prevIndex >= 0 ) {
                    return Levels[prevIndex];
                }
            } else if(Levels[i].NextLevelCount == count ) {
                if(prevIndex >= 0 ) {
                    return Levels[prevIndex];
                }
            }
        }
        return Levels[Levels.Count - 2];
    }*/
}

public class Level
{
    public int Id;
    public int NextLevelCount;
    public RewardType RewardType;
    public int RewardValue;

    public Level()
    {

    }

    public Level(int c, RewardType rt, int nextLevelCount, int val)
    {
        Id = c;
        NextLevelCount = nextLevelCount;
        RewardType = rt;
        RewardValue = val;
    }

    public override string ToString() {
        return $"[Level]: Id => {Id}, nextLevelCount => {NextLevelCount}, rewardType => {RewardType}, rewardValue => {RewardValue}";
    }
}