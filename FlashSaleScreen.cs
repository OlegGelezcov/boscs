using Bos;
using Ozh.Tools.Functional;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class FlashSaleScreen : GameBehaviour
{
    private FlashSaleItem _current;

    public Text ExpirationText;
    public Text CoinText;
    public Text PriceText;
    public Text SaveText;
    public Text RewardText;
    public Text DescriptionText;
    public Image RewardImage;

    public GameObject Button;

    public override void Update() {
        base.Update();
        var dt = _current.ExpirationDate - DateTime.Now;
        if (dt.TotalHours < 0) {
            _current = Player.LegacyPlayerData.CurrentFlashSale;
            dt = _current.ExpirationDate - DateTime.Now;
            if (dt.TotalHours < 0) {
                gameObject.SetActive(false);
                Debug.LogError("Current offer expiration");
            }
        }

        ExpirationText.text = dt.ToString(@"hh\:mm\:ss");

        if (_current.ShouldHide) {
            gameObject.SetActive(false);
        }
    }

    public void Show()
    {
        var fs = Player.LegacyPlayerData.CurrentFlashSale;
        if (fs == null)
            return;

        _current = fs;

        CoinText.text = string.Format("{0} " + "COINS".GetLocalizedString(), fs.Listing.Coins);


        string storeId = _current.Listing.StoreIdentifier;
        #if UNITY_IOS && !BOS
            if(!storeId.StartsWith("bos2_")) {
                storeId = "bos2_" + storeId;
            }
        #endif

        Debug.Log($"flash store id: {storeId}");


        Services.Inap.GetProductByStoreId(storeId).Match(() => {
            PriceText.text = string.Empty;
            return F.None;
        }, product => {
            PriceText.text = product.metadata.localizedPriceString;
            return F.Some(product);
        });

            
        
        SaveText.text = string.Format("SAVE20".GetLocalizedString(), fs.Listing.AmountSaved);
        RewardText.text = GetRewardText(fs.Listing.BonusType, fs.Listing.BonusValue);
        DescriptionText.text = string.Format("BONUS.DESC".GetLocalizedString(),  fs.Listing.AmountSaved, fs.Listing.Coins,
            RewardText.text, PriceText.text);
        RewardImage.sprite = fs.Sprite;

        gameObject.SetActive(true);
        _current.ActiveOnScreen = true;


        Analytics.CustomEvent($"FLASH_SALE_{fs.Listing.StoreIdentifier}_OPEN");
    }

    public void Close()
    {
        Analytics.CustomEvent($"FLASH_SALE_{_current.Listing.StoreIdentifier}_CLOSE");

        gameObject.SetActive(false);
        _current.ActiveOnScreen = false;
    }

    private string GetRewardText(RewardType rt, int value)
    {
        switch (rt)
        {
            case RewardType.SpeedUpgrade:
                return string.Format("SPEED.UPGRADE".GetLocalizedString(), value);
            case RewardType.ProfitUpgrade:
                return string.Format("PROFIT.UPGRADE".GetLocalizedString(), value);
            case RewardType.Lootbox:
                return string.Format("REWARD.UPGRADE".GetLocalizedString(), value);
            default:
                return null;
        }
    }

    public void Buy()
    {
        Services.Inap.PurchaseProduct(ResourceService.Products.GetProductByStoreId(_current.Listing.StoreIdentifier));
        //GlobalRefs.IAP.BuyProduct(_current.Listing.StoreIdentifier);
        Analytics.CustomEvent($"FLASH_SALE_{_current.Listing.StoreIdentifier}_BUY");
        _current.ShouldHide = true;
    }

    public void NotInterested()
    {
        Analytics.CustomEvent($"FLASH_SALE_{_current.Listing.StoreIdentifier}_NOTINTERESTED");
        _current.ShouldHide = true;
    }
}