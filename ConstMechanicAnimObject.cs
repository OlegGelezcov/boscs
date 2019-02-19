namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConstMechanicAnimObject : GameBehaviour {

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
        private RectTransform rectTransform;
        private Animator animator;
        public GameObject effectPrefab;

        private bool isInitialized = false;


        public MechanicInfo Mechanic { get; private set; }

        public void Setup(MechanicInfo mechanic) {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();
            rectTransform.anchoredPosition = leftPosition;
            countText.text = string.Empty;
            Mechanic = mechanic;
            isInitialized = true;
            UpdateState(Mechanic.State);
            var mechanicAnimObj = GetComponent<MechanicAnimObject>();
            if(mechanicAnimObj != null ) {
                Destroy(mechanicAnimObj);
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.MechanicStateChanged += OnMechanicStateChanged;
            GameEvents.MechanicWorkCircleCompleted += OnWorkCompleted;
        }

        public override void OnDisable() {
            GameEvents.MechanicStateChanged -= OnMechanicStateChanged;
            GameEvents.MechanicWorkCircleCompleted -= OnWorkCompleted;
            base.OnDisable();
        }

        private void OnMechanicStateChanged(MechanicState oldState, MechanicState newState, MechanicInfo mechanic ) {
            if(isInitialized) {
                if(Mechanic != null && (Mechanic.Id == mechanic.Id)) {
                    UpdateState(newState);
                    //Debug.Log($"new mechanic  => {newState}");
                }
            }
        }

        private void OnWorkCompleted(MechanicInfo otherMechanic, int circles) {
            if(isInitialized) {
                if(otherMechanic.Id == Mechanic.Id ) {
                    StartCoroutine(CreateEffectImpl(circles));
                }
            }
        }

        private IEnumerator CreateEffectImpl(int count) {
            for(int i = 0; i < count; i++ ) {
                MechanicAnimObject.CreateWorkEffect(rectTransform, effectPrefab);
                yield return new WaitForSeconds(0.2f);
            }
        }

        //private float prevTimer = 0;
        public override void Update() {
            base.Update();
            if(isInitialized) {
                switch(Mechanic.State) {
                    case MechanicState.MoveToLoad: {
                            rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, Mechanic.NormalizedTimer);
                        }
                        break;
                    case MechanicState.Loading: {
                            progressFill.fillAmount = Mechanic.NormalizedTimer;
                        }
                        break;
                    case MechanicState.MoveToUnload: {
                            //Vector2 oldPosition = rectTransform.anchoredPosition;
                            rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, Mechanic.NormalizedTimer);
                            //print($"delta pos => {rectTransform.anchoredPosition - oldPosition}, delta timer =>{Mechanic.NormalizedTimer - prevTimer}");
                            //prevTimer = Mechanic.NormalizedTimer;
                        }
                        break;
                    case MechanicState.Unloading: {
                            progressFill.fillAmount = 1f - Mechanic.NormalizedTimer;
                        }
                        break;
                }

            }
        }

        private void UpdateState(MechanicState state ) {
            print($"update state => {state}");
            switch(state) {
                case MechanicState.MoveToLoad: {
                        progressParent.Deactivate();
                        rectTransform.localScale = new Vector3(-1, 1, 1);
                        animator.SetTrigger(animNames[Direction.Left]);
                        rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, Mechanic.NormalizedTimer);
                    }
                    break;
                case MechanicState.Loading: {
                        animator.SetTrigger(animNames[Direction.StayLeft]);
                        rectTransform.anchoredPosition = leftPosition;
                        progressParent.Activate();
                    }
                    break;
                case MechanicState.MoveToUnload: {
                        progressParent.Activate();
                        rectTransform.localScale = Vector3.one;
                        animator.SetTrigger(animNames[Direction.Right]);
                        rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, Mechanic.NormalizedTimer);
                    }
                    break;
                case MechanicState.Unloading: {
                        rectTransform.localScale = Vector3.one;
                        progressParent.Activate();
                        animator.SetTrigger(animNames[Direction.StayRight]);
                        rectTransform.anchoredPosition = rightPosition;
                    }
                    break;
            }
        }
    }

}