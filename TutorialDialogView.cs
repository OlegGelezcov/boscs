using UnityEngine.UI;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TutorialDialogView : TypedView {
        public Image back;
        public RectTransform girlTransform;
        public RectTransform bubbleTransform;
        public RectTransform textTransform;
        public RectTransform okButtonTransform;
        public Sprite greenSprite;
        public Sprite blueSprite;
        public Image buttonImage;
        public Text buttonText;
        //public ScrollRect textScroll;
        //public FloatAnimator textScrollAnimator;

        
        
        #region TypedView overrides
        public override CanvasType CanvasType => CanvasType.UI;
        public override bool IsModal => false;
        public override ViewType Type => ViewType.TutorialDialogView;
        public override int ViewDepth => 300;

        private int currentTextIndex = 0;

        private TutorialDialogData Data
            => (ViewData?.UserData as TutorialDialogData) ?? null;

        private int TextCount
            => Data.Texts.Count;

        private string GetText(int index)
            => Data.Texts[index];

        private bool HasNextText
            => currentTextIndex + 1 < TextCount;
        
        public override void Setup(ViewData data) {
            base.Setup(data);
            StartCoroutine(FadeInBack());
            UpdateButtonView();
            textTransform.GetComponent<Text>().text = GetText(currentTextIndex);
        }

        /*
        private void AnimateTextScroll() {
            textScroll.verticalNormalizedPosition = 1;
            textScrollAnimator.StartAnimation(new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = 1,
                EaseType = EaseType.Linear,
                Target = textScroll.gameObject,
                OnStart = (v, o) => textScroll.verticalNormalizedPosition = v,
                OnUpdate = (v, t, o) => textScroll.verticalNormalizedPosition = v,
                OnEnd = (v, o) => textScroll.verticalNormalizedPosition = v
            });
        }*/

        private IEnumerator FadeInBack() {
            yield return new WaitForEndOfFrame();
            ColorAnimator colorAnimator = back.gameObject.GetOrAdd<ColorAnimator>();
            var colorData = AnimUtils.GetColorAnimData(Color.black.ChangeAlpha(0), Color.black.ChangeAlpha(0.74f), .1f, EaseType.EaseInOutQuad, back.GetComponent<RectTransform>(), BosAnimationMode.Single, () => {
                StartCoroutine(ShowGirlImpl());
            });
            colorAnimator.StartAnimation(colorData);
        }

        private IEnumerator ShowGirlImpl() {
            yield return new WaitForSeconds(0.3f);
            Vector2Animator posAnimator = girlTransform.gameObject.GetOrAdd<Vector2Animator>();
            Vector2AnimationData posData = new Vector2AnimationData() {
                StartValue = new Vector2(-650, -110),
                EndValue = new Vector2(-360, -110),
                Duration = .2f,
                EaseType = EaseType.EaseInOutQuad,
                Target = girlTransform.gameObject,
                OnStart = girlTransform.UpdatePositionFunctor(),
                OnUpdate = girlTransform.UpdatePositionTimedFunctor(),
                OnEnd = girlTransform.UpdatePositionFunctor(() => { /*StartCoroutine(ShowBubbleImpl());*/
                    StartCoroutine(ShowOkButtonImpl());
                })

            };
            posAnimator.StartAnimation(posData);
        }

        /*
        private IEnumerator ShowBubbleImpl() {
            yield return new WaitForSeconds(0.2f);
            Vector3AnimationData bubbleData = bubbleTransform.ConstructScaleAnimationData(new Vector3(0, 0, 1),
                Vector3.one, 0.15f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                    StartCoroutine(ShowBubbleTextImpl());
                });
            bubbleTransform.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(bubbleData);
        }

        private IEnumerator ShowBubbleTextImpl() {
            yield return new WaitForSeconds(.1f);
            textTransform.GetComponent<Text>().text = GetText(currentTextIndex) ;
            Vector3AnimationData textData = textTransform.ConstructScaleAnimationData(new Vector3(1, 0, 1), Vector3.one,
                0.1f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                    StartCoroutine(ShowOkButtonImpl());
                    //AnimateTextScroll();
                });
            textTransform.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(textData);
        }*/

            
        private IEnumerator ShowOkButtonImpl() {
            yield return new WaitForSeconds(.2f);
            Vector3AnimationData buttonData = okButtonTransform.ConstructScaleAnimationData(new Vector3(0, 0, 1),
                Vector3.one, 0.1f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                    var button = okButtonTransform.GetComponent<Button>();
                    button.interactable = true;


                    button.SetListener(() => {
                        if (HasNextText) {
                            currentTextIndex++;
                            textTransform.GetComponent<Text>().text = GetText(currentTextIndex);
                            //AnimateTextScroll();
                            UpdateButtonView();
                        } else {
                            Services.ViewService.Remove(ViewType.TutorialDialogView);
                            Services.SoundService.PlayOneShot(SoundName.click);
                            Data?.OnOk?.Invoke();
                        }
                    });
                });
            okButtonTransform.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(buttonData);
            
        }

        private void UpdateButtonView() {
            if (HasNextText) {
                buttonImage.overrideSprite = blueSprite;
                buttonText.text = Services.ResourceService.Localization.GetString("btn_next");
                
            } else {
                buttonImage.overrideSprite = greenSprite;
                buttonText.text = Services.ResourceService.Localization.GetString("btn_ok");
            }
        }
        #endregion
    }

    public class TutorialDialogData {
        public List<string> Texts { get; set; }
        public System.Action OnOk { get; set; }
    }
}