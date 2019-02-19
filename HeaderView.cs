namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class HeaderView : TypedView {

        public Text companyCashText;
        public Text playerCashText;
        public Text coinsText;
        public Text securitiesText;

        public override ViewType Type => ViewType.HeaderGameView;

        public override void Setup(ViewData data) {
            base.Setup(data);
            var playerService = Services.GetService<IPlayerService>();
            companyCashText.text = playerService.Currency.CompanyCash.ToCurrencyNumber().Abbreviation;
            playerCashText.text = playerService.Currency.PlayerCash.ToCurrencyNumber().Abbreviation;
            coinsText.text = playerService.Currency.Coins.ToString();
            securitiesText.text = playerService.Currency.Securities.ToCurrencyNumber().Abbreviation;
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.PlayerCashChanged += OnPlayerCashChanged;
            GameEvents.CoinsChanged += OnCoinsChanged;
            GameEvents.SecuritiesChanged += OnSecuritiesChanged;
        }

        public override void OnDisable() {
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.PlayerCashChanged -= OnPlayerCashChanged;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            GameEvents.SecuritiesChanged -= OnSecuritiesChanged;
            base.OnDisable();
        }

        public override int ViewDepth => 10;

        private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue)
            => companyCashText.text = newValue.Abbreviation;

        private void OnPlayerCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue)
            => playerCashText.text = newValue.Abbreviation;

        private void OnCoinsChanged(int oldValue, int newValue)
            => coinsText.text = newValue.ToString();

        private void OnSecuritiesChanged(CurrencyNumber oldValue, CurrencyNumber newValue)
            => securitiesText.text = newValue.Abbreviation;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => false;
    }

}