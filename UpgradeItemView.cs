namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class UpgradeItemView : GameBehaviour, IListItemView<UpgradeData> {

        public Text nameText;
        public Text descriptionText;
        public Text priceTextShort;
        //public Text priceTextLong;

        public Image generatorIconImage;
        public Button buyButton;

        public UpgradeData Data { get; private set; }

        public void Setup(UpgradeData data ) {
            this.Data = data;
            nameText.text = Services.ResourceService.Localization.GetString(data.Name);
            descriptionText.text = ConstructDescriptionString(data);
            priceTextShort.text = ConstructPriceShortString(data); //ConstructPriceString(data);
            //priceTextLong.text = ConstructPriceString(data);
            generatorIconImage.overrideSprite = GetGeneratorIcon(data);
            buyButton.SetListener(() => {
                Services.UpgradeService.BuyUpgrade(data);
                Services.SoundService.PlayOneShot(SoundName.buyUpgrade);
            });
            UpdateBuyButtonState(data);
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.SecuritiesChanged += OnSecuritiesChanged;
        }

        public override void OnDisable() {
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.SecuritiesChanged -= OnSecuritiesChanged;
            base.OnDisable();
        }

        private void OnSecuritiesChanged(CurrencyNumber oldCount, CurrencyNumber newCount ) {
            if(Data != null && Data.CurrencyType == CurrencyType.Securities ) {
                UpdateBuyButtonState(Data);
            }
        }

        private void OnCompanyCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount ) {
            if(Data != null && Data.CurrencyType == CurrencyType.CompanyCash ) {
                UpdateBuyButtonState(Data);
            }
        }

        private void UpdateBuyButtonState(UpgradeData data) {
            buyButton.interactable = Services.UpgradeService.IsAllowBuy(data);
        }

        private Sprite GetGeneratorIcon(UpgradeData data ) {
            if(data.GeneratorId >= 0 ) {
                return Services.ResourceService.GetSpriteByKey(
                    Services.ResourceService.
                    GeneratorLocalData.
                    GetLocalData(data.GeneratorId).
                    GetIconData(Services.
                        PlanetService.
                        CurrentPlanet.Id).icon_id);
            } else {
                if(data.GeneratorId == -2 ) {
                    return Services.ResourceService.GetSpriteByKey("investor_icon");
                } else {
                    return Services.ResourceService.GetSpriteByKey("level_up");
                }
            }
        }

        private string ConstructPriceShortString(UpgradeData data ) {
            switch(data.CurrencyType) {
                case CurrencyType.CompanyCash: {
                        return "$" + BosUtils.GetCurrencyString(new CurrencyNumber(data.Price(() => {
                            return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                        })), "#FFFFFF", "#FBEF20");
                    }
                case CurrencyType.Securities: {
                        return BosUtils.GetCurrencyString(new CurrencyNumber(data.Price( () => {
                            return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                        })), "#FFFFFF", "#FBEF20");
                    }
            }
            return string.Empty;
        }

        private string ConstructPriceString(UpgradeData data) {
            switch(data.CurrencyType) {
                case CurrencyType.CompanyCash: {
                        return Currencies.DefaultCurrency.CreatePriceString(data.Price(() => {
                            return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                        }), false, " ").ToUpper();
                    }
                case CurrencyType.Securities: {
                        return Currencies.Investors.CreatePriceString(data.Price(() => {
                            return BosUtils.GetUpgradePriceMult(Planets.CurrentPlanet.Data, data);
                        }), false, " ").ToUpper();
                    }
            }
            return string.Empty;
        }

        private string ConstructDescriptionString(UpgradeData data ) {
            switch (data.UpgradeType) {
                case UpgradeType.Profit: {
                        return string.Format(Services.ResourceService.Localization.GetString("up_desc_1_fmt"), (int)data.Value);
                    }
                case UpgradeType.Time: {
                        return string.Format(Services.ResourceService.Localization.GetString("up_desc_2_fmt"), (int)data.Value);
                    }
                case UpgradeType.InvestorEffectiveness: {
                        return string.Format(Services.ResourceService.Localization.GetString("up_desc_3_fmt"), (int)data.Value);
                    }
                case UpgradeType.FreeGenerators: {
                        return string.Format(Services.ResourceService.Localization.GetString("up_desc_4_fmt"), (int)data.Value);
                    }
                default:
                    return string.Empty;
            }
        }
    }

}