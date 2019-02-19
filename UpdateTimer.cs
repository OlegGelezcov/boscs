namespace Bos {
    using UnityEngine;

    public class UpdateTimer : GameElement {

        public float Interval { get; private set; }
        public System.Action<float> Action { get; private set; }

        private float timer;

        private float realTimer = 0f;

        public void Setup(float interval, System.Action<float> action, bool invokeImmediatly = false) {
            Interval = interval;
            Action = action;
            timer = interval;
            if(invokeImmediatly) {
                Action?.Invoke(0);
            }
        }

        public void SetInterval(float newInterval) {
            Interval = newInterval;
        }

        public void Update() {
            Update(Time.deltaTime);
        }

        public void Update(float deltaTime) {
            timer -= deltaTime;
            realTimer += deltaTime;

            if (timer <= 0.0f ) {
                timer = Interval;
                Action?.Invoke(realTimer);
                realTimer = 0;
            }
        }
    }

}