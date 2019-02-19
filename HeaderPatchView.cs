namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class HeaderPatchView : GameBehaviour {

        public Button planetsButton;

        public override void Start() {
            base.Start();
            planetsButton?.SetListener(() => {
                Services.ViewService.Show(ViewType.PlanetsView);
            });
        }

       
    }

}