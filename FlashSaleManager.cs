using Bos;
using System;
using System.Linq;
using UnityEngine;

public class FlashSaleManager : GameBehaviour
{
    private bool _noShow = false;
    public GameObject FlashSaleButton;

    public override void Start()
    {
        GlobalRefs.FlashSalesManager = this;

        if (Player.LegacyPlayerData.SessionCount < 5)
        {
            Player.LegacyPlayerData.CurrentFlashSale = null;
            FlashSaleButton.SetActive(false);
            _noShow = true;
            enabled = false;
            return;
        }

        var currentFlashSale = Player.LegacyPlayerData.CurrentFlashSale;

        if (currentFlashSale == null || Player.LegacyPlayerData.CurrentFlashSale.ExpirationDate < DateTime.Now)
        {
            Player.LegacyPlayerData.CurrentFlashSale = GetNextSale();
        }
    }

    private float _frame;
    public override void Update()
    {
        if (_frame < 1.1f)
        {
            _frame += Time.deltaTime;
        }

        _frame = 0;
        
        var currentFlashSale = Player.LegacyPlayerData.CurrentFlashSale;

        if (currentFlashSale.ShouldHide || currentFlashSale.ActiveOnScreen )
            FlashSaleButton.SetActive(false);
        else
            FlashSaleButton.SetActive(currentFlashSale != null);

        if (currentFlashSale == null || 
        Player.LegacyPlayerData.CurrentFlashSale.ExpirationDate < DateTime.Now)
        {
            Player.LegacyPlayerData.CurrentFlashSale = GetNextSale();
        }
    }

    private FlashSaleItem GetNextSale()
    {
        var items = Services.ResourceService.Products.Promotions.ToArray(); //GlobalRefs.IAP.Promotions.ToArray();
        var curPromo = Player.LegacyPlayerData.CurrentPromotionIndex % items.Length;

        FlashSaleItem rv = new FlashSaleItem()
        {
            Listing = items[curPromo].ToListing(),
            ExpirationDate = DateTime.Now.AddHours(2),
        };
        Player.LegacyPlayerData.CurrentPromotionIndex++;

        return rv;
    }
}
