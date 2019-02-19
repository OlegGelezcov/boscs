using Bos;
using Bos.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfilePageUI : GameBehaviour
{
    public Sprite FemaleAvatar;
    public Sprite MaleAvatar;

    [Header("UI Elements")]
    public Text Title1;
    //public Text Title2;
    //public Text CurrentBalance;
    //public Text SessionEarnings;
    //public Text LiftimeEarnings;
    //public Text TotalResets;
    //public Text TotalInvestors;

    public GameObject Landing;
    //public GameObject Stats;

    public Toggle MaleToggle;
    public Toggle FemaleToggle;
    public Image ProfileImage;

    public Text AchiUnlocked;
    public Text RewardsUnlocked;
    public Text GamesWon;
    private LocalEventManager _bman;

    public Button consoleButton;
    public Button statsButton;


    public override void Start()
    {
        var q = GameObject.Find("LocalEventManager");
        _bman = q.GetComponent<LocalEventManager>();

        //UpdateStats();
        IPlayerService playerService = Services.PlayerService;

        if (playerService.Gender == Gender.Male)
        {
            MaleToggle.isOn = true;
            FemaleToggle.isOn = false;
        }
        else
        {
            MaleToggle.isOn = false;
            FemaleToggle.isOn = true;
        }

        UpdateProfileImage();


        if (_bman != null)
        {
            AchiUnlocked.text = Services.GetService<IAchievmentServcie>().CompletedAchievmentsCount.ToString();//_bman.BalanceManager.PlayerData.CompletedAchievements.Count.ToString();
            RewardsUnlocked.text = StatsCollector.Instance[global::Stats.REWARDS_UNLOCKED].ToString();
            GamesWon.text = Services.GameModeService.SlotGameWonCount.ToString(); //StatsCollector.Instance[global::Stats.SLOTS_WON].ToString();
        }

        consoleButton?.SetListener(() => {
            var consoleService = Services.GetService<IConsoleService>();
            consoleService.SetVisible(!consoleService.IsVisible);
        });
    }

    /*
    private void UpdateStats()
    {
        var cur = Services.Currency;

        var lifetimeEarnings = Services.PlayerService.LifetimeEarnings; //LocalData.LifetimeSavings;
        var lestr = cur.CreatePriceStringSeparated(lifetimeEarnings);

        if (lestr.Length > 1)
            Title2.text = Title1.text = lestr[1] + "aire";
        else
            Title2.text = Title1.text = "Poor";

        CurrentBalance.text = cur.CreatePriceString(Services.PlayerService.CompanyCash.Value, false, " ");
        SessionEarnings.text = cur.CreatePriceString(Services.PlayerService.SessionEarningsCompanyCash, false, " ");

        LiftimeEarnings.text = lestr.Length > 1 ? string.Format("{0} {1}", lestr[0], lestr[1]) : string.Format("{0}", lestr[0]);

        TotalResets.text = Services.GameModeService.ResetCount.ToString(); //LocalData.Resets.ToString();
        var i = Services.PlayerService.Securities.Value; //_bman.BalanceManager.PlayerData.Investors;

        TotalInvestors.text = NumberMinifier.MinifyBigInt(i, false, " "); ;
    }*/

    public override void OnEnable()
    {
        if (_bman != null)
        {
            AchiUnlocked.text = Services.GetService<IAchievmentServcie>().CompletedAchievmentsCount.ToString(); //_bman.BalanceManager.PlayerData.CompletedAchievements.Count.ToString();
            RewardsUnlocked.text = StatsCollector.Instance[global::Stats.REWARDS_UNLOCKED].ToString();
            GamesWon.text = Services.GameModeService.SlotGameWonCount.ToString(); //StatsCollector.Instance[global::Stats.SLOTS_WON].ToString();
            //UpdateStats();
        }
        statsButton.SetListener(() => {
            Services.ViewService.Show(Bos.UI.ViewType.StatsView);
            Services.SoundService.PlayOneShot(SoundName.click);
        });
    }



    private void UpdateProfileImage()
    {
        //var gender = LocalData.Gender;
        Gender gender = Services.PlayerService.Gender;

        switch (gender)
        {
            case Gender.Male:
                ProfileImage.sprite = MaleAvatar;
                break;
            case Gender.Female:
                ProfileImage.sprite = FemaleAvatar;
                break;
            default:
                break;
        }
    }

    /*
    public void CheckYourStats()
    {
        Landing.SetActive(false);
        Stats.SetActive(true);
    }

    public void CloseStats()
    {
        Landing.SetActive(true);
        Stats.SetActive(false);
    }*/


    public void Close()
    {
        transform.parent.gameObject.SetActive(false);
    }

    public void SetMale(bool value)
    {
        if (value)
        {
            //LocalData.Gender = 0;
            Services.PlayerService.SetGender(Gender.Male);
            Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            UpdateProfileImage();
        }
        else
            SetFemale(true);
    }

    public void SetFemale(bool value)
    {
        if (value)
        {
            //LocalData.Gender = 1;
            Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            Services.PlayerService.SetGender(Gender.Female);
            UpdateProfileImage();
        }
        else
            SetMale(true);
    }

    
}
