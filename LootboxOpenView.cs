using Bos;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LootboxOpenView : GameBehaviour
{
    private DailyBonusItem _item;


    private LootboxRewardManager rewardManager;
    public LootboxRewardManager Manager
        => (rewardManager != null) ? rewardManager : (rewardManager = FindObjectOfType<LootboxRewardManager>());

    /// <summary>
    /// 4     5
    ///    1
    /// 0     2
    ///    3
    /// 6     7
    /// </summary>
    public LootboxItemView[] Views;
    public GameObject OpenLootboxButton;
    public GameObject ContinueButton;
    public GameObject ParticleFX;
    public GameObject TapText;
    public Image Icon;
    public float InterItemDelay = 0.2f;

    public bool UseDailyBonusItemPrep = true;
    public Sprite DefaultSprite;
    public int DefaultRewardCount = 3;

    public override void OnEnable()
    {
        base.OnEnable();
        ContinueButton.SetActive(false);
        OpenLootboxButton.SetActive(true);
        foreach (var x in Views)
        {
            x.gameObject.SetActive(false);
        }
    }

    public void PrepareClaim(DailyBonusItem item)
    {
        gameObject.SetActive(true);
        _item = item;
        Icon.sprite = UseDailyBonusItemPrep ? _item.ButtonBG : DefaultSprite;
    }

    public void Prepare()
    {
        Icon.sprite = UseDailyBonusItemPrep ? _item.ButtonBG : DefaultSprite;
        gameObject.SetActive(true);
    }

    public void OpenLootbox()
    {
        ParticleFX.SetActive(true);
        OpenLootboxButton.SetActive(false);
        TapText.SetActive(false);

        var rc = UseDailyBonusItemPrep ? _item.RewardCount : DefaultRewardCount;

        var lb = Manager.CreateLootbox(rc);

        Manager.GiveRewards(lb);

        if (!UseDailyBonusItemPrep) {
            //GlobalRefs.PlayerData.AvailableRewards--;
            Services.RewardsService.RemoveAvailableRewards(1);
        }

        if (rc == 1)
        {
            ActivateLootboxItem(lb[0], Views[1]);
            ContinueButton.SetActive(true);
            return;
        }

        if (rc == 2)
        {
            StartCoroutine(StartAnim2LB(lb));
            return;
        }

        if (rc >= 3)
        {
            StartCoroutine(StartAnimNLB(rc, lb));
            return;
        }
    }

    private IEnumerator StartAnimNLB(int rc, Lootbox lb)
    {
        for (int i = 0; i < rc; i++)
        {
            ActivateLootboxItem(lb[i], Views[i]);
            yield return new WaitForSeconds(InterItemDelay);
        }
        ContinueButton.SetActive(true);
    }

    private IEnumerator StartAnim2LB(Lootbox lb)
    {
        ActivateLootboxItem(lb[0], Views[0]);
        yield return new WaitForSeconds(InterItemDelay);
        ActivateLootboxItem(lb[1], Views[2]);
        yield return new WaitForSeconds(InterItemDelay);
        ContinueButton.SetActive(true);
    }

    private void ActivateLootboxItem(LootboxItem lbi, LootboxItemView view)
    {
        if (lbi == null)
        {
            //TEMP placeholders
            lbi = new LootboxItem();
            lbi.Rarity = RarityHelper.GetWeightedRarity();
            lbi.DescText = "Temp Item";
        }
        view.Rarity = lbi.Rarity;
        view.DescText.text = lbi.DescText;
        view.Sprite = lbi.Icon;
        view.gameObject.SetActive(true);
    }
}
