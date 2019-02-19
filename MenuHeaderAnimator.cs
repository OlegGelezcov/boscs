namespace Bos.UI {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class MenuHeaderAnimator : GameBehaviour {

        public float duration = 0.3f;
        public EaseType easeType = EaseType.EaseInOutCubic;
        public RectTransform rectangularBackgroundTransform;
        public Image rectangularBackgroundImage;
        public RectTransform rightPlanetFrameTransform;
        public RectTransform rightPlanetSmallTransform;
        public RectTransform leftModuleTransform;
        public RectTransform leftModuleSmallTransform;
        //public Button x2ProfitButton;
        public Button debugButton;

        private Vector2AnimationData rectangularBackgroundInData = null;
        private Vector2AnimationData rectangularBackgroundOutData = null;

        private Vector2AnimationData rightPlanetFrameInData = null;
        private Vector2AnimationData rightPlanetFrameSmallOutData = null;

        private Vector2AnimationData leftModuleFrameInData = null;
        private Vector2AnimationData leftModuleFrameSmallOutData = null;

        public HeaderState State { get; private set; } = HeaderState.Expanded;

        private GameScrollView gameScroll = null;

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        private bool isAnimating = false;

        private int totalCountActiveGenerators;


        public override void Start() {
            base.Start();

            if(debugButton != null ) {
                debugButton.SetListener(() => ViewService.Show(ViewType.DebugView));
            }



            gameScroll = FindObjectOfType<GameScrollView>();
            totalCountActiveGenerators = gameScroll.GetComponentsInChildren<PlanetGeneratorView>().Length;
            if(totalCountActiveGenerators == 0 ) {
                totalCountActiveGenerators = 1;
            }
            //Debug.Log($"founded total count of active generators => {totalCountActiveGenerators}".BoldItalic().Colored(ConsoleTextColor.navy));

            Color startColor = rectangularBackgroundImage.color;

            leftModuleFrameInData = new Vector2AnimationData {
                StartValue = new Vector2(-641, -380),
                EndValue = new Vector2(-1056, -380),
                Duration = duration * 0.5f,
                EaseType = easeType,
                Target = leftModuleTransform.gameObject,
                OnStart = (pos, go) => leftModuleTransform.anchoredPosition = pos,
                OnEnd = (pos, go) => {
                    leftModuleTransform.anchoredPosition = pos;
                    Vector2AnimationData smallData = new Vector2AnimationData {
                        StartValue = new Vector2(-1053, -253.4f),
                        EndValue = new Vector2(-903, -253.4f),
                        Duration = duration * 0.5f,
                        EaseType = easeType,
                        Target = leftModuleSmallTransform.gameObject,
                        OnStart = (pos2, go2) => leftModuleSmallTransform.anchoredPosition = pos2,
                        OnEnd = (pos2, go2) => leftModuleSmallTransform.anchoredPosition = pos2,
                        OnUpdate = (pos2, t2, go2) => leftModuleSmallTransform.anchoredPosition = pos2
                    };
                    leftModuleSmallTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        smallData
                    });
                },
                OnUpdate = (pos, t, go) => leftModuleTransform.anchoredPosition = pos
            };

            leftModuleFrameSmallOutData = new Vector2AnimationData {
                StartValue = new Vector2(-903, -253.4f),
                EndValue = new Vector2(-1053, -253.4f),
                Duration = duration * 0.5f,
                EaseType = easeType,
                Target = leftModuleSmallTransform.gameObject,
                OnStart = (pos, go) => leftModuleSmallTransform.anchoredPosition = pos,
                OnEnd = (pos, go) => {
                    leftModuleSmallTransform.anchoredPosition = pos;
                    Vector2AnimationData leftData = new Vector2AnimationData {
                        StartValue = new Vector2(-1056, -380),
                        EndValue = new Vector2(-641, -380),
                        EaseType = EaseType.EaseOutCubic,
                        Duration = duration ,
                        OnStart = (pos2, go2) => leftModuleTransform.anchoredPosition = pos2,
                        OnEnd = (pos2, go2) => leftModuleTransform.anchoredPosition = pos2,
                        OnUpdate = (pos2, t2, go2) => leftModuleTransform.anchoredPosition = pos2
                    };
                    leftModuleTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        leftData
                    });
                },
                OnUpdate = (pos, t, go) => leftModuleSmallTransform.anchoredPosition = pos,
            };

            rightPlanetFrameInData = new Vector2AnimationData {
                StartValue = new Vector2(640, -380),
                EndValue = new Vector2(1050, -380),
                Duration = duration * 0.5f,
                EaseType = easeType,
                OnStart = (pos, go) => rightPlanetFrameTransform.anchoredPosition = pos,
                OnEnd = (pos, go) => {
                    rightPlanetFrameTransform.anchoredPosition = pos;
                    Vector2AnimationData smallData = new Vector2AnimationData {
                        StartValue = new Vector2(1056, -253.4f),
                        EndValue = new Vector2(900, -253.4f),
                        Duration = 0.5f * duration,
                        EaseType = easeType,
                        OnStart = (pos2, go2) => rightPlanetSmallTransform.anchoredPosition = pos2,
                        OnEnd = (pos2, go2) => rightPlanetSmallTransform.anchoredPosition = pos2,
                        OnUpdate = (pos2, t2, go2) => rightPlanetSmallTransform.anchoredPosition = pos2,
                        Target = rightPlanetSmallTransform.gameObject
                    };
                    rightPlanetSmallTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        smallData
                    });
                },
                OnUpdate = (pos, t, go) => rightPlanetFrameTransform.anchoredPosition = pos,
                Target = rightPlanetFrameTransform.gameObject
            };



            rightPlanetFrameSmallOutData = new Vector2AnimationData {
                StartValue = new Vector2(900, -253.4f),
                EndValue = new Vector2(1056, -253.4f),
                Duration = 0.5f * duration,
                EaseType = easeType,
                OnStart = (pos2, gos2) => rightPlanetSmallTransform.anchoredPosition = pos2,
                OnEnd = (pos2, go2) => {
                    rightPlanetSmallTransform.anchoredPosition = pos2;
                    Vector2AnimationData rightData = new Vector2AnimationData {
                        StartValue = new Vector2(1050, -380),
                        EndValue = new Vector2(640, -380),
                        Duration = duration,
                        EaseType = EaseType.EaseOutCubic,
                        Target = rightPlanetFrameTransform.gameObject,
                        OnStart = (pos, go) => rightPlanetFrameTransform.anchoredPosition = pos,
                        OnEnd = (pos, go) => rightPlanetFrameTransform.anchoredPosition = pos,
                        OnUpdate = (pos, t, go) => rightPlanetFrameTransform.anchoredPosition = pos
                    };
                    rightPlanetFrameTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        rightData
                    });
                },
                OnUpdate = (pos2, t2, go2) => rightPlanetSmallTransform.anchoredPosition = pos2
            };




            rectangularBackgroundInData = new Vector2AnimationData {
                StartValue = new Vector2(0, 0),
                EndValue = new Vector2(0, 383),
                Duration = duration,
                EaseType = easeType,
                Target = rectangularBackgroundTransform.gameObject,
                OnStart = (pos, go) => {
                    rectangularBackgroundTransform.anchoredPosition = pos;
                    rightPlanetFrameTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        rightPlanetFrameInData
                    });
                    leftModuleTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        leftModuleFrameInData
                    });
                },
                OnEnd = (pos, go) => {
                    State = HeaderState.Collapsed;
                    isAnimating = false;
                    rectangularBackgroundTransform.anchoredPosition = pos; 
                },
                OnUpdate = (pos, t, go) => {
                    rectangularBackgroundTransform.anchoredPosition = pos;
                },
                Events = new List<AnimationEvent<Vector2>> {
                     new AnimationEvent<Vector2> {
                          Mode = AnimationEventMode.Single,
                           IsValid = (pos, t, go) => {
                               if(pos.y >= 0f) {
                                   return true;
                               }
                               return false;
                           },
                            OnEvent = (pos, t, go) => {
                                Color sourceColor = rectangularBackgroundImage.color;
                                ColorAnimationData colorData = new ColorAnimationData {
                                     Duration = duration * Mathf.Clamp01( 60 / rectangularBackgroundTransform.sizeDelta.y) * 0.2f,
                                     EaseType = easeType,
                                     StartValue = sourceColor,
                                     EndValue = new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0),
                                     Target = rectangularBackgroundImage.gameObject,
                                     OnStart = (c, go2) => rectangularBackgroundImage.color = c,
                                     OnEnd = (c, go2) => rectangularBackgroundImage.color = c,
                                     OnUpdate = (c, t2, go2) => rectangularBackgroundImage.color = c
                                };
                                rectangularBackgroundImage.GetComponent<ColorAnimator>().StartAnimation(new List<ColorAnimationData> {
                                    colorData
                                });
                            }
                     }
                 }
            };

            rectangularBackgroundOutData = new Vector2AnimationData {
                StartValue = new Vector2(0, 383),
                EndValue = new Vector2(0, 0),
                Duration = duration,
                EaseType = easeType,
                OnStart = (pos, go) => {
                    
                    rectangularBackgroundTransform.anchoredPosition = pos;
                    /*
                    rightPlanetFrameTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        rightPlanetFrameOutData
                    });*/
                    rightPlanetSmallTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        rightPlanetFrameSmallOutData
                    });
                    leftModuleSmallTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                        leftModuleFrameSmallOutData
                    });
                },
                OnEnd = (pos, go) => {
                    State = HeaderState.Expanded;
                    isAnimating = false;
                    rectangularBackgroundTransform.anchoredPosition = pos; },
                OnUpdate = (pos, t, go) => rectangularBackgroundTransform.anchoredPosition = pos,
                Target = rectangularBackgroundTransform.gameObject,
                Events = new List<AnimationEvent<Vector2>> {
                          new AnimationEvent<Vector2> {
                              Mode = AnimationEventMode.Single,
                               IsValid = (pos, t, go) => {
                                   if(pos.y <= 60 ) {
                                       return true;
                                   }
                                   return false;
                               },
                               OnEvent = (pos, t, go) => {
                                   ColorAnimationData colorData = new ColorAnimationData {
                                        Duration = (1.0f - t) * duration,
                                        EaseType = easeType,
                                        StartValue = new Color(0, 0, 0, 0),
                                        EndValue = startColor,
                                        Target = rectangularBackgroundImage.gameObject,
                                        OnStart = (c, go2) => rectangularBackgroundImage.color = c,
                                        OnEnd = (c, go2) => rectangularBackgroundImage.color = c,
                                        OnUpdate = (c, t2, go2 ) => rectangularBackgroundImage.color = c
                                   };
                                   rectangularBackgroundImage.GetComponent<ColorAnimator>().StartAnimation(new List<ColorAnimationData>{
                                       colorData
                                   });
                               }
                          }
                      }
            };

            updateTimer.Setup(0.4f, (delta) => {
                if (gameScroll.VerticalNormalizedPosition >= MaxVerticalNormalizedPositionWhenEnableHiding) {
                    if (State == HeaderState.Collapsed && !isAnimating) {
                        Expand();
                    }
                } else {
                    if (State == HeaderState.Expanded && !isAnimating) {
                        Collapse();
                    }
                }
            }, true);

        }

        private float MaxVerticalNormalizedPositionWhenEnableHiding {
            get {
                //default value
                if(totalCountActiveGenerators == 1 ) {
                    return 0.8f;
                }
                return 1f - 1f / totalCountActiveGenerators;
            }
        }

        private void Collapse() {
            if (State == HeaderState.Expanded && !isAnimating) {
                rectangularBackgroundTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                    rectangularBackgroundInData
                });
                isAnimating = true;
            }
        }

        private void Expand() {
            if (State == HeaderState.Collapsed && !isAnimating) {
                rectangularBackgroundTransform.GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData> {
                    rectangularBackgroundOutData
                });
                isAnimating = true;
            }
        }

        public override void Update() {
            base.Update();

            /*
            if(Input.GetKeyUp(KeyCode.Space)) {
                if(State == HeaderState.Expanded) {
                    Collapse();
                } else {
                    Expand();
                }
            }

            Debug.Log($"game scroll view position => {gameScroll.VerticalNormalizedPosition}".Colored(ConsoleTextColor.orange).Bold());*/

            updateTimer.Update();
        }
    }

    public enum HeaderState { Expanded, Collapsed}

}