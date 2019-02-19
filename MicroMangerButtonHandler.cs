using System.Collections;
using System.Collections.Generic;
using Bos;
using Bos.UI;
using GooglePlayGames;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class MicroMangerButtonHandler : GameBehaviour
{

    public Text ButtonText, PriceText, descText;
    public GameObject BuyContent;
    private ShopItem _item;
    //private ShopItemView _itemView;
    
    public ReactiveProperty<bool> _buyed = new ReactiveProperty<bool>(false);

    public override void OnEnable() {
        GameEvents.ShopItemPurchased += OnItemPurchased;
    }

    public override void OnDisable() {
        GameEvents.ShopItemPurchased -= OnItemPurchased;
    }
    
    public override  void Start()
    {
        _item = GetComponent<ShopItem>();
        //_itemView = GetComponent<ShopItemView>();
        PriceText.text = "x" + _item.Price.ToString();


        UpdateButtonView();
    }

    private void OnItemPurchased(IShopItem shopItem) {
        if(_item != null ) {
            if(shopItem.ItemId == _item.ItemId ) {
                UpdateButtonView();
            }
        }
    }

    private void UpdateButtonView() {
        _buyed.Subscribe(val => {
            BuyContent.SetActive(!Services.PlayerService.IsHasMicromanager);
            ButtonText.gameObject.SetActive(Services.PlayerService.IsHasMicromanager);
            descText.text = Services.PlayerService.IsHasMicromanager ?
                "Continuous tapping of the BOOST button grants you a 20x profit multiplier." :
                "Once you unlock the Profit Booster the BOOST! button becomes available. Tapping it will grant you a 20x profit multiplier";
        });
        _buyed.Value = Services.PlayerService.IsHasMicromanager;
    }

    public void Click()
    {
        if (Services.PlayerService.IsHasMicromanager)
        {
            StartCoroutine(SetMultipliers());
        }
        else
        {
            var status = Services.StoreService.Purchase(Services.ResourceService.CoinUpgrades.GetData(_item.ItemId));

            if ( status == TransactionState.Success) {
                _buyed.Value = true;
            } else if(status == TransactionState.DontEnoughCurrency) {
                //NotEnoughCoinsScreen.Instance.Show(_item.Price);
                Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData() {
                    UserData = _item.Price
                });
            }
        }
    }

    private bool _run;
    private IEnumerator SetMultipliers()
    {
        if (!_run)
            _run = true;
        else
            yield break; 
        
        Player.LegacyPlayerData.TempMultiplier *= 20;
        yield return new WaitForSeconds(0.1f);
        Player.LegacyPlayerData.TempMultiplier /= 20;
        _run = false;
    }
}
