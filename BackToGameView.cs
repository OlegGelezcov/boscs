namespace Bos.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BackToGameView : TypedView {

        public Text offlineTimeText;
        public Text offlineBalanceText;
        public Text offlineBalanceWordText;
        public Text doubleBalanceText;
        public Text doubleBalanceWordText;
        public Button continueButton;
        public Button watchAdButton;


        public override void Setup(ViewData data) {
            base.Setup(data);
            BackToGameData backToGameData = data.UserData as BackToGameData;
            if(backToGameData == null ) {
                throw new System.ArgumentException(nameof(data.UserData));
            }

            /*
            string[] cashStrings = Services.Currency.CreatePriceStringSeparated(backToGameData.Cash);
            string[] doubleCashStrings = Services.Currency.CreatePriceStringSeparated(backToGameData.Cash * 2);

            offlineBalanceText.text = cashStrings[0];
            BosUtils.If(() => cashStrings.Length > 1, () => offlineBalanceWordText.text = cashStrings[1], () => offlineBalanceWordText.text = string.Empty);

            doubleBalanceText.text = doubleCashStrings[0];
            BosUtils.If(() => doubleCashStrings.Length > 1, () => doubleBalanceWordText.text = doubleCashStrings[1], () => doubleBalanceWordText.text = string.Empty);
            */

            offlineBalanceText.text = BosUtils.GetStandardCurrencyString(backToGameData.Cash);
            doubleBalanceText.text = BosUtils.GetStandardCurrencyString(backToGameData.Cash * 2);

            TimeSpan ts = TimeSpan.FromSeconds(backToGameData.Interval);
            offlineTimeText.text = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

            continueButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                ViewService.Remove(ViewType.BackToGameView, BosUISettings.Instance.ViewCloseDelay);
            });

            watchAdButton.SetListener(() => {
                
                ViewService.Remove(ViewType.BackToGameView, BosUISettings.Instance.ViewCloseDelay);
                Services.AdService.WatchAd("WellcomeBack", () => {
                    Player.AddGenerationCompanyCash(backToGameData.Cash);
                    FacebookEventUtils.LogADEvent("WellcomeBack");
                });

            });
        }

        #region TypedView overrides
        public override ViewType Type => ViewType.BackToGameView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 90;
        #endregion
    }

    public class BackToGameData {
        public float Interval { get; set; }
        public double Cash { get; set; }
    }
}