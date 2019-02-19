namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class PlayerIconView : GameBehaviour {

		public Image playerIconImage;

		public override void OnEnable(){
			base.OnEnable();
			UpdateView();
			GameEvents.GenderChanged += OnGenderChanged;
			GameEvents.CurrentPlanetChanged += OnPlanetChanged;
		}

		public override void OnDisable(){
			GameEvents.GenderChanged -= OnGenderChanged;
			GameEvents.CurrentPlanetChanged -= OnPlanetChanged;
			base.OnDisable();
		}

		private void UpdateView() {
			Services.ViewService.Utils.UpdatePlayerSmallIcon(playerIconImage);
		}

		private void OnGenderChanged(Gender oldGender, Gender newGender ) 
			=> UpdateView();

		private void OnPlanetChanged(PlanetInfo oldPlanet, PlanetInfo newPlanet)
			=> UpdateView();
		
	}

}