using Bos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class LootboxRewardManager : GameBehaviour
{
    public LootboxItem[] PotentialRewards;

    public AllManagers AllManagers;

    public Lootbox CreateLootbox(int size = 3)
    {
        Lootbox lb = new Lootbox(size);

        for (int i = 0; i < size; i++)
        {
            var r = RarityHelper.GetWeightedRarity();
            LootboxItem item = null;
            int retardCheck = 0;
            do
            {
                repeat:
                item = PotentialRewards.RandomElement(x => x.Rarity == r);
                if (item.RewardType == LootboxRewardType.Generators && !Services.TransportService.HasUnits(item.TargetGenerator))
                    goto repeat;
                r--; // if not item of that rarity was found, go one lower
                retardCheck++;

                if (retardCheck > 10)
                    break;
            } while (item == null && r > 0);

            lb[i] = item;
        }

        return lb;
    }

    public void GiveRewards(Lootbox lb)
    {
        if (!lb.IsValid)
            return;
        var pdata = Player.LegacyPlayerData;
        foreach (var item in lb.Items)
        {
            switch (item.RewardType)
            {
                case LootboxRewardType.Generators:
                    if (Services.TransportService.HasUnits(item.TargetGenerator))
                        Services.TransportService.AddLiveUnits(item.TargetGenerator, (int)item.RewardValue);
                    else {
                        var generator = Services.GenerationService.Generators.GetGeneratorInfo(item.TargetGenerator);
                        Services.GenerationService.BuyGenerator(generator, true);
                        //AllManagers.BalanceManager.BuyGenerator(generator, free: true);
                    }
                    break;
                case LootboxRewardType.ProfitUpgrade:

                    if (item.TargetGenerator == -1)
                        Services.GenerationService.Generators.AddProfitBoost(
                            boost: BoostInfo.CreateTimed(
                                id: $"lootbox_{item.RewardType}_".GuidSuffix(5),
                                value: item.RewardValue,
                                endTime: Services.TimeService.UnixTimeInt + 600));
                    else {
                        if (Services.TransportService.HasUnits(item.TargetGenerator)) {
                            Services.GenerationService.Generators.AddProfitBoost(
                                generatorId: item.TargetGenerator,
                                boost: BoostInfo.CreateTimed(
                                    id: $"lootbox_{item.RewardType}_".GuidSuffix(5),
                                    value: item.RewardValue,
                                    endTime: Services.TimeService.UnixTimeInt + 600));
                        }
                    }
                    break;
                case LootboxRewardType.SpeedUpgrade:

                    if (item.TargetGenerator == -1)
                        Services.GenerationService.Generators.AddTimeBoost(
                            boost: BoostInfo.CreateTimed(
                                id: $"lootbox_{item.RewardType}_".GuidSuffix(5),
                                value: item.RewardValue,
                                endTime: Services.TimeService.UnixTimeInt + 600));
                    else
                    {
                        if (Services.TransportService.HasUnits(item.TargetGenerator))
                        {
                            Services.GenerationService.Generators.AddTimeBoost(
                                generatorId: item.TargetGenerator,
                                boost: BoostInfo.CreateTimed(
                                    id: $"lootbox_{item.RewardType}_".GuidSuffix(5),
                                    value: item.RewardValue,
                                    endTime: Services.TimeService.UnixTimeInt + 600));
                        }
                    }
                    break;
                case LootboxRewardType.Manager:
                    // TODO : ??
                    Debug.Log("TODO: implement Lootbox manager reward");
                    break;
                case LootboxRewardType.Investors:
                    //pdata.Investors += item.RewardValue;
                    //pdata.LifeTimeInvestors += item.RewardValue;
                    Services.PlayerService.AddSecurities(item.RewardValue.ToCurrencyNumber());
                    break;
                case LootboxRewardType.Balance:
                    Player.AddGenerationCompanyCash(item.RewardValue);
                    break;
                case LootboxRewardType.Coins:
                    Player.AddCoins((int)item.RewardValue);
                    break;
                case LootboxRewardType.CoinsAndBalance:
                    Player.AddGenerationCompanyCash(item.RewardValue);
                    Player.AddCoins((int)item.RewardValue2);
                    break;
                case LootboxRewardType.PercentBalance:
                    var q = Services.PlayerService.CompanyCash.Value * item.RewardValue;
                    if (q < 20)
                        q = 20;
                    Player.AddGenerationCompanyCash(q);
                    item.DescText = Currencies.DefaultCurrency.CreatePriceString(q, false, " ");
                    break;
                default:
                    break;
            }

            StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
        }

        pdata.Save();

        Analytics.CustomEvent(AnalyticsStrings.LOOTBOX_OPEN);

    }
}
