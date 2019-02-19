public class ManagerReward : Reward
{
    public int GeneratorIDToManage;

    public override void Apply(AllManagers bman)
    {
        Services.ManagerService.HireManagerFree(GeneratorIDToManage);
        StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
    }
}