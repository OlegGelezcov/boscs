namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class BadgeService : SaveableGameBehaviour, IBadgeService {

        public List<int> EarnedBadges { get; } = new List<int>();

        private readonly List<Badge> badgeDatas = new List<Badge>();

        private readonly UpdateTimer timer = new UpdateTimer();



        public void Setup(object data) {
            badgeDatas.Clear();

            badgeDatas.Add(new MinimumGeneratorBadge() {
                Id = 0,
                ShouldProcAfterReset = false,
                Name = "Business Afficionado",
                Description = "Unlock all businesses",
                MaxGeneratorCount = 10,
                RewardType = RewardType.SpeedUpgrade,
                RewardValue = 4,
                SpriteId = "badge_0"
            }
);
            badgeDatas.Add(new AtLeastNGeneratorsBadge() {
                Id = 1,
                ShouldProcAfterReset = false,
                Name = "Entrepreneur",
                Description = "Have at least 5 of each business",
                N = 5,
                RewardType = RewardType.SpeedUpgrade,
                RewardValue = 2,
                SpriteId = "badge_1"});

            badgeDatas.Add(new AtLeastNGeneratorsBadge() {
                Id = 2,
                ShouldProcAfterReset = false,
                Name = "Mr. Worldwide",
                Description = "Have at least 50 of each business",
                N = 50,
                RewardType = RewardType.SpeedUpgrade,
                RewardValue = 3,
                SpriteId = "badge_2" });
            badgeDatas.Add(new AllManagersBadge() {
                Id = 3,
                ShouldProcAfterReset = false,
                Name = "Managers Inc.",
                Description = "Hire all managers",
                MaxManagersCount = 10,
                RewardType = RewardType.SpeedUpgrade,
                RewardValue = 4,
                SpriteId = "badge_3" });
            badgeDatas.Add(new MinStatBadge() { Id = 4, ShouldProcAfterReset = false, Name = "Treasurer",
                Description = "Aquire 200 coins.", Min = 200, DescString = "Coins earned", StatString = Stats.COINS_AQUIRED,
                RewardType = RewardType.SpeedUpgrade, RewardValue = 2, SpriteId = "badge_4" });
            badgeDatas.Add(new MinStatBadge() { Id = 5, ShouldProcAfterReset = false, Name = "Ad-dict",
                Description = "Watch 20 ads.", Min = 20, DescString = "Ads watched", StatString = "ads_watched",
                RewardType = RewardType.CoinReward, RewardValue = 5, SpriteId = "badge_5" });
            badgeDatas.Add(new MinStatBadge() { Id = 6, ShouldProcAfterReset = false, Name = "Chicken Dinner",
                Description = "Win 50 times at minigames.", Min = 50, DescString = "Games won", StatString = "slotsWon",
                RewardType = RewardType.None, RewardValue = 2, SpriteId = "badge_6" });
            badgeDatas.Add(new MinStatBadge() { Id = 7, ShouldProcAfterReset = false, Name = "The Capitalist", Description = "Spend 20$ in the store",
                Min = 20, DescString = "Dolars spent:", StatString = Stats.MONEY_SPENT,
                RewardType = RewardType.None, RewardValue = 4, SpriteId = "badge_7" });
            badgeDatas.Add(new AllUpgradesBadge() { Id = 8, ShouldProcAfterReset = false, Name = "Upgrade Complete",
                Description = "Own at least one upgrade per business",
                RewardType = RewardType.SpeedUpgrade, RewardValue = 2, SpriteId = "badge_8" });
            badgeDatas.Add(new ShopItemBadge() { Id = 9, ShopItemId = 2, ShouldProcAfterReset = false, Name = "Speed x2",
                Description = "Buy a x2 speed increase from the shop",
                RewardType = RewardType.SpeedUpgrade, RewardValue = 2, SpriteId = "badge_9" });
            badgeDatas.Add(new ShopItemBadge() { Id = 10, ShopItemId = 3, ShouldProcAfterReset = false, Name = "Speed x4",
                Description = "Buy a x4 speed increase from the shop", RewardType = RewardType.SpeedUpgrade, RewardValue = 2, SpriteId = "badge_10" });
            badgeDatas.Add(new ShopItemBadge() { Id = 11, ShopItemId = 4, ShouldProcAfterReset = false, Name = "Speed x6",
                Description = "Buy a x6 speed increase from the shop",
                RewardType = RewardType.SpeedUpgrade, RewardValue = 2, SpriteId = "badge_11" });
            badgeDatas.Add(new ShopItemBadge() { Id = 12, ShopItemId = 5, ShouldProcAfterReset = false,
                Name = "Speed x8", Description = "Buy a x8 speed increase from the shop",
                RewardType = RewardType.SpeedUpgrade, RewardValue = 2, SpriteId = "badge_12" });
            badgeDatas.Add(new AtLeastNGeneratorsBadge() { Id = 13, ShouldProcAfterReset = false, Name = "Tycoon Level 1",
                Description = "Have at least 100 of each business", N = 100,
                RewardType = RewardType.SpeedUpgrade, RewardValue = 4, SpriteId = "badge_13" });

            for (int i = 2; i < 50; i++) {
                var n = 100 * i;

                badgeDatas.Add(new AtLeastNGeneratorsBadge() {
                    Id = -1,
                    ShouldProcAfterReset = false,
                    Name = "Tycoon Level " + i,
                    Description = $"Have at least {n} of each business",
                    N = n,
                    RewardType = RewardType.SpeedUpgrade,
                    RewardValue = 2,
                    SpriteId = "badge__1"
                });
            }

            SetupUniqueIds(badgeDatas);

            timer.Setup(1, (delta) => {
                if (Services.GameModeService.IsGame && IsLoaded) {
                    foreach(var badgeData in badgeDatas ) {
                        if (!IsBadgeEarned(badgeData)) {
                            if ( badgeData.Check()) {
                                ApplyReward(badgeData);
                                StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
                                EarnBadge(badgeData);
                            }
                        }
                    }
                }
            });
        }

        public void UpdateResume(bool pause) { }

        private void SetupUniqueIds(List<Badge> badges ) {
            int uniqueId = 1000;
            badges.ForEach(badge => badge.SetUniqueId(uniqueId++));
        }

        public override void Update() {
            base.Update();
            timer.Update();
        }


        public bool IsBadgeEarned(Badge badge)
            => EarnedBadges.Contains(badge.UniqueId);

        public void EarnBadge(Badge badge) {
            if (!IsBadgeEarned(badge)) {
                EarnedBadges.Add(badge.UniqueId);
                GameEvents.OnBadgeAdded(badge);
            }
        }

        
        /*
        public void SetBadges(IEnumerable<int> newBadges) {
            Badges.Clear();
            Badges.AddRange(newBadges);
            foreach (int id in newBadges) {
                GameEvents.OnBadgeAdded(id);
            }
        }*/

        public Badge GetUniqueData(int uniqueId)
            => badgeDatas.FirstOrDefault(d => d.UniqueId == uniqueId);

        public Badge GetNonUniqueData(int id)
            => badgeDatas.FirstOrDefault(d => d.Id == id);

        public bool IsAllEarned {
            get {
                return badgeDatas.All(d => IsBadgeEarned(d));
            }
        }

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "badge_service";

        public override Type SaveType => typeof(BadgeServiceSave);


        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        //private List<int> PrepareForSave() {
        //    /*
        //    if(IsLoaded) {
        //        SetBadges(badgeDatas.Where(d => d.IsEarned).Select(d => d.Id));
        //    }*/

        //    List<int> badgesToSave = Badges.Select(b => b).ToList();
        //    return badgesToSave;
        //}

        public override object GetSave() {
            return new BadgeServiceSave {
                badges = EarnedBadges
            };
        }

        public override void LoadDefaults() {
            EarnedBadges.Clear();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            BadgeServiceSave save = obj as BadgeServiceSave;
            if(save != null ) {
                save.CheckNotNull();
                EarnedBadges.Clear();
                EarnedBadges.AddRange(save.badges);

                //foreach(var data in badgeDatas ) {
                //    data.IsEarned = HasBadge(data.Id);
                //}

                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        #endregion

        private readonly Dictionary<RewardType, System.Action<IBosServiceCollection, Badge>> rewards = new Dictionary<RewardType, Action<IBosServiceCollection, Badge>> {
            [RewardType.SpeedUpgrade] = (services, badge) => {
                //services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateTimeMult(badge.RewardValue));
                services.GenerationService.Generators.AddTimeBoost(
                    boost: BoostInfo.CreatePersist(
                        id: $"badge_{badge.Id}_".GuidSuffix(5),
                        value: badge.RewardValue));
            },
            [RewardType.ProfitUpgrade] = (services, badge) => {
                //services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateProfitMult(badge.RewardValue));
                services.GenerationService.Generators.AddProfitBoost(
                    boost: BoostInfo.CreatePersist(
                        id: $"badge_{badge.Id}_".GuidSuffix(5),
                        value: badge.RewardValue));
            },
            [RewardType.CoinReward] = (services, badge) => {
                services.PlayerService.AddCoins(badge.RewardValue);
            }
        };

        private void ApplyReward(Badge badge) {
            if(rewards.ContainsKey(badge.RewardType)) {
                rewards[badge.RewardType].Invoke(Services, badge);
            }
        }
    }

    public interface IBadgeService : IGameService {
        void EarnBadge(Badge badge);
        bool IsBadgeEarned(Badge badge);
        //void SetBadges(IEnumerable<int> newBadges);
        Badge GetUniqueData(int uniqueId);
        Badge GetNonUniqueData(int id);
        bool IsAllEarned { get; }
    }

    [System.Serializable]
    public class BadgeServiceSave {
        public List<int> badges;

        public void CheckNotNull() {
            if(badges == null) {
                badges = new List<int>();
            }
        }
    }
}