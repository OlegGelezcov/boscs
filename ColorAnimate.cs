namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(ColorAnimator))]
    public class ColorAnimate : GameBehaviour {

        public Color startValue;
        public Color endValue;
        public float duration;
        public EaseType easeType;
        public BosAnimationMode mode;
        public float delay;

        private Graphic targetGraphic;

        public override void OnEnable() {
            base.OnEnable();
            targetGraphic = GetComponent<Graphic>();

            if (Mathf.Approximately(delay, 0f)) {
                Animate();
            } else {
                targetGraphic.color = startValue;
                StartCoroutine(AnimateDelayedImpl());
            }
        }

        private IEnumerator AnimateDelayedImpl() {
            yield return new WaitForSeconds(delay);
            Animate();
        }

        private void Animate() {
            ColorAnimator animator = gameObject.GetOrAdd<ColorAnimator>();
            animator.Stop();

            animator.StartAnimation(new ColorAnimationData {
                StartValue = startValue,
                EndValue = endValue,
                Duration = duration,
                EaseType = easeType,
                AnimationMode = mode,
                Target = gameObject,
                OnStart = targetGraphic.UpdateColorFunctor(),
                OnUpdate = targetGraphic.UpdateColorTimedFunctor(),
                OnEnd = targetGraphic.UpdateColorFunctor()
            });
        }
    }

}