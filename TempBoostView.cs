using System;
using System.Collections.Generic;
using System.Linq;
using Bos.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Bos
{
	public class TempBoostView : BoostViewBase 
	{
		public Text Timer;
		public Text CountText;
		public TempBoostType Type;
		
		protected override void FillInner(GeneratorInfo generatorInfo)
		{
			var boosts = GetBootInfo();
			if (boosts != null && boosts.Count > 0)
			{
				var count = boosts.Count;

				if (count > 1)
				{
					CountText.Activate();
					CountText.text = count.ToString();
				}
				else
				{
					CountText.Deactivate();
				}

				Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ =>
				{
					var endTime = boosts.Min(val => val.EndTime);
					int interval = endTime - TimeService.UnixTimeInt;
					if (interval == 0)
					{
						Timer.Deactivate();
					}
					else {
						Timer.Activate();
						var ts = TimeSpan.FromSeconds(interval);
						Timer.text = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
					}
				}).AddTo(gameObject);
			}
			else
			{
				Timer.Deactivate();
				CountText.Deactivate();
			}
		}

		protected override void Invoke(GeneratorInfo generatorInfo)
		{
			ViewService.Show(ViewType.MiniGameView);
			ViewService.Remove(ViewType.TransportInfoView);
		}

		protected override bool CheckState(GeneratorInfo generatorInfo)
		{
			var boost = GetBootInfo();
			return boost.Count > 0;
		}

		private List<BoostInfo> GetBootInfo()
		{
			if (Type == TempBoostType.Profit)
			{
				return Services.GenerationService.Generators.ProfitBoosts.GetBoosts(BoostIds.RewardTempProfit(false));
			}
			else
			{
				return Services.GenerationService.Generators.TimeBoosts.GetBoosts(BoostIds.RewardTempTime(false));
			}
		}
	}

	public enum TempBoostType
	{
		Profit,
		Time
	}
}

