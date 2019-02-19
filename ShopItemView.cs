using Bos;
using Bos.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Bos.UI;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

[RequireComponent(typeof(ShopItem))]
public class ShopItemView : GameBehaviour
{
    private ShopItem _item;

    //public IAPManager IAPManager;


    [Header("UI Components")]
    public Text NameView;
    public Text DescriptionView;
    public Text PriceView;
    public Image Icon;
    public Button BuyButton;

    public bool NewLineWord = true;

    //public ParticleSystem fxBuyEffect;


    public override void Start()
    {
        _item = GetComponent<ShopItem>();
        NameView.text = _item.Name;
        DescriptionView.text = _item.Description;
        PriceView.text = _item.Price.ToString();
        Icon.sprite = _item.Icon;

       
        UpdateState();
    }

    public override void OnEnable() {
        GameEvents.ShopItemPurchased += OnShopItemPurchased;
        if (_item != null ) {
            UpdateState();
        }
        BuyButton?.SetListener(() => Buy());
        
    }

    public override void OnDisable() {
        GameEvents.ShopItemPurchased -= OnShopItemPurchased;
    }

    private void OnShopItemPurchased(IShopItem shopItem) {
        if (_item != null && _item.ItemId == shopItem.ItemId) {
            if (BuyButton != null) {
                
                GameObject prefab = GameServices.Instance.ResourceService.Prefabs.GetPrefab("coins");
                GameObject instance = Instantiate<GameObject>(prefab);
                instance.transform.SetParent(BuyButton.transform, false);
                instance.transform.localPosition = new Vector3(0, 0, -1);
                instance.transform.localScale = Vector3.one;
                instance.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                Destroy(instance, 3);
                instance.transform.SetParent(FindObjectOfType<IAPScreen>().transform, true);
                instance.GetComponent<ParticleSystem>().Play();
                FindObjectOfType<SoundManager>()?.PlayOneShot("buyGenerator");
                print("coins created...");
                //if(fxBuyEffect != null ) {
                //    fxBuyEffect.SetActive(true);
                //}
            }
        }
    }

    public void Buy()
    {
        //Services.GetService<IShopItemTransaction>().Buy(BalanceManager, _item);

        var status = Services.StoreService.Purchase(Services.ResourceService.CoinUpgrades.GetData(_item.ItemId));
        if (status == TransactionState.Success) {

            if (_item.OneTimePurchase) {
                Destroy(gameObject);
            }
        } else if(status == TransactionState.DontEnoughCurrency) {
            //NotEnoughCoinsScreen.Instance.Show(_item.Price);
            Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                UserData = _item.Price
            });
        }
    }

    private BosCoinUpgradeData data = null;
    private BosCoinUpgradeData Data {
        get {
            return (data != null) ? data :
                (data = Services.ResourceService.CoinUpgrades.GetData(_item.ItemId));
        }
    }

    private void UpdateState()
    {
        if (_item.OneTimePurchase)
        {
            bool disable = Services.StoreService.IsCoinUpgradePurchased(Data);//Services.GetService<IShopItemTransaction>().IsTransactionForCompleted(_item.ItemId);
            if (disable)
                Destroy(gameObject);
        }
    }
}

/*
public class ShopItemTransactionHelper : GameElement, IShopItemTransaction {

    public bool Buy(BalanceManager balanceManager, ShopItem shopItem) {
        PlayerData playerData = balanceManager.PlayerData;
        IAPManager iapManager = balanceManager.IAPManager;

        if(shopItem.Price > Services.PlayerService.Coins) {
            NotEnoughCoinsScreen.Instance.Show(shopItem.Price);
            return false;
        } else {
            Services.GetService<IShopUpgrader>().ApplyUpgrade(balanceManager, playerData, shopItem);
            //iapManager.Coins.Value -= shopItem.Price;
            Services.PlayerService.RemoveCoins(shopItem.Price);
            FacebookEventUtils.LogCoinSpendEvent(shopItem.Name, shopItem.Price, Services.PlayerService.Coins);
            if(shopItem.OneTimePurchase) {
                SaveTransactionFor(shopItem.ItemId);
            }
            GameEvents.OnShopItemPurchased(
                new ShopItemInfo(shopItem.ItemId, shopItem.TargetId, 
                    Price.CreateCoins(shopItem.Price)));
            return true;
        }
    }

    public bool IsTransactionForCompleted(int itemId ) {
        return PlayerPrefs.GetInt(GetSaveKey(itemId.ToString()), 0) == 1;
    }

    public void Setup(object data = null) {
    }

    private string GetSaveKey(string itemId) => "purchased_shop_item_" + itemId;

    private void SaveTransactionFor(int itemId) {
        PlayerPrefs.SetInt(GetSaveKey(itemId.ToString()), 1);
        PlayerPrefs.Save();
    }
}

public interface IShopItemTransaction : Bos.IGameService {
    bool Buy(BalanceManager balanceManager, ShopItem shopItem);
    bool IsTransactionForCompleted(int itemId);
}*/





