namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Bos.Data;

    public class ModuleView : GameBehaviour {

        public int moduleId;

        public Text nameText;
        public Image iconImage;
        public Text requirementText;
        public Button buyButton;
        public Image currencyIconImage;
        public Text currencyText;
        public GameObject checkObject;
        public Text buyText;

        public ParticleSystem buyParticles;


        private ShipModuleInfo module;
        private IShipModuleService moduleService;
        private ModuleNameData moduleNameData;

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ShipModuleStateChanged += OnModuleStateChnaged;
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.PlayerCashChanged += OnPlayerCashChanged;
            GameEvents.CoinsChanged += OnCoinsChanged;
            GameEvents.SecuritiesChanged += OnSecuritiesChanged;
            Setup();
        }

        public override void OnDisable() {
            GameEvents.ShipModuleStateChanged -= OnModuleStateChnaged;
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.PlayerCashChanged -= OnPlayerCashChanged;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            GameEvents.SecuritiesChanged -= OnSecuritiesChanged;
            base.OnDisable();
        }

        private void OnCompanyCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount ) {
            UpdateForCurrencyType(CurrencyType.CompanyCash);
        }

        private void OnPlayerCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount) {
            UpdateForCurrencyType(CurrencyType.PlayerCash);
        }

        private void OnCoinsChanged(int oldCount, int newCount ) {
            UpdateForCurrencyType(CurrencyType.Coins);
        }

        private void OnSecuritiesChanged(CurrencyNumber oldCount, CurrencyNumber newCount) {
            UpdateForCurrencyType(CurrencyType.Securities);
        }

        private void UpdateForCurrencyType(CurrencyType type) {
            if(module != null ) {
                if(module.Data.Currency.Type == type ) {
                    Setup();
                }
            }
        }

        private void Setup() {
            if (moduleService == null) {
                moduleService = Services.GetService<IShipModuleService>();
            }
            if (module == null) {
                module = moduleService.GetModule(moduleId);
            }

            if (moduleNameData == null) {
                moduleNameData = Services.ResourceService.ModuleNameRepository.GetModuleNameData(moduleId);
            }

            nameText.text = Services.ResourceService.Localization.GetString(moduleNameData.name);
            iconImage.overrideSprite = Services.ResourceService.GetSpriteByKey(moduleNameData.icon);

            currencyIconImage.overrideSprite = Services.ResourceService.GetCurrencySprite(module.Data.Currency);
            currencyText.text = BosUtils.GetCurrencyString(module.Data.Currency);

            ModuleTransactionState status;
            bool isAllowedToBuy = moduleService.IsAllowBuyModule(moduleId, out status);
            if(isAllowedToBuy) {
                requirementText.text = string.Empty;
                buyButton.interactable = true;
            } else {
                switch(status ) {
                    case ModuleTransactionState.NotEnoughCurrency: {
                            requirementText.text = string.Empty;
                            currencyText.text = BosUtils.GetCurrencyString(module.Data.Currency, "#FF0000");
                            buyButton.interactable = false;
                        }
                        break;
                    case ModuleTransactionState.NotValidState: {
                            var planetData = Services.ResourceService.Planets.GetPlanet(module.Data.PlanetId);
                            if(planetData == null ) {
                                Debug.LogError($"not found planet for upgrade level => {module.Data.PlanetId}");
                                return;
                            }
                            PlanetNameData planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(planetData.Id);
                            requirementText.text = string.Format(Services.ResourceService.Localization.GetString("fmt_module_requirement"),
                                Services.ResourceService.Localization.GetString(planetNameData.name));
                            buyButton.interactable = false;
                            buyText.color = Color.grey;
                        }
                        break;
                }
            }

            buyButton.SetListener(() => {

                var buyStatus = moduleService.BuyModule(moduleId);
                if(buyStatus == ModuleTransactionState.Success) {
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.Poof);
                    buyParticles?.Play();
                } else {
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                }

            });

            switch(module.State) {
                case ShipModuleState.Opened: {
                        checkObject.Activate();
                        buyButton.Deactivate();
                        requirementText.text = string.Empty;
                    }
                    break;
                default: {
                        checkObject.Deactivate();
                        buyButton.Activate();
                    }
                    break;
            }
        }

        private void OnModuleStateChnaged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo module ) {
            if(module.Id == moduleId ) {
                Setup();
            }
        }

        #region Old Prototype
        /*
        //id of module [0, 1, 2, .. etc.]
        public int moduleId;

        public Image moduleImage;

        //color applied to module image when it in locked state
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        //color applied to module image when module available to purchase
        public Color availableColor = Color.yellow;

        //Trigger for click handle
        public Button button;

        public override void OnEnable() {
            base.OnEnable();
            UpdateView();
            GameEvents.ShipModuleStateChanged += OnShipModuleStateChanged;
        }

        public override void OnDisable() {
            GameEvents.ShipModuleStateChanged -= OnShipModuleStateChanged;
            base.OnDisable();
        }

        private void UpdateView() {
            IShipModuleService service = Services.GetService<IShipModuleService>();
            var module = service.GetModule(moduleId);
            if(module.State == ShipModuleState.Locked) {
                moduleImage.color = lockedColor;
                button.SetListener(() => {
                    GetComponent<Animator>().SetTrigger("click");
                });
            } else if(module.State == ShipModuleState.Available) {
                moduleImage.color = availableColor;
                button.SetListener(() => {
                    GetComponent<Animator>().SetTrigger("click");
                    Services.ViewService.ShowDelayed(ViewType.BuyModuleView, 0.5f, module);
                });
            } else {
                moduleImage.color = Color.white;
                button.SetListener(() => {
                    GetComponent<Animator>().SetTrigger("click");
                });
            }
        }

        private void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo module) {
            if(module.Id == moduleId) {
                UpdateView();
            }
        }
        */
        #endregion
    }

}