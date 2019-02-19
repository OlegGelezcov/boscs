namespace Bos.UI {
    using UnityEngine;
    using UnityEngine.UI;

    public class MainView : TypedViewWithCloseButton {

        public Toggle soundToggle;
        //public Button helpButton;
        public Toggle helpToggle;
        public Button achievmentsButton;

        public Button profileButton;
        public Button leaderBoardButton;
        public Button managersButton;
        public Button investorButton;
        public Button rewardsButton;
        public Button spaceshipButton;
        public Button upgradeButton;
        public Button shopButton;
        public Button socialButton;
        public Button adButton;
          


        public GameObject managerAlertObject;
        public GameObject investorAlertObject;
        public GameObject rewardAlertObject;
        public GameObject upgradeAlertObject;
        public GameObject shopAlertObject;
        private readonly UpdateTimer managerTimer = new UpdateTimer();
        private readonly UpdateTimer investorTimer = new UpdateTimer();
        private readonly UpdateTimer rewardButtonTimer = new UpdateTimer();
        private readonly UpdateTimer upgradeTimer = new UpdateTimer();

        #region TypedView overrides
        public override ViewType Type => ViewType.MainView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override void Setup(ViewData data) {
            base.Setup(data);
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.MainView, BosUISettings.Instance.ViewCloseDelay);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            soundToggle.onValueChanged.RemoveAllListeners();
            soundToggle.isOn = Services.SoundService.IsMute;
            soundToggle.SetListener(isOn => {
                Services.SoundService.SetMute(isOn);
            });

            helpToggle.onValueChanged.RemoveAllListeners();
            helpToggle.isOn = Services.TutorialService.IsPaused;
            helpToggle.SetListener(isOn => {
                Services.TutorialService.SetPaused(isOn);
                Sounds.PlayOneShot(SoundName.click);
            });

            /*
            helpButton.SetListener(() => {
                Services.ViewService.Show(ViewType.HelpView, new ViewData {
                    ViewDepth = ViewDepth + 1
                });
                Services.SoundService.PlayOneShot(SoundName.click);
            });*/

            achievmentsButton.SetListener(() => {
                //FindObjectOfType<StoreAchievement>()?.ShowAchievementUI();  
                Services.AchievmentService.ShowAchievementUI();
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            profileButton.SetListener(() => {
                Services.ViewService.Show(ViewType.ProfileView, new ViewData { ViewDepth = ViewDepth + 1, UserData = ProfileViewTab.Office  });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            leaderBoardButton.SetListener(() => {
                Services.LegacyGameManager.GetComponent<UnityLeaderboardMediator>().ShowLeaderboardUI_Score();
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            managersButton.SetListener(() => {
                Services.ViewService.Show(ViewType.ManagersView, new ViewData { UserData = 0, ViewDepth = ViewDepth + 1 });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            investorButton.SetListener(() => {
                Services.ViewService.ShowDelayed(ViewType.InvestorsView, BosUISettings.Instance.ViewShowDelay, new ViewData { ViewDepth = ViewDepth + 1});
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            rewardsButton.SetListener(() => {
                Services.ViewService.Show(ViewType.RewardsView, new ViewData { ViewDepth = ViewDepth + 1 });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            spaceshipButton.SetListener(() => {
                Services.ViewService.ShowDelayed(ViewType.BuyModuleView, BosUISettings.Instance.ViewShowDelay,  new ViewData { ViewDepth = ViewDepth + 1, UserData = new ModuleViewModel {
                    ScreenType = ModuleScreenType.Normal,
                    ModuleId = 0
                } });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            upgradeButton.SetListener(() => {
                Services.ViewService.Show(ViewType.UpgradesView, new ViewData { ViewDepth = ViewDepth + 1 });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            shopButton.SetListener(() => {
                Services.ViewService.ShowDelayed(ViewType.UpgradesView, BosUISettings.Instance.ViewShowDelay, 
                    new ViewData {  ViewDepth = ViewDepth + 1,
                        UserData = new UpgradeViewData {
                            TabName = UpgradeTabName.Shop,
                            StoreSection = StoreItemSection.CompanyCash} });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            socialButton.SetListener(() => {
                Services.ViewService.Show(ViewType.SocialView, new ViewData { ViewDepth = ViewDepth + 1 });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            adButton.SetListener(() => {
                //Services.LegacyGameUI.ShowWatchAd();
                ViewService.Show(ViewType.X2ProfitView, new ViewData {
                     ViewDepth = ViewDepth + 1
                });
                Services.SoundService.PlayOneShot(SoundName.click);
            });
            managerTimer.Setup(1, dt => Services.ViewService.Utils.UpdateManagerAlert(managerAlertObject), true);
            investorTimer.Setup(1, dt => Services.ViewService.Utils.UpdateInvestorAlert(investorAlertObject), true);
            rewardButtonTimer.Setup(0.5f, (dt) => rewardsButton.interactable = (Services.RewardsService.AvailableRewards > 0));
            upgradeTimer.Setup(1, (dt) => Services.ViewService.Utils.UpdateUpgradeAlert(upgradeAlertObject), true);
            OnAvailableRewardsChanged(0, 0);
            OnCoinsChanged(0, 0);
        }

        public override int ViewDepth => 4;
        #endregion

        public override void Update() {
            base.Update();
            managerTimer.Update();
            investorTimer.Update();
            rewardButtonTimer.Update();
            upgradeTimer.Update();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.AvailableRewardsChanged += OnAvailableRewardsChanged;
            GameEvents.CoinsChanged += OnCoinsChanged;
        }

        public override void OnDisable() {
            GameEvents.AvailableRewardsChanged -= OnAvailableRewardsChanged;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            base.OnDisable();
        }

        private void OnAvailableRewardsChanged(int oldCount, int newCount)
            => Services.ViewService.Utils.UpdateRewardAlert(rewardAlertObject);

        private void OnCoinsChanged(int oldCount, int newCount)
            => Services.ViewService.Utils.UpdateShopAlert(shopAlertObject);
    }

}