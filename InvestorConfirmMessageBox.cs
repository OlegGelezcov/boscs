namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class InvestorConfirmMessageBox : TypedViewWithCloseButton {

        public override ViewType Type => ViewType.InvestorConfirmMessageBox;

        public override CanvasType CanvasType => CanvasType.UI;

        public override int ViewDepth => 50;



        public Image selfObject;
        public RectTransform background;
        public GameObject[] texts;
        public Text priceText;
        //public EaseType easeType = EaseType.EaseOutQuad;

        public Image panel;
        public ColorAnimator panelColorAnimator;

        public Vector3Animator backScaleAnimator;
        public FloatAnimator backAlphaAnimator;

        public override bool IsModal => false;

        public override void Setup(ViewData data) {
            base.Setup(data);
            MoveIn();
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.InvestorConfirmMessageBox, 0.2f);
                Services.SoundService.PlayOneShot(SoundName.click);
                MoveOut();
            });
            if (data != null) {
                priceText.text = BosUtils.GetCurrencyString(((double)data.UserData).ToCurrencyNumber());
            } else {
                priceText.text = string.Empty;
            }

            Services.SoundService.PlayOneShot(SoundName.race_win);
        }


        public override void OnViewRemove() {
            base.OnViewRemove();
            Services.InvestorService.ReloadAfterSold();
        }

        private void MoveIn() {
            CanvasGroup canvasGroup = background.GetComponent<CanvasGroup>();

            panelColorAnimator.StartAnimation(AnimUtils.GetColorAnimData(panel.color.ChangeAlpha(0), panel.color.ChangeAlpha(.5f), .1f,
                 EaseType.Linear, panel.GetComponent<RectTransform>(), BosAnimationMode.Single, () => {
                     backAlphaAnimator.StartAnimation(new FloatAnimationData {
                         StartValue = 0,
                         EndValue = 1,
                         Duration = 0.15f,
                         AnimationMode = BosAnimationMode.Single,
                         EaseType = EaseType.EaseInOutQuad,
                         Target = background.gameObject,
                         OnStart = canvasGroup.UpdateAlphaFunctor(),
                         OnUpdate = canvasGroup.UpdateAlphaTimedFunctor(),
                         OnEnd = canvasGroup.UpdateAlphaFunctor()
                     });
                     backScaleAnimator.StartAnimation(background.ConstructScaleAnimationData(new Vector3(1.5f, 1.5f, 1), new Vector3(1, 1, 1), 0.15f,
                          BosAnimationMode.Single, EaseType.EaseInOutQuad));
                 }));
        }

        private void MoveOut() {
            CanvasGroup canvasGroup = background.GetComponent<CanvasGroup>();
            backAlphaAnimator.StartAnimation(new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = 0.1f,
                EaseType = EaseType.EaseInOutQuad,
                Target = background.gameObject,
                OnStart = canvasGroup.UpdateAlphaFunctor(),
                OnUpdate = canvasGroup.UpdateAlphaTimedFunctor(),
                OnEnd = canvasGroup.UpdateAlphaFunctor(()=> {
                    panelColorAnimator.StartAnimation(
                        AnimUtils.GetColorAnimData(
                            panel.color.ChangeAlpha(.5f), 
                            panel.color.ChangeAlpha(0), 
                            0.1f, 
                            EaseType.EaseInOutQuad,
                            panel.GetComponent<RectTransform>()));
                })
            });
        }



        //public Vector2 startPosition = new Vector2(-1000, 0); 
        //public Vector2 midPosition = new Vector2();
        //public Vector2 endPosition = new Vector2(0, 1400);

        //public float moveInterval = 0.4f;

        //public Vector3 startScale = new Vector3(0.3f, 0.3f, 1.0f);
        //public Vector3 secondScale = new Vector3(1.1f, 0.3f, 1.0f);
        //public Vector3 thirdScale = new Vector3(1.1f, 1.1f, 1.0f);
        //public Vector3 fourScale = Vector3.one;
        //public float firstScaleInterval = 0.2f;
        //public float secondScaleInterval = 0.1f;
        //public float thirdScaleInterval = 0.1f;


        /*
        private void MoveOut() {
            Vector3AnimationData scale1Data = new Vector3AnimationData {
                Duration = thirdScaleInterval,
                StartValue = fourScale,
                EndValue = thirdScale,
                EaseType = easeType,
                Target = gameObject,
                OnStart = (s, go) => {
                    background.localScale = s;
                    texts.ToList().ForEach(t => t.Deactivate());
                },
                OnUpdate = (s, t, go) => background.localScale = s,
                OnEnd = (s, go) => {
                    background.localScale = s;
                    Vector3AnimationData scale2Data = new Vector3AnimationData {
                        Duration = secondScaleInterval,
                        StartValue = thirdScale,
                        EndValue = secondScale,
                        EaseType = easeType,
                        Target = gameObject,
                        OnStart = (s2, go2) => {
                            background.localScale = s2;
                            
                        },
                        OnUpdate = (s2, t2, go2) => background.localScale = s2,
                        OnEnd = (s2, go2) => {
                            background.localScale = s2;
                            Vector3AnimationData scale3Data = new Vector3AnimationData {
                                Duration = firstScaleInterval,
                                StartValue = secondScale,
                                EndValue = startScale,
                                EaseType = easeType,
                                Target = gameObject,
                                OnStart = (s3, go3) => background.localScale = s3,
                                OnUpdate = (s3, t3, go3) => background.localScale = s3,
                                OnEnd = (s3, go3) => {
                                    background.localScale = s3;
                                    Vector2AnimationData moveOutData = new Vector2AnimationData {
                                        Duration = moveInterval,
                                        StartValue = midPosition,
                                        EndValue = endPosition,
                                        EaseType = easeType,
                                        Target = gameObject,
                                        OnStart = (pos4, go4) => background.anchoredPosition = pos4,
                                        OnUpdate = (pos4, t4, go4) => background.anchoredPosition = pos4,
                                        OnEnd = (pos4, go4) => {
                                            background.anchoredPosition = pos4;
                                            
                                        }
                                    };
                                    background.gameObject.GetOrAdd<Vector2Animator>().StartAnimation(moveOutData);
                                }
                            };
                            background.gameObject.GetComponent<Vector3Animator>().StartAnimation(scale3Data);
                        }
                    };
                    background.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(scale2Data);
                }
            };
            background.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(scale1Data);

            ColorAnimationData colorAnimationData = new ColorAnimationData {
                Duration = firstScaleInterval + secondScaleInterval + thirdScaleInterval,
                StartValue = new Color(0, 0, 0, 0.6f),
                EndValue = new Color(0, 0, 0, 0.0f),
                EaseType = easeType,
                Target = selfObject.gameObject,
                OnStart = (c, img) => selfObject.color = c,
                OnUpdate = (c, timg, img) => selfObject.color = c,
                OnEnd = (c, img) => selfObject.color = c
            };
            selfObject.gameObject.GetOrAdd<ColorAnimator>().StartAnimation(colorAnimationData);
        }

        private void MoveIn() {

            Vector2AnimationData moveInData = new Vector2AnimationData {
                Duration = moveInterval,
                EaseType = easeType,
                EndValue = midPosition,
                StartValue = startPosition,
                Target = gameObject,
                OnStart = (pos, go) => { background.anchoredPosition = pos; },
                OnUpdate = (pos, t, go) => { background.anchoredPosition = pos; },
                OnEnd = (pos, go) => {
                    background.anchoredPosition = pos;
                    Vector3AnimationData scale1Data = new Vector3AnimationData {
                        Duration = firstScaleInterval,
                        EaseType = easeType,
                        StartValue = startScale,
                        EndValue = secondScale,
                        Target = gameObject,
                        OnStart = (s, go2) => background.localScale = s,
                        OnUpdate = (s, t2, go2) => background.localScale = s,
                        OnEnd = (s, go2) => {
                            background.localScale = s;
                            Vector3AnimationData scale2Data = new Vector3AnimationData {
                                Duration = secondScaleInterval,
                                EaseType = easeType,
                                StartValue = secondScale,
                                EndValue = thirdScale,
                                Target = gameObject,
                                OnStart = (s2, go3) => background.localScale = s2,
                                OnUpdate = (s2, t3, go3) => background.localScale = s2,
                                OnEnd = (s2, go3) => {
                                    background.localScale = s2;
                                    Vector3AnimationData scale3Data = new Vector3AnimationData {
                                        Duration = thirdScaleInterval,
                                        EaseType = easeType,
                                        StartValue = thirdScale,
                                        EndValue = fourScale,
                                        Target = gameObject,
                                        OnStart = (s3, go4) => background.localScale = s3,
                                        OnUpdate = (s3, t4, go4) => background.localScale = s3,
                                        OnEnd = (s3, go4) => {
                                            background.localScale = s3;
                                            texts.ToList().ForEach(t => t.Activate());
                                        }
                                    };
                                    background.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(scale3Data);
                                }
                            };
                            background.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(scale2Data);


                        }
                    };
                    background.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(scale1Data);

                    ColorAnimationData colorAnimationData = new ColorAnimationData {
                        Duration = firstScaleInterval + secondScaleInterval + thirdScaleInterval,
                        StartValue = new Color(0, 0, 0, 0),
                        EndValue = new Color(0, 0, 0, 0.6f),
                        EaseType = easeType,
                        Target = selfObject.gameObject,
                        OnStart = (c, img) => selfObject.color = c,
                        OnUpdate = (c, timg, img) => selfObject.color = c,
                        OnEnd = (c, img) => selfObject.color = c
                    };
                    selfObject.gameObject.GetOrAdd<ColorAnimator>().StartAnimation(colorAnimationData);
                }
            };
            background.gameObject.GetOrAdd<Vector2Animator>().StartAnimation(moveInData);
        }

        */
    }

}