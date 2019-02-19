using System;
using Bos.Data;

namespace Bos {
    
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class StoreService : SaveableGameBehaviour, IStoreService {

        //public bool isDebug;

        public Dictionary<int, int> PurchasedCoinUpgrades { get; } = new Dictionary<int, int>();
       //public bool IsDebug => isDebug;
        
        public void Setup(object data = null) {
//#if !BOSDEBUG
//            isDebug = false;
//#endif
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        public bool IsCoinUpgradePurchased(BosCoinUpgradeData data) {
            return PurchasedCoinUpgrades.ContainsKey(data.Id) && PurchasedCoinUpgrades[data.Id] > 0;
        }

        private void AddCoinPurchase(BosCoinUpgradeData data) {
            if(PurchasedCoinUpgrades.ContainsKey(data.Id)) {
                PurchasedCoinUpgrades[data.Id]++;
            } else {
                PurchasedCoinUpgrades.Add(data.Id, 1);
            }
            GameEvents.OnShopItemPurchased(new ShopItemInfo(data.Id, data.GeneratorId, Price.CreateCoins(data.Price)));
        }

        public TransactionState Purchase(BosCoinUpgradeData data) {

            if(data.IsOneTime && IsCoinUpgradePurchased(data)) {
                return TransactionState.AlreadyPurchased;
            }

            if(Services.PlayerService.IsEnoughCoins(data.Price)) {
                Services.GetService<IShopUpgrader>().ApplyUpgrade(data);
                Services.PlayerService.RemoveCoins(data.Price);
                FacebookEventUtils.LogCoinSpendEvent(data.Name, data.Price, Services.PlayerService.Coins);
                AddCoinPurchase(data);
                return TransactionState.Success;
            } else {
                return TransactionState.DontEnoughCurrency;
            }
        }

        public List<BosCoinUpgradeData> GetNotPurchasedCoinUpgradeDataList() {
            List<BosCoinUpgradeData> result = new List<BosCoinUpgradeData>();
            foreach (var data in Services.ResourceService.CoinUpgrades.UpgradeList) {
                if (!data.IsOneTime) {
                    result.Add(data);
                }
                else {
                    if (!IsCoinUpgradePurchased(data)) {
                        result.Add(data);
                    }
                }
            }

            return result;
        }

        public bool HasAvailableCoinUpgrades {
            get {
                return GetNotPurchasedCoinUpgradeDataList().Any(u => {
                    return Player.IsEnoughCoins(u.Price);
                });
            }
        }
        
        #region SaveableGameBehaviour overrides
        public override object GetSave() {
            return new StoreServiceSave {
                purchasedCoinUpgrades = PurchasedCoinUpgrades
            };
        }

        public override void LoadDefaults() {
            PurchasedCoinUpgrades.Clear();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            StoreServiceSave save = obj as StoreServiceSave;
            if (save != null) {
                PurchasedCoinUpgrades.Clear();
                if (save.purchasedCoinUpgrades != null) {
                    foreach (var kvp in save.purchasedCoinUpgrades) {
                        PurchasedCoinUpgrades.Add(kvp.Key, kvp.Value);
                    }
                }

                IsLoaded = true;
            }
            else {
                LoadDefaults();
            }
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override string SaveKey => "store_service";

        public override Type SaveType => typeof(StoreServiceSave);
        #endregion

    }

    public interface IStoreService : IGameService {
        TransactionState Purchase(BosCoinUpgradeData data);
        bool IsCoinUpgradePurchased(BosCoinUpgradeData data);
        List<BosCoinUpgradeData> GetNotPurchasedCoinUpgradeDataList();
        bool HasAvailableCoinUpgrades { get; }
        //bool IsDebug { get; }
    }

    public class StoreServiceSave {
        public Dictionary<int, int> purchasedCoinUpgrades;
        
    }
}