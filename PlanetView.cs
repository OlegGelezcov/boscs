namespace Bos.UI {
    using System;
    using System.Collections;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlanetView : GameBehaviour {

        public int planetId;
        public Material grayMaterial;
        public Button actionButton;
        public Text actionButtonName;
        public Image buttonRocketImage;
        public GameObject progressParent;
        public Image progressFillImage;
        public Text progressText;
        public GameObject priceParent;
        public Text priceCashText;
        public Text priceSecuritiesText;
        public TMPro.TextMeshProUGUI planetNameText;
        public GameObject closedContent;
        public GameObject openedContent;

        public PlanetShipModuleCollection moduleCollection;
        public GameObject modulePriceSection;

        private bool isExpanded = false;

        private PlanetInfo planet;
        private IPlanetViewContext context;

        private Color cashColor = Color.white;
        private Color securitiesColor = Color.white;

        public readonly Subject<bool> ButtonAnimatedSubject = new Subject<bool>();

        private PlanetInfo Planet {
            get {
                if(planet == null ) {
                    planet = Planets.GetPlanet(planetId);
                }
                return planet;
            }
        }
        private bool IsInitialized { get; set; }
        private bool IsCashChanged { get; set; }

        public void Setup(IPlanetViewContext context) {
            this.context = context;
           
            if(!IsInitialized) {
                if (actionButton != null) {
                    actionButton.GetComponent<Image>().material =
                        ResourceService.Materials.GetMaterial($"open_planet_button_{planetId}", grayMaterial);
                }
                SetButtonGrayed(false);

                GameEvents.ModuleStateChangedObservable.Subscribe(args => {
                    if(args.NewState == ShipModuleState.Opened ) {
                        if(context != null ) {
                            Setup(context);
                        }
                    }
                }).AddTo(gameObject);

                GameEvents.PlanetStateChangedObservable.Value.Subscribe(args => {
                    OnPlanetStateChanged(args.OldState, args.NewState, args.Planet);
                }).AddTo(gameObject);

                GameEvents.CompanyCashChangedObservable.Subscribe(args => {
                    OnCompanyCashChanged(args.OldValue, args.NewValue);
                    IsCashChanged = true;
                }).AddTo(gameObject);

                GameEvents.SecuritiesChangedObservable.Subscribe(args => {
                    OnSecuritiesChanged(args.OldValue, args.NewValue);
                    IsCashChanged = true;
                }).AddTo(gameObject);

                Observable.Interval(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => {
                    if (Planet.State == PlanetState.Opening) {
                        UpdateOpeningFilling();
                    }
                }).AddTo(gameObject);

                Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                    if (IsCashChanged) {
                        IsCashChanged = false;
                        UpdatePlanetView();
                    }
                }).AddTo(gameObject);

                IsInitialized = true;
            }

            UpdatePlanetView();
            UpdatePlanetNameText();
            UpdateOpenedClosedContent();
        }


        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planetInfo ) {
            UpdatePlanetView();
            UpdatePlanetNameText();

            if(planetInfo.Id == planetId) {
                UpdateOpenedClosedContent();
            }
        }

        private void OnCompanyCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount) {
            if(Planet.State == PlanetState.Closed) {
                UpdatePlanetView();
            }
        }

        private void OnSecuritiesChanged(CurrencyNumber oldCount, CurrencyNumber newCount) {
            if(Planet.State == PlanetState.Closed) {
                UpdatePlanetView();
            }
        }

        public void UpdatePlanetView() {
            if (context != null) {

                if (Planet.Id != PlanetConst.EARTH_ID) {

                    switch (Planet.State) {
                        case PlanetState.Closed: {
                                
                                UpdateClosedPlanetView();
                            }
                            break;
                        case PlanetState.Opening: {
                                UpdateOpeningPlanetView();
                            }
                            break;
                        case PlanetState.ReadyToOpen: {
                                UpdateReadytOpenPlanetView();
                            }
                            break;
                        case PlanetState.Opened: {
                                UpdateOpenedPlanetView();
                            }
                            break;
                    }


                }
            }
        }


        private void UpdateReadytOpenPlanetView() {
            actionButton.Activate();
            actionButton.interactable = true;
            SetButtonGrayed(false);

            actionButtonName.Activate();
            actionButtonName.text = Services.ResourceService.Localization.GetString("btn_launch");

            progressParent.Deactivate();
            priceParent.Deactivate();
            buttonRocketImage.Activate();
            moduleCollection.Deactivate();
            //buttonRocketImage.overrideSprite = context.EndOpenedSprite;

            actionButton.SetListener(() => {
                Services.GetService<IPlanetService>().SetOpened(planetId);
                actionButton.GetComponent<Animator>()?.SetTrigger("click");
                Services.ViewService.Remove(ViewType.PlanetsView);
                GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.OpenPlanetClicked, Planet.Id));
            });
        }

        /*
        private void ViewWhenModuleNotPurchasedCollapsed()
        {
            actionButton.Activate();
            actionButton.SetListener(() => {
                ExpandClickAction();
            });
            SetButtonGrayed(false);

            actionButtonName.Activate();
            actionButtonName.text = LocalizationObj.GetString("btn_change");

            buttonRocketImage.Activate();

            priceParent.Deactivate();
            progressParent.Deactivate();
            moduleCollection.Deactivate();
            modulePriceSection.Deactivate();

        }
        
        private void ViewWhenModulePurchasedCollapsed()
        {
            actionButton.Activate();
            actionButton.SetListener(() => ExpandClickAction());
            SetButtonGrayed(false);

            actionButtonName.Activate();
            actionButtonName.text = LocalizationObj.GetString("lbl_simple_fly");

            buttonRocketImage.Activate();

            priceParent.Deactivate();
            progressParent.Deactivate();
            moduleCollection.Deactivate();
            modulePriceSection.Deactivate();
        }*/

        private void DrawModuleNotPurchased()
        {
            actionButton.Activate();
            actionButton.SetListener(() => {
                OnBuyModuleClick();
            });
            if(IsAllowBuyModuleForCurrentPlanet()) {
                SetButtonGrayed(false);
                actionButtonName.text = LocalizationObj.GetString("lbl_simple_buy");
            } else {
                SetButtonGrayed(true);
                actionButtonName.text = LocalizationObj.GetString("lbl_hide");
            }

            actionButtonName.Deactivate();
            buttonRocketImage.Deactivate();
            priceParent.Deactivate();
            progressParent.Deactivate();
            moduleCollection.Activate();
            modulePriceSection.Activate();
        }

        private void OnBuyModuleClick() {
            ModuleTransactionState transactionState;
            if (Services.Modules.IsAllowBuyModule(Planet.LocalData.module_id, out transactionState)) {
                var result = Services.Modules.BuyModule(Planet.LocalData.module_id);
                if (result == ModuleTransactionState.Success) {
                    Debug.Log($"purchase module with status: {result}");
                }
                AnimateButton(() => UpdatePlanetView());
            } else {
                if (!isExpanded) {
                    Expand();
                }
            }
            Sounds.PlayOneShot(SoundName.click);
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.OpenPlanetClicked, Planet.Id));
        }



        private void DrawModulePurchased()
        {
            actionButton.Activate();
            actionButton.SetListener(() => OnFlyPlanetClick());

            actionButtonName.Activate();
            actionButtonName.text = LocalizationObj.GetString("lbl_simple_fly");

            SetButtonGrayed(IsAllowBuy() ? false : true);


            buttonRocketImage.Activate();

            priceParent.Activate();
            UpdatePrice();

            progressParent.Deactivate();
            moduleCollection.Deactivate();
            modulePriceSection.Deactivate();
        }

        private void OnFlyPlanetClick() {
            if (IsAllowBuy()) {
                priceParent.Deactivate();
                Planets.StartOpening(planetId);
                ShowFlyingModuleView();
                AnimateButton(() => UpdatePlanetView());
                if(!isExpanded) {
                    Expand();
                } 
            } else {
                if (isExpanded) {
                    Collapse();
                } else {
                    Expand();
                }
            }
            Sounds.PlayOneShot(SoundName.click);
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.OpenPlanetClicked, Planet.Id));
        }

        private void ExpandClickAction()
        {
            foreach (var view in context.PlanetViews)
            {
                if (view.planetId != planetId)
                {
                    view.Collapse();

                }
                else
                {
                    view.Expand();
                }
                view.UpdatePlanetView();
            }

            AnimateButton(() => { });
            Sounds.PlayOneShot(SoundName.click);
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.OpenPlanetClicked, Planet.Id));
        }



        private void ShowFlyingModuleView() {
            ViewService.Show(ViewType.BuyModuleView, new ViewData {
                ViewDepth = ViewService.NextViewDepth,
                UserData = new ModuleViewModel {
                    ScreenType = ModuleScreenType.Flight
                }
            });
        }

        private void UpdateClosedPlanetView() {
            UpdatePrice();
            if (Services.Modules.IsOpened(Planet.LocalData.module_id))
            {
                DrawModulePurchased();
            } else
            {
                DrawModuleNotPurchased();
            }

            planetNameText.colorGradientPreset = context.NotSelectedColorGradient;

        }

        private void AnimateButton(Action completeAction)
        {
            SendButtonStartAnimation();
            actionButton.GetComponent<Animator>()?.SetTrigger("click");
            SendButtonCompleteAnimation();
            if(completeAction != null ) {
                Services.Execute(completeAction, 0.25f);
            }
        }


        private void SendButtonStartAnimation()
        {
            ButtonAnimatedSubject.OnNext(true);
        }

        private void SendButtonCompleteAnimation()
        {
            //send button animation complete after 0.25 seconds (duration of animation)
            IDisposable disposable = null;
            Observable.Timer(TimeSpan.FromSeconds(0.25f)).Do(_ => ButtonAnimatedSubject.OnNext(false)).Subscribe(_ => { }, () =>
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                    disposable = null;
                }
            });
        }

        private bool IsAllowBuy() {
            var prevPlanet = Services.GetService<IPlanetService>().GetPlanet(planetId - 1);
            if(prevPlanet == null || prevPlanet.State == PlanetState.Opened ) {
                if(Player.IsEnoughCurrencies(Planet.Data.Prices)) {
                    if (IsModuleBuyed()) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsModuleBuyed() {
            var planetLocalData = Planets.GetPlanet(planetId).LocalData;
            if(!planetLocalData.IsModuleRequired) {
                return true;
            }
            return Services.Modules.IsOpened(planetLocalData.module_id);
        }

        private bool IsAllowBuyModuleForCurrentPlanet() {
            var planetLocalData = Planets.GetPlanet(planetId).LocalData;
            if(!planetLocalData.IsModuleRequired ){
                return true;
            }
            ModuleTransactionState state;
            if(Services.Modules.IsAllowBuyModule(planetLocalData.module_id, out state)) {
                return true;
            }
            return false;
        }

        private void UpdateOpeningPlanetView() {
            actionButton.Activate();
            actionButton.interactable = true;
            SetButtonGrayed(false);
            actionButtonName.Activate();
            actionButtonName.text = LocalizationObj.GetString("btn_speedup");
            buttonRocketImage.Activate();   
            priceParent.Deactivate();
            progressParent.Activate();
            UpdateOpeningFilling();
            moduleCollection.Deactivate();

            actionButton.SetListener(() => {
                /*
                Services.AdService.WatchAd("SpeedUpPlanet", () => {
                    planet.ApplySpeedMult();
                });*/
                ShowFlyingModuleView();
                actionButton.GetComponent<Animator>()?.SetTrigger("click");
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.OpenPlanetClicked, Planet.Id));
                if(!isExpanded) {
                    Expand();
                }
            });
        }

        private string ConstructSpeedUpText()
            => LocalizationObj.GetString("lbl_30m"); 

        private void UpdateOpenedPlanetView() {
            actionButton?.Deactivate();
            progressParent.Deactivate();
            priceParent.Deactivate();
            planetNameText.colorGradientPreset = context.SelectedColorGradient;
            moduleCollection.Deactivate();
        }

        private void UpdateOpeningFilling() {
            progressFillImage.fillAmount = (float)Planet.OpeningProgress;
            progressText.text = BosUtils.FormatTimeWithColon(Planet.OpeningRemaningTime);
        }

        private void UpdatePlanetNameText() {
            var planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(Planet.Id);
            planetNameText.text = Services.ResourceService.Localization.GetString(planetNameData.name);
        }

        private void UpdatePrice() {
            
            var companyCashPrice = Bos.Data.Currency.CreateCompanyCash(Planet.Data.CompanyCashPrice);
            var securitiesPrice = Bos.Data.Currency.CreateSecurities(Planet.Data.SecuritiesPrice);
            priceCashText.text = new CurrencyNumber(companyCashPrice.Value).AbbreviationColored("#" + ColorUtility.ToHtmlStringRGB(cashColor), "#ffe565");
            priceSecuritiesText.text = new CurrencyNumber(securitiesPrice.Value).AbbreviationColored("#" + ColorUtility.ToHtmlStringRGB(securitiesColor), "#ffe565");
        }

        public void Collapse(Action action = null) {
            if (isExpanded) {
                var animator = GetComponent<Animator>();
                if (animator != null) {
                    isExpanded = false;
                    animator.SetBool("expand", isExpanded);
                    StartCoroutine(OnCollapsed(action));
                }
            }
        }

        private System.Collections.IEnumerator OnCollapsed(Action action)
        {
            yield return new WaitForSeconds(0.25f);
            action?.Invoke();
        }

        public void Expand() {
            if (!isExpanded) {
                var animator = GetComponent<Animator>();
                if (animator != null) {
                    foreach (var view in context.PlanetViews) {
                        if (view.planetId != planetId) {
                            view.Collapse();
                            view.UpdatePlanetView();
                        }                  
                    }
                    isExpanded = true;
                    GetComponent<Animator>()?.SetBool("expand", isExpanded);
                }
            }
        }

        private void UpdateOpenedClosedContent() {
            var planet = Planets.GetPlanet(planetId);
            if(planet.State == PlanetState.Opened ) {
                openedContent?.Activate();
                closedContent?.Deactivate();
            } else {
                openedContent?.Deactivate();
                closedContent?.Activate();
            }
        }

        private void SetButtonGrayed(bool isGrayed ) {
            if (actionButton != null) {
                var image = actionButton.GetComponent<Image>();
                image.material?.SetFloat("_Enabled", isGrayed ? 1.0f : 0.0f);
            }
        }

    }

}