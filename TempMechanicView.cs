namespace Bos.UI {
    using Bos.Data;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using TMPro;
    using UniRx;

    public class TempMechanicView : GameBehaviour {

        private const int kMaxMechanicViewCount = 10;

        public GameObject mechanicPrefab;
        public Button buyButton;
        public Text repairCountText;
        public Text priceText;
        //public Text priceSuffixText;
        public Button adButton;
        public Text adButtonText;
        public RectTransform animationParent;
        //public ProgressShaderController progressController;

        public Image progressImage;
        public CanvasGroup buyButtonTextsGroup;
        public TextMeshProUGUI speedText;

        //public Sprite enabledBuyButtonSprite;
        //public Sprite disabledBuyButtonSprite;
        private bool isEnabledSprite = true;

        private readonly Dictionary<string, MechanicAnimObject> mechanicViews = new Dictionary<string, MechanicAnimObject>();
        private readonly List<string> animatedViews = new List<string>();

        private readonly UpdateTimer updateTimer = new UpdateTimer();
        private readonly UpdateTimer buyButtonStateTimer = new UpdateTimer();


        public override void Start() {
            base.Start();
            UpdateSpeedTextWithValue(Services.TempMechanicService.GetSpeedMult(generator.GeneratorId));
            UniRx.MessageBroker.Default.Receive<SpeedMultChangedArgs>().Where(args => args.Name == "mechanic" && args.GeneratorId == generator.GeneratorId).Subscribe(args => {
                UpdateSpeedTextWithValue(args.SpeedModifier.GetSpeedMult(generator.GeneratorId));
            }).AddTo(gameObject);
        }

        private void UpdateSpeedTextWithValue(int val) {
            speedText.text = string.Format(LocalizationObj.GetString("lbl_speed_mult_q"), val);
        }

        private void ClearMechanicViews() {
            foreach(var kvp in mechanicViews) {
                if(kvp.Value && kvp.Value.gameObject) {
                    Destroy(kvp.Value.gameObject);
                }
            }
            mechanicViews.Clear();
        }

        private void SetupTempMechanicsView() {
            ClearMechanicViews();

            var mechanics = Services.TempMechanicService.GetMechanics(generator.GeneratorId);
            List<TempMechanicInfo> notCompletedMechanics = mechanics.Values.Where(m => !m.IsCompleted).ToList();
            for(int i = 0; i < Mathf.Min(kMaxMechanicViewCount, notCompletedMechanics.Count); i++ ) {
                AddTempMechanicView(notCompletedMechanics[i], false);
            }
        }

        private void AddTempMechanicView(TempMechanicInfo mechanic, bool useAnimation) {
            //guard from duplicate animations
            if (!animatedViews.Contains(mechanic.Id)) {
                GameObject mechanicObj = Instantiate<GameObject>(mechanicPrefab);
                mechanicObj.GetComponent<RectTransform>().SetParent(animationParent, false);
                var mechanicViewComponent = mechanicObj.GetComponent<MechanicAnimObject>();
                animatedViews.Add(mechanic.Id);
                if (useAnimation) {
                    AnimateMechanicObject(mechanicViewComponent, mechanic);
                } else {
                    mechanicViewComponent.Setup(mechanic);
                    mechanicViews.Add(mechanicViewComponent.TempMechanic.Id, mechanicViewComponent);
                    animatedViews.Remove(mechanicViewComponent.TempMechanic.Id);
                }
            }
        }

        public void RemoveTempMechanicView(TempMechanicInfo mechanic ) {
            if (mechanicViews.ContainsKey(mechanic.Id)) {
                var view = mechanicViews[mechanic.Id];
                mechanicViews.Remove(mechanic.Id);
                if(view && view.gameObject) {
                    Destroy(view.gameObject);
                }
            }
        }

        private void RemoveAllMechanicViews() {
            List<MechanicAnimObject> views = mechanicViews.Values.ToList();
            foreach(var view in views) {
                if(view && view.gameObject && view.TempMechanic != null ) {
                    RemoveTempMechanicView(view.TempMechanic);
                }
            }
            mechanicViews.Clear();
        }

        private void AddMissedMechanics() {
            if(generator != null && mechanicViews.Count < kMaxMechanicViewCount ) {
                var additionalMechanics = Services.
                    TempMechanicService.
                    GetMechanics(generator.GeneratorId).
                    Where(kvp => !kvp.Value.IsCompleted && !mechanicViews.ContainsKey(kvp.Value.Id)).
                    Select(kvp => kvp.Value).
                    Take(kMaxMechanicViewCount - mechanicViews.Count).
                    ToList();
                foreach(var mechanic in additionalMechanics) {
                    AddTempMechanicView(mechanic, false);
                }
            }
        }
        

        private GeneratorInfo generator;
        private MechanicData mechanicData;

        private readonly UpdateTimer adTimer = new UpdateTimer();

        private void SetBuyButtonSprite(bool value) {
            bool oldValue = isEnabledSprite;
            isEnabledSprite = value;
            if (isEnabledSprite != oldValue) {
                if (isEnabledSprite) {
                    //buyButton.GetComponent<Image>().sprite = enabledBuyButtonSprite;
                    //buyButton.GetComponent<Image>().overrideSprite = enabledBuyButtonSprite;
                } else {
                    //buyButton.GetComponent<Image>().sprite = disabledBuyButtonSprite;
                    //buyButton.GetComponent<Image>().overrideSprite = disabledBuyButtonSprite;
                }
            }
        }

        public void Setup(GeneratorInfo generator) {
            this.generator = generator;
            this.mechanicData = Services.ResourceService.MechanicDataRepository.GetMechanicData(Services.PlanetService.CurrentPlanet.Id);
            UpdateViews();
            buyButton.SetListener(() => {
                BosError error = Services.TempMechanicService.Buy(generator);
                Debug.Log($"Purhase temp mechanic result =>{error}");
                if(error == BosError.Ok ) {
                    Services.SoundService.PlayOneShot(SoundName.buyGenerator);
                }
            });
            adButton.SetListener(() => {
                Services.AdService.WatchAd("SpeedUpMechanic", () => {
                    //Services.TempMechanicService.ApplyAd(generator.GeneratorId);
                    Services.MechanicService.ForceRepair(generator.GeneratorId, CountToAdRepair());
                });
            });
            adTimer.Setup(.5f, dt => UpdateAdButton(), invokeImmediatly: true);
            buyButtonStateTimer.Setup(0.167f, dt => UpdateBuyButtonState());

            //adButtonText.text = string.Format(
            //    Services.ResourceService.Localization.GetString("btn_fmt_speed_up_x_2"),
            //    "x".Colored("#FDEE21").Size(45),
            //    "2".Colored("white").Size(54));

            SetupTempMechanicsView();
            updateTimer.Setup(1, dt => AddMissedMechanics());
            SetBuyButtonSprite(true);
        }

        private int CountToAdRepair() {
            int brokenedCount = Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId);

            /*
            if(brokenedCount == 0 ) {
                return 0;
            }

            if(brokenedCount < 3) {
                return 1;
            } else {
                return brokenedCount / 3;
            }*/
            return brokenedCount;
        }

        private void UpdateAdButton() {
            if(generator != null ) {
                int countToRepair = CountToAdRepair();
                if(countToRepair > 0 ) {
                    adButtonText.text = string.Format(LocalizationObj.GetString("btn_repair_mech_ad"), countToRepair);
                    adButton.interactable = true;
                } else {
                    adButtonText.text = LocalizationObj.GetString("btn_no_repair");
                    adButton.interactable = false;
                }
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.TempMechanicAdded += OnTempMechanicAdded;
            GameEvents.TempMechanicInfoStateChanged += OnTempMechanicStateChanged;
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
        }

        public override void OnDisable() {
            GameEvents.TempMechanicAdded -= OnTempMechanicAdded;
            GameEvents.TempMechanicInfoStateChanged -= OnTempMechanicStateChanged;
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            RemoveAllMechanicViews();
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            updateTimer.Update();
            adTimer.Update();
            buyButtonStateTimer.Update();
        }

        private void UpdateViews() {
            UpdateRepairCountText();
            UpdatePriceText();
            UpdateBuyButtonState();
            UpdateAdButton();
        }

        private void UpdateRepairCountText() {
            if(mechanicData != null && generator != null) {
                repairCountText.text = "x" + Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId);  //"x10"; //"x".Colored("#FDEE21").Size(18) + $"10".Colored("#332424").Size(24);
            } else {
                repairCountText.text = string.Empty;
            }
        }

        private void UpdatePriceText() {
            if (generator != null) {
                var price = Services.TempMechanicService.GetTempMechanicPrice(generator, Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId)).ToCurrencyNumber();
                string[] priceComponents = price.LegacyComponents();
                priceText.text = priceComponents[0] + (priceComponents[1].IsValid() ? " " + priceComponents[1].Colored("#fdee21") : string.Empty); ;
            } else {
                priceText.text  = string.Empty;
            }
        }

        private void UpdateBuyButtonState() {
            if(generator != null ) {
                float progress = 0f;
                if(Services.TempMechanicService.IsBusy(generator, out progress)) {
                    buyButton.SetInteractable(false);
                    //progressController.SetFillAmount(progress);
                    progressImage.fillAmount = progress;
                    buyButtonTextsGroup.alpha = 0.3f;
                    SetBuyButtonSprite(true);
                } else {
                    bool isValid = Services.TempMechanicService.IsConditionsForBuyValid(generator);
                    if(!isValid) {
                        buyButton.SetInteractable(false);
                        //progressController.SetFillAmount(1);
                        progressImage.fillAmount = 1;
                        buyButtonTextsGroup.alpha = 1;
                        SetBuyButtonSprite(false);
                    } else {
                        buyButton.SetInteractable(true);
                        //progressController.SetFillAmount(1);
                        progressImage.fillAmount = 1;
                        buyButtonTextsGroup.alpha = 1f;
                        SetBuyButtonSprite(true);
                    }
                }
            }
        }



        #region Game Events
        private void OnTempMechanicAdded(TempMechanicInfo mechanic ) {
            if(generator != null && (generator.GeneratorId == mechanic.GeneratorId)) {
                UpdateViews();
                if(mechanicViews.Count < kMaxMechanicViewCount) {
                    AddTempMechanicView(mechanic, true);
                }
            }
        }

        private void OnTempMechanicStateChanged(TempMechanicState oldState, TempMechanicState newState, TempMechanicInfo mechanic) {
            if (generator != null && (generator.GeneratorId == mechanic.GeneratorId)) {
                UpdateViews();
                if (newState == TempMechanicState.Completed) {
                    RemoveTempMechanicView(mechanic);
                }
            }
        }
        
        private void OnUnitCountChanged(TransportUnitInfo unit) {
            if((generator != null) && (generator.GeneratorId == unit.GeneratorId)) {
                UpdateViews();
                UpdateAdButton();
            }
        }

        private void OnCompanyCashChanged(CurrencyNumber oldCound, CurrencyNumber newCount ) {
            if(generator != null && mechanicData != null ) {
                UpdateViews();
            }
        }
        #endregion

        private void AnimateMechanicObject(MechanicAnimObject mechanicView, TempMechanicInfo info) {
            Vector3Animator scaleAnimator = mechanicView.gameObject.GetOrAdd<Vector3Animator>();
            Vector2Animator positionAnimator = mechanicView.gameObject.GetOrAdd<Vector2Animator>();
            ColorAnimator colorAnimator = mechanicView.gameObject.GetOrAdd<ColorAnimator>();
            FloatAnimator rotateAnimator = mechanicView.gameObject.GetOrAdd<FloatAnimator>();


            RectTransform rectTransform = mechanicView.GetComponent<RectTransform>();
            Image image = mechanicView.GetComponent<Image>();

            Vector2 endOffset = Random.insideUnitCircle.normalized * 300;
            if(endOffset.y < 0 ) {
                endOffset.y = -endOffset.y;
            }
            Vector2 startPosition = buyButton.GetComponent<RectTransform>().anchoredPosition;
            Vector2 endPosition = startPosition + endOffset;

            Vector2AnimationData positionData = new Vector2AnimationData {
                StartValue = startPosition,
                EndValue = endPosition,
                Duration = 0.3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdatePositionFunctor(),
                OnUpdate = rectTransform.UpdatePositionTimedFunctor(),
                OnEnd = rectTransform.UpdatePositionFunctor(() => {
                    StartCoroutine(MoveInPlaceImpl(mechanicView, info));
                })
            };

            Vector3AnimationData scaleData = new Vector3AnimationData {
                StartValue = Vector3.one,
                EndValue = 2f * Vector3.one,
                Duration = .3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdateScaleFunctor(),
                OnUpdate = rectTransform.UpdateScaleTimedFunctor(),
                OnEnd = rectTransform.UpdateScaleFunctor()
            };

            ColorAnimationData colorData = new ColorAnimationData {
                StartValue = new Color(1, 1, 1, 0),
                EndValue = new Color(1, 1, 1, 1),
                Duration = .1f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = image.UpdateColorFunctor(),
                OnUpdate = image.UpdateColorTimedFunctor(),
                OnEnd = image.UpdateColorFunctor()
            };

            FloatAnimationData rotateData = new FloatAnimationData {
                StartValue = 0,
                EndValue = Random.Range(-40, 40),
                Duration = .3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdateZRotation(),
                OnUpdate = rectTransform.UpdateZRotationTimed(),
                OnEnd = rectTransform.UpdateZRotation()
            };

            positionAnimator.StartAnimation(positionData);
            scaleAnimator.StartAnimation(scaleData);
            colorAnimator.StartAnimation(colorData);
            rotateAnimator.StartAnimation(rotateData);
        }

        private IEnumerator MoveInPlaceImpl(MechanicAnimObject mechanicView, TempMechanicInfo info) {
            yield return new WaitForSeconds(.2f);

            Vector3Animator scaleAnimator = mechanicView.gameObject.GetOrAdd<Vector3Animator>();
            Vector2Animator positionAnimator = mechanicView.gameObject.GetOrAdd<Vector2Animator>();
            FloatAnimator rotateAnimator = mechanicView.gameObject.GetOrAdd<FloatAnimator>();

            RectTransform rectTransform = mechanicView.GetComponent<RectTransform>();

            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 endPosition = mechanicView.leftPosition;

            Vector2AnimationData positionData = new Vector2AnimationData {
                StartValue = startPosition,
                EndValue = endPosition,
                Duration = .3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdatePositionFunctor(),
                OnUpdate = rectTransform.UpdatePositionTimedFunctor(),
                OnEnd = rectTransform.UpdatePositionFunctor(() => {
                    Debug.Log($"setup temp mechanic on state => {info.State}");
                    mechanicView.Setup(info);
                    mechanicViews.Add(mechanicView.TempMechanic.Id, mechanicView);
                    animatedViews.Remove(mechanicView.TempMechanic.Id);
                })
            };
            Vector3AnimationData scaleData = new Vector3AnimationData {
                StartValue = 2f * Vector3.one,
                EndValue =  Vector3.one,
                Duration = .3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdateScaleFunctor(),
                OnUpdate = rectTransform.UpdateScaleTimedFunctor(),
                OnEnd = rectTransform.UpdateScaleFunctor()
            };

            FloatAnimationData rotateData = new FloatAnimationData {
                StartValue = rectTransform.localRotation.eulerAngles.z,
                EndValue = 0,
                Duration = .3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdateZRotation(),
                OnUpdate = rectTransform.UpdateZRotationTimed(),
                OnEnd = rectTransform.UpdateZRotation()
            };

            positionAnimator.StartAnimation(positionData);
            scaleAnimator.StartAnimation(scaleData);
            rotateAnimator.StartAnimation(rotateData);
        }
    }

}