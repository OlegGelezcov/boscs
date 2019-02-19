using UnityEngine.UI;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class NonOfficialTransferEffect : GameBehaviour {

        public RectTransform startObject;
        public RectTransform endObject;
        public float interval = 1;
        public float linerFactor = 0.25f;
        public float orthoFactor = 0.25f;
        public GameObject particles;
        //public bool isOfficial;
        public GameObject effectObjectPrefab;
        private readonly List<GameObject> animObjects = new List<GameObject>();

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.UnofficialTransfer += OnUnofficialTransfer;
        }

        public override void OnDisable() {
            GameEvents.UnofficialTransfer -= OnUnofficialTransfer;
            base.OnDisable();
            foreach(GameObject obj in animObjects) {
                if(obj && obj != null ) {
                    Destroy(obj);
                }
            }
            animObjects.Clear();
            particles.Deactivate();
        }

        private void OnUnofficialTransfer(UnofficialTransferCashInfo info) {
            if (info.IsSuccess) {
                StartCoroutine(EmitObjectImpl());
                StartCoroutine(ShowParticles());
                StartCoroutine(ShowMoveTextImpl(info));
            }
            else {
                StartCoroutine(EmitObjectImpl());
                StartCoroutine(ShowParticles());
                StartCoroutine(ShowMoveTextFailImpl(info));
            }
        }

        private IEnumerator ShowMoveTextFailImpl(UnofficialTransferCashInfo info) {
            yield return new WaitForSeconds(interval);
            
            GameObject textPrefab = Services.ResourceService.Prefabs.GetPrefab("movetext");
            GameObject textInstance = Instantiate(textPrefab);

            Text text = textInstance.GetComponent<Text>();
            text.text = $"-{BosUtils.GetCurrencyString(info.LooseValue.ToCurrencyNumber(), "", "")}";
            //text.SetAllDirty();
            
            RectTransform textTransform = textInstance.GetComponent<RectTransform>();
            textTransform.SetParent(transform, false);
            textTransform.anchoredPosition = endObject.anchoredPosition;
            textTransform.localScale = new Vector3(2, 2, 1);
            Color startColor = Color.red;
            Color endColor = Color.red.ChangeAlpha(0.1f);
            var colorData = AnimUtils.GetColorAnimData(startColor, endColor, 1.5f, EaseType.EaseOutQuintic,
                textTransform, BosAnimationMode.Single,
                () => { });
            var positionData = new Vector2AnimationData() {
                StartValue = textTransform.anchoredPosition,
                EndValue = textTransform.anchoredPosition - new Vector2(0, 300),
                Duration = 1.5f,
                EaseType = EaseType.EaseOutQuintic,
                Target = textInstance,
                OnStart = (p, o) => textTransform.anchoredPosition = p,
                OnUpdate = (p, t, o) => textTransform.anchoredPosition = p,
                OnEnd = (p, o) => {
                    textTransform.anchoredPosition = p;
                    Destroy(textInstance);
                }
            };
            ColorAnimator colorAnimator = textInstance.GetComponent<ColorAnimator>();
            Vector2Animator positionAnimator = textInstance.GetComponent<Vector2Animator>();
            colorAnimator.StartAnimation(colorData);
            positionAnimator.StartAnimation(positionData);
            Sounds.PlayOneShot(SoundName.slotFail);
            ViewService.ShowDelayed(ViewType.TransferWarningView, 0.5f, new ViewData {
                ViewDepth = ViewService.NextViewDepth,
                UserData = info.LooseValue
            });
            animObjects.Add(textInstance);
        }

        private IEnumerator ShowMoveTextImpl(UnofficialTransferCashInfo info) {
            yield return new WaitForSeconds(interval);
            
            GameObject textPrefab = Services.ResourceService.Prefabs.GetPrefab("movetext");
            GameObject textInstance = Instantiate(textPrefab);

            Text text = textInstance.GetComponent<Text>();
            text.text = $"+{BosUtils.GetCurrencyString(info.InputValue.ToCurrencyNumber(), "", "")}";
            //text.SetAllDirty();
            
            RectTransform textTransform = textInstance.GetComponent<RectTransform>();
            textTransform.SetParent(transform, false);
            textTransform.anchoredPosition = endObject.anchoredPosition;
            textTransform.localScale = new Vector3(2, 2, 1);
            Color startColor = text.color;
            Color endColor = text.color.ChangeAlpha(0.1f);
            var colorData = AnimUtils.GetColorAnimData(startColor, endColor, 1.5f, EaseType.EaseOutQuintic,
                textTransform, BosAnimationMode.Single,
                () => { });
            var positionData = new Vector2AnimationData() {
                StartValue = textTransform.anchoredPosition,
                EndValue = textTransform.anchoredPosition + new Vector2(0, 300),
                Duration = 1.5f,
                EaseType = EaseType.EaseOutQuintic,
                Target = textInstance,
                OnStart = (p, o) => textTransform.anchoredPosition = p,
                OnUpdate = (p, t, o) => textTransform.anchoredPosition = p,
                OnEnd = (p, o) => {
                    textTransform.anchoredPosition = p;
                    Destroy(textInstance);
                }
            };
            ColorAnimator colorAnimator = textInstance.GetComponent<ColorAnimator>();
            Vector2Animator positionAnimator = textInstance.GetComponent<Vector2Animator>();
            colorAnimator.StartAnimation(colorData);
            positionAnimator.StartAnimation(positionData);
            Sounds.PlayOneShot(SoundName.slotWin);
            animObjects.Add(textInstance);
        }
        
        private IEnumerator ShowParticles() {
            particles.Activate();
            particles.GetComponent<ParticleSystem>().Play();
            yield return new WaitForSeconds(interval * 1.1f);
            particles.Deactivate();
        }

        private IEnumerator EmitObjectImpl() {
            for (int i = 0; i < 4; i++) {
                yield return new WaitForSeconds(0.15f);
                GameObject inst = Instantiate(effectObjectPrefab);
                inst.transform.SetParent(transform, false);
                inst.GetComponent<RectTransform>().anchoredPosition = startObject.anchoredPosition;
                PrepareAnimationObject(inst);
                animObjects.Add(inst);
            }
        }

        private void PrepareAnimationObject(GameObject obj) {
            RectTransform trs = obj.GetComponent<RectTransform>();
            Image image = obj.GetComponent<Image>();
            
            Vector3Animator scaleAnimator = obj.GetComponent<Vector3Animator>();
            ColorAnimator colorAnimator = obj.GetComponent<ColorAnimator>();
            BezierMover mover = obj.GetComponent<BezierMover>();
            RectTransformPositionObject positionObject = obj.GetComponent<RectTransformPositionObject>();
            
            Vector3AnimationData scaleData = new Vector3AnimationData() {
                AnimationMode = BosAnimationMode.Single,
                Duration = interval * 0.5f,
                EaseType = EaseType.EaseInOutQuad,
                EndValue = Vector3.one * 1.25f,
                Events = null,
                StartValue = Vector3.one,
                Target = obj,
                OnStart = (s, o) => trs.localScale = s,
                OnUpdate = (s, t, o) => trs.localScale = s,
                OnEnd = (s, o) => {
                    trs.localScale = s;

                    Vector3AnimationData scale2Data = new Vector3AnimationData() {
                        StartValue = Vector3.one * 1.25f,
                        EndValue = Vector3.one,
                        Duration = interval * 0.5f,
                        EaseType = EaseType.EaseInOutQuad,
                        Target = obj,
                        OnStart = (s2, o2) => trs.localScale = s2,
                        OnUpdate = (s2, t2, o2) => trs.localScale = s2,
                        OnEnd = (s2, o2) => trs.localScale = s2
                    };
                    scaleAnimator.StartAnimation(scale2Data);

                    ColorAnimationData colorData = new ColorAnimationData() {
                        StartValue = Color.white,
                        EndValue = new Color(1, 1, 1, 0.2f),
                        Duration = interval * 0.5f,
                        EaseType = EaseType.EaseInOutQuad,
                        Target = obj,
                        OnStart = (c2, o2) => image.color = c2,
                        OnUpdate = (c2, t2, o2) => image.color = c2,
                        OnEnd = (c2, o2) => image.color = c2
                    };
                    
                    colorAnimator.StartAnimation(colorData);
                    
                    
                }
            };
            scaleAnimator.StartAnimation(scaleData);

            var bezierData = AnimUtils.GetBizerQuadData(positionObject,
                startObject.anchoredPosition,
                endObject.anchoredPosition,
                interval * 1.1f, (go) => Destroy(go),
                linerFactor,
                orthoFactor);
            mover.Setup(bezierData);
        }
        
    }

    

}