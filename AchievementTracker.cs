using Bos;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AchievementTracker : GameBehaviour
{
    public AllManagers Managers;
    public Achievement[] Achievements;
    public int Points;

    //public AchievementEvent Proc;

    public float PercentCompletion
    {
        get { return (float)Points / _maxPoints; }
    }

    private int _maxPoints;

    public override void Start()
    {
        Points = Services.AchievmentService.AchievmentPoints;

        foreach (var x in Achievements)
        {
            _maxPoints += x.Points;
            x.IsCompleted = Services.GetService<IAchievmentServcie>().IsAchievmentCompleted(x.Id.ToString()); //_pdata.CompletedAchievements.Contains(x.Id.ToString());

            if (x.IsCompleted)
            {
                ChangeGeneratorIcon(x);
            }
        }

        _maxPoints += LevelDb.Levels.Count * 5;
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.GeneratorAchievmentsReceived += OnGeneratorAchievmentsReceived;
    }

    public override void OnDisable() {
        GameEvents.GeneratorAchievmentsReceived -= OnGeneratorAchievmentsReceived;
        base.OnDisable();
    }

    private void OnGeneratorAchievmentsReceived(int generatorId, List<ExtendedAchievmentInfo> achievments) {
        Points += achievments.Count * 5;
    }

    private static void ChangeGeneratorIcon(Achievement x)
    {
        return;
    }

    private float frameTime = 0;
    public override void Update()
    {
        if (frameTime < 1.13f)
        {
            frameTime += Time.deltaTime;
            return;
        }
        frameTime = 0;

        foreach (var x in Achievements)
        {
            if (x.IsCompleted)
                continue;

            if (Services.TransportService.HasUnits(x.TargetGeneratorId))
            {
                var tCount = Services.TransportService.GetUnitTotalCount(x.TargetGeneratorId); //_pdata.OwnedGenerators[x.TargetGeneratorId];
                if (tCount >= x.TargetCount)
                {
                    Services.GetService<IAchievmentServcie>().AddCompletedAchievment(x.Id.ToString());
                    Services.AchievmentService.AddAchievmentPoints(x.Points);
                    Points += x.Points;
                    x.IsCompleted = true;

                    switch (x.RewardType)
                    {
                        case RewardType.SpeedUpgrade:                            
                            //Services.GenerationService.Generators.ApplyTime(x.TargetGeneratorId, x.RewardValue);
                            Services.GenerationService.Generators.AddTimeBoost(x.TargetGeneratorId, BoostInfo.CreateTemp(BosUtils.PrefixedId("speed_achiv_"), x.RewardValue));
                            break;
                        case RewardType.ProfitUpgrade:
                            Services.GenerationService.Generators.AddProfitBoost(x.TargetGeneratorId, BoostInfo.CreateTemp(BosUtils.PrefixedId("profit_achiv_"), x.RewardValue));
                            //Services.GenerationService.Generators.ApplyProfit(x.TargetGeneratorId, ProfitMultInfo.Create(x.RewardValue));
                            break;
                        default:
                            break;
                    }

                    //Proc.Invoke(x);
                    GameEvents.OnAchievmentCompleted(new AchievmentInfo(x.Id, x.Name));
                    StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
                    ChangeGeneratorIcon(x);
                    Player.LegacyPlayerData.Save();
                }
            }
        }
    }

    public Achievement GetAchievement(int id)
    {
        foreach (var x in Achievements)
        {
            if (x.Id == id)
                return x;
        }

        return null;
    }
}


[Serializable]
public class AchievementEvent : UnityEvent<Achievement>
{

}