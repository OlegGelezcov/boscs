namespace Bos.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TutorialFinger : GameBehaviour {
        public RectTransform pointTransform;
        public RectTransform fingerTransform;
        public TutorialWaveController waveController;
        public LayoutElement textLayout;
        public RectTransform tooltipParent;
        public Text tooltipText;

        public TutorialFingerData Data { get; private set; }

        public void Setup(TutorialFingerData data) {
            Data = data;
            GetComponent<RectTransform>().anchoredPosition = data.Position;

            if(data.IsTooltipVisible) {
                tooltipParent.gameObject.Activate();
                tooltipText.text = data.TooltipText;
                SetupTooltipLayout(data);
                var colorData = AnimUtils.GetColorAnimData(tooltipText.color.ChangeAlpha(0), tooltipText.color, 0.3f, EaseType.EaseInOutQuad, tooltipText.GetComponent<RectTransform>(), BosAnimationMode.Single);
                tooltipText.gameObject.GetOrAdd<ColorAnimator>().StartAnimation(colorData);
            } else {
                tooltipParent.gameObject.Deactivate();
            }

            if(Data.Timeout > 0f ) {
                StartCoroutine(RemoveAfterTimeout(Data.Timeout));
            }
        }

        private IEnumerator RemoveAfterTimeout(float timeout) {
            yield return new WaitForSeconds(timeout);
            Data.TimeoutAction?.Invoke();
            Services.TutorialService.RemoveFinger(Data.Id);
        }

        public override void OnEnable() {
            base.OnEnable();
            StartCoroutine(PlayAnimationProcImpl());
        }

        private IEnumerator PlayAnimationProcImpl() {
            while (true) {
                PlayAnimation();
                yield return new WaitForSeconds(2.2f);
            }
        }

        private void PlayAnimation() {
            Vector3Animator fingerAnimator = fingerTransform.gameObject.GetOrAdd<Vector3Animator>();
            var fingerSecondData = fingerTransform.ConstructScaleAnimationData(0.7f * Vector3.one, Vector3.one, 1,
                BosAnimationMode.Single,
                EaseType.EaseInOutQuad, () => { });
            
            var fingerFirstData = fingerTransform.ConstructScaleAnimationData(Vector3.one, 0.7f * Vector3.one, 1,
                BosAnimationMode.Single,
                EaseType.EaseInOutQuad, () => {
                    fingerAnimator.StartAnimation(fingerSecondData);
                });
            fingerAnimator.StartAnimation(fingerFirstData);

            Vector3Animator pointAnimator = pointTransform.gameObject.GetOrAdd<Vector3Animator>();
            var pointSecondData = pointTransform.ConstructScaleAnimationData(1.5f * Vector3.one, Vector3.one, 1,
                BosAnimationMode.Single,
                EaseType.EaseInOutQuad, () => { });
            
            var pointFirstData = pointTransform.ConstructScaleAnimationData(Vector3.one, 1.5f * Vector3.one, 1,
                BosAnimationMode.Single,
                EaseType.EaseInOutQuad, () => {
                    pointAnimator.StartAnimation(pointSecondData);
                    
                });
            pointAnimator.StartAnimation(pointFirstData);
            StartCoroutine(StartWaves());
        }

        private IEnumerator StartWaves() {
            for (int i = 0; i < 5; i++) {   
                waveController.StartWave();
                yield return new WaitForSeconds(0.3f);
            }
        }

        private void SetupTooltipLayout(TutorialFingerData data) {
            textLayout.preferredWidth = data.TooltipWidth;
            tooltipParent.anchoredPosition = data.TooltipPosition;
            tooltipParent.localScale = data.TooltipScale;
        }

    }

    public class TutorialFingerData {
        public Vector2 Position { get; set; }
        public string Id { get; set; }
        public string TooltipText { get; set; }
        public bool IsTooltipVisible { get; set; }
        public float TooltipWidth { get; set; } = 500;
        public Vector2 TooltipPosition { get; set; }
        public Vector3 TooltipScale { get; set; } = Vector3.one;
        public float Timeout { get;  set; } = 7;
        public Action TimeoutAction { get; set; }
    }

}