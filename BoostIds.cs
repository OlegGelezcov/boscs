using Bos;

public class BoostIds
{
	public static string RewardTempProfit(bool withSuffix = true)
	{
		var str = "reward_profit_";
		return SuffixResolve(str, withSuffix);
	}
	
	public static string RewardTempTime(bool withSuffix = true)
	{
		var str = "reward_time_";
		return SuffixResolve(str, withSuffix);
	}
	
	public static string GetPersistCoinUpId(int data, bool withSuffix = true)
	{
		var str = $"persist_coin_up_{data}_";
		return SuffixResolve(str, withSuffix);
	}
	public static string GetTempCoinUpId(int data, bool withSuffix = true)
	{
		var str = $"temp_coin_up_{data}_";
		return SuffixResolve(str, withSuffix);
	}
	
	public static string GetPersistCoinUpgradeId(int data,  bool withSuffix = true)
	{
		var str = $"persist_coin_upgrade_{data}_";
		return SuffixResolve(str, withSuffix);
	}
	
	public static string GetTempCoinUpgradeId(int data,  bool withSuffix = true)
	{
		var str = $"temp_coin_upgrade_{data}_";
		return SuffixResolve(str, withSuffix);
	}
	
	public static string GetPersistLocalCoinUpId(int data,  bool withSuffix = true)
	{	var str = $"persist_local_coin_up_{data}_";
		return SuffixResolve(str, withSuffix);
	}
	public static string GetTempLocalCoinUpId(int data, bool withSuffix = true)
	{
		var str = $"temp_local_coin_up_{data}_";
		return SuffixResolve(str, withSuffix);
	}

	private static string SuffixResolve(string str, bool withSuffix = true)
	{
		return !withSuffix ? str : str.GuidSuffix(5);
	}
}
