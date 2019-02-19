namespace Bos.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using Ozh.Tools.Functional;
    using System.Linq;

    public class SpecialOfferView : TypedViewWithCloseButton {

        public Text playerCashRewardCountText;
        public Text playerCashMaxBalanceText;
        public Text coinsRewardText;
        public Text companyCashRewardText;
        public Text companyCashMaxBalanceText;
        public Text expireText;
        public Button buyButton;
        public Text priceText;
        public TMPro.TextMeshProUGUI welcomText;
        public SwitchContent[] skins;
        public Animator[] plusAnimators;
        public RectTransform[] playerCashEffectPoints;
        public RectTransform[] coinsEffectPoints;
        public RectTransform[] companyCashEffectPoints;
        public GameObject playerCashEffect;
        public GameObject coinsEffect;
        public GameObject companyCashEffect;
        public GameObject[] particles;

        private bool[] rewardCompletes = { false, false, false };


        public override ViewType Type => ViewType.SpecialOfferView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 20;

        public override void Setup(ViewData data) {
            base.Setup(data);

            ISpecialOfferService specialOfferService = Services.GetService<ISpecialOfferService>();
            playerCashRewardCountText.text = "$" + BosUtils.GetCurrencyString(specialOfferService.PlayerCashReward.ToCurrencyNumber(), string.Empty, string.Empty);
            playerCashMaxBalanceText.text = MaxBalanceFormattedString(specialOfferService.MaxBalanceBonus);
            coinsRewardText.text = string.Format(LocalizationObj.GetString("fmt_coins_2"), specialOfferService.CoinsReward);
            companyCashRewardText.text = "$" + BosUtils.GetCurrencyString(specialOfferService.ComplanyCashReward.ToCurrencyNumber(), string.Empty, string.Empty);
            companyCashMaxBalanceText.text = MaxBalanceFormattedString(specialOfferService.MaxBalanceBonus);
            UpdateAvailable();
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                UpdateAvailable();
                UpdateParticles();
            }).AddTo(gameObject);



            buyButton.SetListener(() => {
                buyButton.SetInteractableWithShader(false);
                specialOfferService.BuyOffer();
            });

            closeButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                ViewService.Remove(ViewType.SpecialOfferView, BosUISettings.Instance.ViewCloseDelay);
            });

            var planetNameData = ResourceService.PlanetNameRepository.GetPlanetNameData(Planets.CurrentPlanetId.Id);
            string planetName = LocalizationObj.GetString(planetNameData.name);
            welcomText.text = string.Format(LocalizationObj.GetString("fmt_welcome"), planetName);
            SetPlanetSkin();
            StartCoroutine(PlusAnimatorsImpl());

            GameEvents.StoreProductPurchasedObservable.Subscribe(prod => {
                if(prod.Id == SpecialOfferService.kOfferProductId) {
                    closeButton.interactable = false;
                    //start emitting
                    EmitEffect(playerCashEffect, 5, playerCashEffectPoints[0], playerCashEffectPoints[1], "playercash", 0);
                    EmitEffect(companyCashEffect, 5, companyCashEffectPoints[0], companyCashEffectPoints[1], "companycash", 1);
                    EmitEffect(coinsEffect, 5, coinsEffectPoints[0], coinsEffectPoints[1], "coins", 2);
                }
            }).AddTo(gameObject);
            UpdateParticles();

            try {
                //IAPManager iapManager = FindObjectOfType<IAPManager>();
                Services.Inap.GetProductByResourceId(SpecialOfferService.kOfferProductId).Match(() => {
                    priceText.text = string.Empty;
                    return F.None;
                }, (prod) => {
                    priceText.text = prod.metadata.localizedPriceString;
                    return F.Some(prod);
                });
            } catch(Exception exception ) {
                Debug.Log(exception.Message);
                Debug.Log(exception.StackTrace);
            }
        }

        private IEnumerator PlusAnimatorsImpl() {
            while(true) {
                yield return new WaitForSeconds(2);
                plusAnimators[0].SetTrigger("scale");
                yield return new WaitForSeconds(0.1f);
                plusAnimators[1].SetTrigger("scale");
                yield return new WaitForSeconds(0.1f);
                plusAnimators[2].SetTrigger("scale");
            }
        }

        private void UpdateAvailable() {
            ISpecialOfferService specialOfferService = Services.GetService<ISpecialOfferService>();
            if(specialOfferService.IsExpired || specialOfferService.IsCompleted) {
                expireText.Deactivate();
                buyButton.SetInteractableWithShader(false);
            } else {
                expireText.Activate();
                TimeSpan timeSpan = TimeSpan.FromSeconds(specialOfferService.ExpireInterval);
                string timeString = timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");
                expireText.text = string.Format(LocalizationObj.GetString("fmt_offer_expire"), timeString).ToUpper();
                buyButton.SetInteractableWithShader(true);
            }
        }

        private void UpdateParticles() {
            int totalCount = ViewService.ModalCount + ViewService.LegacyCount;
            if(totalCount > 1) {
                particles.Deactivate();
            } else {
                particles.Activate();
            }
        }

        private string MaxBalanceFormattedString(int count) {
            return "x" + count.ToString().Size(28).Colored("#ff9712") + " " + LocalizationObj.GetString("lbl_max_balance");
        }

        private void SetPlanetSkin() {
            int planetId = Planets.CurrentPlanetId.Id;
            foreach(var skin in skins ) {
                if(skin.key == planetId) {
                    skin.content.Activate();
                } else {
                    skin.content.Deactivate();
                }
            } 
        }

        private void CompleteRewardIndex(int index) {
            rewardCompletes[index] = true;
            if(rewardCompletes.All(v => v == true)) {
                closeButton.interactable = true;
                StartCoroutine(CloseAfterEffectImpl());
            }
        }

        private IEnumerator CloseAfterEffectImpl() {
            yield return new WaitForSeconds(0.3f);
            ViewService.Remove(ViewType.SpecialOfferView);
        }

        private void EmitEffect(GameObject prefab, int count, 
            RectTransform startPoint, RectTransform endPoint,  string rewardType, int rewardIndex) {
            StartCoroutine(EmitEffectImpl(prefab, count, startPoint, endPoint, rewardType, rewardIndex));
        }

        private IEnumerator EmitEffectImpl(GameObject prefab, int count, RectTransform startPoint, RectTransform endPoint, string rewardType, int rewardIndex) {

            float interval = 1;
            float controlPointLength = 400;

            for(int i = 0; i < count; i++ ) {
                yield return new WaitForSeconds(0.06f);
                GameObject instance = Instantiate<GameObject>(prefab);
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                rectTransform.SetParent(transform, false);
                rectTransform.anchoredPosition = startPoint.anchoredPosition;

                BezierMover mover = instance.GetComponent<BezierMover>();
                BezierData bezierData = null;
                Vector2 diff = endPoint.anchoredPosition - startPoint.anchoredPosition;
                if (rewardType == "playercash") {

                    bezierData = new BezierData {
                        Interval = interval,
                        Type = BezierType.Quadratic,
                        Target = instance.GetComponent<RectTransformPositionObject>(),
                        Points = new Vector3[] {
                                startPoint.anchoredPosition,
                                diff * 0.5f + diff.normalized.Orthogonal() * UnityEngine.Random.Range(-controlPointLength, 0),
                                endPoint.anchoredPosition
                            },
                        OnComplete = g => Destroy(g)
                    };
                } else if(rewardType == "companycash") {
                    bezierData = new BezierData {
                        Interval = interval,
                        Type = BezierType.Quadratic,
                        Target = instance.GetComponent<RectTransformPositionObject>(),
                        Points = new Vector3[] {
                                startPoint.anchoredPosition,
                                diff * 0.5f + diff.normalized.Orthogonal() * UnityEngine.Random.Range(0, controlPointLength),
                                endPoint.anchoredPosition
                            },
                        OnComplete = g => Destroy(g)
                    };
                } else {
                    bezierData = new BezierData {
                        Interval = interval,
                        Type = BezierType.Quadratic,
                        Target = instance.GetComponent<RectTransformPositionObject>(),
                        Points = new Vector3[] {
                                startPoint.anchoredPosition,
                                diff * 0.5f + diff.normalized.Orthogonal() * UnityEngine.Random.Range(-controlPointLength * 0.5f, controlPointLength * 0.5f),
                                endPoint.anchoredPosition
                            },
                        OnComplete = g => Destroy(g)
                    };
                }

                mover.Setup(bezierData);

                ColorAnimator colorAnimator = instance.GetComponent<ColorAnimator>();
                colorAnimator.StartAnimation(AnimUtils.GetColorAnimData(new Color(1, 1, 1, 1f), Color.white, 0.3f,
                    EaseType.EaseInOutQuad, rectTransform, BosAnimationMode.Single, () => {
                        colorAnimator.StartAnimation(AnimUtils.GetColorAnimData(Color.white, new Color(1, 1, 1, 1f), 0.4f, 
                            EaseType.EaseInOutQuad, rectTransform,
                            BosAnimationMode.Single, () => { }));
                    }));

                Vector3Animator scaleAnimator = instance.GetComponent<Vector3Animator>();
                scaleAnimator.StartAnimation(new Vector3AnimationData {
                    AnimationMode = BosAnimationMode.PingPong,
                    Duration = 0.2f,
                    EaseType = EaseType.EaseInOutQuad,
                    StartValue = new Vector3(1, 1, 1),
                    EndValue = new Vector3(2f, 2f, 1),
                    Target = instance,
                    OnStart = rectTransform.UpdateScaleFunctor(),
                    OnUpdate = rectTransform.UpdateScaleTimedFunctor(),
                    OnEnd = rectTransform.UpdateScaleFunctor()
                });

                FloatAnimator rotationAnimator = instance.GetComponent<FloatAnimator>();
                rotationAnimator.StartAnimation(new FloatAnimationData {
                    Duration = interval,
                    AnimationMode = BosAnimationMode.Single,
                    EaseType = EaseType.Linear,
                    StartValue = 0,
                    EndValue = 720,
                    Target = instance,
                    OnStart = rectTransform.UpdateZRotation(),
                    OnUpdate = rectTransform.UpdateZRotationTimed(),
                    OnEnd = rectTransform.UpdateZRotation()
                });
            }

            yield return new WaitForSeconds(interval - 0.1f);
            CompleteRewardIndex(rewardIndex);
        }
    }

    [Serializable]
    public class SwitchContent {
        public int key;
        public GameObject content;
    }

}