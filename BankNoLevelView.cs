namespace Bos.UI {
    using Bos.Data;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class BankNoLevelView : GameBehaviour {

        public Text countText;
        public Text timeText;
        public Text openButtonText;
        public Button openButton;
        public GameObject openParticles;


        public void Setup() {
            BankLevelData bankLevelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(1);
            

            if (bankLevelData.Profit.IsWhole()) {
                countText.text = ((int)bankLevelData.Profit).ToString();
                int hours = TimeSpan.FromSeconds(bankLevelData.ProfitInterval).Hours;
                UpdateTimeText(hours);
            } else {
                countText.text = ((int)(bankLevelData.Profit * 2)).ToString();
                int hours = TimeSpan.FromSeconds(bankLevelData.ProfitInterval * 2).Hours;
                UpdateTimeText(hours);
            }

            openButtonText.text = bankLevelData.LevelPriceCoins.ToString();

            openButton.SetListener(() => {
                if (IsAllowOpenBank(bankLevelData)) {
                    StartCoroutine(OpenEffectImpl());
                    Sounds.PlayOneShot(SoundName.buyCoins);
                } else {
                    ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                        UserData = bankLevelData.LevelPriceCoins,
                        ViewDepth = GetComponentInParent<BankView>().ViewDepth + 1
                    });
                    Sounds.PlayOneShot(SoundName.click);
                }
            });

            //UpdateOpenButtonState(bankLevelData);
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.CoinsChanged += OnCoinsChanged;
        }

        public override void OnDisable() {
            GameEvents.CoinsChanged -= OnCoinsChanged;
            base.OnDisable();
        }

        private void UpdateTimeText(int hours) {
            ILocalizationRepository localization = Services.ResourceService.Localization;
            if (hours == 1) {
                timeText.text = localization.GetString("fmt_hour_1");
            } else {
                timeText.text = string.Format(localization.GetString("fmt_hour_several"), hours);
            }
        }

        private bool IsAllowOpenBank(BankLevelData levelData)
            => Player.Coins >= levelData.LevelPriceCoins;

        //private void UpdateOpenButtonState(BankLevelData levelData) {
        //    openButton.interactable = (Services.PlayerService.Coins >= levelData.LevelPriceCoins);
        //}

        private void OnCoinsChanged(int oldCoins, int newCoins) {
            //var levelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(1);
            //UpdateOpenButtonState(levelData);
        }

        private System.Collections.IEnumerator OpenEffectImpl() {
            openButton.interactable = false;
            openParticles.Activate();
            Services.GetService<IBankService>().OpenBank();
            Services.GetService<ISoundService>().PlayOneShot(SoundName.Poof);
            yield return new WaitForSeconds(0.7f);
            openParticles.Deactivate();
            openButton.interactable = true;
        }
    }

}