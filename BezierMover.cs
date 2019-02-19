namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BezierMover : GameBehaviour {

        private BezierType bezierType = BezierType.Quadratic;

        private bool isStarted = false;
        private Vector3[] points;
        private System.Action<GameObject> onComplete;
        private IPositionObject target;
        private float interval;
        private float timer;

        public void Setup(BezierData data)
            => Setup(data.Target, data.Points, data.Interval, data.Type, data.OnComplete);
        
        public void Setup(IPositionObject target, Vector3[] points, float interval, BezierType type, System.Action<GameObject> onComplete) {
            this.bezierType = type;
            this.points = points;
            this.onComplete = onComplete;
            this.target = target;
            this.interval = interval;
            this.timer = 0f;
            target.SetPosition(points[0]);
            this.isStarted = true;
        }

        public override void Update() {
            base.Update();
            if (isStarted) {
                float t = NormalizedTimer;
                Vector3 nextPoint = Vector3.zero;
                switch (bezierType) {
                    case BezierType.Linear: {
                            nextPoint = (1f - t) * points[0] + t * points[1];
                        }
                        break;
                    case BezierType.Quadratic: {
                            nextPoint = (1f - t) * (1f - t) * points[0] + 2f * t * (1f - t) * points[1] + t * t * points[2];
                        }
                        break;
                    case BezierType.Qubic: {
                            nextPoint = (1f - t) * (1f - t) * (1f - t) * points[0] + 3f * t * (1f - t) * (1f - t) * points[1] +
                                3f * t * t * (1f - t) * points[2] + t * t * t * points[3];
                        }
                        break;
                }
                target?.SetPosition(nextPoint);
                if (t >= 1f) {
                    onComplete?.Invoke(target.GameObject);
                    isStarted = false;
                }
                timer += Time.deltaTime;
            }
        }

        private float NormalizedTimer
            => Mathf.Clamp01(timer / interval);
        
        

    }

    public enum BezierType {
        Linear,
        Quadratic,
        Qubic
    }

    public class BezierData {
        public IPositionObject Target { get; set; }
        public Vector3[] Points { get; set; }
        public float Interval { get; set; }
        public BezierType Type { get; set; }
        public System.Action<GameObject> OnComplete { get; set; }
    }

    public interface IPositionObject {
        Vector3 Position { get; }
        void SetPosition(Vector3 pos);
        GameObject GameObject { get; }
    }
}