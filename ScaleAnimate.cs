namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(Vector3Animator))]
    public class ScaleAnimate : GameBehaviour {

        public Vector3 startValue;
        public Vector3 endValue;
        public float duration;
        public EaseType easeType;
        public BosAnimationMode mode;
        public float delay = 0f;

        private RectTransform rectTransform;

        public override void OnEnable() {
            base.OnEnable();
            rectTransform = GetComponent<RectTransform>();
            if(Mathf.Approximately(delay, 0f) ) {
                Animate();
            } else {
                rectTransform.localScale = startValue;
                StopCoroutine("AnimateDelayedImpl");
                StartCoroutine(AnimateDelayedImpl());
            }
        }

        private IEnumerator AnimateDelayedImpl() {
            yield return new WaitForSeconds(delay);
            Animate();
        }
        private void Animate() {
            Vector3Animator animator = gameObject.GetOrAdd<Vector3Animator>();
            animator.Stop();

            animator.StartAnimation(new Vector3AnimationData {
                StartValue = startValue,
                EndValue = endValue,
                Duration = duration,
                EaseType = easeType,
                AnimationMode = mode,
                Target = gameObject,
                OnStart = rectTransform.UpdateScaleFunctor(),
                OnUpdate = rectTransform.UpdateScaleTimedFunctor(),
                OnEnd = rectTransform.UpdateScaleFunctor()
            });
        }
    }

}