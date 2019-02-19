using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class DailyBonusManager : GameBehaviour
{

    public DailyBonusItem[] Items;
    public GameObject DailyBonusScreen;

    public LootboxOpenView LootboxOpenView;
    private DailyBonusItem _lastClickedItem;

    public GameObject dailyBonusButton;
    
    public override void Start()
    {
        CheckGathered();   
        UpdateStatus();
        
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.DailyBonusClaimed += Item_Claim;
    }

    public override void OnDisable() {
        GameEvents.DailyBonusClaimed -= Item_Claim;
        base.OnDisable();
    }

    private void UpdateStatus()
    {
        for (int i = 0; i < Player.LegacyPlayerData.ConsecutiveDaysEntered; i++)
        {
            Items[i].Status = DailyBonusStatus.Claimed;
        }
        
        if (!Player.LegacyPlayerData.DailyBonusGathered)
            Items[Player.LegacyPlayerData.ConsecutiveDaysEntered].Status = DailyBonusStatus.Available;
        else
            Items[Player.LegacyPlayerData.ConsecutiveDaysEntered].Status = DailyBonusStatus.Future;

        if (Player.LegacyPlayerData.ConsecutiveDaysEntered < 6)
        {
            for (int i = Player.LegacyPlayerData.ConsecutiveDaysEntered + 1; i < Items.Length; i++)
            {
                Items[i].Status = DailyBonusStatus.Future;
            }
        }

        //DailyBonusScreen.SetActive(false);
    }


    private void CheckGathered()
    {
        if (DateTime.Now > Player.LegacyPlayerData.CurrentOfferExpires)
        {
            var diff = DateTime.Now.Date - Player.LegacyPlayerData.DateOfferClicked.Date;
            
            Player.LegacyPlayerData.CurrentOfferExpires = DateTime.Now.Date.AddDays(1);
            Player.LegacyPlayerData.DailyBonusGathered = false;

            if (Player.LegacyPlayerData.DateOfferClicked.Year != 1)
            {
                if (diff.TotalHours > 24)
                {
                    Player.LegacyPlayerData.ConsecutiveDaysEntered = 0;
                }
            }
        }
        
        //foreach (var item in Items)
        //{
        //    item.Claim += Item_Claim;
        //}
    }

    private float frameTime = 0;
    public override void Update()
    {
        dailyBonusButton.gameObject.SetActive(!Player.LegacyPlayerData.DailyBonusGathered);
        
        if (frameTime < 10)
        {
            frameTime += Time.deltaTime;
            return;
        }
        frameTime = 0;

        CheckGathered();
        UpdateStatus();
    }

    private void Item_Claim(DailyBonusItem item)
    {
        //var item = sender as DailyBonusItem;

        if (item.Status != DailyBonusStatus.Available)
            return;

        _lastClickedItem = item;
        LootboxOpenView.PrepareClaim(_lastClickedItem);

        Player.LegacyPlayerData.DailyBonusGathered = true;
        Player.LegacyPlayerData.DateOfferClicked = DateTime.Now;

        Items[Player.LegacyPlayerData.ConsecutiveDaysEntered].Status = DailyBonusStatus.Claimed;

        Player.LegacyPlayerData.ConsecutiveDaysEntered++;
        if (Player.LegacyPlayerData.ConsecutiveDaysEntered > 6)
        {
            Player.LegacyPlayerData.ConsecutiveDaysEntered = 0;
        }
    }
}