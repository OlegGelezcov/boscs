using System;
using UniRx;
using UnityEngine.UI;

namespace Bos
{
	using UI;
	public class AdBoostView : BoostViewBase
	{
		public Text Timer;
		protected override void FillInner(GeneratorInfo generatorInfo)
		{
			var service = Services.GetService<IX2ProfitService>();
			Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
				int interval = service.AvailableAfterInterval;
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

		protected override void Invoke(GeneratorInfo generatorInfo)
		{
			Sounds.PlayOneShot(SoundName.click);
			ViewService.Show(ViewType.X2ProfitView);
		}

		protected override bool CheckState(GeneratorInfo generatorInfo)
		{
			var service = Services.GetService<IX2ProfitService>();
			return service.HasUsedSlots;
		}
	}
}

