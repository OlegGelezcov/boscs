namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class AnimationExtensions  {

        public static void MoveFromTo(this RectTransform transform,
            Vector2 startPosition, Vector2 endPosition, float duration, EaseType easeType, Action onComplete = null) {
            Vector2Animator animator = transform.gameObject.GetOrAdd<Vector2Animator>();
            Vector2AnimationData data = new Vector2AnimationData {
                StartValue = startPosition,
                EndValue = endPosition,
                Duration = duration,
                EaseType = easeType,
                AnimationMode = BosAnimationMode.Single,
                Target = transform.gameObject,
                OnStart = transform.UpdatePositionFunctor(),
                OnUpdate = transform.UpdatePositionTimedFunctor(),
                OnEnd = transform.UpdatePositionFunctor(onComplete)
            };
            animator.StartAnimation(data);
        }
    }
}
