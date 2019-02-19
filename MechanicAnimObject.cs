namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class MechanicAnimObject : GameBehaviour {

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

        private Direction direction = Direction.StayLeft;
        private RectTransform rectTransform;
        private Animator animator;
        public TempMechanicInfo TempMechanic { get; private set; }
        public GameObject effectPrefab;

        private bool isInitialized = false;

        private int OldRepairCount { get; set; }
       
        public void Setup(TempMechanicInfo mechanic ) {
            this.TempMechanic = mechanic;
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();
            rectTransform.anchoredPosition = leftPosition;
            UpdateState(TempMechanic.State);
            isInitialized = true;
            Destroy(GetComponent<ConstMechanicAnimObject>());
            OldRepairCount = mechanic.RemainCount;
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.TempMechanicInfoStateChanged += OnTempMechanicStateChanged;
        }

        public override void OnDisable() {
            GameEvents.TempMechanicInfoStateChanged -= OnTempMechanicStateChanged;
            base.OnDisable();
        }



        private void OnTempMechanicStateChanged(TempMechanicState oldState, TempMechanicState newState, TempMechanicInfo info ) {
            if (isInitialized) {
                if (TempMechanic != null && (TempMechanic.Id == info.Id)) {
                    UpdateState(newState);
                }
            }
        }

        private float completedTimer = 0;
        private bool isDeleted = false;

        private void CreateEffect(int count) {
            StartCoroutine(CreateEffectImpl(count));
        }

        private IEnumerator CreateEffectImpl(int count) {
            for (int i = 0; i < count; i++) {
                CreateWorkEffect(rectTransform, effectPrefab);
                yield return new WaitForSeconds(0.1f);
            }
        }

        public override void Update() {
            base.Update();

            if (isInitialized) {

                if(OldRepairCount > TempMechanic.RemainCount) {
                    int count = Mathf.Abs(OldRepairCount - TempMechanic.RemainCount);
                    CreateEffect(count);
                    OldRepairCount = TempMechanic.RemainCount;
                }
                switch (TempMechanic.State) {
                    case TempMechanicState.MoveToLoad: {
                            rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, TempMechanic.NormalizedTimer);
                        }
                        break;
                    case TempMechanicState.Loading: {
                            progressFill.fillAmount = TempMechanic.NormalizedTimer;
                            countText.text = TempMechanic.RemainCount.ToString(); //Mathf.RoundToInt(TempMechanic.NormalizedTimer * TempMechanic.Count).ToString();
                        }
                        break;
                    case TempMechanicState.MoveToUnload: {
                            rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, TempMechanic.NormalizedTimer);
                        }
                        break;
                    case TempMechanicState.Unloading: {
                            progressFill.fillAmount = 1.0f - TempMechanic.NormalizedTimer;
                            int newCount = TempMechanic.RemainCount; //(int)((1f - TempMechanic.NormalizedTimer) * TempMechanic.Count);
                            int oldCount = GetIntFromText(countText.text);
                            countText.text = (newCount).ToString();
                            //if(oldCount > 0 && newCount < oldCount) {
                            //    CreateWorkEffect(rectTransform, effectPrefab);
                            //}
                            //CreateEffect(3);
                        }
                        break;
                    default: {
                            //Debug.Log($"invalid mechanic state => {TempMechanic.State}");
                            completedTimer += Time.deltaTime;
                            if(completedTimer > 2 ) {
                                if(!isDeleted) {
                                    isDeleted = true;
                                    GetComponentInParent<TempMechanicView>()?.RemoveTempMechanicView(TempMechanic);
                                }
                            }
                        }
                        break;
                }
            } else {
                //Debug.Log($"not initialized...");
            }

        }

        public static int GetIntFromText(string text) {
            int val;
            if(int.TryParse(text, out val)) {
                return val;
            }
            return -100;
        }

        public static void CreateWorkEffect(Transform mechanic, GameObject effectPrefab) {
            GameObject inst = Instantiate<GameObject>(effectPrefab);
            RectTransform effectRect = inst.GetComponent<RectTransform>();
            effectRect.SetParent(mechanic.parent, false);
            effectRect.anchoredPosition = mechanic.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 120);

            var colorData = AnimUtils.GetColorAnimData(Color.white, Color.white.ChangeAlpha(0), 2, EaseType.EaseInOutQuad, effectRect);
            var rotateData = new FloatAnimationData {
                StartValue = -40,
                EndValue = 40,
                Duration = 0.5f,
                EaseType = EaseType.EaseInOutQuad,
                OnStart = effectRect.UpdateZRotation(),
                OnUpdate = effectRect.UpdateZRotationTimed(),
                OnEnd = effectRect.UpdateZRotation(),
                Target = inst,
                AnimationMode = BosAnimationMode.PingPong
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
            inst.GetOrAdd<FloatAnimator>().StartAnimation(rotateData);
            inst.GetOrAdd<Vector2Animator>().StartAnimation(posData);
            Destroy(inst, 2);
        }

        private void UpdateState(TempMechanicState state) {
            switch (state) {
                case TempMechanicState.MoveToLoad: {
                        //progressParent.Deactivate();
                        progressParent.Activate();
                        progressFill.fillAmount = 0;
                        //countText.Deactivate();
                        countText.Activate();
                        rectTransform.localScale = Vector3.one;
                        countText.GetComponent<RectTransform>().localScale = Vector3.one;
                        animator.SetTrigger(animNames[Direction.Right]);
                        //progressParent.Deactivate();
                        //countText.Deactivate();
                        rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, TempMechanic.NormalizedTimer);
                    }
                    break;
                case TempMechanicState.Loading: {
                        animator.SetTrigger(animNames[Direction.StayRight]);
                        progressParent.Activate();
                        countText.Activate();
                        rectTransform.anchoredPosition = rightPosition;
                    }
                    break;
                case TempMechanicState.MoveToUnload: {
                        progressParent.Activate();
                        countText.Activate();
                        rectTransform.localScale = new Vector3(-1, 1, 1);
                        countText.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                        animator.SetTrigger(animNames[Direction.Left]);
                        rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, TempMechanic.NormalizedTimer);
                    }
                    break;
                case TempMechanicState.Unloading: {
                        progressParent.Activate();
                        countText.Activate();
                        animator.SetTrigger(animNames[Direction.StayLeft]);
                        rectTransform.anchoredPosition = leftPosition;
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
                        rectTransform.anchoredPosition = leftPosition;
                    }
                    break;
            }
        }


    }

}