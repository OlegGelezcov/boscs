namespace Bos.UI {
    using Bos.SplitLiner;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SplitFooterView : GameBehaviour {

        //public RectTransform cashTransform;
        //public RectTransform coinTransform;
        public Text cashText;
        public Text coinsText;

        public Button baseButton;

        private bool isHided = true;
        private bool isAnimating = false;

        public override void OnEnable() {
            base.OnEnable();
            SplitLinerUI.CashPercentChanged += OnCashPercentChanged;
            SplitLinerUI.CoinsCountChanged += OnCoinChanged;
            //ToggleVisibility(false);
            baseButton.SetListener(() => {
                FindObjectOfType<PlayerMotor>()?.GoOnBase();
                baseButton.interactable = false;
                ToggleVisibility(false);
            });
        }

        public override void OnDisable() {
            SplitLinerUI.CashPercentChanged -= OnCashPercentChanged;
            SplitLinerUI.CoinsCountChanged -= OnCoinChanged;
            base.OnDisable();
        }

        private void OnCashPercentChanged(int count) {
            if(count > 0 ) {
                cashText.text = $"+{count}%";
            } else {
                cashText.text = $"0%";
            }
            AnimateCashText();
        }

        private void OnCoinChanged(int count) {
            if(count > 0 ) {
                coinsText.text = $"+{count}";
            } else {
                coinsText.text = "0";
            }
            AnimateCoinText();
        }

        private void AnimateCoinText() {
            RectTransform trs = coinsText.GetComponent<RectTransform>();
            Vector3Animator animator = coinsText.GetComponent<Vector3Animator>();

            var thirdScale = trs.ConstructScaleAnimationData(new Vector3(1.5f, 1.5f, 1), Vector3.one, 0.15f, BosAnimationMode.Single,
                EaseType.EaseInOutQuad, () => { });
            var secondScale = trs.ConstructScaleAnimationData(new Vector3(1.5f, 1, 1), new Vector3(1.5f, 1.5f, 1), 0.2f, BosAnimationMode.Single,
                EaseType.EaseInOutQuad, () => animator.StartAnimation(thirdScale));
            var firstScale = trs.ConstructScaleAnimationData(Vector3.one, new Vector3(1.5f, 1, 1), 0.2f, BosAnimationMode.Single,
                 EaseType.EaseInOutQuad, () => { animator.StartAnimation(secondScale); });
            animator.StartAnimation(firstScale);
        }

        private void AnimateCashText() {
            RectTransform trs = cashText.GetComponent<RectTransform>();
            Vector3Animator animator = cashText.GetComponent<Vector3Animator>();

            var thirdScale = trs.ConstructScaleAnimationData(new Vector3(1.5f, 1.5f, 1), Vector3.one, 0.15f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => { });
            var secondScale = trs.ConstructScaleAnimationData(new Vector3(1, 1.5f, 1), new Vector3(1.5f, 1.5f, 1), 0.2f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                animator.StartAnimation(thirdScale);
            });
            var firstScale = trs.ConstructScaleAnimationData(Vector3.one, new Vector3(1, 1.5f, 1), 0.2f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                animator.StartAnimation(secondScale);
            });
            animator.StartAnimation(firstScale);
        }

        public void ToggleVisibility(bool isVisible ) {
            StartCoroutine(ToggleVisibilityImpl(isVisible));
        }

        private IEnumerator ToggleVisibilityImpl(bool isVisible) {
            yield return new WaitUntil(() => !isAnimating);
            if(isVisible) {
                Unhide();
            } else {
                Hide();
            }
        }

        private void Hide() {
            if(!isHided) {
                isAnimating = true;
                isHided = true;
                RectTransform trs = GetComponent<RectTransform>();
                Vector2Animator animator = GetComponent<Vector2Animator>();
                animator.StartAnimation(new Vector2AnimationData {
                    StartValue = new Vector2(0, 92.5f),
                    EndValue = new Vector2(0, -110),
                    Duration = .3f,
                    AnimationMode = BosAnimationMode.Single,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = gameObject,
                    OnStart = trs.UpdatePositionFunctor(),
                    OnUpdate = trs.UpdatePositionTimedFunctor(),
                    OnEnd = trs.UpdatePositionFunctor(() => {
                        isAnimating = false;
                    })
                });
            }
        }

        private void Unhide() {
            if(isHided) {
                isAnimating = true;
                isHided = false;
                RectTransform trs = GetComponent<RectTransform>();
                Vector2Animator animator = GetComponent<Vector2Animator>();
                animator.StartAnimation(new Vector2AnimationData {
                    StartValue = new Vector2(0, -110),
                    EndValue = new Vector2(0, 92.5f),
                    Duration = 0.3f,
                    AnimationMode = BosAnimationMode.Single,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = gameObject,
                    OnStart = trs.UpdatePositionFunctor(),
                    OnUpdate = trs.UpdatePositionTimedFunctor(),
                    OnEnd = trs.UpdatePositionFunctor(() => {
                        isAnimating = false;
                    })
                });
            }
        }
    }



}