using System;

namespace Bos.UI {
    using Ozh.Tools.Functional;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

    public class ProfileView : TypedViewWithCloseButton
    {
		public Toggle officeToggle;
        public RectTransform officeToggleIcon;

		public Toggle transferToggle;
        public RectTransform transferToggleIcon;

		public Toggle goodsToggle;
        public RectTransform goodsToggleIcon;


		public GameObject playerView;
        public GameObject transferView;
        public GameObject productsView;

        public override ViewType Type => ViewType.ProfileView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

		public override int ViewDepth => 10;

        private Vector2Animator[] ToggleIconAnimators
            => new Vector2Animator[] { officeToggleIcon.gameObject.GetOrAdd<Vector2Animator>(),
                                        transferToggleIcon.gameObject.GetOrAdd<Vector2Animator>(),
                                        goodsToggleIcon.gameObject.GetOrAdd<Vector2Animator>() };


        private void AnimateToggleIcon(RectTransform target) {
            foreach(var anim in ToggleIconAnimators) {
                anim.Stop();
            }
            target.gameObject.GetOrAdd<Vector2Animator>().StartAnimation(AnimUtils
                .GetScaleAnimData(1, 1.15f, 1, EaseType.EaseInOutQuad, target)
                .AsAnimationMode(BosAnimationMode.PingPong));
        }

		public override void Setup(ViewData data){



			officeToggle.SetListener((isOn) => {
				if(isOn) {
					playerView.Activate();
					Services.SoundService.PlayOneShot(SoundName.click);
                    AnimateToggleIcon(officeToggleIcon);
				} else {
					playerView.Deactivate();
				}
			});

			transferToggle.SetListener(isOn => {
				if(isOn) {
                    transferView.Activate();
					Services.SoundService.PlayOneShot(SoundName.click);
                    AnimateToggleIcon(transferToggleIcon);
				} else {
                    transferView.Deactivate();
				}
			});

			goodsToggle.SetListener(isOn => {
				if(isOn) {
                    productsView.Activate();
                    productsView.GetComponent<ProductTab>().Setup(
                        Services.PlayerService.ProductNotifier.AvailableProduct.Bind(product => F.Some(product.Type))
                        );
					Services.SoundService.PlayOneShot(SoundName.click);
                    AnimateToggleIcon(goodsToggleIcon);
                    GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.StatusGoodsOpened));
				} else {
                    productsView.Deactivate();
				}
			});

			closeButton.SetListener(()=>{
				Services.ViewService.Remove(ViewType.ProfileView, BosUISettings.Instance.ViewCloseDelay);
				closeButton.interactable = false;
				Services.SoundService.PlayOneShot(SoundName.click);
			});

            AnimateToggleIcon(officeToggleIcon);

			if (data != null) {
				ProfileViewTab tab = (ProfileViewTab) data.UserData;
				switch (tab) {
					case ProfileViewTab.Office: {
                            ShowOffice();
					}
						break;
					case ProfileViewTab.MoneyTransfer: {
                            ShowTransfer(() => { });
					}
						break;
					case ProfileViewTab.StatusGoods: {
                            ShowStatusGoods();
					}
						break;
				}
			}
			else {
			    var tutSvc = Services.TutorialService;
			    if (tutSvc.IsStateActiveOnStage(TutorialStateName.ShowTransferAnsPersonalPurchasesState,
			        ShowTransferAndPersonalPurchasesState.FINGER_ON_PROFILE_SHOWED_STAGE)) {
                    /*
                    ShowTransfer(() => {
                        tutSvc.SetStage(TutorialStateName.ShowTransferAnsPersonalPurchasesState, ShowTransferAndPersonalPurchasesState.WAIT_FOR_LEGAL_TRANSFER);
                    });*/

                    //change - now we don't open transfer tab, but show finger on this tab to wait user click on it
                    tutSvc.SetStage(TutorialStateName.ShowTransferAnsPersonalPurchasesState, ShowTransferAndPersonalPurchasesState.SHOW_FINGER_ON_TRANSFER_TAB);

			    }
			}
			
			
		}

        private void ShowOffice() {
            playerView.Activate();
            transferView.Deactivate();
            productsView.Deactivate();
            officeToggle.isOn = true;
            AnimateToggleIcon(officeToggleIcon);
        }

        private void ShowTransfer(Action afterAction) {
            playerView.Deactivate();
            transferView.Activate();
            productsView.Deactivate();
            transferToggle.isOn = true;
            AnimateToggleIcon(transferToggleIcon);
            afterAction?.Invoke();
        }

        private void ShowStatusGoods() {
            playerView.Deactivate();
            transferView.Deactivate();
            productsView.Activate();
            productsView.GetComponent<ProductTab>().Setup(Services.PlayerService.ProductNotifier.AvailableProduct.Bind(product => F.Some(product.Type)));
            goodsToggle.isOn = true;
            AnimateToggleIcon(goodsToggleIcon);
        }
    }

	public enum ProfileViewTab {
		Office,
		MoneyTransfer,
		StatusGoods
	}
}

