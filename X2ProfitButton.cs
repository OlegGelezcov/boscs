namespace Bos.UI {

	using UnityEngine.UI;
	using UniRx;
	using System;
    using UnityEngine;

    public class X2ProfitButton : GameBehaviour {

		public Button button;
		public Text nameWithoutTimer;
		public Text nameWithTimer;
		public Text timerText;
        public Animator parentAnimator;
		
		
		public override void Start() {
			IX2ProfitService service = Services.GetService<IX2ProfitService>();
			
			button.SetListener(() => {
				Sounds.PlayOneShot(SoundName.click);
				ViewService.Show(ViewType.X2ProfitView);
			});

            Material buttonMaterial = button.GetComponent<Image>().material;
            int _enabledPropId = Shader.PropertyToID("_Enabled");

            UpdateTimer(service, buttonMaterial, _enabledPropId);

			Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                UpdateTimer(service, buttonMaterial, _enabledPropId);
            }).AddTo(gameObject);

            Observable.Interval(TimeSpan.FromSeconds(4)).Subscribe(_ => {
                if (service.AvailableAfterInterval == 0) {
                    parentAnimator.SetTrigger("effect");
                }
            }).AddTo(gameObject);
		}

        private void UpdateTimer(IX2ProfitService service, Material buttonMaterial, int _enabledPropId) {
            int interval = service.AvailableAfterInterval;
            if (interval == 0) {
                nameWithoutTimer.Activate();
                nameWithTimer.Deactivate();
                if (buttonMaterial != null) {
                    buttonMaterial.SetInt(_enabledPropId, 1);
                }
            } else {
                nameWithoutTimer.Deactivate();
                nameWithTimer.Activate();
                TimeSpan ts = TimeSpan.FromSeconds(interval);
                timerText.text = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                if (buttonMaterial != null) {
                    buttonMaterial.SetInt(_enabledPropId, 0);
                }
            }
        }
		
		
	}
}
