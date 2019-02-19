namespace Bos {
    using UnityEngine;

    public class ReportAnimIcon : MonoBehaviour {

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        private void Start() {
            RectTransform trs = GetComponent<RectTransform>();
            FloatAnimator animator = gameObject.GetOrAdd<FloatAnimator>();

            updateTimer.Setup(4, (delta) => {
                animator.StartAnimation(new FloatAnimationData {
                    StartValue = 0,
                    EndValue = 30,
                    Duration = 0.5f,
                    AnimationMode = BosAnimationMode.Single,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = gameObject,
                    OnStart = (v, o) => trs.localRotation = Quaternion.Euler(0, 0, v),
                    OnUpdate = (v, t, o) => trs.localRotation = Quaternion.Euler(0, 0, v),
                    OnEnd = (v, o) => {
                        trs.localRotation = Quaternion.Euler(0, 0, v);
                        animator.StartAnimation(new FloatAnimationData {
                            StartValue = 30,
                            EndValue = 0,
                            Duration = 0.5f,
                            AnimationMode = BosAnimationMode.Single,
                            EaseType = EaseType.EaseInOutQuad,
                            Target = gameObject,
                            OnStart = (v2, o2) => trs.localRotation = Quaternion.Euler(0, 0, v2),
                            OnUpdate = (v2, t2, o2) => trs.localRotation = Quaternion.Euler(0, 0, v2),
                            OnEnd = (v2, o2) => trs.localRotation = Quaternion.Euler(0, 0, v2)
                        });
                    }

                });
            });
        }

        private void Update() {
            updateTimer.Update();
        }
    }

}