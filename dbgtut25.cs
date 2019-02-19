namespace Bos.UI.Test {
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    public class dbgtut25 : GameBehaviour {

        public Text stageText;
        public Text activeText;
        public Text completedText;
        public Text activeStatesText;
        public Text isProductAvailableText;



        private ShowTransferAndPersonalPurchasesState state;

        public override void OnEnable() {
            base.OnEnable();
            state = Services.TutorialService.GetState(TutorialStateName.ShowTransferAnsPersonalPurchasesState) as ShowTransferAndPersonalPurchasesState;
        }

        public override void Update() {
            base.Update();
            if(state != null ) {
                stageText.text = $"stage: {state.Stage}";
                activeText.text = $"active: {state.IsActive}";
                completedText.text = $"completed: {state.IsCompleted}";
                isProductAvailableText.text = $"is product available: {state.IsProductAvailable}";
            }
            UpdateActiveStateTexts();
        }

        private void UpdateActiveStateTexts() {
            var sb = new StringBuilder();
            sb.AppendLine("active states:");
            foreach(var state in Services.TutorialService.States ) {
                if(state.IsActive) {
                    sb.AppendLine($"{state.Name}");
                }
            }
            activeStatesText.text = sb.ToString();
        }
    }

}