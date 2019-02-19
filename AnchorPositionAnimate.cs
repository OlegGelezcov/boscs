namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(Vector2Animator))]
    public class AnchorPositionAnimate : GameBehaviour {

        public Vector2 startValue;
        public Vector2 endValue;
        public float duration;
        public EaseType easeType;
        public BosAnimationMode mode;
        public float delay = 0f;

        private RectTransform rectTransform;

        public override void OnEnable() {
            base.OnEnable();
            rectTransform = GetComponent<RectTransform>();
            if(Mathf.Approximately(delay, 0f)) {
                Animate();
            } else {
                rectTransform.anchoredPosition = startValue;
                StopCoroutine("AnimateDelayedImpl");
                StartCoroutine(AnimateDelayedImpl());
            }
        }

        private IEnumerator AnimateDelayedImpl() {
            yield return new WaitForSeconds(delay);
            Animate();
        }

        private void Animate() {
            var animator = gameObject.GetOrAdd<Vector2Animator>();
            animator.Stop();

            animator.StartAnimation(new Vector2AnimationData {
                StartValue = startValue,
                EndValue = endValue,
                Duration = duration,
                EaseType = easeType,
                AnimationMode = mode,
                Target = gameObject,
                OnStart = rectTransform.UpdatePositionFunctor(),
                OnUpdate = rectTransform.UpdatePositionTimedFunctor(),
                OnEnd = rectTransform.UpdatePositionFunctor()
            });

        }
    }

}