using System;
using Bos;
using UniRx;
using UnityEngine.UI;

public class FlashSaleButton : GameBehaviour
{
	public Text expireText;
	
	private IDisposable disposable;
	private void Start()
	{
		disposable = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
			UpdateExpire();
		});
	}

	private void UpdateExpire()
	{
		if (expireText == null) return;
		
		var currentFlashSale = Player.LegacyPlayerData.CurrentFlashSale;
		var ts = currentFlashSale.ExpirationDate - DateTime.Now;

		if (ts.TotalHours > 0)
		{
			expireText.text = $"{ts.Hours.ToString("00")}:{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";
		}
		else
		{
			expireText.text = ts.TotalSeconds > 0 ? $"{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}" : "00:00";
		}
	}

	private void OnDestroy()
	{
		base.OnDestroy();
		disposable?.Dispose();
	}
}
