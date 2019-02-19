namespace Bos.UI {
    using Bos.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class BankOpenedView : GameBehaviour {

        public Text timerFromLastCollectText;
        public Text timeToFollowingCoinText;
        public Text accumulatedCoinCount;
        public Button takeButton;
        public Text takeButtonText;

        public GameObject nextLevelControlsParent;
        public Text openBankNextLevelLabel;
        public Text nextLevelProfit;
        //public Text nextLevelProfitInterval;
        public Button openNextLevelButton;
        public Text openNextLevelPriceText;
        public GameObject levelParticles;
        public GameObject accumulatedCoinPrefab;
        public Text maxLevelText;

        private readonly UpdateTimer updateControlsTimer = new UpdateTimer();

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.BankLevelChanged += OnBankLevelChanged;
            GameEvents.BankAccumulatedCoinsChanged += OnAccumulatedViewsChanged;
            GameEvents.CoinsChanged += OnPlayerCoinsChanged;
        }

        public override void OnDisable() {
            GameEvents.BankLevelChanged -= OnBankLevelChanged;
            GameEvents.BankAccumulatedCoinsChanged -= OnAccumulatedViewsChanged;
            GameEvents.CoinsChanged -= OnPlayerCoinsChanged;
            base.OnDisable();
        }

        private void OnPlayerCoinsChanged(int oldCoins, int newCoins ) {
            UpdateViews();
            UpdateNextLevelControls();
        }

        private void OnAccumulatedViewsChanged(int oldCoins, int newCoins ) {
            UpdateViews();
        }

        private void OnBankLevelChanged(int oldLevel, int nextLevel) {
            Setup();
        }

        public void Setup() {
            IBankService bankService = Services.GetService<IBankService>();
            ILocalizationRepository localization = Services.ResourceService.Localization;

            updateControlsTimer.Setup(1.0f, (deltaTime) => UpdateViews(), invokeImmediatly: true);
            takeButton.SetListener(() => {
                //takeButton.interactable = false;
                //takeButton.SetInteractable
                //takeButton.image.material.SetFloat("_Enabled", 1);
                takeButton.SetInteractableWithShader(false);
                Services.RunCoroutine(AccumulateImpl(bankService.CoinsAccumulatedCount));
            });

            UpdateNextLevelControls();
        }

        private bool isNoCollectionProcess = true;

        public static event Action<bool> AccumulationStateChanged;


        private static void OnAccumulationStateChanged(bool value ) {
            AccumulationStateChanged?.Invoke(value);
        }

        private IEnumerator AccumulateImpl(int count) {
            OnAccumulationStateChanged(true);
            isNoCollectionProcess = false;
            int index = 0;
            IBankService bankService = Services.GetService<IBankService>();

            for(int i = 0; i < count; i++) {
                if(index < 6 ) {
                    GameObject obj = Instantiate<GameObject>(accumulatedCoinPrefab);
                    obj.transform.SetParent(transform, false);
                    obj.GetComponent<AccumulatedCoin>().StartMoving((coinObj) => {
                        bankService.CollectAccumulated(1);
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.buyCoinUpgrade);
                        Destroy(coinObj);
                        UpdateViews();
                    });
                    index++;
                    yield return new WaitForSeconds(0.15f);
                } else {

                    int cnt = count - index;
                    if (cnt > 0) {
                        bankService.CollectAccumulated(cnt);
                        break;
                    }
                    //yield return new WaitForSeconds(0.05f);         
                    //yield return new WaitForEndOfFrame();
                }
            }

            isNoCollectionProcess = true;
            UpdateViews();
            OnAccumulationStateChanged(false);
        }

        private void UpdateNextLevelControls() {
            IBankService bankService = Services.GetService<IBankService>();
            ILocalizationRepository localization = Services.ResourceService.Localization;

            int currentLevel = bankService.CurrentBankLevel;
            if (bankService.IsMaxLevel(currentLevel)) {
                nextLevelControlsParent.Deactivate();
                maxLevelText.Activate();

            } else {
                nextLevelControlsParent.Activate();
                maxLevelText.Deactivate();

                openBankNextLevelLabel.text = string.Format(localization.GetString("lbl_next_level_bank_desc"), bankService.NextLevel);
                BankLevelData bankLevelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(bankService.NextLevel);

                ProfitInterval profitInterval = AdjustProfitInterval(bankLevelData.Profit, (int)bankLevelData.ProfitInterval);
                string profitString = profitInterval.Profit.ToString().Size(72).Colored("#fde090");
                string intervalString = UpdateTimeText(profitInterval.Interval).Size(72).Colored("#00fdf7");
                nextLevelProfit.text = profitString + " " + intervalString;

                /*
                if (bankLevelData.Profit.IsWhole()) {
                    string profitString = ((int)bankLevelData.Profit).ToString().Size(80).Colored("#fde090");
                    int hours = TimeSpan.FromSeconds(bankLevelData.ProfitInterval).Hours;
                    string intervalString = UpdateTimeText(hours).Size(72).Colored("#00fdf7");
                    nextLevelProfit.text = profitString + " " + intervalString;
                } else {
                    string profitString = ((int)(bankLevelData.Profit * 2)).ToString().Size(80).Colored("#fde090");
                    int hours = TimeSpan.FromSeconds(bankLevelData.ProfitInterval * 2).Hours;
                    string intervalString = UpdateTimeText(hours).Size(72).Colored("#00fdf7");
                    nextLevelProfit.text = profitString + " " + intervalString;
                }*/

                openNextLevelPriceText.text = bankLevelData.LevelPriceCoins.ToString();
                openNextLevelButton.SetListener(() => {
                    if (IsAllowBuyNextLevel(bankLevelData)) {
                        StartCoroutine(OpenEffectImpl());
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.buyCoins);
                    } else {
                        ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                            UserData = bankLevelData.LevelPriceCoins,
                            ViewDepth = GetComponentInParent<BankView>().ViewDepth + 1
                        });
                        Sounds.PlayOneShot(SoundName.click);
                    }
                });
                openNextLevelButton.interactable = (!bankService.IsMaxLevel(bankService.CurrentBankLevel));
            }
        }

        public class ProfitInterval {
            public int Profit;
            public int Interval;

            public ProfitInterval AdjustedByGCD() {
                int a = Profit;
                int b = Interval;
                while(b != 0 ) {
                    int t = b;
                    b = a % b;
                    a = t;
                }

                return new ProfitInterval { Profit = Profit / a, Interval = Interval / a };
            }
        }

        private ProfitInterval AdjustProfitInterval(float profit, int interval ) {

            float workValue = profit;
            int workInterval = interval;
            for(int i = 0; i < 10; i++ ) {
                if(workValue.IsWhole()) {
                    break;
                }
                workValue *= 2;
                workInterval *= 2;
            }

            ProfitInterval result = new ProfitInterval { Profit = (int)workValue, Interval = TimeSpan.FromSeconds(workInterval).Hours };
            return result.AdjustedByGCD();
        }
        

        private bool IsAllowBuyNextLevel(BankLevelData bankLevelData)
            => Player.Coins >= bankLevelData.LevelPriceCoins;

        private System.Collections.IEnumerator OpenEffectImpl() {
            openNextLevelButton.interactable = false;
            levelParticles.Activate();
            //yield return new WaitForSeconds(0.05f);
            Services.GetService<IBankService>().OpenNextLevel();
            //levelParticles.Deactivate();
            StartCoroutine(DeactivateLevelParticlesImpl());
            openNextLevelButton.interactable = true;
            Setup();
            yield break;
        }

        private IEnumerator DeactivateLevelParticlesImpl() {
            yield return new WaitForSeconds(2);
            levelParticles.Deactivate();
        }

        private string UpdateTimeText(int hours) {
            ILocalizationRepository localization = Services.ResourceService.Localization;
            if (hours == 1) {
                //nextLevelProfitInterval.text = localization.GetString("fmt_hour_1");
                return localization.GetString("fmt_hour_1");
            } else {
                //nextLevelProfitInterval.text = string.Format(localization.GetString("fmt_hour_several"), hours);
                return string.Format(localization.GetString("fmt_hour_several"), hours);
            }
        }

        private void UpdateViews() {
            IBankService bankService = Services.GetService<IBankService>();
            ILocalizationRepository localization = Services.ResourceService.Localization;

            int coinsAccumulatedCount = bankService.CoinsAccumulatedCount;
            if (coinsAccumulatedCount > 0) {
                TimeSpan timeSpan = TimeSpan.FromSeconds(bankService.TimerFromLastCollect);
                int hours = timeSpan.Hours;
                if (hours == 0) { hours = 1; }

                timerFromLastCollectText.text = string.Format(localization.GetString("fmt_bank_time_from_last_collect"), hours);
                timeToFollowingCoinText.text = string.Empty;
                //takeButton.interactable = true;
                takeButton.SetInteractableWithShader(true && isNoCollectionProcess);
                Color takeButtonTextColor;
                if (ColorUtility.TryParseHtmlString("#fbef21", out takeButtonTextColor)) {
                    takeButtonText.color = takeButtonTextColor;
                }

            } else {
                timerFromLastCollectText.text = string.Empty;
                timeToFollowingCoinText.text = string.Format(localization.GetString("fmt_bank_next_coin"), 
                    BosUtils.FormatTimeWithColon(bankService.TimeToNextCoin).ToString().Colored("#fbef21"));
                //takeButton.interactable = false;
                takeButton.SetInteractableWithShader(false);
                Color takeButtonTextColor;
                if (ColorUtility.TryParseHtmlString("#b3b3b3", out takeButtonTextColor)) {
                    takeButtonText.color = takeButtonTextColor;
                }
            }

            accumulatedCoinCount.text = bankService.CoinsAccumulatedCount.ToString();
        }

        public override void Update() {
            base.Update();
            updateControlsTimer.Update();
        }
    }

}