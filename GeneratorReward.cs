public class GeneratorReward : Reward
{
    public int GeneratorID;
    public int Count = 1;

    public override void Apply(AllManagers bman)
    {
        Services.TransportService.AddLiveUnits(GeneratorID, Count);
        Player.LegacyPlayerData.Save();
        StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
    }
}