namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class CompletedModulesView : GameBehaviour {

        public Button flyButton;

        public override void OnEnable() {
            base.OnEnable();
            flyButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.BuyModuleView, BosUISettings.Instance.ViewCloseDelay);
                Services.ViewService.ShowDelayed(ViewType.PlanetsView, BosUISettings.Instance.ViewShowDelay);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        }
    }

}