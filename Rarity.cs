using UnityEngine;

public enum Rarity : int
{
    Common = 0,
    Rare = 1,
    Epic = 2,
    Legendary = 3
}
public static class RarityHelper
{
    public const float RareWeight = 0.1f;
    public const float EpicWeight = 0.05f;
    public const float LegendaryWeight = 0.01f;

    public static Color CommonColor = new Color(1, 1, 1, 1);
    public static Color RareColor = new Color(0.529F, 0.808F, 0.922F, 1);
    public static Color EpicColor = new Color(0.933F, 0.510F, 0.933F, 1);
    public static Color LegendaryColor = new Color(1, 0.74F, 0.39F, 1);
    public static Color FallbackColor = new Color(255, 255, 0);

    public static Rarity GetWeightedRarity()
    {
        var rand = Random.Range(0f, 1f);

        if (rand <= LegendaryWeight)
            return Rarity.Legendary;

        if (rand <= EpicWeight)
            return Rarity.Epic;

        if (rand <= RareWeight)
            return Rarity.Rare;

        return Rarity.Common;
    }

    public static Color GetRarityColor(Rarity r)
    {
        switch (r)
        {
            case Rarity.Common:
                return CommonColor;
            case Rarity.Rare:
                return RareColor;
            case Rarity.Epic:
                return EpicColor;
            case Rarity.Legendary:
                return LegendaryColor;
            default:
                return FallbackColor;
        }
    }
}
