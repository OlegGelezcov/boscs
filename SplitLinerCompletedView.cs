namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class SplitLinerCompletedView : GameBehaviour {

        public Image backgroundImage;
        public RectTransform backFrameTransform;

        public GameObject contentParent;
        public Text failText;

        public Text cashText;
        public Text coinText;
        public CanvasGroup canvasGroup;
        public Button okButton;

        public void Setup(double cashCount, int coinCount, bool success) {


            //var colorAnimator = backgroundImage.gameObject.GetOrAdd<ColorAnimator>();
            //var scaleAnimator = backFrameTransform.gameObject.GetOrAdd<Vector3Animator>();

            /*
            Vector3AnimationData scaleData = new Vector3AnimationData {
                StartValue = Vector3.zero,
                EndValue = Vector3.one,
                Duration = 0.2f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuintic,
                Target = backFrameTransform.gameObject,
                OnStart = (s, o) => backFrameTransform.localScale = s,
                OnUpdate = (s, t, o) => backFrameTransform.localScale = s,
                OnEnd = (s, o) => {
                    backFrameTransform.localScale = s;
                }
            };

            ColorAnimationData colorData = new ColorAnimationData {
                StartValue = new Color(0, 0, 0, 0),
                EndValue = new Color(0, 0, 0, 0.7f),
                Duration = 0.5f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = backgroundImage.gameObject,
                OnStart = (c, o) => backgroundImage.color = c,
                OnUpdate = (c, t, o) => backgroundImage.color = c,
                OnEnd = (c, o) => {
                    backgroundImage.color = c;
                    scaleAnimator.StartAnimation(scaleData);
                }
            };
            colorAnimator.StartAnimation(colorData);*/


            FloatAnimationData alphaData = new FloatAnimationData {
                StartValue = 0,
                EndValue = 1,
                Duration = 0.3f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.Linear,
                Target = gameObject,
                OnStart = (v, o) => canvasGroup.alpha = v,
                OnUpdate = (v, t, o) => canvasGroup.alpha = v,
                OnEnd = (v, o) => canvasGroup.alpha = v
            };
            GetComponent<FloatAnimator>().StartAnimation(alphaData);

            cashText.text = BosUtils.GetCurrencyString(new CurrencyNumber(cashCount), "#FFFFFF", "#FFDF5F");
            coinText.text = coinCount.ToString();

            if(!success) {
                contentParent.Deactivate();
                failText.Activate();
            } else {
                failText.Deactivate();

            }

            okButton.SetListener(() => {
                if (Services != null) {
                    Services.SoundService.PlayOneShot(SoundName.click);
                    backgroundImage.Deactivate();

                    if (success) {
                        Services.PlayerService.AddGenerationCompanyCash(cashCount);
                        if (coinCount > 0) {
                            Services.PlayerService.AddCoins(coinCount);
                        }
                    }

                    SceneManager.UnloadSceneAsync(6);
                } else {
                    backgroundImage.Deactivate();
                }
            });
        }
    }

}