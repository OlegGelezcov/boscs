namespace Bos.UI {
    
    using Bos.Data;
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Analytics;
    using UnityEngine.UI;
    using UniRx;
    public class UnlockedGeneratorView : GameBehaviour {


        private GeneratorInfo generator = null;
        private ManagerInfo manager = null;

        private readonly UpdateTimer updateProfitTimer = new UpdateTimer();
        private readonly UpdateTimer updateBuyManagerTimer = new UpdateTimer();
        private readonly UpdateTimer updateBuyGeneratorCountControls = new UpdateTimer();
        private readonly UpdateTimer automaticParticlesUpdater = new UpdateTimer();


        private bool IsAlwaysProgressFull {
            get
            {
                if (generator != null) {
                    var defaultSettings = Services.ResourceService.Defaults;
                    if (generator.IsAutomatic && generator.ProfitResult.GenerationInterval < defaultSettings.minManualGeneratorInterval) {
                        return true;
                    }
                }
                return false;
            }
        }


        public Image progressImage;
        public Text profitCountText;
        public Text profitSuffixText;
        public Text timerText;

        public Button buyGeneratorButton;
        public TriggerParticleEffect buyGeneratorParticlesTrigger;
        public Text buyCountText;
        public Text buyPriceText;
        public Text buyPriceSuffixText;
        public GeneratorCountButton generatorCountButton;

        public Image levelProgressFill;
        public Button buyManagerButton;

        public Button generateButton;
        public Text unitCountText;
        public Text nameText;

        public GameObject automaticParticles;
        public GameObject enhancedParticles;

        public GameObject[] enhancedViewObjects;

        public Button enhanceButton;
        public Sprite enhancedBuySprite;
        public Sprite enhancedBuySpritePressed;
        public Sprite enhancedProgressBG;
        public Sprite enhancedProgressFill;
        public Sprite enhancedIconSprite;
        public Sprite baseSprite;
        public Image progressBGImage;
        //public GameObject enhanceConfirm;
        public Color buyBaseColor;
        public Color moneyTextColor;
        public Color progressTextColor;
        //public GameObject notEnoughCoinsPopup;
        public Image managerIcon;

        public Image staticGeneratorImage;
        //public GameObject animatedGeneratorImage;
        public AnimatedGeneratorObject[] animatedObjects;

        public GameObject megaObject;
        public GameObject reportObject;
        public GameObject toolObject;
        public Button reportButton;
        public Button toolButton;

        private Vector3AnimationData buyGeneratorAnimationData = null;
        private Vector3AnimationData BuyGeneratorButtonAnimationData {
            get {
                if (buyGeneratorAnimationData == null) {
                    RectTransform butTrs = buyGeneratorButton.GetComponent<RectTransform>();
                    Vector3Animator butAnimator = buyGeneratorButton.gameObject.GetOrAdd<Vector3Animator>();

                    Vector3AnimationData data = new Vector3AnimationData {
                        StartValue = Vector3.one,
                        EndValue = 0.85f * Vector3.one,
                        Duration = 0.06f,
                        EaseType = EaseType.EaseInOutQuad,
                        AnimationMode = BosAnimationMode.Single,
                        Target = buyGeneratorButton.gameObject,
                        OnStart = (s, o) => butTrs.localScale = s,
                        OnUpdate = (s, t, o) => butTrs.localScale = s,
                        OnEnd = (s, o) => {
                            butTrs.localScale = s;
                            butAnimator.StartAnimation(new Vector3AnimationData {
                                StartValue = 0.85f * Vector3.one,
                                EndValue = Vector3.one,
                                Duration = 0.06f,
                                EaseType = EaseType.EaseInOutQuad,
                                AnimationMode = BosAnimationMode.Single,
                                Target = buyGeneratorButton.gameObject,
                                OnStart = (s2, o2) => butTrs.localScale = s2,
                                OnUpdate = (s2, t2, o2) => butTrs.localScale = s2,
                                OnEnd = (s2, o2) => butTrs.localScale = s2
                            });
                        }
                    };
                    buyGeneratorAnimationData = data;
                }
                return buyGeneratorAnimationData;
            }
        }


        private void UpdateMegaObject() {
            if(generator.Data.Type == GeneratorType.Normal ) {
                var isFullMega = Services.ManagerService.GetManagerEfficiencyRollbackLevel(generator.GeneratorId).IsMega;
                if(megaObject != null ) {
                    if(isFullMega ) {
                        megaObject.Activate();
                    } else {
                        megaObject.Deactivate();
                    }
                }
            }
        }

        private void UpdateManagerIcon() {
            Services.ViewService.Utils.ApplyManagerIcon(managerIcon, generator, generator.IsAutomatic);
        }

        private void UpdateGeneratorIcon() {
            if (Services.PlanetService.CurrentPlanet.Id == PlanetConst.EARTH_ID) {
                if (generator.State == GeneratorState.Active) {
                    staticGeneratorImage?.Deactivate();
                    ToggleAnimatedObject(Services.PlanetService.CurrentPlanet.Id, true);
                } else {
                    staticGeneratorImage?.Activate();
                    ToggleAnimatedObject(Services.PlanetService.CurrentPlanet.Id, false);
                }
            } else {

                if (HasAnimatedImageForPlanet(Services.PlanetService.CurrentPlanet.Id)) {
                    ToggleAnimatedObject(Services.PlanetService.CurrentPlanet.Id, true);
                    staticGeneratorImage?.Deactivate();
                } else {
                    var generatorIconData = Services.ResourceService.GeneratorLocalData.GetLocalData(generator.GeneratorId).GetIconData(Services.PlanetService.CurrentPlanet.Id);
   
                    ToggleAnimatedObject(Services.PlanetService.CurrentPlanet.Id, false);
                    staticGeneratorImage?.Activate();
                    if (staticGeneratorImage != null) {
                        staticGeneratorImage.overrideSprite = Services.ResourceService.GetSpriteByKey(generatorIconData.icon_id);
                    }
                }

            }
        }

        private readonly GameObject2DPull textPull = new GameObject2DPull();

        private GameObject accumulatedTextPrefab = null;
        private GameObject AccumulatedTextPrefab
            => (accumulatedTextPrefab == null) ? (accumulatedTextPrefab = GameServices.Instance.ResourceService.Prefabs.GetPrefab("accum_text")) : accumulatedTextPrefab;

        public void Setup(int generatorId ) {
            textPull.Setup(10, AccumulatedTextPrefab, transform);

            generator = Services.GenerationService.Generators.GetGeneratorInfo(generatorId);
            updateProfitTimer.Setup(0.3f, (delta) => {
                UpdateProfitValues();
            }, true);

            updateBuyGeneratorCountControls.Setup(0.5f, (deltaTime) => {
                if (generatorCountButton.State == GeneratorButtonState.MAX) {
                    UpdateBuyGeneratorControls(generatorCountButton.GetBuyInfo());
                }
            });

            generatorCountButton.Setup(generatorId);
            UpdateBuyGeneratorControls(generatorCountButton.GetBuyInfo());
            UpdateGeneratorLevelProgress();
            buyManagerButton.SetListener(() => {
                
                Services.ViewService.Show(ViewType.ManagementView, new ViewData() {
                    UserData = generator.GeneratorId
                });
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });

            //disable buy manager button for planet transport
            if(generator.Data.Type == GeneratorType.Planet) {
                buyManagerButton.interactable = false;
                buyManagerButton.Deactivate();
            }

            updateBuyManagerTimer.Setup(1.0f, (delta) => {
                UpdateBuyManagerButton();
            }, true);

            Vector3Animator buyButtonAnimator = buyGeneratorButton.gameObject.GetOrAdd<Vector3Animator>();

            buyGeneratorButton.SetListener(() => {
                if (generator.IsGenerationStarted) {
                    generator.ForceFinalization();
                }
                if (Services.GenerationService.BuyGenerator(generator, generatorCountButton.GetBuyInfo().Count) > 0) {
                    UpdateGeneratorLevelProgress();
                    UpdateBuyGeneratorControls(generatorCountButton.GetBuyInfo());
                }
                buyGeneratorParticlesTrigger.TriggerAnimation();
                Services.GetService<ISoundService>().PlayOneShot(SoundName.buyGenerator);
                buyButtonAnimator.StartAnimation(BuyGeneratorButtonAnimationData);
            });

            generateButton.SetListener(() => {
                if(generator.IsManual) {
                    if(!generator.IsGenerationStarted) {
                        generator.StartGeneration();
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                        if(generator.IsEnhanced) {
                            enhancedParticles.Activate();
                        }
                        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.GenerationButtonClicked, generatorId));
                    }
                }
            });

            if(IsAlwaysProgressFull) {
                progressImage.fillAmount = 1;
                //isAccumulationStarted = true;
                generator.SetGenerationStarted(true);
            } else {
                progressImage.fillAmount = 0;
                if(generator.IsAutomatic) {
                    generator.SetGenerationStarted(true);
                    //isAccumulationStarted = true;
                }
            }

            UpdateUnitCountText();

            /*
            if(generator.Data.Type == GeneratorType.Planet ) {
                PlanetNameData planetNameData =
                    Services.ResourceService.PlanetNameRepository.GetPlanetNameData(
                        generator.PlanetId);
                nameText.text = Services.ResourceService.Localization.GetString(
                    planetNameData.name);
            } else {
                nameText.text = Services.ResourceService.Localization.GetString(generator.Data.Name);
            }*/
            //var generatorLocalData = Services.res

            Services.ViewService.Utils.ApplyGeneratorName(nameText, generator);

            UpdateGeneratorLevelProgress();

            UpdateEnhancementViews(generator);
            UpdateAutomaticParticles();

            var soundService = Services.SoundService;
            enhanceButton.SetListener(() => {
                if(generator.Data.Type == GeneratorType.Normal) {
                    StartEnhance();
                    soundService.PlayOneShot(SoundName.click);
                }
            });
            UpdateTimerText(generator.RemainTime);

            //make automatic planet transport and hire manager
            if(generator.IsManual && generator.Data.Type == GeneratorType.Planet) {
                Services.GetService<IManagerService>().Hire(generator.GeneratorId);
                Services.GetService<IGenerationService>().Generators.SetAutomatic(generator.GeneratorId, true);
            }

            UpdateGeneratorIcon();
            UpdateManagerIcon();
            UpdateMegaObject();

            automaticParticlesUpdater.Setup(2, (dt) => UpdateAutomaticParticles(), true);

            if(generator.Data.Type == GeneratorType.Normal ) {
                UpdateReportObject(Services.SecretaryService.GetReportCount(generator.GeneratorId));
                UpdateToolObject(Services.TransportService.GetUnitBrokenedCount(generator.GeneratorId));
                if(reportButton != null ) {
                    reportButton.OnClickAsObservable().Subscribe(_ => {
                        ShowManagementView();
                    }).AddTo(gameObject);
                }
                if(toolButton != null ) {
                    toolButton.OnClickAsObservable().Subscribe(_ => {
                        ShowManagementView();
                    }).AddTo(gameObject);
                }
            }
        }

        public override void Start() {
            base.Start();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GeneratorButtonStateChanged += OnGeneratorCountButtonChanged;
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;
            GameEvents.GeneratorAchievmentsReceived += OnGeneratorAchievmentReceived;
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.GeneratorEnhanced += OnGeneratorEnhanced;
            GameEvents.AutomaticChanged += OnAutomaticChanged;
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
            GameEvents.ManagerMegaChanged += OnMegaChanged;
            GameEvents.AccumulationProgressChanged += OnAccumualtionProgressChanged;
            GameEvents.AccumulationCompleted += OnAccumulationCompleted;
            GameEvents.ReportCountChanged += OnReportCountChanged;
            GameEvents.X20BoostMultStarted += OnX20Boost;
        }

        public override void OnDisable() {
            GameEvents.GeneratorButtonStateChanged -= OnGeneratorCountButtonChanged;
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            GameEvents.GeneratorAchievmentsReceived -= OnGeneratorAchievmentReceived;
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.GeneratorEnhanced -= OnGeneratorEnhanced;
            GameEvents.AutomaticChanged -= OnAutomaticChanged;
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            GameEvents.AccumulationProgressChanged -= OnAccumualtionProgressChanged;
            GameEvents.AccumulationCompleted -= OnAccumulationCompleted;
            GameEvents.ReportCountChanged -= OnReportCountChanged;
            GameEvents.X20BoostMultStarted -= OnX20Boost;
            GameEvents.ManagerMegaChanged -= OnMegaChanged;
            base.OnDisable();
        }

        private void OnX20Boost(bool started ) {
            if (generator != null) {
                UpdateProfitValues();
            }
        }

        private void ShowManagementView() {
            if (!ViewService.Exists(ViewType.ManagementView)) {
                ViewService.Show(ViewType.ManagementView, new ViewData { UserData = generator.GeneratorId });
                Sounds.PlayOneShot(SoundName.click);
            }
        }

        private void UpdateAutomaticParticles() {
            if(IsAlwaysProgressFull) {
                if (generator.IsEnhanced) {
                    enhancedParticles.Activate();
                    automaticParticles.Deactivate();
                } else {
                    enhancedParticles.Deactivate();
                    automaticParticles.Activate();
                }
            } else {
                automaticParticles.Deactivate();
                if(generator.IsAutomatic && generator.IsEnhanced) {
                    enhancedParticles.Activate();
                } else {
                    enhancedParticles.Deactivate();
                }
            }
        }

        private void UpdateUnitCountText() {
            int brokenedCount = Services.TransportService.GetUnitBrokenedCount(GeneratorId);
            int liveCount = Services.TransportService.GetUnitLiveCount(GeneratorId);
            if (brokenedCount == 0) {
                unitCountText.text = liveCount.ToString();
            } else {
                //unitCountText.text = liveCount.ToString() + "/" + brokenedCount.ToString().Colored(ConsoleTextColor.red);
                unitCountText.text = liveCount.ToString();
            }
        }


        private void UpdateProfitValues() {

            //profitResult.UpdateFromOther(Services.GenerationService.CalculateProfitPerSecond(generator: generator,
            //    countOfOwnedGenerators: Services.TransportService.GetUnitLiveCount(generator.GeneratorId)));
            UpdateProfitTexts();
        }



        private void OnReportCountChanged(int oldCount, int newCount, ReportInfo info ) {
            if(generator != null ) {
                if(generator.GeneratorId == info.ManagerId ) {
                    UpdateReportObject(newCount);
                }
            }
        }

        private void UpdateReportObject(int count) {
            if (reportObject != null) {
                reportObject.SetActive(count > 0);
            }
        }

        private void UpdateToolObject(int count) {
            if (toolObject != null) {
                toolObject?.SetActive(count > 0);
            }
        }

        public override void Update() {
            base.Update();
            updateProfitTimer.Update();
            updateBuyManagerTimer.Update();
            updateBuyGeneratorCountControls.Update();
            automaticParticlesUpdater.Update();
        }

        private void OnAccumualtionProgressChanged(GeneratorInfo targetGenerator, double timer, double interval, ProfitResult profit ) {
            if(targetGenerator.GeneratorId == generator.GeneratorId ) {
                OnAccumulationProgressUpdated((float)BosUtils.Clamp01(timer / interval));
                UpdateTimerText(targetGenerator.RemainTime);
            }
        }

        private void OnAccumulationCompleted(GeneratorInfo targetGenerator, ProfitResult profit ) {
            if(targetGenerator.GeneratorId == generator.GeneratorId ) {
                AddCompanyCash(targetGenerator.AccumulatedCash);
                CreateAccumulatedText(targetGenerator.AccumulatedCash, targetGenerator.IsManual);

                OnAccumulationProgressUpdated((float)BosUtils.Clamp01(targetGenerator.GenerateTimer / targetGenerator.AccumulateInterval));
                UpdateTimerText(generator.RemainTime);
            }
        }



        private void CreateAccumulatedText(double val, bool isManual) {
            if (textPull.HasObject) {

                float duration = 1; //isManual ? 0.6f : 0.3f;

                GameObject inst = textPull.UseAtPosition(new Vector2(UnityEngine.Random.Range(0, 330), -353)); //Instantiate(AccumulatedTextPrefab, transform, false);
                Text txt = inst.GetComponent<Text>();
                txt.text = val.ToCurrencyNumber().PrefixedAbbreviation();
                RectTransform textTrs = inst.GetComponent<RectTransform>();

                float finalDuration = duration + UnityEngine.Random.Range(-.2f, .2f);

                Vector2Animator vec2Animator = inst.GetComponent<Vector2Animator>();
                vec2Animator.StartAnimation(new Vector2AnimationData {
                    StartValue = textTrs.anchoredPosition,
                    EndValue = textTrs.anchoredPosition + Vector2.down * 5/* + Vector2.up * 60*/,
                    Duration = finalDuration,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = inst,
                    OnStart = textTrs.UpdatePositionFunctor(),
                    OnUpdate = textTrs.UpdatePositionTimedFunctor(),
                    OnEnd = textTrs.UpdatePositionFunctor(() => textPull.Free(inst) )
                });

                

                FloatAnimator alphaAnimator = inst.GetComponent<FloatAnimator>();
                CanvasGroup cg = inst.GetComponent<CanvasGroup>();
                alphaAnimator.Stop();
                cg.alpha = 1;
                alphaAnimator.StartAnimation(new FloatAnimationData {
                    Duration = finalDuration * 1.5f,
                    EaseType = EaseType.EaseInOutQuad,
                    StartValue = 1,
                    EndValue = 0,
                    Target = inst,
                    OnStart = cg.UpdateAlphaFunctor(),
                    OnUpdate = cg.UpdateAlphaTimedFunctor(),
                    OnEnd = cg.UpdateAlphaFunctor()
                });

                Vector3Animator v3animator = inst.GetOrAdd<Vector3Animator>();
                v3animator.Stop();
                v3animator.StartAnimation(textTrs.ConstructScaleAnimationData(Vector3.one * .1f, Vector3.one, finalDuration * 0.5f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                    v3animator.StartAnimation(textTrs.ConstructScaleAnimationData(Vector3.one, Vector3.one * .1f, finalDuration * 0.5f, BosAnimationMode.Single, EaseType.EaseInOutQuad));
                }));

                //ColorAnimator colorAnimator = inst.GetComponent<ColorAnimator>();

                //colorAnimator.StartAnimation(AnimUtils.GetColorAnimData(txt.color, txt.color.ChangeAlpha(0), finalDuration, EaseType.EaseInOutQuad, textTrs));
            }
        }
        /*
        private void FinalizeAccumulation() {
            //Debug.Log($"add company cash => {accumulatedBalance}".Colored(ConsoleTextColor.yellow));
            AddCompanyCash(accumulatedBalance);
            SetAccumulationTimer(0f);
            accumulatedBalance = 0;
        }*/


        /*
        private void AddAccumulationTimer(float delta) {
            //accumulateTimer += delta;
            generator?.AddGenerateTimer(delta);
            OnAccumulationProgressUpdated((float)BosUtils.Clamp01(accumulateTimer / AccumulateInterval));
            UpdateTimerText(RemainTime);
        }

        private void SetAccumulationTimer(float val) {
            //accumulateTimer = val;
            generator?.SetGenerateTimer(0f);
            OnAccumulationProgressUpdated((float)BosUtils.Clamp01(accumulateTimer / AccumulateInterval));
            UpdateTimerText(RemainTime);
        }*/

        private void UpdateTimerText(float interval) {
            if (IsAlwaysProgressFull) {
                timerText.text = string.Empty;
            } else {
                if (interval < 0f) {
                    interval = 0f;
                }
                System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(interval);
                timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }
        
        private void OnMegaChanged(bool isMega, ManagerEfficiencyRollbackLevel otherManager) {
            if (generator.Data.Type == GeneratorType.Normal && isMega) {
                if (generator.GeneratorId == otherManager.Id) {
                    UpdateMegaObject();
                }
            }
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet ) {
            if(newState == PlanetState.Opened ) {
                UpdateGeneratorIcon();
                UpdateManagerIcon();
                UpdateProfitValues();
            }
        }

        private void OnAutomaticChanged(int generatorId, bool isAutomatic ) {
            Debug.Log($"automatic changed on generator =>{generatorId}, my generator is automatic => {generator.IsAutomatic}");

            if(generator != null && (generator.GeneratorId == generatorId)) {
                UpdateAutomaticParticles();
                UpdateProfitValues();
                UpdateProfitTexts();
                UpdateBuyManagerButton();
                if(isAutomatic) {
                    generator = Services.GenerationService.Generators.GetGeneratorInfo(generatorId);
                    Setup(generatorId);
                }
                UpdateManagerIcon();
            }
        }

        private void OnGeneratorEnhanced(GeneratorInfo info) {
            UpdateEnhancementViews(info);
            
            UpdateProfitValues();
            UpdateProfitTexts();
            if((generator != null) && (generator.GeneratorId == info.GeneratorId)) {
                UpdateProfitTexts();
            }
        }

        private void UpdateEnhancementViews(GeneratorInfo info) {
            if (generator != null && (info.GeneratorId == generator.GeneratorId)) {
                if (info.IsEnhanced) {
                
                    enhancedViewObjects.Activate();
                    //.Deactivate();
                    UpdateViewsWhenEnhanced();
                } else {
                    enhancedViewObjects.Deactivate();
                    //enhanceButton.Activate();
                }
            }
        }



        private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) {
            //UpdateBuyGeneratorControls(LastBuyCount > 0 ? LastBuyCount : generatorCountButton.StateInt);
            UpdateBuyManagerButton();
            UpdateBuyGeneratorControls(generatorCountButton.GetBuyInfo());         
        }

        private void OnGeneratorCountButtonChanged(int generatorId, BuyInfo buyInfo) {
            if (generator.GeneratorId == generatorId) {
                UpdateBuyGeneratorControls(buyInfo);
            }
        }

        private void OnAccumulationProgressUpdated(float procent) {
            progressImage.fillAmount = IsAlwaysProgressFull ? 1.0f : procent;
        }

        private void OnUnitCountChanged(TransportUnitInfo unit) {
            if(generator != null && (generator.GeneratorId == unit.GeneratorId)) {
                UpdateGeneratorLevelProgress();
                UpdateBuyGeneratorControls(generatorCountButton.GetBuyInfo());
                UpdateUnitCountText();
                UpdateProfitValues();
                UpdateProfitTexts();
                UpdateToolObject(unit.BrokenedCount);
            }
        }

        private void OnGeneratorAchievmentReceived(int generatorId, List<ExtendedAchievmentInfo> achievments) {
            if(generator != null && (generator.GeneratorId == generatorId)) {
                UpdateGeneratorLevelProgress();
            }
        }

        private void AddCompanyCash(double value) {
            if (generator.IsAutomatic) {
                Manager.AddCash(value, isGenerateEvent: true);
            }
            Player.AddGenerationCompanyCash(value);
        }

        private void UpdateProfitTexts() {
            
                string[] priceArray = null;
                priceArray = Services.Currency.CreatePriceStringSeparated(IsAlwaysProgressFull ? generator.ProfitResult.ValuePerSecond : generator.ProfitResult.ValuePerRound);
                profitCountText.text = priceArray[0];

                if (priceArray.Length > 1) {
                    if (IsAlwaysProgressFull) {
                        profitCountText.text = $"{priceArray[0]} {priceArray[1].ToUpper()} / {SecStr}";
                    } else
                    {
                        profitCountText.text = $"{priceArray[0]} {priceArray[1].ToUpper()}";
                    }
                }

            profitSuffixText.text = DollarsStr;
        }

        private void UpdateBuyGeneratorControls(BuyInfo buyInfo) {
            /*
            if(generator.GeneratorId == 0 ) {
                Debug.Log(buyInfo.ToString().Bold().Colored(ConsoleTextColor.cyan));
            }*/
            string[] priceArray = Services.Currency.CreatePriceStringSeparated(buyInfo.Price);
            
            if (priceArray.Length > 1) {
                buyPriceText.text = $"{priceArray[0]} {priceArray[1]}";
            } else
            {
                buyPriceText.text = priceArray[0];
            }
            buyPriceSuffixText.text = DollarsStr;
            buyGeneratorButton.interactable = buyInfo.IsAllowed;
            buyCountText.text = buyInfo.Count.ToString();
        }

        private void UpdateGeneratorLevelProgress() {
            if(generator != null ) {
                int unitTotalCount = Services.TransportService.GetUnitTotalCount(generator.GeneratorId);
                if(unitTotalCount > 0 ) {
                    levelProgressFill.fillAmount =
                        AchievmentService.GetProgressForGenerator(generator.GeneratorId, unitTotalCount);
                } else {
                    levelProgressFill.fillAmount = 0;
                }
            } else {
                levelProgressFill.fillAmount = 0;
            }


        }

        private void UpdateBuyManagerButton() {

            if(generator != null  && generator.Data.Type == GeneratorType.Normal) {
                IPlayerService playerService = Services.PlayerService;
                if(generator.IsManual && playerService.IsEnoughCompanyCash(Manager.Cost)) {
                    buyManagerButton.image.overrideSprite = ButtonPlusSprite;
                    buyManagerButton.image.enabled = true;
                } else if(generator.IsAutomatic && Manager.CashOnHand > 0 && Manager.NextKickBackTime < Services.TimeService.Now){
                    if(AlertKickbackSprite != null ) {
                        buyManagerButton.image.overrideSprite = AlertKickbackSprite;
                    }
                    buyManagerButton.image.enabled = true;
                } else {
                    buyManagerButton.image.enabled = false;
                }
            }
        }

        private void StartEnhance() {
            var generatorIconData =  ResourceService
                .GeneratorLocalData
                .GetLocalData(generator.GeneratorId)
                .GetIconData(Planets.CurrentPlanet.Id);
            Sprite generatorSprite = ResourceService.GetSpriteByKey(generatorIconData.icon_id);

            var isEarth = Services.PlanetService.CurrentPlanetId.Id == 0; 
            var viewService = Services.ViewService;

           /* viewService.Show(ViewType.EnahnceManagerView, new ViewData {
                UserData = new EnhanceManagerData {
                    normalGeneratorSprite = isEarth? staticGeneratorImage.overrideSprite : generatorSprite,
                    enhancedGeneratorSprite = isEarth ? enhancedIconSprite : generatorSprite,
                    managerSprite = managerIcon.overrideSprite,
                    generatorName = GeneratorName,
                    generator = generator
                }
            });*/

            viewService.Show(ViewType.TransportInfoView, new ViewData
            {
                UserData = generator
            });
        }
        /*
        public void OnEnhanceConfirm() {
            IPlayerService playerService = Services.PlayerService;
            if(playerService.IsEnoughCoins(generator.Data.EnhancePrice)) {
                EnhanceAction();
                Analytics.CustomEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_BUY");
                playerService.RemoveCoins(generator.Data.EnhancePrice);
                enhanceConfirm.SetActive(false);
                FacebookEventUtils.LogCoinSpendEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_BUY",
                    generator.Data.EnhancePrice, playerService.Coins);
            } else {
                //notEnoughCoinsPopup.GetComponent<NotEnoughCoinsScreen>().Show(generator.Data.EnhancePrice);
                Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                    UserData = generator.Data.EnhancePrice
                });
                Analytics.CustomEvent($"ENHANCE_WINDOW_{generator.GeneratorId}_CLICK_NOCOINS");
            }
        }

        private void EnhanceAction() {
            Services.GenerationService.Enhance(generator.GeneratorId);
            
        }*/

        private void UpdateViewsWhenEnhanced() {
            if(generator.IsEnhanced) {
                progressBGImage.overrideSprite = enhancedProgressBG;
                progressImage.overrideSprite = enhancedProgressFill;
                buyGeneratorButton.GetComponent<Image>().sprite
                    = enhancedBuySprite;
                buyGeneratorButton.GetComponent<Image>().overrideSprite
                    = enhancedBuySprite;
                buyGeneratorButton.spriteState = new SpriteState {
                    disabledSprite = buyGeneratorButton.spriteState.disabledSprite,
                    highlightedSprite = enhancedBuySprite,
                    pressedSprite = enhancedBuySpritePressed
                };
                buyPriceText.color = buyBaseColor;
                buyCountText.color = buyBaseColor;
                buyPriceSuffixText.color = moneyTextColor;
                profitCountText.color = progressTextColor;
                profitSuffixText.color = progressTextColor;
                //enhanceButton.Deactivate();
            }
        }

        private string dollarsStr = null;
        private string DollarsStr
            => (dollarsStr != null) ? dollarsStr : (dollarsStr = Services.ResourceService.Localization.GetFrequentString("DOLLARS"));

        private string secStr = null;
        private string SecStr
            => (secStr != null) ? secStr : (secStr = GameServices.Instance.ResourceService.Localization.GetFrequentString("SEC"));

        private ManagerInfo Manager
            => (manager != null) ? manager : (manager = Services.ManagerService.GetManager(generator.GeneratorId));

        private IAchievmentServcie achievmentService;
        private IAchievmentServcie AchievmentService
            => (achievmentService != null) ? achievmentService :
            (achievmentService = Services.GetService<IAchievmentServcie>());

        private Sprite buttonPlusSprite = null;
        private Sprite ButtonPlusSprite {
            get {
                if(SpriteDB.SpriteRefs != null ) {
                    return  (buttonPlusSprite != null) ? buttonPlusSprite : (buttonPlusSprite = SpriteDB.SpriteRefs.FirstOrDefault(val => val.Key == "but_plus").Value);
                }
                return null;
            }
        }

        private Sprite alertKickbackSprite = null;
        private Sprite AlertKickbackSprite {
            get {
                if(SpriteDB.SpriteRefs == null ) {
                    return null;
                }
                return (alertKickbackSprite != null) ? alertKickbackSprite : (alertKickbackSprite = SpriteDB.SpriteRefs.FirstOrDefault(val => val.Key == "alert_KickBack").Value);
            }
        }

        public string GeneratorName
            => generator?.Data.Name ?? string.Empty;

        public int GeneratorId
            => generator?.GeneratorId ?? -1;

        private GameObject GetAnimatedObject(int planetId )
            => animatedObjects.FirstOrDefault(obj => obj.planetId == planetId)?.animatedObject ?? null;

        private void ToggleAnimatedObject(int planetId, bool isActivate) {
            foreach(var obj in animatedObjects) {
                obj.animatedObject?.Deactivate();
            }
            if (isActivate) {
                GetAnimatedObject(planetId)?.Activate();
            } else {
                GetAnimatedObject(planetId)?.Deactivate();
            }
        }

        private bool HasAnimatedImageForPlanet(int planetId ) {
            foreach(var animObj in animatedObjects ) {
                if(animObj.planetId == planetId && animObj.animatedObject != null ) {
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public class AnimatedGeneratorObject {
        public int planetId;
        public GameObject animatedObject;
    }

    public class GameObject2DPull {

        private int MaxSize { get; set;  }
        private GameObject Prefab { get; set;  }
        private Transform Parent { get; set; }

        private readonly Stack<GameObject> freeInstances = new Stack<GameObject>();
        private readonly List<GameObject> usedInstances = new List<GameObject>();

        public bool HasObject
            => freeInstances.Count + usedInstances.Count < MaxSize;

        public void Setup(int maxSize, GameObject prefab, Transform parent ) {
            MaxSize = maxSize;
            Prefab = prefab;
            Parent = parent;
        }

        public GameObject UseAtPosition(Vector2 position) {
            if(freeInstances.Count > 0 ) {
                GameObject obj = freeInstances.Pop();
                usedInstances.Add(obj);
                obj.Activate();
                obj.GetComponent<RectTransform>().anchoredPosition = position;
                return obj;
            } else {
                if(HasObject ) {
                    GameObject newInst = GameObject.Instantiate(Prefab, Parent, false);
                    newInst.GetComponent<RectTransform>().anchoredPosition = position;
                    usedInstances.Add(newInst);
                    return newInst;
                } else {
                    return null;
                }
            }
        }

        public void Free(GameObject obj) {
            usedInstances.Remove(obj);
            obj.Deactivate();
            freeInstances.Push(obj);
        }
    }

}