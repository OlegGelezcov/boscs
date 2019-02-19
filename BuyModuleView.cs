namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using Ozh.Tools.Functional;

    public class BuyModuleView : TypedViewWithCloseButton {

        public Image[] dots;
        public Sprite darkDot;
        public Sprite lightDot;

        public SingleModuleView moduleView;
        public GameObject statics;
        public float swipeDistance = 300;
        public CompletedModulesView completedView;
        public ModuleFlightView flightView;
        public GameObject normalBackground;
        public TMPro.TextMeshProUGUI titleText;


        private ManagementView.SwipeResultAction currentSwipeAction = ManagementView.SwipeResultAction.None;


        public override ViewType Type 
            => ViewType.BuyModuleView;
        public override CanvasType CanvasType 
            => CanvasType.UI;
        public override bool IsModal 
            => true;

        private bool isSwipeEnabled = true;
        private bool isSwiped = false;
        private const int kMinModuleId = 0;
        private const int kMaxModuleId = 5;
        private ShipModuleInfo currentModule;
        private Vector2 startSwipePoint;
        private Vector2 endSwipePoint;

        private bool HasRightModule
            => currentModule?.Id + 1 <= kMaxModuleId;

        private bool HasLeftModule
            => currentModule?.Id - 1 >= kMinModuleId;

        private ModuleViewModel Model { get; set; }
        private bool IsInitialized { get; set; }




        public override void Setup(ViewData data) {
            base.Setup(data);

            Model = ExtractModel(data);

            if (Model.ScreenType == ModuleScreenType.Normal)
            {
                SetupNormalView();
            } else
            {
                SetupFlightView();
            }

            closeButton.SetListener(() => {
                closeButton.interactable = false;
                Services.ViewService.Remove(ViewType.BuyModuleView, 0.15f);
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });

            if(!IsInitialized ) {
                GameEvents.PlanetStateChangedObservable.Value.Subscribe(args => {
                    Planets.GetOpeningPlanet().Match(() => {
                        Setup(new ViewData { UserData = new ModuleViewModel { ScreenType = ModuleScreenType.Normal, ModuleId = currentModule?.Id ?? 0 } });
                        return F.None;
                    }, p => {
                        Setup(new ViewData { UserData = new ModuleViewModel { ScreenType = ModuleScreenType.Flight } });
                        return F.Some(p);
                    });
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        private ModuleViewModel ExtractModel(ViewData data) {
            if (data == null) {
                data = new ViewData {
                    UserData = new ModuleViewModel {
                        ScreenType = ModuleScreenType.Normal,
                        ModuleId = 0
                    }
                };
            }

            ModuleViewModel model = data.UserData as ModuleViewModel;
            Debug.Log($"Setup modules with model: {model.ScreenType}");
            return model;
        }

        private void SetupNormalView() {
            titleText.text = LocalizationObj.GetString("lbl_modules_title");
            normalBackground.Activate();
            flightView.Deactivate();
            UpdateCompletedState();
            dots.ToggleActivity(true);

            if (isSwipeEnabled) {
                this.currentModule = Services.GetService<IShipModuleService>().GetModule((int)Model.ModuleId);
                EnableDot((int)Model.ModuleId);
                moduleView.Setup(currentModule);
            } else {
                moduleView?.Deactivate();
                dots.ToggleActivity(false);

            }
        }

        private void SetupFlightView() {
            normalBackground.Deactivate();
            moduleView.Deactivate();
            completedView.Deactivate();
            isSwipeEnabled = false;
            dots.ToggleActivity(false);

            flightView.Activate();
            flightView.Setup();
        }

        private void UpdateCompletedState() {
            if (Services.Modules.IsAllModulesOpened) {
                moduleView?.Deactivate();
                completedView.Activate();
                isSwipeEnabled = false;
            } else {
                moduleView?.Activate();
                completedView.Deactivate();
                isSwipeEnabled = true;
            }
        }

        private void EnableDot(int index) {
           for(int i = 0; i < dots.Length; i++ ) {
                if(index == i ) {
                    dots[i].overrideSprite = lightDot;
                    dots[i].GetComponent<ModuleDot>().Select();
                } else {
                    dots[i].overrideSprite = darkDot;
                    dots[i].GetComponent<ModuleDot>().Unselect();
                }
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            SwipeDetector.OnSwipe += OnSwipe;
            GameEvents.ShipModuleStateChanged += OnShipModuleStateChanged;
            StartCoroutine(AutoSwipeToLastBuyed());
        }


        private IEnumerator AutoSwipeToLastBuyed()
        {
            yield return new WaitUntil(() => Model != null);
            yield return new WaitForSeconds(0.3f);
            if(Model.ScreenType == ModuleScreenType.Flight )
            {
                yield break;
            }

            int index = 0;
            for (int i = 1; i <= kMaxModuleId; i++)
            {
                var module = Services.GetService<IShipModuleService>().GetModule(i);
                if (module.State != ShipModuleState.Opened)
                {
                    index = i - 1;
                    break;
                }

                if (i == kMaxModuleId && module.State == ShipModuleState.Opened)
                {
                    index = kMaxModuleId;
                    break;
                }
            }
            
            Setup(new ViewData {
                UserData = new ModuleViewModel
                {
                    ScreenType = ModuleScreenType.Normal,
                    ModuleId = index
                }
            });
        }

        public override void OnDisable() {
            SwipeDetector.OnSwipe -= OnSwipe;
            GameEvents.ShipModuleStateChanged -= OnShipModuleStateChanged;
            base.OnDisable();
        }

        private void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo module) {
            if (Model != null && Model.ScreenType == ModuleScreenType.Normal)
            {
                UpdateCompletedState();
            }
        }


        private float GetNormalizedOffset(float offset) {
            return Mathf.Clamp01(Mathf.Abs(offset) / (Screen.width * .5f));
        }

        private bool IsModelValidForSwipe
            => Model != null && Model.ScreenType == ModuleScreenType.Normal;

        private void OnSwipe(SwipeData data ) {
            if(isSwipeEnabled && IsModelValidForSwipe) {
                if(!data.IsEnd) {
                    if(!isSwiped ) {
                        isSwiped = true;
                        startSwipePoint = data.StartPosition;
                        //Debug.Log("start wait..");
                    }  else {
                        //Debug.Log("swipe continue..");
                        Vector2 currentPosition = data.EndPosition;
                        float delta = currentPosition.x - startSwipePoint.x;
                        float sign = Mathf.Sign(delta);
                        if(sign > 0) {
                            moduleView.LerpPart(currentModule.Id, GetNormalizedOffset(delta));
                        } else if(sign < 0) {
                            moduleView.LerpPart(currentModule.Id + 1, GetNormalizedOffset(delta));
                        }
                    }
                } else {
                    //Debug.Log("end swipe");
                    isSwiped = false;
                    endSwipePoint = data.EndPosition;
                    Vector2 offset = endSwipePoint - startSwipePoint;
                    if(offset.x > swipeDistance ) {
                        if (currentModule.Id == 0) {
                            currentSwipeAction = ManagementView.SwipeResultAction.ReturnToCenter;
                        } else {
                            currentSwipeAction = ManagementView.SwipeResultAction.MoveToRight;
                        }
                    } else if(offset.x < -swipeDistance ) {
                        currentSwipeAction = ManagementView.SwipeResultAction.MoveToLeft;
                    } else {
                        currentSwipeAction = ManagementView.SwipeResultAction.ReturnToCenter;
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (IsModelValidForSwipe)
            {
                if (isSwipeEnabled)
                {
                    if (!isSwiped)
                    {
                        if (currentSwipeAction != ManagementView.SwipeResultAction.None)
                        {
                            Finalize(currentSwipeAction);
                            currentSwipeAction = ManagementView.SwipeResultAction.None;
                        }
                    }
                }
            }
        }

        private void Finalize(ManagementView.SwipeResultAction action) {
            switch (action) {
                case ManagementView.SwipeResultAction.ReturnToCenter: {
                        moduleView.Setup(currentModule);
                    }
                    break;
                case ManagementView.SwipeResultAction.MoveToLeft: {
                        if(HasRightModule ) {
                            Setup(new ViewData {
                                UserData = new ModuleViewModel
                                {
                                    ScreenType = ModuleScreenType.Normal,
                                    ModuleId = currentModule.Id + 1
                                }
                            });
                        }

                    }
                    break;
                case ManagementView.SwipeResultAction.MoveToRight: {
                        if(HasLeftModule ) {
                            Setup(new ViewData {
                                UserData = new ModuleViewModel
                                {
                                    ScreenType = ModuleScreenType.Normal,
                                    ModuleId = currentModule.Id - 1
                                }
                            });
                        }
                    }
                    break;
            }
        }

    }

    public class ModuleViewModel
    {
        public ModuleScreenType ScreenType { get; set; }
        public int ModuleId { get; set; }
    }

    public enum ModuleScreenType
    {
        Normal,
        Flight
    }

    public class ModuleViewCache {
        private GameObject prefab;
        private Transform parent;

        private readonly Stack<GameObject> cache = new Stack<GameObject>();

        public void Setup(Transform parent, GameObject prefab) {
            this.prefab = prefab;
            this.parent = parent;
        }

        public GameObject PopObject() {
            if(cache.Count == 0 ) {
                GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                instance.transform.SetParent(parent, false);
                return instance;
            } else {
                GameObject instance = cache.Pop();
                instance.Activate();
                return instance;
            }
        }

        public void PushObject(GameObject instance) {
            instance.Deactivate();
            cache.Push(instance);
            instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(40000, 0);
        }

        public void Clear() {
            while(cache.Count > 0 ) {
                GameObject obj = cache.Pop();
                if(obj != null && obj ) {
                    GameObject.Destroy(obj);
                }
            }
        }
    }

}