using Bos;
using System.Collections.Generic;
using UnityEngine;
public class PlayerLevelManager : GameBehaviour
{
    public const int MaxLevelElements = 10;


    public UnityIntEvent AddXpEvent;

    public LevelPortraitDetails PortraitDetails;

    public override void Awake()
    {
        GlobalRefs.LevelManager = this;
    }

    public override void Start()
    {
        PortraitDetails = new LevelPortraitDetails();

        PortraitDetails.Stars = Services.PlayerService.Level / 100;
        PortraitDetails.MaxElement = (int)(((Services.PlayerService.Level - (PortraitDetails.Stars * 100)) / 100f) * MaxLevelElements);
    }

    public void AddXP(int xp)
    {
        Services.PlayerService.AddXP(xp);

        if (AddXpEvent != null)
        {
            AddXpEvent.Invoke(xp);
        }

        if (Services.PlayerService.CurrentXP >= Services.PlayerService.XPLevelLimit)
        {
            int c = 0;
            do
            {
                Services.PlayerService.AddLevel(1);
                Services.PlayerService.RemoveXP(Services.PlayerService.XPLevelLimit);

                //_pdata.Level++;
                //_pdata.CurrentXP -= _pdata.XPLevelLimit;
                c++;
            } while (Services.PlayerService.CurrentXP >= Services.PlayerService.XPLevelLimit);


            var achiInfo = new ExtendedAchievmentInfo(id: -1,
                    name: $"Level {Services.PlayerService.Level}",
                    iconSprite: SpriteDB.SpriteRefs["lvlUp"],
                    rewardType: RewardType.None,
                    rewardValue: 0,
                    points: 10 * c);
            achiInfo.CustomText = "Grats!";
            GameEvents.OnGeneratorAchievmentsReceived(-1, new List<ExtendedAchievmentInfo> {
                    achiInfo
                });

            //_pdata.AvailableRewards += c;
            Services.RewardsService.AddAvailableRewards(c);


            PortraitDetails.Stars = Services.PlayerService.Level / 100;
            PortraitDetails.MaxElement = (int)(((Services.PlayerService.Level - (PortraitDetails.Stars * 100)) / 100f) * MaxLevelElements);

            //_pdata.XPLevelLimit = GetXPLimit(_pdata.Level);
            Services.PlayerService.AddXPLevelLimit(GetXPLimit(Services.PlayerService.Level));
        }
    }

    private int GetXPLimit(int level)
    {
        if (level == 5) return 4000;
        if (level == 4) return 3500;
        if (level == 3) return 3000;
        if (level == 2) return 2500;
        if (level == 1) return 2000;

        return 1500 + (int)(Mathf.Sqrt(level - 5) * 9500);
    }
}

public static class XPSources
{
    public static int BuyGenerator = 65;
    public static int UnlockGenerator = 400;
    public static int BuyUpgrade = 70;
}

public class LevelPortraitDetails
{
    public int MaxElement;
    public int Stars;
}
