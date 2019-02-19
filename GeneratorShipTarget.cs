namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class GeneratorShipTarget : MonoBehaviour {

        //public int targetId;
        public bool isStartTarget;
        public Image image;

        public bool IsActive { get; private set; }
        public float InactiveTimer { get; private set; } = 10;

        public void ActivateTarget() {

            if(!IsActive) {
                image.enabled = true;
                IsActive = true;
                Vector3Animator scaleAnimator = gameObject.GetOrAdd<Vector3Animator>();
                RectTransform rts = GetComponent<RectTransform>();

                Vector3AnimationData scaleData = new Vector3AnimationData {
                    StartValue = Vector3.one,
                    EndValue = Vector3.one * 3,
                    Duration = 0.25f,
                    EaseType = EaseType.EaseOutSin,
                    Target = gameObject,
                    OnStart = (s, o) => rts.localScale = s,
                    OnUpdate = (s, t, o) => rts.localScale = s,
                    OnEnd = (s, o) => {
                        rts.localScale = s;
                        Vector3AnimationData scale2Data = new Vector3AnimationData {
                            StartValue = Vector3.one * 3,
                            EndValue = Vector3.one,
                            Duration = 0.25f,
                            EaseType = EaseType.EaseInOutSin,
                            Target = gameObject,
                            OnStart = (s2, o2) => rts.localScale = s2,
                            OnUpdate = (s2, t2, o2) => rts.localScale = s2,
                            OnEnd = (s2, o2) => rts.localScale = s2
                        };
                        scaleAnimator.StartAnimation(scale2Data);
                    }
                };
                scaleAnimator.StartAnimation(scaleData);
            }
        }

        public void DeactivateTarget() {
            if (IsActive) {
                InactiveTimer = 0;
            }
            IsActive = false;
            Vector3Animator scaleAnimator = gameObject.GetOrAdd<Vector3Animator>();
            RectTransform rts = GetComponent<RectTransform>();

            Vector3AnimationData scaleData = new Vector3AnimationData {
                StartValue = Vector3.one * 1.5f,
                EndValue = Vector3.one * 0.01f,
                Duration = 0.4f,
                EaseType = EaseType.EaseOutSin,
                Target = gameObject,
                OnStart = (s, o) => rts.localScale = s,
                OnUpdate = (s, t, o) => rts.localScale = s,
                OnEnd = (s, o) => {
                    rts.localScale = s;
                    image.enabled = false;
                }
            };
            scaleAnimator.StartAnimation(scaleData);
            
        }

        private void Update() {
            if(!IsActive) {
                InactiveTimer += Time.deltaTime;
            }
        }

        public bool IsInactiveLongerThen(float duration)
            => InactiveTimer >= duration;
    }

}