namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ManagementView : TypedViewWithCloseButton, IManagementView {

        public BaseManagerView baseManagerView;
        public Toggle officeToggle;
        public Toggle reportsToggle;
        public Toggle garageToggle;
        public Image accountingIconImage;
        public Image garageIconImage;
        public GameObject managerViewPrefab;
        public GameObject garageViewPrefab;
        public GameObject secretaryViewPrefab;

        public ReportsViewUnavailable reportsViewUnavailable;

        public GarageViewUnavailable garageViewUnavailable;
        public GameObject statics;


        public Sprite activeAccountingIconSprite;
        public Sprite notActiveAccountingIconSprite;
        public Sprite activeGarageIconSprite;
        public Sprite notActiveGarageIconSprite;
        public Image marsImage;
        public Image moonImage;
        public float swipeDistance = 300;
        public float swipeSpeed = 300;
        public float swipeMult = 1.5f;
        public ManagementSwipeController managerSwipeController;
        public ManagementSwipeController secretarySwipeController;
        public ManagementSwipeController mechanicSwipeController;

        public Color32 EnableTabColor, DisableTabColor;
        public Text OfficeText, AccauntText, GarageText;
        public ReportsToggle reportsAlert;
        public MechanicsToggle mechanicsAlert;
        
        private const int kMinManagerId = 0;
        //private const int kMaxManagerId = 9;

        private int managerId = 0;
        private ActiveTab activeTab = ActiveTab.Office;

        public Button rightManagerButton;
        public Button leftManagerButton;

        public ActiveTab CurrentActiveTab => activeTab;
        public int ManagerId => managerId;

        private void UpdateLeftRightManagersButton(int managerId) {
            switch(activeTab) {
                case ActiveTab.Office: {
                        DefaultLeftRightManagerButtonBehaviour(managerId);
                    }
                    break;
                case ActiveTab.Accaunt: {
                        if(Planets.IsMoonOpened ) {
                            DefaultLeftRightManagerButtonBehaviour(managerId);
                        } else {
                            rightManagerButton.Deactivate();
                            leftManagerButton.Deactivate();
                        }
                    }
                    break;
                case ActiveTab.Garage: {
                        if(Planets.IsMarsOpened ) {
                            DefaultLeftRightManagerButtonBehaviour(managerId);
                        } else {
                            rightManagerButton.Deactivate();
                            leftManagerButton.Deactivate();
                        }
                    }
                    break;

            }
        }

        private void DefaultLeftRightManagerButtonBehaviour(int managerId) {
            if (HasRightManager(managerId)) {
                rightManagerButton.Activate();
            } else {
                rightManagerButton.Deactivate();
            }
            if (HasLeftManager(managerId)) {
                leftManagerButton.Activate();
            } else {
                leftManagerButton.Deactivate();
            }
        }

        #region IManagementView
        private int MaxManagerId
            => (Planets.IsMarsOpened) ? 9 : 8;

        public bool HasRightManager(int managerId)
            => true; //managerId + 1 <= MaxManagerId;

        public bool HasLeftManager(int managerId)
            => true; //managerId - 1 >= kMinManagerId;

        public int RightManagerId(int managerId) {
            if(managerId < MaxManagerId ) {
                return managerId + 1;
            } else {
                return 0;
            }
        }

        public int LeftManagerId(int managerId) {
            if(managerId > kMinManagerId ) {
                return managerId - 1;
            } else {
                return MaxManagerId;
            }
        }

        public void InitializeView(GameObject obj, int managerId ) {
            switch(activeTab) {
                case ActiveTab.Office: {
                        obj.GetComponentInChildren<ManagerView>().Setup(managerId);
                    }
                    break;
                case ActiveTab.Accaunt: {
                        obj.GetComponentInChildren<ReportsView>().Setup(managerId);
                    }
                    break;
                case ActiveTab.Garage: {
                        obj.GetComponent<GarageView>().Setup(managerId);
                    }
                    break;
            }
        }

        public float DragMult
            => swipeMult;

        public float TransitionSpeed
            => swipeSpeed;

        public float TransitionDistance
            => swipeDistance;

        public void OnTransitionCompletedIn(int newManagerId) {
            UpdateView(newManagerId);
            UpdateLeftRightManagersButton(newManagerId);
        }

        #endregion

        private readonly Dictionary<ActiveTab, ManagementSwipeController> swipeControllers = new Dictionary<ActiveTab, ManagementSwipeController>();

        private ManagementSwipeController GetSwipeController(ActiveTab tab) {
            if(swipeControllers.Count == 0) {
                swipeControllers.Add(ActiveTab.Accaunt, secretarySwipeController);
                swipeControllers.Add(ActiveTab.Garage, mechanicSwipeController);
                swipeControllers.Add(ActiveTab.Office, managerSwipeController);
            }
            return swipeControllers[tab];
        }

        private int DeactivateSwipeController() {
            return GetSwipeController(activeTab).DeactivateController();
        }

        private void ActivateSwipeController() {
            GetSwipeController(activeTab).ActivateController(managerId, this);
            UpdateView(managerId);
        }

        private void DeactivateAndChangeManager() {
            int newManagerId = DeactivateSwipeController();
            if (newManagerId >= 0) {
                managerId = newManagerId;
            } 
        }

        public override ViewType Type => ViewType.ManagementView;

        public override CanvasType CanvasType => CanvasType.UI;

  
        public override bool IsModal => true;

        private void UpdateView(int newManagerId) {
            ManagerInfo targetManager = Services.ManagerService.GetManager(newManagerId);
            if (targetManager != null) {
                baseManagerView.Activate();
                baseManagerView.Setup(targetManager);
            } else {
                baseManagerView.Deactivate();
            }

            UpdateMiscImages();

            statics.GetComponent<RectTransform>().SetAsLastSibling();
            reportsAlert.Setup(newManagerId);
            mechanicsAlert.Setup(newManagerId);
        }

        public override void Setup(ViewData data) {
            base.Setup(data);
            this.managerId = (int)data.UserData;

            reportsAlert.Setup(managerId);
            mechanicsAlert.Setup(managerId);

            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.ManagementViewOpenedForManager, managerId));

            ActivateSwipeController();

            officeToggle.SetListener((isOn) => {
                if(isOn) {
                    DeactivateAndChangeManager();
                    activeTab = ActiveTab.Office;
                    ActivateSwipeController();
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                    UpdateLeftRightManagersButton(managerId);
                }

                OfficeText.color = isOn ? EnableTabColor : DisableTabColor;
                
            });

            garageToggle.SetListener((isOn) => {
                if (isOn) {
                    if (Planets.IsOpened(PlanetConst.MARS_ID)) {
                        DeactivateAndChangeManager();
                        garageViewUnavailable.Deactivate();
                        activeTab = ActiveTab.Garage;
                        ActivateSwipeController();
                        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.MechanicOpened));
                    } else {
                        DeactivateAndChangeManager();
                        activeTab = ActiveTab.Garage;
                        garageViewUnavailable.Activate();
                    }
                    garageIconImage.overrideSprite = activeGarageIconSprite;
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                    UpdateLeftRightManagersButton(managerId);
                } else {
                    garageIconImage.overrideSprite = notActiveGarageIconSprite;
                    garageViewUnavailable.Deactivate();
                }
                
                GarageText.color = isOn ? EnableTabColor : DisableTabColor;
            });

            reportsToggle.SetListener((isOn) => {
                if (isOn) {
                    IPlanetService planetService = Services.GetService<IPlanetService>();
                    if (planetService.IsOpened(PlanetConst.MOON_ID)) {
                        DeactivateAndChangeManager();
                        reportsViewUnavailable.Deactivate();
                        activeTab = ActiveTab.Accaunt;
                        ActivateSwipeController();
                        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.SecretariesOpened));
                        
                    } else {
                        DeactivateAndChangeManager();
                        activeTab = ActiveTab.Accaunt;
                        reportsViewUnavailable.Activate();
                    }

                    
                    
                    accountingIconImage.overrideSprite = activeAccountingIconSprite;
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                    UpdateLeftRightManagersButton(managerId);
                } else {
                    reportsViewUnavailable.Deactivate();
                    accountingIconImage.overrideSprite = notActiveAccountingIconSprite;
                }
                
                AccauntText.color = isOn ? EnableTabColor : DisableTabColor;
            });

            closeButton.SetListener(() =>  { 
                Services.ViewService.Remove(ViewType.ManagementView, 0.2f);
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
             });

            garageViewUnavailable.Deactivate();
            reportsViewUnavailable.Deactivate();

            UpdateView(managerId);
            UpdateLeftRightManagersButton(managerId);
            leftManagerButton.SetListener(() => {
                var swipeControl = GetSwipeController(activeTab);
                if (!swipeControl.IsTransitionStarted) {
                    swipeControl.MakeTransitionProgrammatically(SwipeResultAction.MoveToRight);
                    leftManagerButton.SetInteractable(false);
                    rightManagerButton.SetInteractable(false);
                    Sounds.PlayOneShot(SoundName.panel_slide);
                }
            });
            rightManagerButton.SetListener(() => {
                var swipeControl = GetSwipeController(activeTab);
                if (!swipeControl.IsTransitionStarted) {
                    swipeControl.MakeTransitionProgrammatically(SwipeResultAction.MoveToLeft);
                    leftManagerButton.SetInteractable(false);
                    rightManagerButton.SetInteractable(false);
                    Sounds.PlayOneShot(SoundName.panel_slide);
                }
            });
        }

        private void UpdateMiscImages() {
            IPlanetService planetService = Services.GetService<IPlanetService>();
            if (planetService.IsOpened(PlanetConst.MARS_ID)) {
                marsImage.Deactivate();
            } else {
                marsImage.Activate();
            }

            if (planetService.IsOpened(PlanetConst.MOON_ID)) {
                moonImage.Deactivate();
            } else {
                moonImage.Activate();
            }
        }

        public override void OnEnable() {
            base.OnEnable();

            GameEvents.CurrentPlanetChanged += OnPlanetChanged;
            ManagementSwipeController.TransitionStartedChanged += OnTransitionStateChanged;

        }

        public override void OnDisable() {
            GameEvents.CurrentPlanetChanged -= OnPlanetChanged;
            ManagementSwipeController.TransitionStartedChanged -= OnTransitionStateChanged;


            base.OnDisable();
        }

        private void OnTransitionStateChanged(bool isTransitionStarted) {
            UpdateLeftRightButtonStates(isTransitionStarted);
        }

        private void UpdateLeftRightButtonStates(bool isTranstionStarted) {
            if(isTranstionStarted) {
                leftManagerButton.SetInteractable(false);
                rightManagerButton.SetInteractable(false);
            } else {
                leftManagerButton.SetInteractable(true);
                rightManagerButton.SetInteractable(true);
            }
        }

        private void OnPlanetChanged(PlanetInfo oldPlanet, PlanetInfo newPlanet) {
            UpdateMiscImages();
        }

        public enum SwipeResultAction {
            None,
            ReturnToCenter,
            MoveToRight,
            MoveToLeft
        }

        public enum ActiveTab {
            Office,
            Accaunt,
            Garage
        }
    }


}