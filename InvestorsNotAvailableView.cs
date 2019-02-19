using UnityEngine;

namespace Bos.UI {
    using UnityEngine.UI;

    public class InvestorsNotAvailableView : GameBehaviour {
		public Button raiseStatusButton;
        public Text descriptionText;
        public Text requiredCashText;

        public GameObject TryReachCashContent;

		public override void OnEnable() {

		    TryReachCashContent.SetActive(false);
		    
            if(Services.InvestorService.TriesCount > 0 ) {
                descriptionText.text = LocalizationObj.GetString("lbl_investor_lock_2");
                raiseStatusButton.Deactivate();

                double requiredCash = Services.InvestorService.GetCompanyCashRequiredToSellInvestors();
                if(requiredCash.Approximately(0.0)) {
                    requiredCashText.text = string.Empty;
                } else {
                    requiredCashText.text =$"{BosUtils.GetCurrencyString(new CurrencyNumber(requiredCash), "#FFE759", "#FFE759")}";
                    TryReachCashContent.SetActive(true);
                }
            } else {
                descriptionText.text = LocalizationObj.GetString("lbl_investor_locked_view");
                raiseStatusButton.Activate();
                requiredCashText.text = string.Empty;
            }

			raiseStatusButton.SetListener(() => {
                //Services.ViewService.ShowDelayed(ViewType.ProductsView, BosUISettings.Instance.ViewShowDelay);
                Services.ViewService.ShowDelayed(ViewType.ProfileView, BosUISettings.Instance.ViewShowDelay, new ViewData {
                    UserData = ProfileViewTab.StatusGoods
                });
				Services.SoundService.PlayOneShot(SoundName.click);
			});
		}
	}	
}

