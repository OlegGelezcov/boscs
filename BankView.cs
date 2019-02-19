namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BankView : TypedViewWithCloseButton {

        public BankNoLevelView noLevelView;
        public BankOpenedView bankOpenedView;


        public override void Setup(ViewData data) {
            base.Setup(data);
            UpdateView();
            closeButton.SetListener(() => Services.ViewService.Remove(ViewType.BankView, 0.2f));
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.BankLevelChanged += OnBankLevelChanged;
            BankOpenedView.AccumulationStateChanged += OnAccumulationStateChanged;
        }

        public override void OnDisable() {
            GameEvents.BankLevelChanged -= OnBankLevelChanged;
            BankOpenedView.AccumulationStateChanged -= OnAccumulationStateChanged;
            base.OnDisable();
        }

        private void OnAccumulationStateChanged(bool isAccumulation) {
            closeButton.interactable = !isAccumulation;
        }

        private void OnBankLevelChanged(int oldLevel, int newLevel) {
            UpdateView();
        }

        private void UpdateView() {
            var bankService = Services.GetService<IBankService>();
            if (bankService.IsOpened) {
                noLevelView.Deactivate();
                bankOpenedView.Activate();
                bankOpenedView.Setup();
            } else {
                bankOpenedView.Deactivate();
                noLevelView.Activate();
                noLevelView.Setup();
                
            }
        }

        public override ViewType Type => ViewType.BankView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override int ViewDepth => 20;

        public override bool IsModal =>  true;
    }

}