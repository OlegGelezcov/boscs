using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using TMPro;

    public class BuyAuditorButtonView : GameBehaviour {
        private const int kMaxAuditorViewCount = 10;
        
        public GameObject auditorPrefab;
        public Text countText;
        public Text priceText;
        //public Text priceSuffixText;
        public Button buyButton;
        //public ProgressShaderController progressController;
        public Image buyImageProgress;
        public CanvasGroup buyButtonTextsGroup;
        
        public Button adButton;
        public Text adButtonText;
        public RectTransform animationParent;
        public TextMeshProUGUI speedText;


        //public Sprite enabledBuyButtonSprite;
        //public Sprite disabledBuyButtonSprite;
        private bool isEnabledSprite = true;

        //private Auditor auditor;
        private GeneratorInfo generator;
        
        private readonly UpdateTimer buyButtonTimer = new UpdateTimer();
        private readonly Dictionary<string, SecretaryAnimObject> auditorViews = 
            new Dictionary<string, SecretaryAnimObject>();
        private readonly List<string> animatedViews = new List<string>();
        private readonly UpdateTimer adTimer = new UpdateTimer();
        private readonly UpdateTimer missedTimer = new UpdateTimer();

        public override void Start() {
            base.Start();
            UpdateSpeedTextWithValue(Services.AuditorService.GetSpeedMult(generator.GeneratorId));
            UniRx.MessageBroker.Default.Receive<SpeedMultChangedArgs>().Where(args => args.Name == "secretary" && args.GeneratorId == generator.GeneratorId).Subscribe(args => {
                UpdateSpeedTextWithValue(args.SpeedModifier.GetSpeedMult(generator.GeneratorId));
            }).AddTo(gameObject);
        }

        private void UpdateSpeedTextWithValue(int val) {
            speedText.text = string.Format(LocalizationObj.GetString("lbl_speed_mult_q"), val);
        }

        private void SetBuyButtonSprite(bool value) {
            bool oldValue = isEnabledSprite;
            isEnabledSprite = value;
            if(isEnabledSprite != oldValue ) {
                if(isEnabledSprite) {
                    //buyButton.GetComponent<Image>().sprite = enabledBuyButtonSprite;
                    //buyButton.GetComponent<Image>().overrideSprite = enabledBuyButtonSprite;
                } else {
                    //buyButton.GetComponent<Image>().sprite = disabledBuyButtonSprite;
                    //buyButton.GetComponent<Image>().overrideSprite = disabledBuyButtonSprite;
                }
            }
        }
        
        public void Setup(GeneratorInfo gen) {
            this.generator = gen;
            UpdateCountText();
            UpdatePriceTexts();
            UpdateBuyButtonState();
            buyButton.SetListener(() => {
                var error = Services.AuditorService.Buy(generator);
                Debug.Log($"purchase auditor result => {error}");
                if (error == BosError.Ok) {
                    Services.SoundService.PlayOneShot(SoundName.buyGenerator);
                }
            });
            buyButtonTimer.Setup(0.167f, dt => {
                UpdateBuyButtonState();
            });
            adButton.SetListener(() => {
                
                Services.AdService.WatchAd("SpeedUpSecretary", () => {
                    //Services.AuditorService.ApplyAd(generator.GeneratorId);
                    Services.SecretaryService.ForceHandle(generator.GeneratorId, CountToHandleWithAd());
                });
            });
            adTimer.Setup(0.5f, dt => UpdateAdButton(), invokeImmediatly: true);
            SetupAuditorViews();
            missedTimer.Setup(1, dt => AddMissedAuditors());
            SetBuyButtonSprite(true);
        }

        private int CountToHandleWithAd() {
            int totalCount = Services.SecretaryService.GetReportCount(generator.GeneratorId);

            /*
            if(totalCount == 0 ) {
                return 0;
            }
            if(totalCount < 3 ) {
                return 1;
            } else {
                return totalCount / 3;
            }*/
            return totalCount;
        }

        private void UpdateAdButton() {
            if (generator != null) {
                int countToHandle = CountToHandleWithAd();
                if (countToHandle > 0) {
                    adButton.interactable = true;
                    adButtonText.text = string.Format(LocalizationObj.GetString("btn_handle_report_ad"), countToHandle);
                } else {
                    adButton.interactable = false;
                    adButtonText.text = LocalizationObj.GetString("btn_no_reports");
                }
            }
        }

        /*
        private void UpdateAdButton() {
            if (generator != null) {
                adButton.interactable = Services.AuditorService.IsAdApplicable(generator.GeneratorId); //Services.AuditorService.IsAdValid(generator.GeneratorId); //Services.AuditorService.IsConditionsForBuyValid(generator);
            } else {
                adButton.interactable = false;
            }
        }*/

        private void UpdateViews() {
            UpdateCountText();
            UpdatePriceTexts();
            UpdateBuyButtonState();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ReportCountChanged += OnReportCountChanged;
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;

            GameEvents.AuditorAdded += OnAuditorAdded;
            GameEvents.AuditorStateChanged += OnAuditorStateChanged;
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
        }

        public override void OnDisable() {
            GameEvents.ReportCountChanged -= OnReportCountChanged;
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            
            GameEvents.AuditorAdded -= OnAuditorAdded;
            GameEvents.AuditorStateChanged -= OnAuditorStateChanged;
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();

            buyButtonTimer.Update();
            adTimer.Update();
            missedTimer.Update();
        }



        private void UpdateCountText() {
            if (generator != null) {
                var secretaryData =
                    Services.ResourceService.SecretaryDataRepository.GetSecretaryData(Services.PlanetService.CurrentPlanetId);
                int reportCount = Services.SecretaryService.GetReportCount(generator.GeneratorId);
                countText.text = "x" + reportCount.ToString();
                /*
                int count = 0;
                if (reportCount > 0) {
                    if (secretaryData != null) {
                        count = Mathf.Min(reportCount, secretaryData.ReportCountProcessedPer10Seconds);
                    } else {
                        Debug.Log($"ERROR: secretary data for generator: {generator.GeneratorId} is null");
                        count = 0;
                    }
                } else {
                    if (secretaryData != null) {
                        count = secretaryData.ReportCountProcessedPer10Seconds;
                    } else {
                        Debug.Log($"ERROR: secretary data for generator: {generator.GeneratorId} is null");
                        count = 0;
                    }
                }
                countText.text = $"x{count}"; //"x".Colored("#fdee21") + $"{count}".Colored("#332424");
                */
            }
        }

        private void UpdatePriceTexts() {
            if (generator != null) {
                var reportCount = Services.SecretaryService.GetReportCount(generator.GeneratorId);
                CurrencyNumber price = Services.AuditorService.GetAuditorPrice(generator, reportCount).ToCurrencyNumber();
                string[] legacyComponents = price.LegacyComponents();
                priceText.text = legacyComponents[0] + (legacyComponents[1].IsValid() ? " " + legacyComponents[1].Colored("#fdee21") : string.Empty);
            }
        }

        private void UpdateBuyButtonState() {
            if (generator != null) {

                float progress = 0;
                if (Services.AuditorService.IsBusy(generator, out progress)) {
                    buyButton.SetInteractable(false);
                    //progressController.SetFillAmount(progress);
                    buyImageProgress.fillAmount = progress;
                    buyButtonTextsGroup.alpha = 0.3f;
                    SetBuyButtonSprite(true);
                } else {
                    bool isValid = Services.AuditorService.IsConditionsForBuyValid(generator);
                    if (!isValid) {
                        buyButton.SetInteractable(false);
                        //progressController.SetFillAmount(1);
                        buyImageProgress.fillAmount = 1;
                        buyButtonTextsGroup.alpha = 1;
                        SetBuyButtonSprite(false);
                    } else {
                        buyButton.SetInteractable(true);
                        //progressController.SetFillAmount(1);
                        buyImageProgress.fillAmount = 1;
                        buyButtonTextsGroup.alpha = 1f;
                        SetBuyButtonSprite(true);
                    }
                }

            }
        }

        private void SetupAuditorViews() {
            ClearAuditorViews();
            var auditors = Services.AuditorService.GetAuditors(generator.GeneratorId);
            List<Auditor> notCompletedAuditors = auditors.Values.Where(a => !a.IsCompleted).ToList();
            int count = Mathf.Min(kMaxAuditorViewCount, notCompletedAuditors.Count);
            for (int i = 0; i < count; i++) {
                AddAuditorView(notCompletedAuditors[i], false);
            }
        }

        private void ClearAuditorViews() {
            foreach (var kvp in auditorViews) {
                if (kvp.Value && kvp.Value.gameObject) {
                    Destroy(kvp.Value.gameObject);
                }
            }
            auditorViews.Clear();
        }

        private void AddAuditorView(Auditor auditor, bool useAnimation) {
            if (!animatedViews.Contains(auditor.Id)) {
                GameObject auditorObj = Instantiate<GameObject>(auditorPrefab);
                auditorObj.GetComponent<RectTransform>().SetParent(animationParent, false);
                var auditorView = auditorObj.GetComponent<SecretaryAnimObject>();
                animatedViews.Add(auditor.Id);
                if (useAnimation) {
                    AnimateAuditorObject(auditorView, auditor);
                }
                else {
                    auditorView.Setup(auditor);
                    auditorViews.Add(auditorView.Auditor.Id, auditorView);
                    animatedViews.Remove(auditorView.Auditor.Id);
                }
            }
        }

        private void RemoveAllAuditorViews() {
            List<SecretaryAnimObject> views = auditorViews.Values.ToList();
            foreach (var view in views) {
                if (view && view.gameObject && view.Auditor != null) {
                    RemoveAuditorView(view.Auditor);
                }
            }
            auditorViews.Clear();
        }

        private void RemoveAuditorView(Auditor auditor) {
            if (auditorViews.ContainsKey(auditor.Id)) {
                var view = auditorViews[auditor.Id];
                auditorViews.Remove(auditor.Id);
                if (view && view.gameObject) {
                    Destroy(view.gameObject);
                }
            }
        }

        private void AddMissedAuditors() {
            if (auditorViews.Count < kMaxAuditorViewCount && generator != null) {
                var additionalAuditors = Services.AuditorService.GetAuditors(generator.GeneratorId)
                    .Where(kvp => !kvp.Value.IsCompleted && !auditorViews.ContainsKey(kvp.Value.Id))
                    .Select(kvp => kvp.Value).Take(kMaxAuditorViewCount - auditorViews.Count).ToList();
                foreach (var auditor in additionalAuditors) {
                    AddAuditorView(auditor, false);
                }
            }
        }
        
        #region Game Events

        private void OnAuditorAdded(Auditor newAuditor) {
            if (generator != null && (generator.GeneratorId == newAuditor.GeneratorId)) {
                UpdateViews();
                if (auditorViews.Count < kMaxAuditorViewCount) {
                    AddAuditorView(newAuditor, true);
                }
            }
        }

        private void OnAuditorStateChanged(AuditorState oldState, AuditorState newState, Auditor auditor) {
            if (generator != null && (generator.GeneratorId == auditor.GeneratorId)) {
                UpdateViews();
                if (newState == AuditorState.Completed) {
                    RemoveAuditorView(auditor);
                }
            }
        }
        

        private void OnCompanyCashChanged(CurrencyNumber oldCound, CurrencyNumber newCount ) {
            if(generator != null ) {
                UpdateViews();
            }
        }
        
        private void OnReportCountChanged(int oldCount, int newCount, ReportInfo report) {
            if (generator != null) {
                if (report.ManagerId == generator.GeneratorId) {
                    UpdateCountText();
                    UpdateAdButton();
                }
            }
        }

        private void OnUnitCountChanged(TransportUnitInfo unit) {
            if (generator != null) {
                if (generator.GeneratorId == unit.GeneratorId) {
                    UpdateViews();
                }
            }
        }
        #endregion
        
        
        private void AnimateAuditorObject(SecretaryAnimObject mechanicView, Auditor info) {
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

        private IEnumerator MoveInPlaceImpl(SecretaryAnimObject mechanicView, Auditor info) {
            yield return new WaitForSeconds(.2f);

            Vector3Animator scaleAnimator = mechanicView.gameObject.GetOrAdd<Vector3Animator>();
            Vector2Animator positionAnimator = mechanicView.gameObject.GetOrAdd<Vector2Animator>();
            FloatAnimator rotateAnimator = mechanicView.gameObject.GetOrAdd<FloatAnimator>();

            RectTransform rectTransform = mechanicView.GetComponent<RectTransform>();

            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 endPosition = mechanicView.rightPosition;

            Vector2AnimationData positionData = new Vector2AnimationData {
                StartValue = startPosition,
                EndValue = endPosition,
                Duration = .3f,
                EaseType = EaseType.EaseInOutQuad,
                Target = mechanicView.gameObject,
                OnStart = rectTransform.UpdatePositionFunctor(),
                OnUpdate = rectTransform.UpdatePositionTimedFunctor(),
                OnEnd = rectTransform.UpdatePositionFunctor(() => {
                    mechanicView.Setup(info);
                    auditorViews.Add(mechanicView.Auditor.Id, mechanicView);
                    animatedViews.Remove(mechanicView.Auditor.Id);
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