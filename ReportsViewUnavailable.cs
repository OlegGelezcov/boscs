namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ReportsViewUnavailable : GameBehaviour {

        public TextMeshProUGUI flyToMarsText;
        public Button changePlanetButton;

        public override void OnEnable() {
            base.OnEnable();
            ILocalizationRepository localization = Services.ResourceService.Localization;
            flyToMarsText.text = localization.GetString("lbl_fly_to_moon");
            changePlanetButton.SetListener(() => {
                Services.ViewService.Show(ViewType.PlanetsView);
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });
        }
    }

}