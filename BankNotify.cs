using System;
using System.Collections;
using System.Collections.Generic;
using Bos.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bos.UI
{
	public class BankNotify : TypedViewWithCloseButton
	{

		public Button GetCoins;
		public Text CoinCount;
        public Text timeText;


		public override void Start()
		{
			GetCoins.SetListener(() =>
			{
				Sounds.PlayOneShot(SoundName.click);
				GetCoins.SetInteractable(false);
				ViewService.Show(ViewType.BankView);
				ViewService.Remove(ViewType.BankNotify, BosUISettings.Instance.ViewCloseDelay);
			});

			closeButton.SetListener(() =>
			{
				Sounds.PlayOneShot(SoundName.click);
				closeButton.SetInteractable(false);
				ViewService.Remove(ViewType.BankNotify, BosUISettings.Instance.ViewCloseDelay);
			});

			CoinCount.text = Services.BankService.CoinsAccumulatedCount.ToString();
		}

        public override void Setup(ViewData data) {
            base.Setup(data);

            string fmtTime = LocalizationObj.GetString("lbl_bank_full_2");
            string fullStr = string.Format(fmtTime, (int)TimeSpan.FromSeconds(Services.BankService.TimerFromLastCollect).TotalHours);
            timeText.text = fullStr;
        }

        #region BaseView overrides

        public override ViewType Type => ViewType.BankNotify;
		public override CanvasType CanvasType => CanvasType.UI;
		public override bool IsModal => false;
		public override int ViewDepth => 1000;

		#endregion
	}
}
