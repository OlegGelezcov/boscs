using System;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class FlashSaleItem
{
    public StoreListing Listing;
    public bool ShouldHide;
    public DateTime ExpirationDate;

    [JsonIgnore]
    public bool ActiveOnScreen;

    [JsonIgnore]
    public Sprite Sprite
    {
        get
        {
            if (Listing == null)
                return null;

            switch (Listing.BonusType)
            {
                case RewardType.SpeedUpgrade:
                    return SpriteDB.SpriteRefs["speed"];
                case RewardType.ProfitUpgrade:
                    return SpriteDB.SpriteRefs["profit"];
                case RewardType.Lootbox:
                    return SpriteDB.SpriteRefs["gift4"];
                default:
                    return null;
            }
        }
    }
}

public class StoreListing {
    public double Price;
    public int Coins;
    public bool HasBonus;
    public RewardType BonusType;
    public int BonusValue;
    public string StoreIdentifier;
    public bool IsPromotion;
    public float AmountSaved;
    public bool useSecurities;
    public bool usePlayerCash;
    public bool isConsumable;
}
