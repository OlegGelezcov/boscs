namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class CurrencyNumberText : GameBehaviour {

        public Text text;

        private float interval = 0.5f;
        private double speed = 0f;
        private float timer = 0f;

        private double previosValue = 0f;
        private double targetValue = 0f;
        private double currentValue = 0f;
        private float updateTimer = 0.15f;


        public CurrencyNumberText WithText(Text text) {
            this.text = text;
            return this;
        }

        private void UpdateText() {
            if (text != null) {
                text.text = BosUtils.GetCurrencyString(new CurrencyNumber(currentValue));
            }
        }

        public void SetValue(double newValue ) {
            previosValue = targetValue;
            currentValue = previosValue;
            UpdateText();
            targetValue = newValue;
            speed = (targetValue - previosValue) / interval;
            timer = 0f;
        }

        public override void Update() {
            base.Update();

            if(timer < interval ) {
                timer += Time.deltaTime;
                updateTimer -= Time.deltaTime;
                currentValue += speed * Time.deltaTime;
                if(updateTimer < 0.0f ) {
                    updateTimer += 0.15f;
                    UpdateText();
                }
                if(timer >= interval) {
                    currentValue = targetValue;
                    speed = 0;
                    UpdateText();
                }
            }
        }
    }

}