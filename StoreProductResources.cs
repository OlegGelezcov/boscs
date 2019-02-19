using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;

namespace Bos.Data {

    public interface IStoreProductRepository : IRepository {
        List<StoreProductData> Products { get; }
        List<StoreProductData> Promotions { get; }
        StoreProductData GetProduct(int id);
        StoreProductData GetProductByStoreId(string storeId);
        bool Contains(int id);
    }
    public class StoreProductRepository : IStoreProductRepository {

        public List<StoreProductData> Products { get; } = new List<StoreProductData>();

        public List<StoreProductData> Promotions
            => Products.Where(p => p.IsPromotion).ToList();


        public bool IsLoaded { get; private set; }

        public StoreProductData GetProduct(int id) {
            return Products.FirstOrDefault(p => p.Id == id);

        }

        public StoreProductData GetProductByStoreId(string storeId)
            => Products.FirstOrDefault(p => p.StoreId == storeId);

        public bool Contains(int id)
            => Products.FirstOrDefault(p => p.Id == id) != null;

        public void Load(string file) {
            if(!IsLoaded) {
                var listItems = JsonConvert.DeserializeObject<List<StoreProductJsonData>>(Resources.Load<TextAsset>(file).text);
                Products.Clear();
                listItems.ForEach(li => Products.Add(new StoreProductData(li)));
                IsLoaded = true;
            }
        }
    }


    public class StoreProductData {

        private string _storeSpecificId;

        public int Id { get; private set; }

        public string StoreId { 
            get {
#if UNITY_IOS
#if BOS
                return BosStoreId;
#else
                return "bos2_" + _storeSpecificId;
#endif
#else
              return _storeSpecificId;  
#endif
            }
        }


        public float Price { get; private set; }
        public int Coins { get; private set; }
        public bool HasBonus { get; private set; }
        public RewardType BonusType { get; private set; }
        public int BonusValue { get; private set; }
        public bool IsPromotion { get; private set; }
        public float AmountSaved { get; private set; }
        public bool UseCash { get; private set; }
        public bool IsBest { get; private set; }
        public bool UseSecurities { get;  }
        public bool UsePlayerCash { get; }
        public bool IsConsumable { get; }
        public string BosStoreId { get; }

        public StoreProductData(StoreProductJsonData jsonData  ) {
            this.Id = jsonData.id;
            this._storeSpecificId = jsonData.storeId;
            this.Price = jsonData.price;
            this.Coins = jsonData.coins;
            this.HasBonus = jsonData.hasBonus;
            this.BonusType = jsonData.bonusType;
            this.BonusValue = jsonData.bonusValue;
            this.IsPromotion = jsonData.isPromotion;
            this.AmountSaved = jsonData.amountSaved;
            this.UseCash = jsonData.useCash;
            this.IsBest = jsonData.isBest;
            this.UseSecurities = jsonData.useSecurities;
            this.UsePlayerCash = jsonData.usePlayerCash;
            this.IsConsumable = jsonData.isConsumable;
            this.BosStoreId = jsonData.bosStoreId;
        }

        public StoreListing ToListing()
            => new StoreListing {
                AmountSaved = AmountSaved,
                BonusType = BonusType,
                BonusValue = BonusValue,
                Coins = Coins,
                HasBonus = HasBonus,
                IsPromotion = IsPromotion,
                Price = Price,
                StoreIdentifier = StoreId,
                useSecurities = UseSecurities,
                usePlayerCash = UsePlayerCash,
                isConsumable = IsConsumable
            };

        public CurrencyType CurrencyType {
            get {
                if (UseCash) {
                    return CurrencyType.CompanyCash;
                } else if (UseSecurities) {
                    return CurrencyType.Securities;
                } else if (UsePlayerCash) {
                    return CurrencyType.PlayerCash;
                }
                return CurrencyType.Coins;
                //throw new InvalidOperationException();
            }
        }
    }

    [Serializable]
    public class StoreProductJsonData {
        public int id;
        public string storeId;
        
        public float price;
        public int coins;
        public bool hasBonus;
        
        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public RewardType bonusType;

        public int bonusValue;
        public bool isPromotion;
        public float amountSaved;

        public bool useCash;
        public bool isBest;

        public bool useSecurities;
        public bool usePlayerCash;
        public bool isConsumable;
        public string bosStoreId;

        
        public static StoreProductJsonData Create(StoreListing item, int id)
            => new StoreProductJsonData {
                id = id,
                storeId = item.StoreIdentifier,
                price = (float) item.Price,
                coins = item.Coins,
                hasBonus = item.HasBonus,
                bonusType = item.BonusType,
                bonusValue = item.BonusValue,
                isPromotion = item.IsPromotion,
                amountSaved = item.AmountSaved,
                useSecurities = item.useSecurities,
                usePlayerCash = item.usePlayerCash,
                isConsumable = item.isConsumable,
                bosStoreId = item.StoreIdentifier
            };
    }



    public class StoreProductJsonDataWriter {
        public void Write(List<StoreProductJsonData> products, string path) {
            JsonSerializer jsonSerializer = new JsonSerializer {
                Formatting = Formatting.Indented
            };
            using (StreamWriter streamWriter = new StreamWriter(path)) {
                using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                    jsonSerializer.Serialize(jsonWriter, products);
                }
            }
        }
    }
}