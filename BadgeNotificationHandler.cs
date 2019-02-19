using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Bos;

public class BadgeNotificationHandler : GameBehaviour
{
    private DateTime _nextTapAvailable;

    public GameObject NotificationContainer;

    public Text Name;
    public Text Desc;
    public Text RewardText;
    public GameObject RewardLabel;
    public Image Image;

    public override void Start()
    {
        NotificationContainer.SetActive(false);
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.BadgeAdded += OnBadgeAdded;
    }

    public override void OnDisable() {
        GameEvents.BadgeAdded -= OnBadgeAdded;
        base.OnDisable();
    }

    private void OnBadgeAdded(Badge ubadge) {
        var badge = Services.BadgeService.GetUniqueData(ubadge.UniqueId);
        StartCoroutine(BadgeAddedImpl(badge));
    }


    private IEnumerator BadgeAddedImpl(Badge badge)
    {
        NotificationContainer.SetActive(false);
        yield return new WaitForEndOfFrame();

        Name.text = Services.ResourceService.Localization.GetString($"B.NAME{badge.Id}"); //$"B.NAME{badge.Id}".GetLocale(LocalizationDataType.ui);
        Desc.text = badge.Description;
        RewardText.text = badge.RewardText;
        Image.sprite = Services.ResourceService.GetSpriteByKey(badge.SpriteId); //e.Sprite;

        RewardLabel.SetActive(!string.IsNullOrWhiteSpace(RewardText.text));

        NotificationContainer.SetActive(true);

        _nextTapAvailable = DateTime.Now.AddSeconds(1.5);
    }

    public void NotificationTap()
    {
        if (DateTime.Now < _nextTapAvailable)
            return;

        NotificationContainer.SetActive(false);
    }
}
