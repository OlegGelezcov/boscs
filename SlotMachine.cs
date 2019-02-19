using Bos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlotMachine : GameBehaviour
{
    public float WinChance;

    public Sprite[] Sprites;

    public GameObject WinScreen;
    public GameObject LoseScreen;

    public GameObject Winner;

    public Button Lever;

    public float TimeDelay;

    public Image Slot1;
    public Image Slot2;
    public Image Slot3;

    public GameObject[] Anims;

    public RewardManagerBase RewardManager;


    public Text RewardText;
    
    public override void Start()
    {
        Slot1.gameObject.SetActive(false);
        Slot2.gameObject.SetActive(false);
        Slot3.gameObject.SetActive(false);

        foreach (var x in Anims)
        {
            x.SetActive(false);
        }

        Pull();
    }

    public void Pull()
    {
        foreach (var x in Anims)
        {
            x.SetActive(true);
        }
        StartCoroutine(InternalPull());
        Lever.interactable = false;
        //GetComponent<AudioSource>().mute = false;
        GetComponent<AudioSource>().Play();


    }

    private IEnumerator InternalPull()
    {
        bool win;
        yield return new WaitForSeconds(0.3f);
        var revs = CalculateRevolutions(3, 3, out win);

        Slot1.sprite = Sprites[revs[0] % 3];
        Slot2.sprite = Sprites[revs[1] % 3];
        Slot3.sprite = Sprites[revs[2] % 3];

        yield return new WaitForSeconds(TimeDelay);

        Slot1.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Anims[0].SetActive(false);

        yield return new WaitForSeconds(TimeDelay);

        Slot2.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Anims[1].SetActive(false);

        yield return new WaitForSeconds(TimeDelay);


        Slot3.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Anims[2].SetActive(false);
        GetComponent<AudioSource>().enabled = false;
        yield return new WaitForSeconds(0.2f);

        if (win)
        {
            //StatsCollector.Instance[Stats.SLOTS_WON]++;
            Services.GameModeService.AddSlotGameWonCount(1);
            var reward = RewardManager.CreateReward();
            SetRewardText(reward);
            Winner.SetActive(true);
            yield return new WaitForSeconds(TimeDelay + 1.254f);
            WinScreen.SetActive(true);
        }
        else
        {
            StatsCollector.Instance[Stats.SLOTS_LOST]++;
            LoseScreen.SetActive(true);
        }
    }

    public byte[] CalculateRevolutions(byte len, byte itemCount, out bool win)
    {
        byte[] rv = new byte[len];
        var random = Random.Range(0f, 1f);

        if (random < WinChance)
        {
            var winIndex = (byte)Random.Range(0, itemCount);
            for (int i = 0; i < len; i++)
            {
                rv[i] = winIndex;
            }

            win = true;
        }
        else
        {
            var w = Random.Range(0, 100);
            var winIndex = (byte)Random.Range(0, itemCount);

            if (w % 2 == 0)
            {
                for (int i = 0; i < len; i++)
                {
                    rv[i] = (byte)((winIndex + i) % itemCount);
                }
            }
            else
            {
                rv[0] = (byte)((winIndex) % itemCount);
                rv[1] = (byte)((winIndex) % itemCount);
                rv[2] = (byte)((winIndex + 1) % itemCount);
            }

            win = false;
        }


        return rv;
    }

    public void Continue()
    {
        FindObjectOfType<GameUI>()?.HideSlotManagerGame();
    }

    private void SetRewardText(Reward reward)
    {
        if (reward is BalanceReward)
        {
            var balanceReward = reward as BalanceReward;
            RewardText.text = balanceReward.Name;
        }
        if (reward is CoinReward)
        {
            var coinReward = reward as CoinReward;
            RewardText.text = coinReward.Name;
        }
        if (reward is LifetimeBalanceReward)
        {
            var balanceReward = reward as LifetimeBalanceReward;
            var bonus = balanceReward.PercentGain * Services.PlayerService.CompanyCash.Value; //bman.BalanceManager.Balance.Value;
            if (bonus < 20) bonus = 20;
            RewardText.text = Services.Currency.CreatePriceString(bonus, false, " ");
        }
        if (reward is MultiplierReward)
        {
            var balanceReward = reward as MultiplierReward;
            RewardText.text = balanceReward.Name;
        }
        if (reward is BalanceReward)
        {
            var balanceReward = reward as BalanceReward;
            RewardText.text = balanceReward.Name;
        }
    }
}
