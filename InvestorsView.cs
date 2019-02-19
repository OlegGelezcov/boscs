namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

    public class InvestorsView : TypedViewWithCloseButton
    {
		public Button historyButton;

		public GameObject investorsAvailableView;
		public GameObject investorsNotAvailableView;

        public override ViewType Type => ViewType.InvestorsView;


        public override CanvasType CanvasType => CanvasType.UI;

		public override int ViewDepth => 5;

		public override bool IsModal => true;

		public override void Setup(ViewData data){
			UpdateChildViews();
			closeButton.SetListener(() => {
				Services.ViewService.Remove(ViewType.InvestorsView, BosUISettings.Instance.ViewCloseDelay);
				Services.SoundService.PlayOneShot(SoundName.click);
				closeButton.interactable = false;
			});

			historyButton.SetListener(() => {
                //show history view
                Services.ViewService.ShowDelayed(ViewType.HistoryView, BosUISettings.Instance.ViewShowDelay, new ViewData {
	                UserData = Services.InvestorService.History,
                    ViewDepth = ViewService.NextViewDepth
                });
				Services.SoundService.PlayOneShot(SoundName.click);
			});
            Services.InvestorService.SellState.ForEach(state => {
                print($"investor state => {state}");
            });
            Services.InvestorService.AlertInfo.SetAlertShowedInSession(true);
            Services.InvestorService.AlertInfo.SetCountOfSecuritiesToShowNextAlert(Player.Securities.Value * 2);
		}

		private void UpdateChildViews()
		{
			var securites = Services.InvestorService.GetSecuritiesCountFromInvestors();
			var isHave1000 = securites >= 1000;

            //var sellStates = Services.InvestorService.SellState;
	
            
			if(Services.InvestorService.TriesCount > 0 &&  isHave1000 && Services.InvestorService.IsSellStateOk) {
				investorsAvailableView.Activate();
				investorsNotAvailableView.Deactivate();
			} else {
				investorsAvailableView.Deactivate();
				investorsNotAvailableView.Activate();
			}
		}

		public override void OnEnable() {
			base.OnEnable();
			GameEvents.StatusPointsChanged += OnStatusPointsChanged;
			GameEvents.InvestorTriesCountChanged += OnTriesCountChanged;
		}

		public override void OnDisable() {
			GameEvents.StatusPointsChanged -= OnStatusPointsChanged;
			GameEvents.InvestorTriesCountChanged -= OnTriesCountChanged;
			base.OnDisable();
		}

        private void OnStatusPointsChanged(long oldCount, long newCount ) 
			=> UpdateChildViews();

		private void OnTriesCountChanged(int oldCount, int newCount)
			=> UpdateChildViews();

    }
}

