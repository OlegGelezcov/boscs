/* 
using Bos;
using Bos.Data;
using Bos.Debug;
using Bos.Ioc;
using Bos.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class SimpleGeneratorView : GameBehaviour {
    #region Fields
    private int _curMultiplier;

    private Image staticGeneratorIconImage;

    private bool isFxActive = false;
    private ProfitResult profitResult = new ProfitResult(0, 0, 0);

    #endregion

    #region Computed Props


    private int UnitLiveCount { get; set; }
    private int UnitTotalCount { get; set; }
    private int UnitBrokenedCount { get; set; }


    //public Generator Model { get { return _gen; } }

    #endregion

    #region EditorProps

    public int generatorId;
    public float ProgressAnimChangeThreshold = 0.2f;
    public BalanceManager BalanceManager;
    public GameManager GameManager;

    [Header("Containers")]
    public GameObject UnlockedContainer;
    public GameObject LockedContainer;
    public GameObject ResearchContainer;

    [Header("UIElements")]

    public Button UnlockButton;
    public Button ResearchButton;

    public Text LockedPriceNumber;
    public Text LockedPriceWord;

    public Text MultiplierButtonCostNumber;
    public Text MultiplierButtonCostWord;

    public Text GeneratorAmmountNumber;
    public Text GeneratorAmmountWord;

    public Text CountView;
    public Image ProgressBar;
    public Text GeneratorTimeView;

    public Text ProfitMultiplierView;
    public Text SpeedMultiplierView;

    public GameObject IconAnimation;
    public GameObject PlaceholderIcon;

    public Image LevelProgressBar;

    public Image ManagerIcon;

    public Image ManagerNotifyIcon;
    public Button BuyManager;

    public GameObject NotEnoughCoinsPopup;
    public GameObject DependentGeneratorMissingPopup;

    public Text ReserchName;
    public Text ReserchPrice;
    public Image ReserchIcon;

    [Header("FX")]
    public GameObject AutoGenerateParticleFX;
    public ParticleSystem TransitionFX;

    [Header("Enhanced Options")]
    public Sprite EnhancedBuySprite;
    public Sprite EnhancedBuySpritePressed;
    public Sprite EnhancedProgressBG;
    public Sprite EnhancedProgressFill;
    public Sprite EnhancedIconSprite;
    public Sprite BaseSprite;
    public GameObject EnhancedAutoFX;
    public Image ProgressBGImage;
    public GameObject EnhanceConfirm;

    public Color BuyBaseColor;
    public Color MoneyTextColor;
    public Color ProgressTextColor;

    public GameObject EnhanceButton;
    public GameObject[] ToEnableOnEnhance;
    public GameObject[] ToDisableOnEnhance;

    public Text MultiplierText;
    public Button BuyButton;

    public GameObject stubView;

    public GameObject megaTextObject;

    #endregion

    private bool isInitialized = false;
    private Sprite managerSprite = null;

    private GeneratorInfo generatorInfo;
    private GeneratorInfo GeneratorInfo {
        get {
            return (generatorInfo != null) ?
                generatorInfo :
                (generatorInfo = GameServices.Instance.GenerationService.Generators.GetGeneratorInfo(generatorId));
        }
    }


    private void UpdateManagerIconSprite(bool isAutomatic) {
        var spriteData = Services.ResourceService
        .ManagerLocalDataRepository
        .GetIconData(generatorId, Services.PlanetService.CurrentPlanet.Id, isAutomatic);
        managerSprite = Services.ResourceService.GetSprite(spriteData);
    }

    private Sprite buttonPlusSprite = null;
    private Sprite ButtonPlusSprite
        => (buttonPlusSprite != null) ? 
        buttonPlusSprite : 
        (buttonPlusSprite = Services.ResourceService.Sprites.GetObject("but_plus"));

    private Sprite alertKickbackSprite = null;
    private Sprite AlertKickbackSprite
        => (alertKickbackSprite != null) ? 
        alertKickbackSprite : 
        (alertKickbackSprite = Services.ResourceService.Sprites.GetObject("alert_kickback"));

    private ManagerInfo _manager = null;

    public ManagerInfo Manager
        => (_manager != null) ? 
        _manager : 
        (_manager = Services.ManagerService.GetManager(generatorId));

    private float _frame;


    public override void Start() {
        UpdateManagerIconSprite(GeneratorInfo.IsAutomatic);
        UpdateManagerIcon();
        AutoGenerateParticleFX.Deactivate();
        if(EnhancedAutoFX != null ) {
            EnhancedAutoFX.Deactivate();
        }
        UpdateProgressImage();


        StartCoroutine(Starting());
        
        StartCoroutine(StartUpdateMegaObjectImpl());
    }

    private void UpdateManagerIcon(){
        if(managerSprite != null ) {
            ManagerIcon.overrideSprite = managerSprite;
        }
    }

    private void UpdateProgressImage(){
        if(IsAlwaysProgressFull) {
            ProgressBar.fillAmount = 1;
        } else {
            ProgressBar.fillAmount = Mathf.Clamp01((float)GeneratorInfo.GenerateTimer / (float)AccumulateInterval);
        }
    }

    private IEnumerator StartUpdateMegaObjectImpl() {
        yield return new WaitUntil(() => Manager != null);
        UpdateMegaTextObject();
    }


    private IEnumerator Starting() {

        //GeneratorInfoCollection generators = Services.GenerationService.Generators;
        //_gen = GetComponent<Generator>();
        //ManagerIcon.sprite = managerIconSprite; 
        //_pdata = BalanceManager.PlayerData;
        //generators.SetAutomatic(_gen.GeneratorId, Services.ManagerService.IsHired(_gen.Id));

        //if (_gen.Id > 5)
            //yield return Services.TimeService.WaitTimerUpdate();


        //AutoGenerateParticleFX.SetActive(false);
        //if (EnhancedAutoFX != null)
            //EnhancedAutoFX.SetActive(false);


        //var d = Services.Currency.CreatePriceString(_gen.BaseCost, false, " ");
        //ProgressBar.fillAmount = 0;
        IGenerationService generationService = Services.GenerationService;
        IInvestorService investorService = Services.InvestorService;
        profitResult = generationService.CalculateProfitPerSecond(Generator.GeneratorId, UnitLiveCount);
        _tDisplay = TimeSpan.FromSeconds(profitResult.GenerationInterval);
        _placeholderIconImage = PlaceholderIcon.GetComponent<Image>();

        LockedPriceNumber.text = Services.Currency.CreatePriceString(GeneratorInfo.Data.BaseCost, false, " ");
        LockedPriceWord.SetStringForKey(GeneratorInfo.Data.Name);

        if (generationService.IsEnhanced(Generator.Id)) {
            EnhanceGenerator();
        } else {
            if (ToDisableOnEnhance != null) {
                foreach (var x in ToDisableOnEnhance) {
                    x.SetActive(true);
                }
            }
            if (ToEnableOnEnhance != null) {
                foreach (var x in ToEnableOnEnhance) {
                    x.SetActive(false);
                }
            }
        }
        UpdateState();
        if (GeneratorInfo.State == GeneratorState.Researchable) {
            ReserchPrice.text = _gen.CoinPrice.ToString();
            ReserchName.SetStringForKey(_gen.Name.ToUpper());
            UpdateReseachableGeneratorIcon();
        }

        BuyManager.onClick.RemoveAllListeners();
        BuyManager.onClick.AddListener(() => {
            GameServices.Instance.ViewService.Show(Bos.UI.ViewType.ManagementView, Generator.GeneratorId);
            Services.SoundService.PlayOneShot(SoundName.click);
        });

        
        UpdateGenerator();
        OnStateChanged();

        TriggerParticleEffect triggerEffect = BuyButton.GetComponent<TriggerParticleEffect>();
        Vector3Animator buyButtonAnimator = BuyButton.gameObject.GetOrAdd<Vector3Animator>();
        BuyButton.SetListener(() => {
            
            BuyGenerator();
            Services.SoundService.PlayOneShot(SoundName.buyGenerator);
            triggerEffect?.TriggerAnimation();
            buyButtonAnimator.StartAnimation(BuyGeneratorButtonAnimationData);
        });

        _init = true;
    }

    private Vector3AnimationData buyGeneratorAnimationData = null;
    private Vector3AnimationData BuyGeneratorButtonAnimationData {
        get {
            if(buyGeneratorAnimationData == null ) {
                RectTransform butTrs = BuyButton.GetComponent<RectTransform>();
                Vector3Animator butAnimator = BuyButton.gameObject.GetOrAdd<Vector3Animator>();

                Vector3AnimationData data = new Vector3AnimationData {
                    StartValue = Vector3.one,
                    EndValue = 0.85f * Vector3.one,
                    Duration = 0.06f,
                    EaseType = EaseType.EaseInOutQuad,
                    AnimationMode = BosAnimationMode.Single,
                    Target = BuyButton.gameObject,
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
                            Target = BuyButton.gameObject,
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

    private void UpdateReseachableGeneratorIcon() {
        if (GeneratorInfo.State == GeneratorState.Researchable) {
            if (PlanetService.CurrentPlanet.Id == Bos.PlanetService.kEarthId) {
                ReserchIcon.sprite = BaseSprite;
            } else {
                int currentPlanetId = PlanetService.CurrentPlanet.Id;
                var iconData = Services.ResourceService.GeneratorLocalData.GetLocalData(Generator.GeneratorId).GetIconData(currentPlanetId);
                if (iconData != null) {
                    ReserchIcon.overrideSprite = Services.ResourceService.Sprites.GetObject(iconData.icon_id);
                } else {
                    ReserchIcon.sprite = BaseSprite;
                }
            }
        }
    }

    public override void OnEnable() {
        //GameServices.Instance.Container.Fill(this);
        GameEvents.CompanyCashChanged += OnCompanyCashChanged;
        GameEvents.GeneratorUnitsCountChanged += OnTransportUnitsChanged;
        GameEvents.GeneratorAchievmentsReceived += OnGeneratorAchievmentsReceived;
        GameEvents.GeneratorStateChanged += OnGeneratorStateChanged;
        GameEvents.AutomaticChanged += OnAutomaticChanged;
        GameEvents.PlanetStateChanged += OnPlanetStateChanged;
        GameEvents.LegacyBuyMultiplierChanged += OnLegacyBuyMultiplierChanged;
        GameEvents.EfficiencyLevelChanged += OnEfficiencyLevelChanged;
        GameEvents.RollbackLevelChanged += OnRollbackLevelChanged;

        UnitLiveCount = Services.TransportService.GetUnitLiveCount(Generator.GeneratorId);
        UnitTotalCount = Services.TransportService.GetUnitTotalCount(Generator.GeneratorId);
        UnitBrokenedCount = Services.TransportService.GetUnitBrokenedCount(Generator.GeneratorId);
        //Debug.Log($"after fill planetservices != null => {planetService != null}".Colored(ConsoleTextColor.magenta));
    }



    public override void OnDisable() {
        GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
        GameEvents.GeneratorUnitsCountChanged -= OnTransportUnitsChanged;
        GameEvents.GeneratorAchievmentsReceived -= OnGeneratorAchievmentsReceived;
        GameEvents.GeneratorStateChanged -= OnGeneratorStateChanged;

        GameEvents.AutomaticChanged -= OnAutomaticChanged;
        GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
        GameEvents.LegacyBuyMultiplierChanged -= OnLegacyBuyMultiplierChanged;
        GameEvents.EfficiencyLevelChanged -= OnEfficiencyLevelChanged;
        GameEvents.RollbackLevelChanged -= OnRollbackLevelChanged;
    }

    private void OnEfficiencyLevelChanged(int oldLevel, int newLevel, ManagerInfo otherManager ) {
        if(Manager != null && (Manager.Id == otherManager.Id)) {
            UpdateMegaTextObject();
        }
    }

    private void OnRollbackLevelChanged(int oldLevel, int newLevel, ManagerInfo otherManager ) {
        if(Manager != null && (Manager.Id == otherManager.Id)) {
            UpdateMegaTextObject();
        }
    }

    private void OnLegacyBuyMultiplierChanged(int generatorId, int value) {
        if(Generator != null) {
            if(generatorId == Generator.GeneratorId) {
                UpdateMultiplier(value);
            }
        }
    }

    private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
        IResourceService resourceService = GameServices.Instance.ResourceService;
        if(Generator != null && (Generator.GeneratorId == resourceService.Defaults.teleporterId)) {
            if(planet.Id == Bos.PlanetService.kMarsId) {
                UpdateState();
            }
        }

        UpdateGeneratorIconOnOtherPlanets();
        UpdateReseachableGeneratorIcon();
    }

    private void OnAutomaticChanged(int generatorId, bool isAutomatic ) {
        if(generatorId == Generator.GeneratorId) {
            UpdateManagerIconSprite(isAutomatic);
        }
    }

    private void OnGeneratorStateChanged(GeneratorState oldState, GeneratorState newState, GeneratorInfo info) {
        if (info.GeneratorId == GeneratorInfo.GeneratorId) {
            OnStateChanged();
        }
    }

    private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue)
        => UpdateGenerator();

    private void OnTransportUnitsChanged(TransportUnitInfo info) {
        if (Generator != null) {
            if (info.GeneratorId == Generator.Id) {
                UnitLiveCount = info.LiveCount;
                UnitTotalCount = info.TotalCount;
                UnitBrokenedCount = info.BrokenedCount;
                UpdateGeneratorLevelProgress();
                UpdateGenerator();
                
            }
        }
    }

    private void OnGeneratorAchievmentsReceived(int generatorId, List<ExtendedAchievmentInfo> achievments) {
        if (_gen != null) {
            if (_gen.GeneratorId == generatorId) {
                UpdateGeneratorLevelProgress();
            }
        }
    }


    private void UpdateGenerator() {
        if (!_init) return;

        UpdateState();
        if (GeneratorInfo.State != GeneratorState.Active) return;

        UpdateCount();
        UpdateMultiplier(_gen.BuyMultiplier);
        //UpdateLevelProgress();

        if (profitResult.GenerationInterval < 1) {
            GeneratorTimeView.gameObject.SetActive(false);
            GeneratorAmmountNumber.alignment = TextAnchor.MiddleCenter;
            GeneratorAmmountWord.alignment = TextAnchor.MiddleCenter;
        }
    }

    private double AccumulateInterval {
        get {
            if(GeneratorInfo.IsManual) {
                return profitResult.GenerationInterval;
            } else {
                if(profitResult.GenerationInterval < UnlockedGeneratorView.kMinRoundInterval) {
                    return 1.0f;
                } else {
                    return profitResult.GenerationInterval;
                }
            }
        }
    }

    private float AccumulateTimer
        => GeneratorInfo?.GenerateTimer ?? 0f;

    private float RemainTime
        => (float)(AccumulateInterval - AccumulateTimer);

    private bool IsAlwaysProgressFull
        => GeneratorInfo.IsAutomatic && profitResult.GenerationInterval < UnlockedGeneratorView.kMinRoundInterval;

    private double accumulatedBalance = 0f;

    public override void Update() {


        if(GeneratorInfo != null && GeneratorInfo.IsAutomatic) {
            if(!GeneratorInfo.IsGenerationStarted) {
                GeneratorInfo.SetGenerationStarted(true);
            }
        }

        if(GeneratorInfo.IsGenerationStarted) {
            AddAccumulationTimer(Time.deltaTime);
            accumulatedBalance += profitResult.ValuePerRound * Time.deltaTime;
            if(AccumulateTimer >= AccumulateInterval) {
                FinalizeAccumulation();
                if(GeneratorInfo.IsManual) {
                    GeneratorInfo.SetGenerationStarted(false);
                    
                }
            }
        }
    }

    private void AddAccumulationTimer(float deltaTime ) {
        GeneratorInfo.AddGenerateTimer(deltaTime);
        OnAccumulationProgressUpdated((float)BosUtils.Clamp01(AccumulateTimer / AccumulateInterval));
        UpdateTimerText(RemainTime);
    }

    private void OnAccumulationProgressUpdated(float percent) {
        ProgressBar.fillAmount = IsAlwaysProgressFull ? 1.0f : percent;
    }

    private void UpdateFirst() {
        if (_frame < 0.1f) {
            _frame += Time.deltaTime;
            return;
        }
        UpdateAutomaticGeneration();
        _frame = 0;
    }

    private void UpdateSecond() {

        if ( !_init || GeneratorInfo.State != GeneratorState.Active) return;

        IGenerationService generationService = GameServices.Instance.GenerationService;
        //var investorPrc = GameServices.Instance.InvestorService.InvestorPercent;

        profitResult = generationService.CalculateProfitPerSecond(Generator.GeneratorId, UnitLiveCount);

        if (!GeneratorInfo.IsGenerationStarted) {
            _tDisplay = TimeSpan.FromSeconds(profitResult.GenerationInterval);
        }

        string answer = string.Format("{0:D2}:{1:D2}:{2:D2}", _tDisplay.Hours, _tDisplay.Minutes, _tDisplay.Seconds);
        GeneratorTimeView.text = answer;

        UpdateIncome();

        var playerService = GameServices.Instance.PlayerService;

        if (GeneratorInfo.IsManual && Manager.BaseCost < playerService.CompanyCash.Value) {
            BuyManager.image.sprite = ButtonPlusSprite; //SpriteDB.SpriteRefs.FirstOrDefault(val => val.Key == "but_plus").Value;
            BuyManager.image.enabled = true;
        } else if (GeneratorInfo.IsAutomatic && Manager.CashOnHand > 0 && Manager.NextKickBackTime < Services.TimeService.Now) {
            BuyManager.image.sprite = AlertKickbackSprite; //SpriteDB.SpriteRefs.FirstOrDefault(val => val.Key == "alert_KickBack").Value;
            BuyManager.image.enabled = true;
        } else {
            BuyManager.image.enabled = false;
        }

        if (PlanetService.CurrentPlanet.Id == 0) {
            if (_gen.WillOverrideIcon) {
                IconAnimation.Deactivate();
                PlaceholderIcon.Activate();
                if (_placeholderIconImage.overrideSprite == null || _placeholderIconImage.overrideSprite.name != EnhancedIconSprite.name) {
                    _placeholderIconImage.overrideSprite = EnhancedIconSprite;
                }
            }
        } else {
            UpdateGeneratorIconOnOtherPlanets();
        }

        UpdateProgressBar();
    }

    private GeneratorPlanetIconData generatorPlanetIconData = null;
    private void UpdateGeneratorIconOnOtherPlanets() {
        if (generatorPlanetIconData == null || (generatorPlanetIconData.planet_id != PlanetService.CurrentPlanet.Id)) {
            generatorPlanetIconData = Services.ResourceService.GeneratorLocalData.GetLocalData(Generator.GeneratorId).GetIconData(PlanetService.CurrentPlanet.Id);
            if (generatorPlanetIconData != null) {
                IconAnimation?.Deactivate();
                PlaceholderIcon.Activate();
                if(generatorPlanetIconData.icon_id.IsValid()) {
                    var sprite = Services.ResourceService.Sprites.GetObject(generatorPlanetIconData.icon_id);
                    if(sprite != null ) {
                        if(_placeholderIconImage) {
                            _placeholderIconImage.overrideSprite = sprite;
                        }
                    }
                } else {
                    _placeholderIconImage.MakeTransparent();
                    Debug.LogError($"generator planet icon ivalid for planet => {generatorPlanetIconData.planet_id}");
                }
            }
        }
    }


    private bool IsLockedByMars() {
        IResourceService resourceService = GameServices.Instance.ResourceService;
        if(Generator.GeneratorId == resourceService.Defaults.teleporterId) {
            bool isMarsOpened = PlanetService.IsMarsOpened;
            if(!isMarsOpened) {
               
                stubView.Activate();
                GeneratorInfoCollection generators = GameServices.Instance.GenerationService.Generators;
                generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Researchable);
                ResearchButton.interactable = false;
                return true;
            } else {
                ResearchButton.interactable = true;
                stubView.Deactivate();
                
            }
        }
        return false;
    }

    private void UpdateState() {
        var playerService = GameServices.Instance.PlayerService;
        GeneratorInfoCollection generators = GameServices.Instance.GenerationService.Generators;

        if (!IsLockedByMars()) {
            if (!GameServices.Instance.GenerationService.IsResearched(Generator.Id)) {
                generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Researchable);
                return;
            } 

            if (!GameServices.Instance.TransportService.HasUnits(_gen.Id)) {
                generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Locked);

                if (playerService.CompanyCash.Value >= _gen.BaseCost) {
                    if (!_gen.IsDependent) {
                        generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Unlockable);
                    } else {
                        if (GameServices.Instance.TransportService.HasUnits(_gen.RequiredOwnedGenerator)) {
                            generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Unlockable);
                        } else {
                            generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Locked);
                        }
                    }
                }
            } else {
                generators.SetState(GeneratorInfo.GeneratorId, GeneratorState.Active);
            }
        }
    }

    private void OnStateChanged() {
        if (GeneratorInfo.State != _lastGeneratorState) {
            switch (GeneratorInfo.State) {
                case GeneratorState.Active:
                    UnlockedContainer.SetActive(true);
                    LockedContainer.SetActive(false);
                    if (ResearchContainer != null)
                        ResearchContainer.SetActive(false);
                    break;
                case GeneratorState.Unlockable:
                    UnlockedContainer.SetActive(false);
                    LockedContainer.SetActive(true);
                    if (ResearchContainer != null)
                        ResearchContainer.SetActive(false);
                    UnlockButton.interactable = true;
                    break;
                case GeneratorState.Locked:
                    UnlockedContainer.SetActive(false);
                    LockedContainer.SetActive(true);
                    if (ResearchContainer != null)
                        ResearchContainer.SetActive(false);
                    UnlockButton.interactable = false;
                    break;
                case GeneratorState.Researchable:
                    UnlockedContainer.SetActive(false);
                    LockedContainer.SetActive(false);

                    if (ResearchContainer != null) {
                        if (!IsLockedByMars()) {
                            ResearchContainer.SetActive(true);
                        } else {
                            ResearchContainer.Deactivate();
                        }
                    }
                    break;
            }
            _lastGeneratorState = GeneratorInfo.State;
        }
    }

    private void UpdateCount() {
        CountView.text = UnitLiveCount.ToString();
        if(UnitBrokenedCount > 0 ) {
            CountView.text += $"/{UnitBrokenedCount}".Colored(ConsoleTextColor.red);
        }
    }
    private void UpdateProgressBar() {
        var isEnhanced = Services.GenerationService.IsEnhanced(generatorId);
        if (GeneratorInfo.IsManual) {
            if (isFxActive) {
                if (!isEnhanced) {
                    AutoGenerateParticleFX.SetActive(false);
                } else {
                    if (EnhancedAutoFX != null) {
                        EnhancedAutoFX.SetActive(false);
                    }
                }
                isFxActive = false;
            }
        }

        if (GeneratorInfo.IsAutomatic && profitResult.GenerationInterval <= ProgressAnimChangeThreshold) {
            if (!isFxActive) {
                if (!isEnhanced) {
                    AutoGenerateParticleFX.SetActive(true);
                }
                else {
                    if (EnhancedAutoFX != null) {
                        EnhancedAutoFX.SetActive(true);
                    }                
                }
                isFxActive = true;
            }
            return;
        }

        if (!GeneratorInfo.IsGenerationStarted) {
            ProgressBar.fillAmount = 0;
        }
    }

    private string dollarsStr = null;
    private string DollarsStr
        => (dollarsStr != null) ? dollarsStr : (dollarsStr = GameServices.Instance.ResourceService.Localization.GetString("DOLLARS"));

    private string secStr = null;
    private string SecStr
        => (secStr != null) ? secStr : (secStr = GameServices.Instance.ResourceService.Localization.GetString("SEC"));

    private void UpdateIncome() {

        if (profitResult.GenerationInterval <= 1 && GeneratorInfo.IsAutomatic) {
            var str = Services.Currency.CreatePriceStringSeparated(profitResult.ValuePerSecond);
            GeneratorAmmountNumber.text = str[0];

            if (str.Length == 1) {
                GeneratorAmmountWord.text = $"{DollarsStr} / {SecStr}";
            } else {
                GeneratorAmmountWord.text = $"{str[1].ToUpper()} / {SecStr}";
            }
        } else {

            var str = Services.Currency.CreatePriceStringSeparated(profitResult.ValuePerRound);
            GeneratorAmmountNumber.text = str[0];

            if (str.Length == 1) {
                GeneratorAmmountWord.text = DollarsStr;
            } else {
                GeneratorAmmountWord.text = str[1].ToUpper();
            }
        }
    }





    private void UpdateMultiplier(int mul) {
        var playerService = GameServices.Instance.PlayerService;
        
        if (mul == 0) {
            double companyCash = playerService.CompanyCash.Value; 
            mul = GenerationService.GetMaxNumberBuyable(companyCash, UnitTotalCount, GeneratorInfo.Data);

            if (mul == 0 || mul == int.MinValue) {
                mul = int.MaxValue;
            }
        }

        if (mul == int.MaxValue) {
            MultiplierText.text = "1";
            mul = 1;
        } else {
            MultiplierText.text = mul.ToString();
        }

        if (mul <= 0) {
            var qq = Services.Currency.CreatePriceStringSeparated( GenerationService.CalculatePrice(1, 0, GeneratorInfo));

            MultiplierText.text = "1";
            BuyButton.interactable = false;


            MultiplierButtonCostNumber.text = qq[0];
            if (qq.Length > 1)
                MultiplierButtonCostWord.text = qq[1].ToUpper();
            else
                MultiplierButtonCostWord.text = DollarsStr; //GameServices.Instance.ResourceService.Localization.GetString("DOLLARS"); //"DOLLARS".GetLocale(LocalizationDataType.ui).ToUpper();

            return;
        }

        var limit = GenerationService.CalculatePrice(mul, UnitTotalCount, GeneratorInfo);

        if (playerService.CompanyCash.Value < limit) {
            BuyButton.interactable = false;
        } else {
            MultiplierText.text = mul.ToString();
            BuyButton.interactable = true;
        }

        _curMultiplier = mul;

        var str = Services.Currency.CreatePriceStringSeparated(limit);

        MultiplierButtonCostNumber.text = str[0];
        if (str.Length > 1)
            MultiplierButtonCostWord.text = str[1].ToUpper();
        else
            MultiplierButtonCostWord.text = DollarsStr; //"DOLLARS".GetLocale(LocalizationDataType.ui).ToUpper();
    }

    private void UpdateGeneratorLevelProgress() {


        if (_gen != null && UnitTotalCount > 0) {
            LevelProgressBar.fillAmount = AchievmentService.GetProgressForGenerator(_gen.GeneratorId, UnitTotalCount);
        } else {
            LevelProgressBar.fillAmount = 0;
        }
    }

    public void Unlock() {
        TransitionFX.Play();
        BalanceManager.BuyGenerator(_gen);
        UpdateGenerator();
    }

    public void Research() {
        var playerService = GameServices.Instance.PlayerService;

        if (playerService.Coins < _gen.CoinPrice) {
            //NotEnoughCoinsPopup.SetActive(true);
            NotEnoughCoinsPopup.GetComponent<NotEnoughCoinsScreen>().Show(_gen.CoinPrice);
            return;
        }


        //BalanceManager.IAPManager.Coins.Value -= _gen.CoinPrice;
        playerService.RemoveCoins(_gen.CoinPrice);

        GameServices.Instance.GenerationService.Research(_gen.Id);

        //_pdata.ResearchedGenerators.Add(_gen.Id, true);
        TransitionFX.Play();

        Analytics.CustomEvent(AnalyticsStrings.RESEARCH_ZEPPELIN);
        UpdateGenerator();
    }

    public void Generate() {
        if(GeneratorInfo.IsManual) {
            if (!GeneratorInfo.IsGenerationStarted) {
                accumulatedBalance = 0f;
                GeneratorInfo.SetGenerateTimer(0f);
                GeneratorInfo.SetGenerationStarted(true);


                if (!GeneratorInfo.IsGenerationStarted) {
                    accumulatedBalance = 0f;
                }
            }
        }
    }

    public void BuyGenerator() {
        var services = GameServices.Instance;
        var console = services?.GetService<IConsoleService>();

        try {
            if (_curMultiplier == int.MaxValue) {
                UpdateGenerator();
                Debug.Log("Current multiplier is int.MaxValue!".Colored(ConsoleTextColor.red));
                return;
            }

            if(GeneratorInfo.IsGenerationStarted) {
                FinalizeAccumulation();
            }
            Services.GenerationService.BuyGenerator(GeneratorInfo, _curMultiplier);
            UpdateGenerator();
        } catch (System.Exception exception) {
            console?.AddOutput(exception.Message, ConsoleTextColor.red, true);
            console?.AddOutput(exception.StackTrace, ConsoleTextColor.red, true);
        }
    }

    private void FinalizeAccumulation() {
        AddCompanyCash(accumulatedBalance);
        SetAccumulationTimer(0);
    }

    private void SetAccumulationTimer(float val) {
        GeneratorInfo?.SetGenerateTimer(0);
        OnAccumulationProgressUpdated((float)BosUtils.Clamp01(AccumulateTimer / AccumulateInterval));
        UpdateTimerText(RemainTime);
    }

    private void AddCompanyCash(double value) {
        if(GeneratorInfo.IsAutomatic) {
            Manager.AddCash(value, isGenerateEvent: true);
        }
        BalanceManager.AddBalance(value);
    }

 
    private void OnGenerationTick(object sender, TimerEventArgs args) {
        _tDisplay = TimeSpan.FromSeconds(args.Duration - args.CurrentTime);

        if (profitResult.GenerationInterval > ProgressAnimChangeThreshold)
            ProgressBar.fillAmount = args.PercentComplete;
    }
    private void timer_Stopped(object sender, StoppedTimerEventArgs e) {
        if (profitResult.GenerationInterval > ProgressAnimChangeThreshold)
            ProgressBar.fillAmount = 0;


        _tDisplay = TimeSpan.FromSeconds(profitResult.GenerationInterval);

        var tempIncome = GameServices.Instance.GenerationService.CalculateProfitPerSecond(Generator.GeneratorId, 
            UnitLiveCount).ValuePerRound;

        var inc = tempIncome;
        AddBalance(inc * e.Times);

        //Debug.Log("TIMER STOPPED");
        //Debug.Log("AddBalance from SimpleGeneratorView::timer_Stopped," + _gen.Name + " -> " + inc * e.Times);
    }



    public void ShowEnhanceConfirm() {
        EnhanceConfirm.GetComponent<EnhancementWindow>().Show(this);
        Analytics.CustomEvent($"ENHANCE_WINDOW_{_gen.Id}_OPEN");
    }

    public void EnhanceClick() {
        var playerService = GameServices.Instance.PlayerService;
        if (playerService.Coins < _gen.EnhancePrice) {
            //NotEnoughCoinsPopup.SetActive(true);
            NotEnoughCoinsPopup.GetComponent<NotEnoughCoinsScreen>().Show(_gen.EnhancePrice);
            Analytics.CustomEvent($"ENHANCE_WINDOW_{_gen.Id}_CLICK_NOCOINS");
            return;
        }

        EnhanceGenerator();
        Analytics.CustomEvent($"ENHANCE_WINDOW_{_gen.Id}_BUY");
        //GlobalRefs.IAP.Coins.Value -= _gen.EnhancePrice;
        playerService.RemoveCoins(_gen.EnhancePrice);
        EnhanceConfirm.SetActive(false);
        FacebookEventUtils.LogCoinSpendEvent($"ENHANCE_WINDOW_{_gen.Id}_BUY", _gen.EnhancePrice, playerService.Coins);
    }

    public void EnhanceGenerator() {
        //IsEnhanced = true;
        GameServices.Instance.GenerationService.Enhance(Generator.Id);

        UnlockButton.gameObject.GetComponent<Image>().sprite = SpriteDB.SpriteRefs["unlock_enhanced"];
        UnlockButton.spriteState = new SpriteState() {
            disabledSprite = UnlockButton.spriteState.disabledSprite,
            pressedSprite = SpriteDB.SpriteRefs["unlock_enhanced_pressed"],
            highlightedSprite = SpriteDB.SpriteRefs["unlock_enhanced"]
        };

        ProgressBGImage.sprite = EnhancedProgressBG;
        ProgressBar.sprite = EnhancedProgressFill;
        BuyButton.gameObject.GetComponent<Image>().sprite = EnhancedBuySprite;
        BuyButton.spriteState = new SpriteState() {
            disabledSprite = BuyButton.spriteState.disabledSprite,
            highlightedSprite = EnhancedBuySprite,
            pressedSprite = EnhancedBuySpritePressed };
        MultiplierButtonCostNumber.color = BuyBaseColor;
        MultiplierText.color = BuyBaseColor;
        MultiplierButtonCostWord.color = MoneyTextColor;
        GeneratorAmmountNumber.color = ProgressTextColor;
        GeneratorAmmountWord.color = ProgressTextColor;

        _gen.WillOverrideIcon = true;
        //_gen.OverrideSource = EnhancedIconSprite;

        if (ToEnableOnEnhance != null) {
            foreach (var x in ToEnableOnEnhance) {
                x.SetActive(true);
            }
        }

        if (ToDisableOnEnhance != null) {
            foreach (var x in ToDisableOnEnhance) {
                x.SetActive(false);
            }
        }

        EnhanceButton.SetActive(false);
    }

    private void AddBalance(double balance) {
        if (GeneratorInfo.IsAutomatic) {
            //_manager.AddBalance(balance);
            Manager.AddCash(balance, isGenerateEvent: true);
        }

        BalanceManager.AddBalance(balance);
    }



        private void UpdateMegaTextObject(){
            if(manager != null ) {
                if(manager.IsFullMega(Services.ResourceService.ManagerImprovements)) {
                    megaTextObject?.Activate();
                    if(megaTextObject != null ) {
                        var scaleData1 = AnimUtils.GetScaleAnimData(1, 1.2f, 0.5f, EaseType.EaseInOutQuad, megaTextObject.GetComponent<RectTransform>(), ()=>{
                            var scaleData2 = AnimUtils.GetScaleAnimData(1.2f, 1, 0.5f, EaseType.EaseInOutQuad, megaTextObject.GetComponent<RectTransform>());
                            megaTextObject.GetOrAdd<Vector2Animator>().StartAnimation(scaleData2);
                        });
                        megaTextObject.GetOrAdd<Vector2Animator>().StartAnimation(scaleData1);
                    }
                } else {
                    megaTextObject?.Deactivate();
                }
            } else {
                megaTextObject?.Deactivate();
            }
        }
}*/

