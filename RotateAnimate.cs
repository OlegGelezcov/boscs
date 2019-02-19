namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(FloatAnimator))]
    public class RotateAnimate : GameBehaviour {

        public float startValue;
        public float endValue;
        public float duration;
        public EaseType easeType;
        public BosAnimationMode mode;
        public float delay = 0f;

        private RectTransform rectTransform;

        public override void OnEnable() {
            base.OnEnable();
            rectTransform = GetComponent<RectTransform>();
            if (Mathf.Approximately(delay, 0f)) {
                Animate();
            } else {
                rectTransform.localRotation = Quaternion.Euler(0, 0, startValue);
                StopCoroutine("AnimateDelayedImpl");
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
                OnStart = rectTransform.UpdateZRotation(),
                OnUpdate = rectTransform.UpdateZRotationTimed(),
                OnEnd = rectTransform.UpdateZRotation()
            });
        }
    }

}