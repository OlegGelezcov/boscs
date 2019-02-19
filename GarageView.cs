namespace Bos.UI {
    using Bos.Data;
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class GarageView : GameBehaviour {
        public Text brokenTransportCountText;
        public Text incomeText;
        public Button buyMechanicButton;
        public Text mechanicPriceText;
        public Text mechanicCountText;
        //public Text countOfBrokenedUnitsText;
        public Text speedUpRepairx2Text;
        public Text speedUpButtonText;
        public TempMechanicView tempMechanicView;
        public GameObject mechanicObjectPrefab;
        public RectTransform animParent;




        private ManagerInfo manager;
        private GeneratorInfo generator;
        private string dollarsString = null;
        private int currentSelectedLeaseManagerCount = 0;
        private IMechanicService mechService = null;
        private GameObject mechanicView = null;

        private IMechanicService MechanicService
            => (mechService != null) ? mechService :
            (mechService = Services.GetService<IMechanicService>());

        private GeneratorInfo Generator {
            get {
                if(manager == null ) {
                    return null;
                }
                if(generator == null ) {
                    generator = Services.GenerationService.GetGetenerator(manager.Id);
                }
                return generator;
            }
        }

        private string DollarsString
            => (dollarsString != null) ? dollarsString
            : (dollarsString = Services.ResourceService.Localization.GetString("DOLLARS"));


        public void Setup(int managerId) {
            IMechanicService mechanicService = Services.GetService<IMechanicService>();
            ILocalizationRepository localization = Services.ResourceService.Localization;     
            manager = Services.GetService<IManagerService>().GetManager(managerId);
            tempMechanicView.Setup(Services.GenerationService.GetGetenerator(managerId));
            UpdateBrokenedAndIncomeTexts();

            buyMechanicButton.SetListener(() => {
                BosError error = BosError.Ok;
                if(mechanicService.IsAllowBuyMechanic(manager.Id, out error)) {
                    BosError status = mechanicService.BuyMechanic(manager.Id);
                    if(status == BosError.Ok) {
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.buyCoins);
                    } else {
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.slotFail);
                    }
                }  else if(error == BosError.NoEnoughCoins ) {
                    ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                        UserData = mechanicService.GetNextMechanicPrice(manager.Id),
                        ViewDepth = ViewService.NextViewDepth
                    });
                    Sounds.PlayOneShot(SoundName.click);
                } else {
                    Debug.LogError("some error");
                }
            });
            UpdateBuyButtonState();
            UpdateMechanicPriceText();
            UpdateMechanicCountText();


            speedUpRepairx2Text.text = string.Format(
                localization.GetString("fmt_speed_up_x2"),
                "x".Colored("#FDEE21").Size(24), 
                "2".Colored("#F9F7BC").Size(36));

            CreateConstMechanicAnimObject();
        }

        private void CreateConstMechanicAnimObject() {
            if(manager != null ) {
                
                if(mechanicView == null ) {
                    int mechanicCount = Services.MechanicService.GetMechanicCount(manager.Id);
                    if (mechanicCount > 0) {
                        mechanicView = Instantiate<GameObject>(mechanicObjectPrefab);
                        mechanicView.GetComponent<RectTransform>().SetParent(animParent, false);
                        mechanicView.GetComponent<ConstMechanicAnimObject>().Setup(Services.MechanicService.GetMechanic(manager.Id));
                    }
                }
            }
        }

        private void DestroyMechanicView() {
            if(mechanicView != null && mechanicView) {
                Destroy(mechanicView);
                mechanicView = null;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.MechanicAdded += OnMechanicAdded;
            GameEvents.CoinsChanged += OnCoinsChanged;
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;
        }

        public override void OnDisable() {
            GameEvents.MechanicAdded -= OnMechanicAdded;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            DestroyMechanicView();
            base.OnDisable();
        }

        private void OnMechanicAdded(MechanicInfo mechanic) {
            if (manager != null) {
                if (mechanic.Id == manager.Id) {
                    UpdateMechanicPriceText();
                    UpdateMechanicCountText();

                    CreateConstMechanicAnimObject();
                }
            }
        }

        private void OnCoinsChanged(int oldCount, int newCount ) {
            if (manager != null) {
                UpdateBuyButtonState();
            } else {
                Debug.LogError($"manager is null");
            }
        }


        private void OnUnitCountChanged(TransportUnitInfo unit) {
            if (manager != null) {
                if (unit.GeneratorId == manager.Id) {
                    UpdateBrokenedAndIncomeTexts();
                }
            }
        }

        private string FormatProfit(CurrencyNumber number) {
            string[] components = number.AbbreviationComponents();
            if(!string.IsNullOrEmpty(components[1])) {
                return $"${components[0]} {components[1]}/SEC";
            } else {
                return $"${components[0]} {DollarsString}/SEC";
            }
        }

        private void UpdateBrokenedCountText(int brokenedCount) {
            if (brokenedCount == 0) {
                brokenTransportCountText.text = Services.ResourceService.Localization.GetString("all_transport_repaired");
            } else {
                brokenTransportCountText.text = string.Format(Services.ResourceService.Localization.GetString("fmt_need_repair_count"),
                    brokenedCount.ToString().Colored(ConsoleTextColor.red));
            }
        }

        private void UpdateIncomeText(int brokenedCount) {
            if (manager != null) {
                ProfitResult profitResult = Generator.ConstructProfitResult(Services.GenerationService.Generators); //Services.GenerationService.CalculateProfitPerSecond(Generator,
                    //Services.TransportService.GetUnitLiveCount(manager.Id));
                CurrencyNumber number = profitResult.ValuePerSecond.ToCurrencyNumber();
                if (brokenedCount == 0) {
                    incomeText.text = string.Format(Services.ResourceService.Localization.GetString("fmt_your_profit_normal"),
                        FormatProfit(number).Colored("#F9F7BC"));
                } else {
                    incomeText.text = string.Format(Services.ResourceService.Localization.GetString("fmt_income_drop"),
                        FormatProfit(number).Colored("#F9F7BC"));
                }
            }
        }

        private void UpdateMechanicPriceText() {
            if (manager != null) {
                mechanicPriceText.text = Services.GetService<IMechanicService>().GetNextMechanicPrice(manager.Id).ToString();
            }
        }

        private void UpdateMechanicCountText() {
            if (manager != null) {
                mechanicCountText.text = Services.GetService<IMechanicService>().GetMechanicCount(manager.Id).ToString();
            }
        }

        private void UpdateBuyButtonState() {
            //buy button always enabled, but when don't enough coins show appropriate view
            if (manager != null) {
                BosError status;
                Services.MechanicService.IsAllowBuyMechanic(manager.Id, out status);
                if (status == BosError.Ok || status == BosError.NoEnoughCoins) {
                    buyMechanicButton.interactable = true;
                } else {
                    buyMechanicButton.interactable = false; //= Services.GetService<IMechanicService>().IsAllowBuyMechanic(manager.Id);
                }
            } else {
                Debug.Log("manager is null");
            }
        }

        private void UpdateBrokenedAndIncomeTexts() {
            if (manager != null) {
                int brokenedCount = Services.TransportService.GetUnitBrokenedCount(manager.Id);
                UpdateBrokenedCountText(brokenedCount);
                UpdateIncomeText(brokenedCount);
            }
        }

    }

}