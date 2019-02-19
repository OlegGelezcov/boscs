using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bos;

public class BadgeReference : GameBehaviour
{
    public int BadgeId;
    public Sprite Sprite;

    public Image Icon;
    public GameObject EarnedBorder;
    public BadgeDetailView DetailView;

    private Badge _badge;

    public override void Start()
    {
        _badge = Services.BadgeService.GetNonUniqueData(BadgeId); //BadgeDb.Badges.FirstOrDefault(x => x.Id == BadgeId);

        Icon.sprite = Sprite;
        EarnedBorder.SetActive(Services.BadgeService.IsBadgeEarned(_badge));
    }

    private void FixedUpdate()
    {
        bool isEarned = Services.BadgeService.IsBadgeEarned(_badge);
        EarnedBorder.SetActive(isEarned);
        if (isEarned)
        {
            Icon.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void Click()
    {
        DetailView.Show(this, _badge);
    }
}