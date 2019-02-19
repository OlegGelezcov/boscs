using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LootboxItemView : MonoBehaviour
{
    public Rarity Rarity;
    public Sprite Sprite;

    [Header("UI")]
    public Text DescText;
    public Image LightEmitter;
    public Image LightRay;
    public Image Icon;

    private void Start()
    {
        var color = RarityHelper.GetRarityColor(Rarity);

        Icon.sprite = Sprite;

        DescText.color = color;
        LightEmitter.color = color;
        LightRay.color = color;
    }

    private void FixedUpdate()
    {
        var color = RarityHelper.GetRarityColor(Rarity);

        Icon.sprite = Sprite;

        DescText.color = color;
        LightEmitter.color = color;
        LightRay.color = color;
    }


}

