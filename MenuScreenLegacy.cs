using System.Xml;

namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class MenuScreenLegacy : GameBehaviour {

		public Button investorButton;
		public Button upgradeButton;
		public Button leaderboardButton;
		public Button managersButton;
		public Button shopButton;
		
		
		public GameObject upgradeAlertObject;
		public GameObject managerAlertObject;
		public GameObject investorAlertObject;
		public GameObject rewardAlertObject;
		public GameObject shopAlertObject;
		
		
		private  readonly UpdateTimer upgradeTimer = new UpdateTimer();
		private readonly UpdateTimer managerTimer = new UpdateTimer();
		private readonly UpdateTimer investorTimer = new UpdateTimer();

		

		public override void OnEnable() {
			base.OnEnable();
			GameEvents.AvailableRewardsChanged += OnAvailableRewardsChanged;
			GameEvents.CoinsChanged += OnCoinsChanged;
			
			investorButton.SetListener(() => {
				Services.ViewService.ShowDelayed(ViewType.InvestorsView, BosUISettings.Instance.ViewShowDelay);
				Services.SoundService.PlayOneShot(SoundName.click);
			});
			
			upgradeButton.SetListener(() => {
				//Services.LegacyGameUI.ShowUpgrades();
				//Services.SoundService.PlayOneShot(SoundName.click);
			});
			
			leaderboardButton.SetListener(() => {
				Services.LegacyGameManager.GetComponent<UnityLeaderboardMediator>().ShowLeaderboardUI_Score();
				Services.SoundService.PlayOneShot(SoundName.click);
			});
			
			managersButton.SetListener(() => {
				//Services.LegacyGameUI.ShowManagers(0);
				//Services.SoundService.PlayOneShot(SoundName.click);
			});
			
			shopButton.SetListener(() => {
                //Services.LegacyGameUI.ShowIAP();
                //Services.ViewService.ShowDelayed(ViewType.StoreView, BosUISettings.Instance.ViewShowDelay);
				Services.SoundService.PlayOneShot(SoundName.click);
			});
			
			upgradeTimer.Setup(1, (dt) => Services.ViewService.Utils.UpdateUpgradeAlert(upgradeAlertObject), true);
			managerTimer.Setup(1, dt => Services.ViewService.Utils.UpdateManagerAlert(managerAlertObject), true);
			investorTimer.Setup(1, dt => Services.ViewService.Utils.UpdateInvestorAlert(investorAlertObject), true);
			
			//force update
			OnAvailableRewardsChanged(0, 0);
			OnCoinsChanged(0, 0);
		}

		public override void OnDisable() {
			GameEvents.AvailableRewardsChanged -= OnAvailableRewardsChanged;
			GameEvents.CoinsChanged -= OnCoinsChanged;
			base.OnDisable();
		}

		public override void Update() {
			base.Update();
			upgradeTimer.Update();
			managerTimer.Update();
			investorTimer.Update();
		}

		private void OnAvailableRewardsChanged(int oldCount, int newCount)
			=> Services.ViewService.Utils.UpdateRewardAlert(rewardAlertObject);

		private void OnCoinsChanged(int oldCount, int newCount)
			=> Services.ViewService.Utils.UpdateShopAlert(shopAlertObject);
		
		
	}	
}

