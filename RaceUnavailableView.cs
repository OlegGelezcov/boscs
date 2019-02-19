namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class RaceUnavailableView : GameBehaviour {

		public Button changePlanetButton;
        //public Button closeButton;

		public override void OnEnable(){
			base.OnEnable();
			changePlanetButton.SetListener(() => {
				Services.ViewService.Show(ViewType.PlanetsView);
				Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
			});
            /*
            closeButton.SetListener(() => {
                GameUI legacyUI = FindObjectOfType<GameUI>();
                legacyUI.HideGames();
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });*/

		}
	}

}