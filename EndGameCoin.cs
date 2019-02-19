using Bos.UI;

namespace Bos {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UDBG = UnityEngine.Debug;

    public class EndGameCoin : GameBehaviour {

        public Vector2 startPosition;
        public Vector2 cp1;
        public Vector2 cp2;
        public Vector2 endPosition;
        public float interval = 2;
        public float offset = 300;
        
        public void Setup(Vector2[] positions, float interval, float offset ) {
            if(positions.Length < 4) {
                UDBG.LogError($"EndGaimCoin: count of positions muste be >= 4");
            }
            this.startPosition = positions[0];
            this.cp1 = positions[1];
            this.cp2 = positions[2];
            this.endPosition = positions[3];
            this.interval = interval;
            this.offset = offset;
            Setup();
        }
        public void Setup() {
            BezierData data = new BezierData() {
                Points = new Vector3[] {startPosition, cp1 + GetRandomOffset(), cp2 + GetRandomOffset(), endPosition},
                Target = GetComponent<RectTransformPositionObject>(),
                Interval = 2,
                Type = BezierType.Qubic,
                OnComplete = (o) => { Destroy(gameObject); }
            };
            GetComponent<BezierMover>().Setup(data);
        }

        private Vector2 GetRandomOffset()
            => Random.insideUnitCircle.normalized * offset;
    }


}