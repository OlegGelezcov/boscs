namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StatsView : TypedViewWithCloseButton {

        public Text nameText;
        public Text companyCashText;
        public Text sessionEarningsText;
        public Text lifetimeEarningsText;
        public Text resetCountText;
        public Text securitiesCountText;
        public Text achievmentCountText;
        public Text rewardUnlockedCountText;
        public Text gameWonCountText;


        public override ViewType Type => ViewType.StatsView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 20;

        public override void Setup(ViewData data) {
            base.Setup(data);
            UpdateCompanyCash();
            UpdateSessionEarningsCompanyCash();
            UpdateLifetimeEarningsCompanyCash();
            UpdateResetsCount();
            UpdateSecuritiesCountText();
            UpdateAchievmentCountText();
            UpdateRewardUnlockedText();
            UpdateGameWonText();
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.StatsView, BosUISettings.Instance.ViewCloseDelay);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
        }

        public override void OnDisable() {
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            base.OnDisable();
        }

        private void OnCompanyCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount) { 
            UpdateCompanyCash();
            UpdateSessionEarningsCompanyCash();
            UpdateLifetimeEarningsCompanyCash();
        }

        private void UpdateCompanyCash() {
            companyCashText.text = Services.Currency.CreatePriceString(Services.PlayerService.CompanyCash.Value, false, " ");
        }

        private void UpdateSessionEarningsCompanyCash()
            => sessionEarningsText.text = Services.Currency.CreatePriceString(Services.PlayerService.SessionEarningsCompanyCash, false, " ");

        private void UpdateLifetimeEarningsCompanyCash()
            => lifetimeEarningsText.text = Services.Currency.CreatePriceString(Services.PlayerService.LifetimeEarnings, false, " ");

        private void UpdateResetsCount()
            => resetCountText.text = Services.GameModeService.ResetCount.ToString();

        private void UpdateSecuritiesCountText()
            => securitiesCountText.text = NumberMinifier.PrettyAbbreviatedValue(Services.PlayerService.LifeTimeSecurities.Value, false, " ");

        private void UpdateAchievmentCountText()
            => achievmentCountText.text = Services.GetService<IAchievmentServcie>().CompletedAchievmentsCount.ToString();

        private void UpdateRewardUnlockedText()
            => rewardUnlockedCountText.text = StatsCollector.Instance[Stats.REWARDS_UNLOCKED].ToString();

        private void UpdateGameWonText()
            => gameWonCountText.text = Services.GameModeService.SlotGameWonCount.ToString();
    }
}

