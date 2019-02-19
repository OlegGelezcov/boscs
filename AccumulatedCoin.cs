namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AccumulatedCoin : MonoBehaviour {

        public Vector2 start;

        public Vector2 control_1_Min;
        public Vector2 control_1_Max;

        public Vector2 control_2_Min;
        public Vector2 control_2_Max;

        public Vector2 end;

        public float interval = 0.7f;


        public void StartMoving( System.Action<GameObject> onComplete) {
            var bezierMover = GetComponent<BezierMover>();
            Vector3[] points = {
                start,
                new Vector3(Random.Range(control_1_Min.x, control_1_Max.x), Random.Range(control_1_Min.y, control_1_Max.y), 0),
                new Vector3(Random.Range(control_2_Min.x, control_2_Max.x), Random.Range(control_2_Min.y, control_2_Max.y), 0),
                end
            };

            bezierMover.Setup(GetComponent<RectTransformPositionObject>(), points, interval, BezierType.Qubic, onComplete);
        }

    }

}