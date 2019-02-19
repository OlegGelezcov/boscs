using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bos.Data;
using Bos;

public class ShopItem : GameBehaviour
{
    public int ItemId;
    public int TargetId = -1;
    public string Name;
    public string Description;
    public int Price;
    public Sprite Icon;

    public bool OneTimePurchase = false;
    public bool Permanent = true;

    public UpgradeType UpgradeType;
    public int ProfitMultiplier;
    public int TimeMultiplier;
    public int DaysOfFutureBalance;


    public override void Awake()
    {
        //var prot = GameData.instance.coinUpgrades.FirstOrDefault(val => val.Id == ItemId);
        BosCoinUpgradeData prot = Services.ResourceService.CoinUpgrades.GetData(ItemId);


        TargetId = prot.GeneratorId;
        Name = Services.ResourceService.Localization.GetString(prot.Name);
        Description = Services.ResourceService.Localization.GetString(prot.Description);
        Price = prot.Price;
        OneTimePurchase = prot.IsOneTime;
        Permanent = prot.IsPermanent;
        UpgradeType = prot.UpgradeType;
        ProfitMultiplier = prot.ProfitMutlitplier;
        TimeMultiplier = prot.TimeMultiplier;
        DaysOfFutureBalance = prot.DaysOfFutureBalance;
    }
}
