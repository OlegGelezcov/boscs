namespace Bos.UI {
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class TimeBoostToggle : GameBehaviour {

        public Toggle toggle;
        public Text toggleText;
        //public float timeDenominator = 60.0f;
        public Text realtimeText;

        private readonly UpdateTimer realUpdateTimer = new UpdateTimer();

        public override void OnEnable() {

            base.OnEnable();

        }

        public override void OnDisable() {
            base.OnDisable();
        }

        public override void Start() {
            base.Start();

            toggle.onValueChanged.RemoveAllListeners();
            bool isOnValue = Services.TimeChangeService.IsEnabled;
            toggle.isOn = isOnValue;
            UpdateToggleText(toggle.isOn);

            toggle?.SetListener(isOn => {
                Services.TimeChangeService.SetEnabled(isOn);
                Services.SoundService.PlayOneShot(SoundName.click);
                UpdateToggleText(isOn);
            });

            realUpdateTimer.Setup(1, dt => realtimeText.text = BosUtils.FormatTimeWithColon(Services.TimeChangeService.RealTime), true);

#if !BOSDEBUG
            gameObject.Deactivate();
#endif
        }

        public override void Update() {
            base.Update();
            realUpdateTimer.Update();
        }

        private void UpdateToggleText(bool isOn) {
            if(isOn) {
                toggleText.text = $"TIME X{Services.TimeChangeService.TimeMult}";
            } else {
                toggleText.text = $"TIME X1";
            }
        }


    }

}