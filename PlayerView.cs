namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class PlayerView : GameBehaviour {

		public Image playerIconImage;
		public Button statsButton;


		
		public void Setup() {
			UpdatePlayerIcon();
			statsButton.SetListener(() => {
				Services.ViewService.Show(ViewType.StatsView);
				Services.SoundService.PlayOneShot(SoundName.click);
			});
		}

		public override void OnEnable(){
			base.OnEnable();
			Setup();
			GameEvents.GenderChanged += OnGenderChanged;
		}

		public override void OnDisable() {
			GameEvents.GenderChanged -= OnGenderChanged;
			base.OnDisable();
		}

		private void OnGenderChanged(Gender oldGender, Gender newGender){ 
			UpdatePlayerIcon();
			UnityEngine.Debug.Log($"Gender changed from {oldGender} to {newGender}");
		}

		private void UpdatePlayerIcon() {
			int currentPlanetId = Services.PlanetService.CurrentPlanet.Id;
			Gender currentGender = Services.PlayerService.Gender;
			var spriteData = Services.ResourceService.PlayerIcons.GetLarge(currentPlanetId, currentGender);
			if(spriteData != null ) {
				Debug.Log($"sprite id => {spriteData.id}, sprite path => {spriteData.path}");
				playerIconImage.overrideSprite = Services.ResourceService.GetSprite(spriteData);
			} else {
				Debug.Log($"sprite data is null for gender => {currentGender}, current planet id => {currentPlanetId}");
			}
		}
	}
}

