using Bos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Badge : GameElement
{
    public int UniqueId { get; private set; }



    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    //public bool IsEarned { get; set; }
    public string SpriteId { get; set; }

    public abstract string ObjectiveText { get; }

    public abstract bool Check();

    public RewardType RewardType { get; set; }
    public int RewardValue { get; set; }
    public bool ShouldProcAfterReset { get; set; }

    public void SetUniqueId(int uniqueId)
        => UniqueId = uniqueId;

    public string RewardText
    {
        get
        {
            //var attrLocale = $"B.DESC{Id}".GetLocalizedString();
            /*switch (RewardType)
            {
                case RewardType.SpeedUpgrade:
                    
                    return string.Format("X.SPEED".GetLocale(LocalizationDataType.ui), attrLocale);
                case RewardType.ProfitUpgrade:
                    return string.Format("X.PROFIT".GetLocale(LocalizationDataType.ui), attrLocale);
                default:
                    return string.Empty;
            }*/
            
            switch (RewardType)
            {
                case RewardType.SpeedUpgrade:
                    return $"x{RewardValue} Speed";
                case RewardType.ProfitUpgrade:
                    return $"x{RewardValue} Profit"; 
                case RewardType.CoinReward:
                    return $"+{RewardValue} COINS";
                default:
                    return string.Empty;
            }
        }
    }
}

public class ShopItemBadge : Badge
{
    public int ShopItemId;
    public bool ShouldActivateAfterReset { get; set; }

    public override string ObjectiveText { get { return string.Empty; } }

    public override bool Check()
    {
        if (ShouldActivateAfterReset && Services.GameModeService.ResetCount == 0)
            return false;

        var t = PlayerPrefs.GetInt("purchased_shop_item_" + ShopItemId, 0) == 1;
        return t;
    }
}

public class AllUpgradesBadge : Badge
{
    public override string ObjectiveText
    {
        get
        {
            return "Buy at least 10 cash upgrades.";
        }
    }

    public override bool Check()
    {
        return Services.TransportService.TotalCountOfGenerators >= 10 && Services.UpgradeService.CashAndSecuritiesBoughtCount >= 10;
    }
}
public class MinStatBadge : Badge
{
    public string StatString { get; set; }
    public string DescString { get; set; }
    public int Min { get; set; }

    public override string ObjectiveText
    {
        get
        {
            var optValue = BosUtils.GetBadgeObjectiveValue(StatString);
            return optValue.Match(() => {
                var q = StatsCollector.Instance[StatString];
                if (q > Min) {
                    return string.Format("{2}: {0}/{1}", Min, Min, DescString);
                }

                return string.Format("{2}: {0}/{1}", q, Min, DescString);
            }, (cnt) => {
                if (cnt > Min) {
                    return string.Format("{2}: {0}/{1}", Min, Min, DescString);
                }

                return string.Format("{2}: {0}/{1}", cnt, Min, DescString);
            });

        }
    }

    public override bool Check()
    {
        var optValue = BosUtils.GetBadgeObjectiveValue(StatString);
        return optValue.Match(() => {
            return StatsCollector.Instance[StatString] >= Min;
        }, (cnt) => {
            return cnt >= Min;
        });
    }
}
public class AllManagersBadge : Badge
{
    public int MaxManagersCount { get; set; }

    public override string ObjectiveText
    {
        get
        {
            var manCount = Services.ManagerService.HiredCount;
            return string.Format("Hired managers: {0}/{1}", manCount, MaxManagersCount);
            return string.Empty;
        }
    }

    public override bool Check()
    {
        return Services.ManagerService.HiredCount >= MaxManagersCount;
    }
}
public class AtLeastNGeneratorsBadge : Badge
{
    public int N;

    public override string ObjectiveText
    {
        get
        {
            return string.Format("Have at least {0} of each generator.", N);
        }
    }

    public override bool Check()
    {
        if (Services.TransportService.TotalCountOfGenerators < 10)
            return false;

        var p = Services.TransportService.IsCountOfUnitsForGeneratorsGreaterOrEqualThan(N); //GlobalRefs.PlayerData.OwnedGenerators.All(x => x.Value >= N);
        return p;
    }
}
public class MinimumGeneratorBadge : Badge
{
    public int MaxGeneratorCount { get; set; }

    public override string ObjectiveText
    {
        get
        {
            if (Check())
                return string.Format("Businesses: {0}/{1}", MaxGeneratorCount, MaxGeneratorCount);

            return string.Format("Businesses: {0}/{1}", Services.TransportService.TotalCountOfGenerators, MaxGeneratorCount);
        }
    }

    public override bool Check()
    {
        return Services.TransportService.TotalCountOfGenerators >= MaxGeneratorCount;
    }
}