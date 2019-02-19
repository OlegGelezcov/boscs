using UnityEngine.SceneManagement;

namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Bos.UI;
    using UnityEngine.UI;

    public class TransferView : GameBehaviour {

        public Text companyCashText1;
        public Text companyCashText2;
        public Text playerCashText1;
        public Text playerCashText2;
        public Button officialTransferButton;
        public Button nonOfficialTransferButton;

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        private void Setup() {
            UpdateCompanyCash();
            UpdatePlayerCash();

            officialTransferButton.SetListener(() => {
                var result = Services.PlayerService.StartTransferCashOfficially();
                Debug.Log(result.ToString());
                Services.GetService<ISoundService>().PlayOneShot(SoundName.buyGenerator);
                /*
                Services.ViewService.Show(ViewType.LoadingView, new ViewData() {
                    UserData = new LoadSceneData() {
                        BuildIndex = 7,
                        Mode = LoadSceneMode.Additive,
                        LoadAction = () => { FindObjectOfType<TransferGameController>()?.Setup(result); }
                    }
                });*/
            });

            nonOfficialTransferButton.SetListener(() => {

                var result = IllegalTransfer(); //Services.PlayerService.TransferIlegally(null);
                Debug.Log(result.ToString());
                if (result.IsSuccess) {
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.buyGenerator);
                } else {
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                }
            });

            updateTimer.Setup(0.25f, (deltaTime) => {
                bool isEnabled = (Services.PlayerService.CompanyCash.Value > 0);
                if(isEnabled && !officialTransferButton.interactable) {
                    UpdateButtons();
                } else if(!isEnabled && officialTransferButton.interactable ) {
                    UpdateButtons();
                }
            }, true);
            UpdateButtons();
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.TransferTabOpened));
        }

        private UnofficialTransferCashInfo IllegalTransfer() {
            var tutSvc = Services.TutorialService;
            if (tutSvc.IsStateActiveOnStage(TutorialStateName.ShowTransferAnsPersonalPurchasesState,
                ShowTransferAndPersonalPurchasesState.WAIT_FOR_FIRST_ILLEGAL_TRANSFER)) {
                return Player.TransferIlegally(true);
            } else if (tutSvc.IsStateActiveOnStage(TutorialStateName.ShowTransferAnsPersonalPurchasesState,
                ShowTransferAndPersonalPurchasesState.WAIT_FOR_SECOND_ILLEGAL_TRANSFER)) {
                return Player.TransferIlegally(false);
            }

            return Player.TransferIlegally(null);
        }

        private void UpdateButtons() {
            bool isEnabled = (Services.PlayerService.CompanyCash.Value > 0);
            if(isEnabled) {
                officialTransferButton.interactable = nonOfficialTransferButton.interactable = true;
                officialTransferButton.GetComponent<Image>().material.SetInt("_Enabled", 1);
                nonOfficialTransferButton.GetComponent<Image>().material.SetInt("_Enabled", 1);
            } else {
                officialTransferButton.interactable = nonOfficialTransferButton.interactable = false;
                officialTransferButton.GetComponent<Image>().material.SetInt("_Enabled", 0);
                nonOfficialTransferButton.GetComponent<Image>().material.SetInt("_Enabled", 0);
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            Setup();
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.PlayerCashChanged += OnPlayerCashChanged;
        }

        public override void OnDisable() {
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.PlayerCashChanged -= OnPlayerCashChanged;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

        private void OnPlayerCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount) {
            UpdatePlayerCash();
        }
        private void OnCompanyCashChanged(CurrencyNumber oldCount, CurrencyNumber newCount ) {
            UpdateCompanyCash();
            UpdatePlayerCash();
        }

        private void UpdateCompanyCash() {
            companyCashText1.text = companyCashText2.text = BosUtils.GetCurrencyString(Services.PlayerService.CompanyCash);
        }

        private void UpdatePlayerCash() {
            playerCashText1.text = BosUtils.GetCurrencyString(Services.PlayerService.CompanyCash);
            playerCashText2.text = BosUtils.GetCurrencyString(
                (Services.ResourceService.PersonalImprovements.ConvertData.OfficialConvertPercent *
                 Services.PlayerService.CompanyCash.Value).ToCurrencyNumber());
        }
    }

}