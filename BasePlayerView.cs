namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
    using Bos.Data;
    using UnityEngine;
	using UnityEngine.UI;

	public class BasePlayerView : GameBehaviour {
		public Text levelText;
		public Toggle maleToggle;
		public Toggle femaleToggle;
		public Image playerIconImage;
		public Text statusNameText;
		public Image expProgressImage;
		public Text statusText;

        public GameObject progressParticles;
        public StatusProgressShaderController progressShaderController;


		public void Setup(){
			UpdateLevelText();
			maleToggle.SetListener(isOn => {});
			femaleToggle.SetListener(isON => {});

			Gender currentGender = Services.PlayerService.Gender;
			if(currentGender == Gender.Male ) {
				maleToggle.isOn = true;
				femaleToggle.isOn = false;
			} else {
				maleToggle.isOn = false;
				femaleToggle.isOn = true;
			}

			maleToggle.SetListener(isOn => {
				if(Services.PlayerService.Gender != Gender.Male ) {
					Services.PlayerService.SetGender(Gender.Male);
					Services.SoundService.PlayOneShot(SoundName.click);
				}
			});

			femaleToggle.SetListener(isOn => {
				if(Services.PlayerService.Gender != Gender.Female) {
					Services.PlayerService.SetGender(Gender.Female);
					Services.SoundService.PlayOneShot(SoundName.click);
				}
			});
			UpdatePlayerIcon();
			UpdateStatusName();
			UpdateExpProgress();
			UpdateStatusText();
            progressShaderController?.ToggleEffect(true);
		}

		public override void OnEnable(){
			base.OnEnable();
			Setup();
			GameEvents.LevelChanged += OnLevelChanged;
			GameEvents.GenderChanged += OnGenderChanged;
			GameEvents.StatusLevelChanged += OnStatusChanged;
            GameEvents.StatusPointsChanged += OnStatusPointsChanged;
		}

		public override void OnDisable(){
			GameEvents.LevelChanged -= OnLevelChanged;
			GameEvents.GenderChanged -= OnGenderChanged;
			GameEvents.StatusLevelChanged -= OnStatusChanged;
            GameEvents.StatusPointsChanged -= OnStatusPointsChanged;
            base.OnDisable();
		}

        private void OnStatusPointsChanged(long oldValue, long newValue)
            => UpdateExpProgress();


		private void OnStatusChanged(int oldStatus, int newStatus){
			UpdateStatusName();
			UpdateStatusText();
            UpdateExpProgress();
		}

		private void OnLevelChanged(int oldLevel, int newLevel) {
			UpdateLevelText();
			UpdateExpProgress();
		}

		private void OnGenderChanged(Gender oldGender, Gender newGender)
			=> UpdatePlayerIcon();
		

		private void UpdateLevelText(){
			levelText.text = string.Format(
				Services.ResourceService.Localization.GetString("fmt_level"), 
				Services.PlayerService.Level.ToString().Colored("#ffef00"));
		}

		private void UpdatePlayerIcon(){
			Gender currentGender = Services.PlayerService.Gender;
			int currentPlanetId = Services.PlanetService.CurrentPlanet.Id;
			SpritePathData spriteData = Services.ResourceService.PlayerIcons.GetSmall(currentPlanetId, currentGender);
			playerIconImage.overrideSprite = Services.ResourceService.GetSprite(spriteData);
		}

		private void UpdateStatusName() {
			int status = Services.PlayerService.StatusLevel;
			StatusNameData statusNameData = Services.ResourceService.StatusNames.GetStatusName(status);
			if(statusNameData != null ) {
				statusNameText.text = Services.ResourceService.Localization.GetString(statusNameData.Name);
			} else {
				Debug.Log($"status name data null for status => {status}");
				statusNameText.text = string.Empty;
			}
		}

		private void UpdateExpProgress() {
			float startValue = expProgressImage.fillAmount;
            float endValue = ResourceService.PersonalImprovements.GetStatusLevelProgress(Player.StatusPoints); //Services.PlayerService.ExpProgress01;
            if(startValue > endValue ) {
                startValue = 0;
            }

			if(endValue != startValue ) {
				float duration = Mathf.Abs(endValue - startValue)  * 4f;
				FloatAnimationData data = new FloatAnimationData{
					StartValue = startValue,
					EndValue = endValue,
					Duration = duration,
					EaseType = EaseType.EaseInOutQuad,
					Target = expProgressImage.gameObject,
					AnimationMode = BosAnimationMode.Single,
					OnStart = (val, go) => {
                        expProgressImage.fillAmount = val;
                        progressParticles.Activate();
                    },
					OnUpdate = (val, t, go) => expProgressImage.fillAmount = val,
					OnEnd = (val, go) => {
                        expProgressImage.fillAmount = val;
                        Services.Execute(() => progressParticles.Deactivate(), 0.5f);
                    }
				};
				expProgressImage.GetComponent<FloatAnimator>().StartAnimation(data);
			}
		}

		private void UpdateStatusText() {
			statusText.text = string.Format(
				Services.ResourceService.Localization.GetString("status_fmt"), 
				Services.PlayerService.StatusLevel.ToString().Colored("#ffef00"));
		}
	}
}

