using UnityEngine.SceneManagement;

namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class CashConvertView : GameBehaviour {

		public Text officialTaxPercentText;
		public Text companyCash1Text;
		public Text companyCash2Text;

		public Text playerCash1Text;
		public Text playerCash2Text;

		public Button officialTransferButton;

		public Text looseChanceText;

		public Button unofficialTransferButton;

		public Button closeButton;

		//public override bool IsModal => true;

		private readonly UpdateTimer updateTimer = new UpdateTimer();

		public void Setup(object data) {

			officialTaxPercentText.text = $"TAX: {(int)(100 * Services.ResourceService.PersonalImprovements.ConvertData.OfficialConvertPercent)}%";
			looseChanceText.text = $"LOOSE CHANCE: {(int)(100 * Services.ResourceService.PersonalImprovements.ConvertData.UnofficialConvertPercent)}%";

			UpdateCompanyCash();
			UpdatePlayerCash();

			officialTransferButton.SetListener(() => {
				var result = Services.PlayerService.StartTransferCashOfficially();
				Debug.Log(result.ToString());
				Services.GetService<ISoundService>().PlayOneShot(SoundName.buyGenerator);
				Services.ViewService.Show(ViewType.LoadingView, new ViewData() {
					UserData = new LoadSceneData() {
						BuildIndex = 7,
						Mode = LoadSceneMode.Additive,
						LoadAction = () => { FindObjectOfType<TransferGameController>()?.Setup(result); }
					}
				});
			});

			unofficialTransferButton.SetListener(() => {
				var result = Services.PlayerService.TransferIlegally(null);
				Debug.Log(result.ToString());
				if(result.IsSuccess) {
					Services.GetService<ISoundService>().PlayOneShot(SoundName.buyGenerator);
				} else {
					Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
				}
			});

			updateTimer.Setup(0.25f, (deltaTime) => {
				officialTransferButton.interactable = 
				unofficialTransferButton.interactable = (Services.PlayerService.CompanyCash.Value > 0.0);
			}, invokeImmediatly: true);

			closeButton.SetListener(() => {
				//Services.ViewService.Remove(ViewType.CashConvertView, BosUISettings.Instance.ViewCloseDelay);
				closeButton.interactable = false;
				Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
			});
		}

		public override void OnEnable() {
			base.OnEnable();
			GameEvents.CompanyCashChanged += OnCompanyCashChanged;
			GameEvents.PlayerCashChanged += OnPlayerCashChanged;
		}

		public override void OnDisable() {
			GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
			GameEvents.PlayerCashChanged -= OnPlayerCashChanged;
			base.OnDisable();
		}

		private void OnCompanyCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount ) {
			UpdateCompanyCash();
		}

		private void OnPlayerCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount) {
			UpdatePlayerCash();
		}

		public override void Update(){
			updateTimer.Update();
		}

		private void UpdateCompanyCash(){
			companyCash1Text.text = 
			companyCash2Text.text = 
				BosUtils.GetCurrencyString(Services.PlayerService.CompanyCash);

		}

		private void UpdatePlayerCash() {
			playerCash1Text.text =
			playerCash2Text.text = 
				BosUtils.GetCurrencyString(Services.PlayerService.PlayerCash);
		}

   //     public override ViewType Type
			//=> ViewType.CashConvertView;

  //      public override CanvasType CanvasType 
		//	=> CanvasType.UI;

		//public override int ViewDepth
		//	=> 13;

    }	


}

