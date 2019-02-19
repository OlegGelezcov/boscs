using System.Collections;

namespace Bos.UI {
    using System;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    public class MenuFooterView : TypedView {

        public Button bankButton;
        public GameObject bankAlertObject;
        public GameObject bankAlertParticles;
        public GameObject bankClosedAlert;
        public Sprite BankSimpleSprite, BankFullSprite;
        public Image BankIcon;
               
        public Button miniGamesButton;
        public GameObject prizeWheelAlertObject;
        public GameObject treasureHuntAlertObject;
        public GameObject splitAlertObject;
        
        public Button empireButton;
        public GameObject upgradeAlertObject;
        public GameObject managerAlertObject;
        public GameObject rewardAlertObject;
        public GameObject investorAlertObject;

        public Button shopButton;
        public GameObject alertShopObject;

        public override CanvasType CanvasType
            => CanvasType.UI;

        public override bool IsModal
            => false;

        public override ViewType Type
            => ViewType.MenuFooterView;

        public override int ViewDepth
            => 120;

        private  readonly UpdateTimer upgradeAlertTimer = new UpdateTimer();
        private readonly UpdateTimer managerAlertTimer = new UpdateTimer();
        private readonly UpdateTimer investorAlertTimer = new UpdateTimer();
        private readonly UpdateTimer visibilityTimer = new UpdateTimer();

        private bool isHided = true;
        private bool isAnimating = false;

        private bool isInitialized = false;

        private float _botOffset => PlatformUtils.IsPhoneXResolution() ? 110 : 92;

        public override void Setup(ViewData data) {
            base.Setup(data);
            bankButton.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.ShowDelayed(ViewType.BankView, BosUISettings.Instance.ViewShowDelay);
            });
            miniGamesButton.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.Show(ViewType.MiniGameView);
            });
            empireButton.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.Show(ViewType.MainView);
                //FindObjectOfType<GameUI>()?.ToggleMenu();
            });
            shopButton.SetListener(() => {
                //Services.ViewService.ShowDelayed(ViewType.StoreView, BosUISettings.Instance.ViewShowDelay);
                Services.ViewService.Show(ViewType.UpgradesView, new ViewData {UserData = new UpgradeViewData {  TabName = UpgradeTabName.Shop, StoreSection = StoreItemSection.CompanyCash } });
                Services.SoundService.PlayOneShot(SoundName.click);
                //FindObjectOfType<GameUI>()?.ShowIAP();
            });

            UpdateBankButtonView();
            UpdatePrizeWheelAlertObject();
            UpdateTreasureHuntAlertObject();
            UpdateSplitAlertObject();
            UpdateUpgradeAlertObject();
            UpdateManagerAlert();
            UpdateAvailableRewardAlertObject();
            UpdateInvestorAlertObject();
            UpdateAlertShopObject();

            upgradeAlertTimer.Setup(3.0f, (dt) => UpdateUpgradeAlertObject(), true);
            managerAlertTimer.Setup(3.0f, dt => UpdateManagerAlert(), true);
            investorAlertTimer.Setup(3.0f, dt => UpdateInvestorAlertObject(), true);
            StartCoroutine(UnhideImpl());

            IViewService viewService = Services.ViewService;
            visibilityTimer.Setup(.5f, dt => {
                if (Services.GameModeService.GameModeName == GameModeName.Game) {
                    if (isHided) {
                        if (viewService.LegacyCount == 0 && viewService.ModalCount == 0) {
                            if (!isAnimating) {
                                Unhide();
                            }
                        }
                    }
                    else {
                        if (viewService.LegacyCount != 0 || viewService.ModalCount != 0) {
                            if (!isAnimating) {
                                Hide();
                            }
                        }
                    }
                }
            });

            if(!isInitialized) {
                Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(_ => {
                    UpdateSplitAlertObject();
                    UpdateBankAlert();
                }).AddTo(gameObject);
                
                isInitialized = true;
            }
        }

        public override void AnimOut() {
            base.AnimOut();
            Hide();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.BankAccumulatedCoinsChanged += OnBankAccumulatedCoinsChanged;
            GameEvents.BankLevelChanged += OnBankLevelChanged;
            GameEvents.PrizeWheelTriesChanged += OnPrizeWheelTriesCountChanged;
            GameEvents.TreasureHuntTriesChanged += OnTreasureHuntTriesCountChanged;
            GameEvents.SplitTriesChanged += OnSplitTriesCountChanged;
            GameEvents.AvailableRewardsChanged += OnAvailableRewardsChaned;
            GameEvents.CoinsChanged += OnCoinsChanged;
            GameEvents.ViewShowed += OnViewAdded;
            GameEvents.ViewHided += OnViewRemoved;
            GameEvents.LegacyViewAdded += OnLegacyViewAdded;
            GameEvents.LegacyViewRemoved += OnLegacyViewRemoved;
        }

        public override void OnDisable() {
            GameEvents.BankAccumulatedCoinsChanged -= OnBankAccumulatedCoinsChanged;
            GameEvents.BankLevelChanged -= OnBankLevelChanged;
            GameEvents.PrizeWheelTriesChanged -= OnPrizeWheelTriesCountChanged;
            GameEvents.TreasureHuntTriesChanged -= OnTreasureHuntTriesCountChanged;
            GameEvents.SplitTriesChanged -= OnSplitTriesCountChanged;
            GameEvents.AvailableRewardsChanged -= OnAvailableRewardsChaned;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            GameEvents.ViewShowed -= OnViewAdded;
            GameEvents.ViewHided -= OnViewRemoved;
            GameEvents.LegacyViewAdded -= OnLegacyViewAdded;
            GameEvents.LegacyViewRemoved -= OnLegacyViewRemoved;
            base.OnDisable();
        }

        private void OnLegacyViewRemoved(string viewName) {
            if (Services.ViewService.LegacyCount == 0 && Services.ViewService.ModalCount == 0) {
                StartCoroutine(UnhideImpl());
            }
        }

        private void OnLegacyViewAdded(string viewName) {
            if (Services.ViewService.LegacyCount > 0) {
                Hide();
            }
        }

        private void OnViewAdded(ViewType viewType) {
            if (Services.ViewService.ModalCount > 0) {
                Hide();
            }
        }

        private void OnViewRemoved(ViewType viewType) {
            if (Services.ViewService.ModalCount == 0 && Services.ViewService.LegacyCount == 0) {
                StartCoroutine(UnhideImpl());
            }
        }

        private IEnumerator UnhideImpl() {
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !isAnimating);
            if (Services.ViewService.LegacyCount == 0 && Services.ViewService.ModalCount == 0) {
                Unhide();
            } 
        }

        public override void Update() {
            base.Update();
            upgradeAlertTimer.Update();
            managerAlertTimer.Update();
            investorAlertTimer.Update();
            visibilityTimer.Update();
            UpdateIcons();
        }


        private bool _oldBankState;
        private void UpdateIcons()
        {
            if (_oldBankState != Services.BankService.IsFull)
            {
                BankIcon.overrideSprite = Services.BankService.IsFull ? BankFullSprite : BankSimpleSprite;
                _oldBankState = BankIcon.overrideSprite;
            }
               
        }
        
        private void Hide() {
            if (!isHided) {
                isAnimating = true;
                isHided = true;
                RectTransform trs = GetComponent<RectTransform>();
                Vector2Animator animator = GetComponent<Vector2Animator>();
                animator.StartAnimation(new Vector2AnimationData {
                    StartValue = new Vector2(0, _botOffset),
                    EndValue = new Vector2(0, -145),
                    Duration = 0.3f,
                    AnimationMode = BosAnimationMode.Single,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = gameObject,
                    OnStart = (p, o) => {
                        
                        trs.anchoredPosition = p;
                        
                    },
                    OnUpdate = (p, t, o) => trs.anchoredPosition = p,
                    OnEnd = (p, o) => {
                        trs.anchoredPosition = p;
                        bankAlertParticles.Deactivate();
                        isAnimating = false;
                        trs.anchoredPosition = new Vector2(0, -5000);
                    }
                });
            }        
        }

        private void Unhide() {
            if (isHided) {
                isAnimating = true;
                isHided = false;
                RectTransform trs = GetComponent<RectTransform>();
                Vector2Animator animator = GetComponent<Vector2Animator>();
                animator.StartAnimation(new Vector2AnimationData {
                    StartValue = new Vector2(0, -145),
                    EndValue = new Vector2(0, _botOffset),
                    Duration = 0.3f,
                    AnimationMode = BosAnimationMode.Single,
                     EaseType = EaseType.EaseInOutQuad,
                    Target = gameObject,
                    OnStart = (p, o) => trs.anchoredPosition = p,
                    OnUpdate = (p, t, o) => trs.anchoredPosition = p,
                    OnEnd = (p, o) => {
                        trs.anchoredPosition = p;
                        bankAlertParticles.Activate();
                        isAnimating = false;
                    }
                });
            }
        }

        private void UpdateBankButtonView() {
            var bankService = Services.BankService;


            if (bankService.IsOpened && bankService.HasAccumulatedCoins && !bankService.IsFull) {
                bankAlertObject.Activate();
            }
            else {
                bankAlertObject.Deactivate();
            }


            /*
            if (bankService.IsOpened && bankService.IsFull)
            {
                bankClosedAlert.Activate();
            }
            else
            {
                bankClosedAlert.Deactivate();
            }*/
            UpdateBankAlert();
        }

        /// <summary>
        /// Alert is visible when bank  opened and full or when bank upgrade available
        /// </summary>
        private void UpdateBankAlert() {

            IBankService bankService = Services.BankService;

            if(bankService.IsOpened && bankService.IsFull ) {
                bankClosedAlert.Activate();
            } else {
                bankClosedAlert.Deactivate();
            }
            /*
            if(bankService.IsOpened ) {
                if(bankService.IsFull) {
                    bankClosedAlert.Activate();
                } else if(bankService.IsAvailableNextLevel ) {
                    bankClosedAlert.Activate();
                } else {
                    bankClosedAlert.Deactivate();
                }
            } else {
                if(bankService.IsAvailableNextLevel ) {
                    bankClosedAlert.Activate();
                } else {
                    bankClosedAlert.Deactivate();
                }
            }*/

        }


        private void UpdatePrizeWheelAlertObject() {
            if (Services.PrizeWheelService.HasTries) {
                prizeWheelAlertObject.Activate();
            }
            else {
                prizeWheelAlertObject.Deactivate();
            }
        }
        
        private void UpdateTreasureHuntAlertObject() {
            if (Services.TreasureHuntService.HasTries) {
                treasureHuntAlertObject.Activate();
            }
            else {
                treasureHuntAlertObject.Deactivate();
            }
        }

        private void UpdateSplitAlertObject() {
            if (Services.SplitService.HasTries && Services.PlanetService.IsMoonOpened) {
                splitAlertObject.Activate();
            }
            else {
                splitAlertObject.Deactivate();
            }
        }

        private void UpdateUpgradeAlertObject() {
            Services.ViewService.Utils.UpdateUpgradeAlert(upgradeAlertObject);
        }

        private void UpdateManagerAlert() {
            Services.ViewService.Utils.UpdateManagerAlert(managerAlertObject);
        }

        private void UpdateAvailableRewardAlertObject() {
            Services.ViewService.Utils.UpdateRewardAlert(rewardAlertObject);
        }

        private void UpdateInvestorAlertObject() {
            
            Services.ViewService.Utils.UpdateInvestorAlert(investorAlertObject);
           
        }

        private void UpdateAlertShopObject() {
            Services.ViewService.Utils.UpdateShopAlert(alertShopObject);
        }

        private void OnBankAccumulatedCoinsChanged(int oldCoins, int newCoins)
            => UpdateBankButtonView();

        private void OnBankLevelChanged(int oldLevel, int newLevel)
            => UpdateBankButtonView();

        private void OnPrizeWheelTriesCountChanged(int oldCount, int newCount)
            => UpdatePrizeWheelAlertObject();        
        
        private void OnTreasureHuntTriesCountChanged(int oldCount, int newCount)
            => UpdateTreasureHuntAlertObject();

        private void OnSplitTriesCountChanged(int oldCount, int newCount)
            => UpdateSplitAlertObject();

        private void OnAvailableRewardsChaned(int oldCount, int newCount)
            => UpdateAvailableRewardAlertObject();

        private void OnCoinsChanged(int oldCount, int newCount)
            => UpdateAlertShopObject();
    }


}