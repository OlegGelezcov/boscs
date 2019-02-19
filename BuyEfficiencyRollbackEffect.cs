namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BuyEfficiencyRollbackEffect : GameBehaviour {

        public RectTransform lightTransform;
        public RectTransform percentTextTransform;
        public RectTransform arrowTransform;
        
        public Text descriptionText;
        public Text descriptionValueText;
        public Image managerIcon;
        public CanvasGroup canvasGroup;

        public override void Start() {
            base.Start();

        }

        private void StartLight() {
            var rotAnimator = lightTransform.gameObject.GetOrAdd<FloatAnimator>();
            rotAnimator.StartAnimation(new FloatAnimationData {
                AnimationMode = BosAnimationMode.Loop,
                Duration = 2,
                EaseType = EaseType.Linear,
                Target = lightTransform.gameObject,
                StartValue = 0,
                EndValue = 360,
                OnStart = lightTransform.UpdateZRotation(),
                OnUpdate = lightTransform.UpdateZRotationTimed(),
                OnEnd = lightTransform.UpdateZRotation()
            });
        }

        private void StartPercentText() {
            percentTextTransform.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(
                percentTextTransform.ConstructScaleAnimationData(
                    Vector3.one, 
                    Vector3.one * 1.25f,
                    1f, 
                    BosAnimationMode.PingPong, 
                    EaseType.EaseInOutQuad, 
                    () => { })
            );
        }

        private void StartArrow() {
            arrowTransform.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(
                arrowTransform.ConstructScaleAnimationData(
                    Vector3.one,
                    Vector3.one * 1.25f,
                    0.5f,
                     BosAnimationMode.PingPong,
                      EaseType.EaseInOutQuad,
                      () => { }
                    )
                );
        }

        public void Setup(string text, float value, int managerId ) {
            descriptionText.text = text;
            descriptionValueText.text = $"+{(int)(value * 100)}%";
            
            ViewService.Utils.ApplyManagerIcon(managerIcon, Services.GenerationService.GetGetenerator(managerId), true);

            RectTransform selfTransform = GetComponent<RectTransform>();
            Vector2Animator positionAnimator = gameObject.GetOrAdd<Vector2Animator>();
            FloatAnimator alphaAnimator = gameObject.GetOrAdd<FloatAnimator>();

            const float kDuration = 2.5f;
            positionAnimator.StartAnimation(new Vector2AnimationData {
                StartValue = selfTransform.anchoredPosition,
                EndValue = selfTransform.anchoredPosition + Vector2.up * 250,
                Duration = kDuration,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = gameObject,
                OnStart = selfTransform.UpdatePositionFunctor(),
                OnUpdate = selfTransform.UpdatePositionTimedFunctor(),
                OnEnd = selfTransform.UpdatePositionFunctor(),
                Events = new List<AnimationEvent<Vector2>> {
                           new AnimationEvent<Vector2> {
                                IsValid = (p, t, o) => t >= 0.5f,
                                 Mode = AnimationEventMode.Single,
                                  OnEvent = (p, t, o) => {
                                      alphaAnimator.StartAnimation(new FloatAnimationData{
                                          StartValue = 1,
                                          EndValue = 0,
                                          AnimationMode = BosAnimationMode.Single,
                                          Duration = kDuration * .5f,
                                          EaseType = EaseType.Linear,
                                          Target = gameObject,
                                          OnStart = canvasGroup.UpdateAlphaFunctor(),
                                          OnUpdate = canvasGroup.UpdateAlphaTimedFunctor(),
                                          OnEnd = canvasGroup.UpdateAlphaFunctor(() => {
                                            Destroy(gameObject);
                                          })
                                      });
                                  }
                           }
                       }
            });

            StartLight();
            StartPercentText();
            StartArrow();
        }
    }

}