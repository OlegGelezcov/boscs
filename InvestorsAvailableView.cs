namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class InvestorsAvailableView : GameBehaviour {

		public Text securitiesCountText;

		public Button sellButton;
        public Text sellButtonText;

        public Button sellAdButton;
        public Text sellAdButtonText;


		public GameObject allowSellTimerParent;
		public Text cooldownTimerText;


		private readonly UpdateTimer updateTimer = new UpdateTimer();

		public override void OnEnable() {
			base.OnEnable();
			Setup();
		}
		private void Setup(){
			sellButton.SetListener(() => {
				Services.InvestorService.SellToInvestors(multiplier: 1);
				Services.SoundService.PlayOneShot(SoundName.buyGenerator);
				Services.ViewService.Remove(ViewType.InvestorsView, BosUISettings.Instance.ViewCloseDelay);
			});
            /*
			noButton.SetListener(() => {
				Services.SoundService.PlayOneShot(SoundName.buyGenerator);
				Services.ViewService.Remove(ViewType.InvestorsView, BosUISettings.Instance.ViewCloseDelay);
			});*/
            sellAdButton.SetListener(() => {
                sellAdButton.interactable = false;
                Services.AdService.WatchAd("x2_investor", () => {
                    StartCoroutine(SellToInvestorsForAdImpl());
                });
                
            });



            string securitiesCount = BosUtils.GetCurrencyString(Services.InvestorService.GetSecuritiesCountFromInvestors().ToCurrencyNumber());
            string securitiesx2Count = BosUtils.GetCurrencyString((Services.InvestorService.GetSecuritiesCountFromInvestors() * 2).ToCurrencyNumber());

			securitiesCountText.text = BosUtils.GetCurrencyString(Services.InvestorService.GetSecuritiesCountFromInvestors().ToCurrencyNumber());
            sellButtonText.text = securitiesCount;
            sellAdButtonText.text = securitiesx2Count;

			updateTimer.Setup(1.0f, (deltaTime) => {
				UpdateButtonInteractability();
			}, true);
		}

        private IEnumerator SellToInvestorsForAdImpl() {
            for(int i = 0; i < 4; i++ ) {
                yield return new WaitForEndOfFrame();
            }
            Services.InvestorService.SellToInvestors(multiplier: 2);
            ViewService.Remove(ViewType.InvestorsView, BosUISettings.Instance.ViewCloseDelay);
        }

		private void UpdateButtonInteractability() {
			int currentTime = Services.TimeService.UnixTimeInt;
			int allowTime = Services.InvestorService.AllowSellTime;
            var states = Services.InvestorService.SellState;

            bool isSellStateOk = ViewService.Utils.IsInvestorSellStateOk(states);
            sellButton.interactable = isSellStateOk;
            sellAdButton.interactable = isSellStateOk;

			if(IsCooldown(states)) {
				allowSellTimerParent.Activate();
				cooldownTimerText.text = BosUtils.FormatTimeWithColon(Mathf.Abs(allowTime - currentTime));
			} else {
				allowSellTimerParent.Deactivate();
			}

		}

        private bool IsCooldown(List<InvestorSellState> states)
            => states.Contains(InvestorSellState.Cooldown);

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

    }	
}

