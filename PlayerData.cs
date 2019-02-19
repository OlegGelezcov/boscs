using Bos;
using Newtonsoft.Json;
using System;
using UnityEngine;

public class PlayerData {
#if UNITY_IOS && BOS
    public const string KEY = "playerDataNew";
#else
    public const string KEY = "playerData";
#endif

    public int ConsecutiveDaysEntered = 0;
    public DateTime CurrentOfferExpires;
    public bool DailyBonusGathered;
    public DateTime DateOfferClicked;
    public int SessionCount = 0;
    public int CurrentPromotionIndex = 0;
    public FlashSaleItem CurrentFlashSale;
    public bool FreeCoinsFB = false;
    public bool FreeCoinsTW = false;

#region LocalData optimization

    public double StoredMaxBalance = 4;
    public double LastPauseBalance = 0;
    public bool LocalDataPorted = false;

    public double TempMultiplier = 1;

    public int GameFinished;

    public RewardedVideoAchievmentStatus rewardedVideoAchievmentStatus = new RewardedVideoAchievmentStatus();

#endregion


    public void Load() {
        var json = PlayerPrefs.GetString(KEY, null);
        if (!string.IsNullOrEmpty(json)) {
            var pdata = JsonConvert.DeserializeObject<PlayerData>(json);

#region LocalData

            LocalDataPorted = pdata.LocalDataPorted;

            if (!LocalDataPorted) {
                //LifetimeEarnings = OLD_LocalData.LifetimeSavings;
                LastPauseBalance = OLD_LocalData.LastPauseBalance;
                LocalDataPorted = true;
            } else {
                //LifetimeEarnings = pdata.LifetimeEarnings;
                LastPauseBalance = pdata.LastPauseBalance;
            }
#endregion

            FreeCoinsFB = pdata.FreeCoinsFB;
            FreeCoinsTW = pdata.FreeCoinsTW;

            GameFinished = pdata.GameFinished;

            CurrentFlashSale = pdata.CurrentFlashSale;
            CurrentPromotionIndex = pdata.CurrentPromotionIndex;
            SessionCount = pdata.SessionCount;
            ConsecutiveDaysEntered = pdata.ConsecutiveDaysEntered;
            CurrentOfferExpires = pdata.CurrentOfferExpires;
            DailyBonusGathered = pdata.DailyBonusGathered;
            DateOfferClicked = pdata.DateOfferClicked;
            rewardedVideoAchievmentStatus = pdata.rewardedVideoAchievmentStatus;
        }

        //if (managers == null || managers.Count == 0) ManagerInit();
    }

    public void Save() {
        var json = JsonConvert.SerializeObject(this);
        PlayerPrefs.SetString(KEY, json);
        GameServices.Instance?.SaveService?.SaveAll();
        PlayerPrefs.Save();
    }


}

[Serializable]
public class RewardedVideoAchievmentStatus {

    public bool achieved10;
    public bool achieved100;
    public bool achieved1000;
}

