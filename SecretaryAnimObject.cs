namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SecretaryAnimObject : GameBehaviour {
        
        public enum Direction {
            Left,
            Right,
            StayRight,
            StayLeft
        }
        
        private readonly Dictionary<Direction, string> animNames = new Dictionary<Direction, string> {
            [Direction.StayLeft] = "stayleft",
            [Direction.Right] = "move",
            [Direction.StayRight] = "stayright",
            [Direction.Left] = "move"
        };
        
        public Vector2 leftPosition;
        public Vector2 rightPosition;
        public Text countText;
        public GameObject progressParent;
        public Image progressFill;

        private Direction direction = Direction.StayRight;
        private RectTransform rectTransform;
        private Animator animator;
        public GameObject effectPrefab;
        
        public Auditor Auditor { get; private set; }
        private bool isInitialized = false;

        private int OldRepairCount { get; set; }

        public void Setup(Auditor auditor) {
            this.Auditor = auditor;
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();
            rectTransform.anchoredPosition = rightPosition;
            UpdateState(Auditor.State);
            isInitialized = true;
            OldRepairCount = Auditor.RemainCount;
        }



        public override void OnEnable() {
            base.OnEnable();
            GameEvents.AuditorStateChanged += OnAuditorStateChanged;
        }

        public override void OnDisable() {
            GameEvents.AuditorStateChanged -= OnAuditorStateChanged;
            base.OnDisable();
        }

        
        public override void Update() {
            base.Update();

            if (isInitialized) {
                if(OldRepairCount > Auditor.RemainCount ) {
                    int count = Mathf.Abs(OldRepairCount - Auditor.RemainCount);
                    if (count > 0) {
                        CreateWorkEffect(rectTransform, effectPrefab);
                    }
                    OldRepairCount = Auditor.RemainCount;
                    countText.text = Auditor.RemainCount.ToString();
                }
                switch (Auditor.State) {
                    case AuditorState.MoveToLoad: {
                        rectTransform.anchoredPosition =
                            Vector2.Lerp(rightPosition, leftPosition, Auditor.NormalizedTimer);
                    }
                        break;
                    case AuditorState.Loading: {
                        progressFill.fillAmount = Auditor.NormalizedTimer;
                        countText.text = Auditor.RemainCount.ToString(); //Mathf.RoundToInt(Auditor.NormalizedTimer * Auditor.Count).ToString();
                    }
                        break;
                    case AuditorState.MoveToUnload: {
                        rectTransform.anchoredPosition =
                            Vector2.Lerp(leftPosition, rightPosition, Auditor.NormalizedTimer);
                    }
                        break;
                    case AuditorState.Unloading: {
                        progressFill.fillAmount = 1f - Auditor.NormalizedTimer;
                            //int newCount = (int)((1f - Auditor.NormalizedTimer) * Auditor.Count);
                            //int oldCount = MechanicAnimObject.GetIntFromText(countText.text);
                        //countText.text = (newCount).ToString();
                        //    if(oldCount > 0 && newCount < oldCount ) {
                        //        CreateWorkEffect(rectTransform, effectPrefab);
                        //    }
                    }
                        break;
                }
            }
        }

        public static void CreateWorkEffect(Transform mechanic, GameObject effectPrefab) {
            GameObject inst = Instantiate<GameObject>(effectPrefab);
            RectTransform effectRect = inst.GetComponent<RectTransform>();
            effectRect.SetParent(mechanic.parent, false);
            effectRect.anchoredPosition = mechanic.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 120);

            var colorData = AnimUtils.GetColorAnimData(Color.white, Color.white.ChangeAlpha(0), 2, EaseType.EaseInOutQuad, effectRect);

            var scaleData = new Vector3AnimationData {
                StartValue = Vector3.one,
                EndValue = Vector3.one * 0.2f,
                Duration = 2,
                EaseType = EaseType.EaseInOutQuad,
                Target = inst,
                OnStart = effectRect.UpdateScaleFunctor(),
                OnUpdate = effectRect.UpdateScaleTimedFunctor(),
                OnEnd = effectRect.UpdateScaleFunctor()
            };

            var posData = new Vector2AnimationData {
                StartValue = effectRect.anchoredPosition,
                EndValue = effectRect.anchoredPosition + new Vector2(0, 250),
                Duration = 2,
                EaseType = EaseType.EaseInOutQuad,
                OnStart = effectRect.UpdatePositionFunctor(),
                OnUpdate = effectRect.UpdatePositionTimedFunctor(),
                OnEnd = effectRect.UpdatePositionFunctor()
            };
            inst.GetOrAdd<ColorAnimator>().StartAnimation(colorData);
            inst.GetOrAdd<Vector3Animator>().StartAnimation(scaleData);
            inst.GetOrAdd<Vector2Animator>().StartAnimation(posData);
            Destroy(inst, 2);
        }

        private void OnAuditorStateChanged(AuditorState oldState, AuditorState newState, Auditor info) {
            if (isInitialized) {
                if (Auditor != null && (Auditor.Id == info.Id)) {
                    UpdateState(newState);
                }
            }
        }

        private void UpdateState(AuditorState state) {
            switch (state) {
                case AuditorState.MoveToLoad: {
                        //progressParent.Deactivate();
                        //countText.Deactivate();
                        progressParent.Activate();
                        countText.Activate();
                    rectTransform.localScale = new Vector3(-1, 1, 1);
                    countText.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                    animator.SetTrigger(animNames[Direction.Right]);
                    rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, Auditor.NormalizedTimer);
                }
                    break;
                case AuditorState.Loading: {
                    animator.SetTrigger(animNames[Direction.StayLeft]);
                    progressParent.Activate();
                    countText.Activate();
                    rectTransform.localScale = Vector3.one;
                    countText.GetComponent<RectTransform>().localScale = Vector3.one;

                    rectTransform.anchoredPosition = leftPosition;
                }
                    break;
                case AuditorState.MoveToUnload: {
                    progressParent.Activate();
                    countText.Activate();
                    rectTransform.localScale = Vector3.one;
                        countText.GetComponent<RectTransform>().localScale = Vector3.one;
                        animator.SetTrigger(animNames[Direction.Right]);
                    rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, Auditor.NormalizedTimer);
                }
                    break;
                case AuditorState.Unloading: {
                    progressParent.Activate();
                    countText.Activate();
                    animator.SetTrigger(animNames[Direction.StayRight]);
                    rectTransform.anchoredPosition = rightPosition;
                    rectTransform.localScale = Vector3.one;
                        countText.GetComponent<RectTransform>().localScale = Vector3.one;
                    }
                    break;
                default: {
                    rectTransform.localScale = Vector3.one;
                        countText.GetComponent<RectTransform>().localScale = Vector3.one;
                        animator.SetTrigger(animNames[Direction.StayRight]);
                    progressParent.Deactivate();
                    countText.Deactivate();
                    rectTransform.anchoredPosition = rightPosition;
                }
                    break;
            }
        }
        
    }

}