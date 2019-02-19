using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BadgeDetailView : MonoBehaviour
{
    public Image Icon;
    public Text Name;
    public Text Objective;
    public Text Details;
    public Text RewardText;

    public GameObject RewardLabel;

    public void Show(BadgeReference bref, Badge b)
    {
        /*Icon.sprite = bref.Sprite;
        Name.text = $"B.NAME{b.Id}".GetLocale(LocalizationDataType.ui);
        Objective.text = b.ObjectiveText;
        Details.text = $"B.DESC{b.Id}".GetLocale(LocalizationDataType.ui);
        RewardText.text = b.RewardText;*/

        Icon.sprite = bref.Sprite;
        Name.text = b.Name;
        Objective.text = b.ObjectiveText;
        Details.text = b.Description;
        RewardText.text = b.RewardText;
        
        
        RewardLabel.SetActive(!string.IsNullOrWhiteSpace(RewardText.text));

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}