
using UnityEngine.UI;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TutorialFingerWave : GameBehaviour {

        public Vector3Animator scaleAnimator;
        public ColorAnimator colorAnimator;
        public RectTransform rectTransform;
        public Image image;
        private float endScale = 4f;
        private float interval = 1.5f;

        public bool IsAvailable { get; private set; } = true;
        private bool isScaleCompleted = false;
        private bool isColorCompleted = false;
        
        public void StartWave() {
            StartCoroutine(StartWaveImpl());
        }

        private IEnumerator StartWaveImpl() {
            yield return new WaitUntil(() => IsAvailable);
            rectTransform.localScale = Vector3.one;
            image.color = Color.white;
            image.enabled = true;

            isScaleCompleted = false;
            isColorCompleted = false;
            IsAvailable = false;
            var scaleData = new Vector3AnimationData {
                StartValue = Vector3.one,
                EndValue = endScale * Vector3.one,
                AnimationMode = BosAnimationMode.Single,
                Duration = interval,
                EaseType = EaseType.EaseInOutQuad,
                Target = rectTransform.gameObject,
                OnStart = (s, o) => { rectTransform.localScale = s; },
                OnUpdate = (s, t, o) => { rectTransform.localScale = s; },
                OnEnd = (s, o) => {
                    rectTransform.localScale = s;
                    isScaleCompleted = true;
                },
                Events = new List<AnimationEvent<Vector3>> {
                    new AnimationEvent<Vector3>() {
                         Mode = AnimationEventMode.Single,
                        IsValid = (s, t, o) => t >= 0.5f,
                        OnEvent = (s, t, o) => {
                            var colorData = new ColorAnimationData {
                                StartValue = Color.white,
                                EndValue = Color.white.ChangeAlpha(0),
                                AnimationMode = BosAnimationMode.Single,
                                Duration = interval * (1f - t),
                                EaseType = EaseType.EaseInOutQuad,
                                Target = image.gameObject,
                                OnStart = image.UpdateColorFunctor(),
                                OnUpdate = image.UpdateColorTimedFunctor(),
                                OnEnd = image.UpdateColorFunctor(() => {
                                    isColorCompleted = true;
                                    StartCoroutine(EndWaveImpl());
                                })
                            };
                            colorAnimator.StartAnimation(colorData);
                        }
                    }
                }
            };
            scaleAnimator.StartAnimation(scaleData);
        }

        private IEnumerator EndWaveImpl() {
            yield return new WaitUntil(() => isScaleCompleted && isColorCompleted);
            image.enabled = false;
            image.color = Color.white;
            rectTransform.localScale = Vector3.one;
            IsAvailable = true;
        }
        
    }


}