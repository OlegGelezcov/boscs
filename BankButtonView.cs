namespace Bos.UI {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BankButtonView : MonoBehaviour {

        /*
        public GameObject alertObject;
        public Button bankButton;


        public override ViewType Type => ViewType.BankButtonView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => false;

        public override void Setup(object data) {
            base.Setup(data);

            UpdateView();
            bankButton.SetListener(() => Services.ViewService.ShowDelayed(ViewType.BankView, 0.2f));
        }

        private void UpdateView() {
            IBankService bankService = Services.GetService<IBankService>();
            if (bankService.IsOpened && bankService.CoinsAccumulatedCount > 0) {
                alertObject.Activate();
            } else {
                alertObject.Deactivate();
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.BankAccumulatedCoinsChanged += OnBankAccumulatedCoinsChanged;
            GameEvents.BankLevelChanged += OnBankLevelChanged;
        }

        public override void OnDisable() {
            GameEvents.BankAccumulatedCoinsChanged -= OnBankAccumulatedCoinsChanged;
            GameEvents.BankLevelChanged -= OnBankLevelChanged;
            base.OnDisable();
        }

        private void OnBankAccumulatedCoinsChanged(int oldCoins, int newCoins ) {
            UpdateView();
        }

        private void OnBankLevelChanged(int oldLevel, int newLevel) {
            UpdateView();
        }*/
    }

}