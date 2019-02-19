namespace Bos.UI {
    using Bos.Services;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using System;
    using System.Collections;

    public class PromoInputView : TypedView {

        public InputField inputField;
        public Button getRewardButton;
        public Button closeButton;
        public Button errorCloseButton;

        public RectTransform inputParent;
        public RectTransform errorParent;
        public GameObject coinPrefab;



        public override ViewType Type => ViewType.PromoInputView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        private bool IsInitialized { get; set; }

        public override void Setup(ViewData data) {
            base.Setup(data);

            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(text => {
                if(string.IsNullOrWhiteSpace(text)) {
                    getRewardButton.interactable = false;
                } else {
                    getRewardButton.interactable = true;
                }
            });

            getRewardButton.SetListener(() => {
                Services.GetService<IPromoService>().RequestPromo(inputField.text.Trim());
                HideInput(() => { });
                Sounds.PlayOneShot(SoundName.click);
            });

            closeButton.SetListener(() => {
                Close();
                Sounds.PlayOneShot(SoundName.click);
            });

            errorCloseButton.SetListener(() => {
                CloseByErrorClose();
                Sounds.PlayOneShot(SoundName.click);
            });

            if(!IsInitialized) {
                GameEvents.PromoReceived.Subscribe(info => {
                    if(info.IsSuccess ) {
                        //create coins effect
                        SpawnCoins();
                    } else {
                        ShowError();
                    }
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        public override void Start() {
            base.Start();
            ShowInput();
        }

        private void Close() {
            HideInput(() => {
                ViewService.Remove(ViewType.PromoInputView);
            });
        }

        private void CloseByErrorClose() {
            errorParent.MoveFromTo(new Vector2(0, 140), new Vector2(0, 1600), 0.25f, EaseType.EaseInOutSin, () => {
                ViewService.Remove(ViewType.PromoInputView);
            });
        }

        private void ShowInput() {
            inputParent.MoveFromTo(new Vector2(0, 1600), new Vector2(0, 140), 0.25f, EaseType.EaseInOutSin, () => { });
        }

        private void HideInput(Action action) {
            inputParent.MoveFromTo(new Vector2(0, 140), new Vector2(0, 1600), 0.25f, EaseType.EaseInOutSin, () => {
                action?.Invoke();
            });
        }

        private void ShowError() {
            errorParent.MoveFromTo(new Vector2(0, 1600), new Vector2(0, 140), 0.25f, EaseType.EaseInOutSin, () => { });
        }

        private void SpawnCoins() {
            if (gameObject) {
                StartCoroutine(SpawnCoinsImpl());
            } else {
                UnityEngine.Debug.Log("GO not valid");
            }
        }

        private IEnumerator SpawnCoinsImpl() {
            for (int i = 0; i < 6; i++) {
                GameObject instance = Instantiate<GameObject>(coinPrefab);
                instance.GetComponent<RectTransform>().SetParent(transform, false);
                instance.GetComponent<AccumulatedCoin>().StartMoving((go) => {
                    Sounds.PlayOneShot(SoundName.buyCoinUpgrade);
                    Destroy(go);
                });
                yield return new WaitForSeconds(.15f);
                
            }
            yield return new WaitForSeconds(0.7f);
            ViewService.Remove(ViewType.PromoInputView);
        }

    }

}