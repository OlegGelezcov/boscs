/*
using Bos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class BadgeDb
{
    public static List<Badge> Badges;

    public static void Load()
    {
        Badges = new List<Badge>();
        Badges.Add(new MinimumGeneratorBadge() 
        {   Id = 0,
            ShouldProcAfterReset = false,
            Name = "Business Afficionado",
            Description = "Unlock all businesses",
            MaxGeneratorCount = 10,
            RewardType = RewardType.SpeedUpgrade,
            RewardValue = 4 }
        );
        Badges.Add(new AtLeastNGeneratorsBadge() { Id = 1, ShouldProcAfterReset = false, Name = "Entrepreneur", Description = "Have at least 5 of each business", N = 5, RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new AtLeastNGeneratorsBadge() { Id = 2, ShouldProcAfterReset = false, Name = "Mr. Worldwide", Description = "Have at least 50 of each business", N = 50, RewardType = RewardType.SpeedUpgrade, RewardValue = 3 });
        Badges.Add(new AllManagersBadge() { Id = 3, ShouldProcAfterReset = false, Name = "Managers Inc.", Description = "Hire all managers", MaxManagersCount = 10, RewardType = RewardType.SpeedUpgrade, RewardValue = 4 });
        Badges.Add(new MinStatBadge() { Id = 4, ShouldProcAfterReset = false, Name = "Treasurer", Description = "Aquire 200 coins.", Min = 200, DescString = "Coins earned", StatString = Stats.COINS_AQUIRED, RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new MinStatBadge() { Id = 5, ShouldProcAfterReset = false, Name = "Ad-dict", Description = "Watch 20 ads.", Min = 20, DescString = "Ads watched", StatString = Stats.ADS_WATCHED, RewardType = RewardType.CoinReward, RewardValue = 5 });
        Badges.Add(new MinStatBadge() { Id = 6, ShouldProcAfterReset = false, Name = "Chicken Dinner", Description = "Win 50 times at minigames.", Min = 50, DescString = "Games won", StatString = "slotsWon", RewardType = RewardType.None, RewardValue = 2 });
        Badges.Add(new MinStatBadge() { Id = 7, ShouldProcAfterReset = false, Name = "The Capitalist", Description = "Spend 20$ in the store", Min = 20, DescString = "Dolars spent:", StatString = Stats.MONEY_SPENT, RewardType = RewardType.None, RewardValue = 4 });
        Badges.Add(new AllUpgradesBadge() { Id = 8, ShouldProcAfterReset = false, Name = "Upgrade Complete", Description = "Own at least one upgrade per business", RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new ShopItemBadge() { Id = 9, ShopItemId = 2, ShouldProcAfterReset = false, Name = "Speed x2", Description = "Buy a x2 speed increase from the shop", RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new ShopItemBadge() { Id = 10, ShopItemId = 3, ShouldProcAfterReset = false, Name = "Speed x4", Description = "Buy a x4 speed increase from the shop", RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new ShopItemBadge() { Id = 11, ShopItemId = 4, ShouldProcAfterReset = false, Name = "Speed x6", Description = "Buy a x6 speed increase from the shop", RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new ShopItemBadge() { Id = 12, ShopItemId = 5, ShouldProcAfterReset = false, Name = "Speed x8", Description = "Buy a x8 speed increase from the shop", RewardType = RewardType.SpeedUpgrade, RewardValue = 2 });
        Badges.Add(new AtLeastNGeneratorsBadge() { Id = 13, ShouldProcAfterReset = false, Name = "Tycoon Level 1", Description = "Have at least 100 of each business", N = 100, RewardType = RewardType.SpeedUpgrade, RewardValue = 4 });

        for (int i = 2; i < 50; i++)
        {
            var n = 100 * i;

            Badges.Add(new AtLeastNGeneratorsBadge()
            {
                Id = -1,
                ShouldProcAfterReset = false,
                Name = "Tycoon Level " + i,
                Description = $"Have at least {n} of each business",
                N = n,
                RewardType = RewardType.SpeedUpgrade,
                RewardValue = 2
            });
        }

        foreach (var badge in Badges)
        {
            badge.IsEarned = GameServices.Instance.BadgeService.HasBadge(badge.Id); //GlobalRefs.PlayerData.EarnedBadges.Contains(badge.Id);
        }
    }



    public static void Save()
    {
        GameServices.Instance.BadgeService.SetBadges(Badges.Where(b => b.IsEarned).Select(b => b.Id));
    }
}
*/
