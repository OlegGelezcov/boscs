namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class SingleModuleView : GameBehaviour {
		public Text moduleNameText;
		public Image currencyIconImage;
		public Text currencyText;
        public ModulePart[] moduleParts;

		public Button buyButton;

		public Image planetImage;
        public float totalInterval = 1;

		private ShipModuleInfo module;

        private readonly UpdateTimer updateTimer = new UpdateTimer();

		public void Setup(ShipModuleInfo module) {          
			this.module = module;
            ModuleNameData moduleNameData = Services.ResourceService.ModuleNameRepository.GetModuleNameData(module.Id);
            moduleNameText.text = Services.ResourceService.Localization.GetString(moduleNameData.name);
            switch(module.State) {
                case ShipModuleState.Opened: {
                        currencyIconImage.Deactivate();
                        currencyText.Deactivate();
                        planetImage.Deactivate();
                        buyButton.Deactivate();
                    }
                    break;
                case ShipModuleState.Available: {
                        currencyIconImage.Activate();
                        currencyText.Activate();
                        planetImage.Deactivate();
                        buyButton.Activate();
                    }
                    break;
                case ShipModuleState.Locked: {
                        currencyIconImage.Activate();
                        currencyText.Activate();
                        planetImage.Activate();
                        buyButton.Activate();
                    }
                    break;
            }
            currencyIconImage.overrideSprite = Services.ResourceService.GetCurrencySprite(module.CurrencyType);
            currencyText.text = BosUtils.GetCurrencyString(Currency.Create(module.CurrencyType, module.Price));

            PlanetNameData planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(module.Data.PlanetId);
            if(planetNameData != null ) {
                planetImage.overrideSprite = Services.ResourceService.GetSpriteByKey(planetNameData.icon);
            } else {
                planetImage.overrideSprite = Services.ResourceService.Sprites.FallbackSprite;
            }

            buyButton.SetListener(() => {
                var status = Services.GetService<IShipModuleService>().BuyModule(module.Id);
                switch(status) {
                    case ModuleTransactionState.Success: {
                            Services.SoundService.PlayOneShot(SoundName.buyUpgrade);
                            Setup(module);
                        }
                        break;
                    case ModuleTransactionState.NotEnoughCurrency: 
                    {
                        if (module.CurrencyType == CurrencyType.Coins)
                        {
                            Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                                UserData = (int)module.Price
                            });
                        }
                    }
                    break;
                }
            });

            IShipModuleService moduleService = Services.GetService<IShipModuleService>();

            updateTimer.Setup(0.5f, (deltaTime) => {
                ModuleTransactionState moduleTransactionState;
                if(moduleService.IsAllowBuyModule(module.Id, out moduleTransactionState)) {
                    buyButton.interactable = true;
                } else if(moduleTransactionState == ModuleTransactionState.NotEnoughCurrency && module.CurrencyType == CurrencyType.Coins){
                    buyButton.interactable = true;
                }
                else
                {
                    buyButton.interactable = false;
                }
            }, true);

            MoveObject(module);
		}

        public void LerpPart(int moduleId, float t) {
            foreach(var part in GetParts(moduleId)) {
                part.LerpPosition(t);
            }
        }

        private IEnumerable<ModulePart> GetParts(int moduleId ) {
            foreach(var part in moduleParts ) {
                if(part.moduleId == moduleId) {
                    yield return part;
                }
            }
        }

        private void MoveObject(ShipModuleInfo module ) {
            foreach(var part in moduleParts ) {
                if(part.moduleId <= module.Id ) {
                    part.MoveToVisible(totalInterval);
                } else {
                    part.MoveToHided(totalInterval);
                }
                
            }
        }
        public override void Update() {
            base.Update();
            updateTimer.Update();
        }
    }
}

