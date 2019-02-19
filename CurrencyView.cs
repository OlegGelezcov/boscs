


namespace Bos.UI {
    using UnityEngine;
    using UnityEngine.UI;

    public class CurrencyView : GameBehaviour {

        public Text companyCashText;
        public Text playerCashText;
        public Text coinsText;
        public Text securitiesText;
        public bool isHandlerActives = true;


        public Button addCompanyCashButton;
        public Button addPlayerCashButton;
        public Button addCoinsButton;
        public Button addSecuritiesButton;

        public override void OnEnable() {
            base.OnEnable();
            
            GameEvents.PlayerCashChanged += OnPlayerCashChanged;
            GameEvents.SecuritiesChanged += OnSecuritiesChanged;
            GameEvents.CompanyCashChanged += OnCompanyCashChanged;
            GameEvents.CoinsChanged += OnCoinsChanged;

            if(addCompanyCashButton != null) {
                addCompanyCashButton.SetListener(() => {
                    if(isHandlerActives) {
                        Services.ViewService.Show(ViewType.UpgradesView, new ViewData {
                            UserData = new UpgradeViewData {
                                TabName = UpgradeTabName.Shop,
                                StoreSection = StoreItemSection.CompanyCash
                            },
                            ViewDepth = Services.ViewService.NextViewDepth
                        });
                        Services.SoundService.PlayOneShot(SoundName.click);
                    }
//#if DEBUG
//                    Services.PlayerService.AddCompanyCash(Services.PlayerService.CompanyCash.Value + 1);
//#else
//                    //invalid
//#endif

                });
            }

            if(addPlayerCashButton != null) {
                addPlayerCashButton.SetListener(() => {
                    if (isHandlerActives) {
                        Services.ViewService.Show(ViewType.UpgradesView, new ViewData {
                            UserData = new UpgradeViewData {
                                TabName = UpgradeTabName.Shop,
                                StoreSection = StoreItemSection.PlayerCash
                            },
                            ViewDepth = Services.ViewService.NextViewDepth
                        });
                        Services.SoundService.PlayOneShot(SoundName.click);
                    }

                    /*
                    Services.ViewService.Show(ViewType.ProfileView, new ViewData {
                        UserData = ProfileViewTab.MoneyTransfer,
                        ViewDepth = Services.ViewService.NextViewDepth
                    });
                    Services.SoundService.PlayOneShot(SoundName.click);*/

#if DEBUG
                    //Services.PlayerService.AddPlayerCash(new CurrencyNumber(Services.PlayerService.PlayerCash.Value + 1));
#else
//invalid
#endif
                });
            }

            if(addCoinsButton != null ) {
                addCoinsButton.SetListener(() => {
                    if (isHandlerActives) {
                        Services.ViewService.Show(ViewType.UpgradesView, new ViewData {
                            UserData = new UpgradeViewData {
                                TabName = UpgradeTabName.Shop,
                                StoreSection = StoreItemSection.Coins
                            },
                            ViewDepth = Services.ViewService.NextViewDepth
                        });
                        Services.SoundService.PlayOneShot(SoundName.click);
                    }
#if DEBUG
                    //Services.PlayerService.AddCoins(Services.PlayerService.Coins + 1);
#else
                    //invaliid
#endif
                });
            }

            if(addSecuritiesButton != null ) {
                addSecuritiesButton.SetListener(() => {
                    if (isHandlerActives) {
                        Services.ViewService.Show(ViewType.UpgradesView, new ViewData {
                            UserData = new UpgradeViewData {
                                TabName = UpgradeTabName.Shop,
                                StoreSection = StoreItemSection.Securities
                            },
                            ViewDepth = Services.ViewService.NextViewDepth
                        });
                        Services.SoundService.PlayOneShot(SoundName.click);
                    }
#if DEBUG
                    //Services.PlayerService.AddSecurities(new CurrencyNumber(Services.PlayerService.Securities.Value + 1));
#else
                    //invalid
#endif
                });

                if(!isHandlerActives ) {
                    addCompanyCashButton.interactable =
                    addPlayerCashButton.interactable =
                    addSecuritiesButton.interactable =
                    addCoinsButton.interactable = false;
                }
            }


            OnPlayerCashChanged(new CurrencyNumber(), Services.PlayerService.PlayerCash);
            OnSecuritiesChanged(new CurrencyNumber(), Services.PlayerService.Securities);
            OnCompanyCashChanged(new CurrencyNumber(), Services.PlayerService.CompanyCash);
            OnCoinsChanged(0, Services.PlayerService.Coins);
            
            
        }

        public override void OnDisable() {
            GameEvents.PlayerCashChanged -= OnPlayerCashChanged;
            GameEvents.SecuritiesChanged -= OnSecuritiesChanged;
            GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            base.OnDisable();
        }

        private string GetCurrencyString(CurrencyNumber num) {
            /*
            if(num != null) {
                string[] prettyArr = num.AbbreviationColoredComponents("", "#FFE565");
                string result = prettyArr[0];
                if (!string.IsNullOrEmpty(prettyArr[1])) {
                    result += " " + prettyArr[1];
                }
                return result;
            }
            return "0";*/
            return BosUtils.GetCurrencyString(num);
        }

        private CurrencyNumberText companyCashCurrencyNumberText;
        private CurrencyNumberText playerCashCurrencyNumberText;
        private CurrencyNumberText securitiesCurrencyNumberText;


        private CurrencyNumberText GetCompanyCashCurrencyNumberText() {
            if(companyCashCurrencyNumberText == null ) {
                if(companyCashText != null ) {
                    companyCashCurrencyNumberText = companyCashText.gameObject.GetOrAdd<CurrencyNumberText>().WithText(companyCashText);
                }
            }
            return companyCashCurrencyNumberText;
        }

        private CurrencyNumberText GetPlayerCashCurrencyNumberText() {
            if(playerCashCurrencyNumberText == null ) {
                if(playerCashText != null ) {
                    playerCashCurrencyNumberText = playerCashText.gameObject.GetOrAdd<CurrencyNumberText>().WithText(playerCashText);
                }
            }
            return playerCashCurrencyNumberText;
        }

        private CurrencyNumberText GetSecuritiesCurrencyNumberText() {
            if(securitiesCurrencyNumberText == null ) {
                if(securitiesText != null ) {
                    securitiesCurrencyNumberText = securitiesText.gameObject.GetOrAdd<CurrencyNumberText>().WithText(securitiesText);
                }
            }
            return securitiesCurrencyNumberText;
        }
        

        private void OnCoinsChanged(int oldValue, int newValue) {
            if(coinsText != null ) {
                coinsText.text = newValue.ToString();
            }
        }

        private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) {
            if (companyCashText != null) {
                //companyCashText.text = GetCurrencyString(newValue);
                GetCompanyCashCurrencyNumberText()?.SetValue(newValue.Value);
            }
        }

        private void OnPlayerCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue ) {
            if(playerCashText != null ) {
                //playerCashText.text = GetCurrencyString(newValue);
                GetPlayerCashCurrencyNumberText()?.SetValue(newValue.Value);
            }
        }

        private void OnSecuritiesChanged(CurrencyNumber oldValue, CurrencyNumber newValue ) {
            if(securitiesText != null ) {
                //securitiesText.text = GetCurrencyString(newValue);
                GetSecuritiesCurrencyNumberText()?.SetValue(newValue.Value);
            }
        }


        private void ShowCurrency(ViewType type)
        {

            /*if (CanvasGroup == null) return;
            
            var currentView = Services.ViewService.GetCurrentViewType();
            if (currentView == ViewType.BankView || currentView == ViewType.InvestorsView || currentView == ViewType.MainView || currentView == ViewType.None)
                CanvasGroup.alpha = 1;
            else
                CanvasGroup.alpha = 0;*/
        }
    }

}