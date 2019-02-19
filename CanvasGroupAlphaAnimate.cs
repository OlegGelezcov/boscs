namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(FloatAnimator))]
    public class CanvasGroupAlphaAnimate : GameBehaviour {

        public float startValue;
        public float endValue;
        public float duration;
        public EaseType easeType;
        public BosAnimationMode mode;
        public float delay;

        private CanvasGroup canvasGroup;

        public override void OnEnable() {
            base.OnEnable();
            canvasGroup = GetComponent<CanvasGroup>();
            if (Mathf.Approximately(delay, 0f)) {
                Animate();
            } else {
                canvasGroup.alpha = startValue; 
                StartCoroutine(AnimateDelayedImpl());
            }
        }

        private IEnumerator AnimateDelayedImpl() {
            yield return new WaitForSeconds(delay);
            Animate();
        }


        private void Animate() {
            FloatAnimator animator = gameObject.GetOrAdd<FloatAnimator>();
            animator.Stop();

            animator.StartAnimation(new FloatAnimationData {
                StartValue = startValue,
                EndValue = endValue,
                Duration = duration,
                EaseType = easeType,
                AnimationMode = mode,
                Target = gameObject,
                OnStart = (v, o) => canvasGroup.alpha = v,
                OnUpdate = (v, t, o) => canvasGroup.alpha = v,
                OnEnd = (v, o) => canvasGroup.alpha = v
            });
        }
    }

}