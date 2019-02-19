namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class LineRendererPositionsAnimate : MonoBehaviour {

        public LineRenderer line;
        public Vector2Animator animator;
        public float offset = 10;
        public float time = 2;

        private void Start() {
            if(line.positionCount < 3) {
                return;
            }
            Vector3[] positions = new Vector3[line.positionCount];
            List<Vector2AnimationData> datas = new List<Vector2AnimationData>();
            for(int i = 1; i < line.positionCount - 1; i++ ) {
                int index = i;
                Vector2 rndVector = Random.insideUnitCircle.normalized * offset;
                Vector2 start = (Vector2)line.GetPosition(index) + rndVector;
                Vector2 end = (Vector2)line.GetPosition(index) - rndVector;
                Vector2AnimationData data = new Vector2AnimationData {
                    AnimationMode = BosAnimationMode.PingPong,
                    Duration = time,
                    StartValue = start,
                    EndValue = end,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = gameObject,
                    OnStart = (v, go) => line.SetPosition(index, v),
                    OnUpdate = (v, t, go) => line.SetPosition(index, v),
                    OnEnd = (v, go) => line.SetPosition(index, v)
                };
                datas.Add(data);
            }

            animator.StartAnimation(datas);
        }

    }

}