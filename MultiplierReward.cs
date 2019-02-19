using Bos;

public class MultiplierReward : Reward
{
    public int Profit = 1;
    public int Speed = 1;

    public override void Apply(AllManagers bman)
    {
        //Services.GenerationService.Generators.AddX2Time(Services.TimeService.Now.AddMinutes(10));
        //Services.GenerationService.Generators.AddX2Profit(Services.TimeService.Now.AddMinutes(10));

        if (Profit > 1) {
            Services.GenerationService.Generators.AddProfitBoost(
                BoostInfo.CreateTimed(BoostIds.RewardTempProfit(), Profit, Services.TimeService.UnixTimeInt + 10 * 60));
        }

        if (Speed > 1) {
            Services.GenerationService.Generators.AddTimeBoost(
                BoostInfo.CreateTimed(BoostIds.RewardTempTime(), Speed, Services.TimeService.UnixTimeInt + 10 * 60));
        }
        Player.LegacyPlayerData.Save();

        StatsCollector.Instance[Stats.REWARDS_UNLOCKED]++;
    }
}