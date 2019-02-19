namespace Bos {
    using Bos.Data;
    using System;
    using System.Collections.Generic;
    using UDBG = UnityEngine.Debug;

    public class StoreProductReward  {

        private IBosServiceCollection services;

        private readonly Dictionary<RewardType, Action<StoreProductData>> bonusMap;

        public StoreProductReward(IBosServiceCollection services) {
            this.services = services;
            this.bonusMap = new Dictionary<RewardType, Action<StoreProductData>> {
                [RewardType.SpeedUpgrade] = ApplySpeedUpgrade,
                [RewardType.ProfitUpgrade] = ApplyProfitUpgrade,
                [RewardType.MiniGameTry] = ApplyPrizeWheelTries,
                [RewardType.MiniGameRaceTry] = ApplyRocketTries,
                [RewardType.MiniGameTreasureHuntTry] = ApplyTreasureHuntTries,
                [RewardType.Lootbox] = ApplyLootbox,
                [RewardType.CashReward] = ApplyCompanyCash,
                [RewardType.SecuritiesReward] = ApplySecurities,
                [RewardType.PlayerCashReward] = ApplyPlayerCash,
                [RewardType.SpecialOfferBundle] = ApplySpecialOffer
            };
        }


        public void Execute(StoreProductData product  ) {
            services.PlayerService.AddCoins(product.Coins);
            StatsCollector.Instance[Stats.MONEY_SPENT] += product.Price;
            if(product.HasBonus ) {
                if(bonusMap.ContainsKey(product.BonusType)) {
                    bonusMap[product.BonusType](product);
                } else {
                    UDBG.Log($"not founded bonus purchase handler {product.BonusType} for product {product.StoreId}".Attrib(bold: true, italic: true, color: "r"));
                }
            }
            GameEvents.StoreProductPurchasedObservable.OnNext(product);
            GameEvents.OnStoreProductPurchased(product);
        }

        #region Reward handlers

        private void ApplySpeedUpgrade(StoreProductData product) {
            services.GenerationService.Generators.AddTimeBoost(
                boost: BoostInfo.CreateTemp(
                    id: $"iap_{product.StoreId}_".GuidSuffix(5),
                    value: product.BonusValue));
        }

        private void ApplyProfitUpgrade(StoreProductData product) {
            services.GenerationService.Generators.AddProfitBoost(
                boost: BoostInfo.CreateTemp(
                    id: $"iap_{product.StoreId}_".GuidSuffix(5),
                    value: product.BonusValue));
        }

        private void ApplyPrizeWheelTries(StoreProductData product) {
            if(GlobalRefs.MiniGamesScreen != null ) {
                GlobalRefs.MiniGamesScreen.IncreaseTriesPaid(MiniGameType.PrizeWheel);
            }
        }

        private void ApplyRocketTries(StoreProductData product) {
            if(GlobalRefs.MiniGamesScreen != null ) {
                GlobalRefs.MiniGamesScreen.IncreaseTriesPaid(MiniGameType.BreakLiner);
            }
        }
        
        private void ApplyTreasureHuntTries(StoreProductData product) {
            if(GlobalRefs.MiniGamesScreen != null ) {
                GlobalRefs.MiniGamesScreen.IncreaseTriesPaid(MiniGameType.TreasureHunt);
            }
        }
        
        

        private void ApplyLootbox(StoreProductData product ) {
            services.RewardsService.AddAvailableRewards(product.BonusValue);
        }

        private void ApplyCompanyCash(StoreProductData product) {
            var Player = services.PlayerService;
            Player.AddGenerationCompanyCash(Player.MaxCompanyCash * product.BonusValue);
        }

        private void ApplySecurities(StoreProductData product ) {
            services.PlayerService.AddSecurities(new CurrencyNumber(services.PlayerService.MaxSecurities * product.BonusValue));
        }

        private void ApplyPlayerCash(StoreProductData product) {
            services.PlayerService.AddPlayerCash(new CurrencyNumber(services.PlayerService.MaxPlayerCash * product.BonusValue));
        }

        private void ApplySpecialOffer(StoreProductData product ) {
            services.GetService<ISpecialOfferService>().OnOfferPurchased();
        }
        #endregion
    }

}