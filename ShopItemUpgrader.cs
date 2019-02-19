namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Analytics;

    public class ShopItemUpgrader : GameElement, IShopUpgrader {

        private readonly Dictionary<UpgradeType, Action<BosCoinUpgradeData>> upgrades;

        public void Setup(object data = null) {
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(ShopItemUpgrader)}.{nameof(UpdateResume)}() => {pause}");

        public ShopItemUpgrader() {
            upgrades = new Dictionary<UpgradeType, Action<BosCoinUpgradeData>> {
                [UpgradeType.Profit] = UpgradeProfit,
                [UpgradeType.Time] = UpgradeTime,
                [UpgradeType.FutureBalance] = UpgradeFutureBalance,
                [UpgradeType.AdvertisementBoost] = UpgradeAdvertisimentBoost,
                [UpgradeType.Reward] = UpgradeReward,
                [UpgradeType.Micromanager] = UpgradeMicromanager,
                [UpgradeType.Enhance] = UpgradeEnhance,
            };
        }

        public void ApplyUpgrade(BosCoinUpgradeData data) {

            if(upgrades.ContainsKey(data.UpgradeType)) {
                upgrades[data.UpgradeType](data);
            } else {
                Services.GetService<IConsoleService>().AddOutput($"Not founded upgrade for type => {data.UpgradeType}", ConsoleTextColor.red, true);
            }
        }


        private GeneratorInfoCollection Generators => Services.GenerationService.Generators;

        private void UpgradeProfit(BosCoinUpgradeData data ) {
            if(data.GeneratorId < 0 ) {
                if(data.IsPermanent) {
                    //Generators.ApplyPermanent(GeneratorBonusMult.CreateProfitMult(data.ProfitMutlitplier));
                    Generators.AddProfitBoost(
                        boost: BoostInfo.CreatePersist(BoostIds.GetPersistCoinUpgradeId(data.Id), data.ProfitMutlitplier, (int)BoostSourceType.CoinUpgrade));
                } else {
                    //Generators.ApplyGlobal(GeneratorBonusMult.CreateProfitMult(data.ProfitMutlitplier));
                    Generators.AddProfitBoost(
                        boost: BoostInfo.CreateTemp(BoostIds.GetTempCoinUpgradeId(data.Id), data.ProfitMutlitplier, (int)BoostSourceType.CoinUpgrade));
                }
            } else {
                if(data.IsPermanent) {
                    //Generators.ApplyPermanent(data.GeneratorId, GeneratorBonusMult.CreateProfitMult(data.ProfitMutlitplier));
                    Generators.AddProfitBoost(
                        generatorId: data.GeneratorId,
                        boost: BoostInfo.CreatePersist(BoostIds.GetPersistLocalCoinUpId(data.Id), data.ProfitMutlitplier, (int)BoostSourceType.CoinUpgrade));
                } else {
                    //Generators.ApplyProfit(data.GeneratorId, ProfitMultInfo.Create(data.ProfitMutlitplier));
                    Generators.AddProfitBoost(
                        generatorId: data.GeneratorId,
                        boost: BoostInfo.CreateTemp(BoostIds.GetTempLocalCoinUpId(data.Id), data.ProfitMutlitplier, (int)BoostSourceType.CoinUpgrade));
                }
            }
            //Services.GetService<IPlayerDataUpgrader>().Upgrade(new UpgradeItem(data.GeneratorId, data.ProfitMutlitplier, data.IsPermanent, data.UpgradeType));
        }

        private void UpgradeTime(BosCoinUpgradeData data) {
            if(data.GeneratorId < 0 ) {
                if(data.IsPermanent) {
                    //Generators.ApplyPermanent(GeneratorBonusMult.CreateTimeMult(data.TimeMultiplier));
                    Generators.AddTimeBoost(
                        boost: BoostInfo.CreatePersist(
                            id: BoostIds.GetPersistCoinUpId(data.Id), 
                            value: data.TimeMultiplier,
                            sourceType: (int)BoostSourceType.CoinUpgrade));
                } else {
                    //Generators.ApplyGlobal(GeneratorBonusMult.CreateTimeMult(data.TimeMultiplier));
                    Generators.AddTimeBoost(
                        boost: BoostInfo.CreateTemp(
                            id: BoostIds.GetTempCoinUpId(data.Id),
                            value: data.TimeMultiplier,
                            sourceType: (int)BoostSourceType.CoinUpgrade));
                }
            } else {
                if(data.IsPermanent) {
                    //Generators.ApplyPermanent(data.GeneratorId, GeneratorBonusMult.CreateTimeMult(data.TimeMultiplier));
                    Generators.AddTimeBoost(
                        generatorId: data.GeneratorId,
                        boost: BoostInfo.CreatePersist(
                            id: BoostIds.GetPersistCoinUpId(data.Id),
                            value: data.TimeMultiplier,
                            sourceType: (int)BoostSourceType.CoinUpgrade));
                } else {
                    //Generators.ApplyTime(data.GeneratorId, data.TimeMultiplier);
                    Generators.AddTimeBoost(
                        generatorId: data.GeneratorId,
                        boost: BoostInfo.CreateTemp(
                            id: BoostIds.GetTempCoinUpId(data.Id),
                            value: data.TimeMultiplier,
                            sourceType: (int)BoostSourceType.CoinUpgrade));
                }
            }
            //Services.GetService<IPlayerDataUpgrader>().Upgrade(new UpgradeItem(data.GeneratorId, data.TimeMultiplier, data.IsPermanent, data.UpgradeType));
        }

        private void UpgradeFutureBalance(BosCoinUpgradeData data) {
            TimeSpan timeSpan = TimeSpan.FromDays(data.DaysOfFutureBalance);
            //double value = Services.GenerationService.CalculateTotalProfitOnInterval((float)timeSpan.TotalSeconds);

            double value = Services.GenerationService.GetTotalProfitOnTime(timeSpan.TotalSeconds);
            Services.PlayerService.AddGenerationCompanyCash(value);
            Services.GetService<IConsoleService>().AddOutput($"add future balance => {value}", ConsoleTextColor.green, true);
        }

        private void UpgradeAdvertisimentBoost(BosCoinUpgradeData data) {
            LocalData.AdvertisementMultiplier = data.ProfitMutlitplier;
        }

        private void UpgradeReward(BosCoinUpgradeData data) {
            Services.RewardsService.AddAvailableRewards(data.ProfitMutlitplier);
        }

        private void UpgradeMicromanager(BosCoinUpgradeData data) {
            Services.PlayerService.SetHasMicromanager(true);
            Analytics.CustomEvent("BOOSTER_BOUGHT");
        }
        
        
        private void UpgradeEnhance(BosCoinUpgradeData data) {
            Services.ManagerService.Enhance(Services.GenerationService.GetGetenerator(data.GeneratorId));
        }
    }


    public interface IShopUpgrader : IGameService {
        void ApplyUpgrade(BosCoinUpgradeData data);
    }
}