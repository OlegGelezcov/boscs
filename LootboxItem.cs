using UnityEngine;

public class LootboxItem : MonoBehaviour
{
    public Sprite Icon;

    public Rarity Rarity;
    public string DescText;

    public int TargetGenerator = -1;

    public LootboxRewardType RewardType;
    public double RewardValue;
    [Header("When CoinsAndBalance is set, this indicates the Coin value")]
    public double RewardValue2;
}

public enum LootboxRewardType
{
    Coins,
    Generators,
    ProfitUpgrade,
    SpeedUpgrade,
    Manager,
    Investors,
    Balance,
    CoinsAndBalance,
    PercentBalance
}