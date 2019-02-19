namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ConstSecretaryAnimObject : GameBehaviour {
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
        public GameObject effectPrefab;

        private RectTransform rectTransform;
        private Animator animator;
        private bool isInitialized = false;
        public SecretaryInfo Secretary { get; private set; }

        public void Setup(SecretaryInfo secretary) {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();
            rectTransform.anchoredPosition = rightPosition;
            countText.text = string.Empty;
            Secretary = secretary;
            isInitialized = true;
            UpdateState(Secretary.State);

        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.SecretaryStateChanged += OnSecretaryStateChanged;
            GameEvents.SecretaryWorkCircleCompleted += OnWorkCompleted;
        }

        public override void OnDisable() {
            GameEvents.SecretaryStateChanged -= OnSecretaryStateChanged;
            GameEvents.SecretaryWorkCircleCompleted -= OnWorkCompleted;
            base.OnDisable();
        }

        private void OnSecretaryStateChanged(SecretaryState oldState, SecretaryState newState, SecretaryInfo secretary) {
            if(isInitialized) {
                if((Secretary != null) && (Secretary.GeneratorId == secretary.GeneratorId)) {
                    UpdateState(newState);
                }
            }
        }

        private void OnWorkCompleted(SecretaryInfo otherSecretary, int circles) {
            if(isInitialized ) {
                if(otherSecretary.GeneratorId == Secretary.GeneratorId ) {
                    StartCoroutine(CreateEffectImpl(circles));
                }
            }
        }

        private IEnumerator CreateEffectImpl(int count ) {
            for(int i = 0; i < count; i++ ) {
                SecretaryAnimObject.CreateWorkEffect(rectTransform, effectPrefab);
                yield return new WaitForSeconds(0.2f);
            }
        }


        private void UpdateState(SecretaryState state) {
            print($"update state => {state}");
            switch (state) {
                case SecretaryState.MoveToLoad: {
                        progressParent.Deactivate();
                        rectTransform.localScale = new Vector3(-1, 1, 1);
                        animator.SetTrigger(animNames[Direction.Left]);
                        rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, Secretary.NormalizedTimer);
                    }
                    break;
                case SecretaryState.Loading: {
                        animator.SetTrigger(animNames[Direction.StayLeft]);
                        rectTransform.anchoredPosition = leftPosition;
                        progressParent.Activate();
                    }
                    break;
                case SecretaryState.MoveToUnload: {
                        progressParent.Activate();
                        rectTransform.localScale = Vector3.one;
                        animator.SetTrigger(animNames[Direction.Right]);
                        rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, Secretary.NormalizedTimer);
                    }
                    break;
                case SecretaryState.Unloading: {
                        rectTransform.localScale = Vector3.one;
                        progressParent.Activate();
                        animator.SetTrigger(animNames[Direction.StayRight]);
                        rectTransform.anchoredPosition = rightPosition;
                    }
                    break;
            }
        }
        public override void Update() {
            base.Update();
            if(isInitialized ) {
                switch(Secretary.State) {
                    case SecretaryState.MoveToLoad: {
                            rectTransform.anchoredPosition = Vector2.Lerp(rightPosition, leftPosition, Secretary.NormalizedTimer);
                        }
                        break;
                    case SecretaryState.Loading: {
                            progressFill.fillAmount = Secretary.NormalizedTimer;
                        }
                        break;
                    case SecretaryState.MoveToUnload: {
                            rectTransform.anchoredPosition = Vector2.Lerp(leftPosition, rightPosition, Secretary.NormalizedTimer);
                        }
                        break;
                    case SecretaryState.Unloading: {
                            progressFill.fillAmount = 1f - Secretary.NormalizedTimer;
                        }
                        break;
                }
            }
        }
    }


}