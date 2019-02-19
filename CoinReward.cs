public class CoinReward : Reward
{
    public int Coins;

    public override void Apply(AllManagers bman)
    {
        Player.AddCoins(Coins, isFree: true);
        StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
    }
}