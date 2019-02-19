namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ChangeThePlanetButton : GameBehaviour {

        public override void OnEnable() {
            base.OnEnable();

            Button button = GetComponent<Button>();
            button?.SetListener(() => {
                Services.ViewService.Show(ViewType.PlanetsView);
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });
        }
    }

}