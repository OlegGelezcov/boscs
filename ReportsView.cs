namespace Bos.UI {
    using Bos.Data;
    using Bos.Debug;
    using UnityEngine;
    using UnityEngine.UI;

    public class ReportsView : GameBehaviour {

        public Text reportCountText;
        public Text managerEfficiencyText;
        public Text secretaryCountText;
        public Button buySecretaryButton;
        public Text secretaryPriceText;
        public BuyAuditorButtonView auditorView;

        public GameObject secretaryObjectPrefab;
        public RectTransform animParent;
        
        private ManagerInfo manager = null;
        private GeneratorInfo generator = null;

        private ISecretaryService secService = null;
        private ISecretaryService SecretaryService
            => (secService != null) ? secService :
                (secService = Services.GetService<ISecretaryService>());
        private GameObject secretaryView;

        private bool isInitialized = false;


        public void Setup(int managerId) {
            ISecretaryService secretaryService = Services.GetService<ISecretaryService>();
            ILocalizationRepository localization = Services.ResourceService.Localization;
            this.manager = Services.GetService<IManagerService>().GetManager(managerId);
            auditorView.Setup(Services.GenerationService.GetGetenerator(managerId));
            UpdateReportsCountText(managerId);
            UpdateManagerEfficiency();
            UpdateSecretaryCountText();
            
            buySecretaryButton.SetListener(() => {

                BosError error = BosError.Ok;

                if (Services.SecretaryService.IsAllowBuySecretary(manager.Id, out error)) {
                    BosError status = secretaryService.BuySecretary(manager.Id);
                    if (status == BosError.Ok) {
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.buyGenerator);
                    } else {
                        Services.GetService<ISoundService>().PlayOneShot(SoundName.slotFail);
                    }
                } else {
                    if(error == BosError.NoEnoughCoins ) {
                        Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                            UserData = Services.SecretaryService.GetNextSecretaryPrice(manager.Id),
                            ViewDepth = ViewService.NextViewDepth,
                        });
                        Sounds.PlayOneShot(SoundName.click);
                    }
                }
            });
            UpdateBuyButtonState();
            UpdateSecretaryPriceText();
            CreateConstSecretaryAnimObject();

            if(!isInitialized) {
                isInitialized = true;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ReportCountChanged += OnReportsCountChanged;
            GameEvents.SecretaryCountChanged += OnSecretaryCountChanged;
            GameEvents.CoinsChanged += OnCoinsChanged;
            GameEvents.EfficiencyChangeEvent += OnEfficiencyChange;
            GameEvents.EfficiencyDropEvent += OnEfficiencyDrop;
        }

        public override void OnDisable() {
            GameEvents.ReportCountChanged -= OnReportsCountChanged;
            GameEvents.SecretaryCountChanged -= OnSecretaryCountChanged;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            GameEvents.EfficiencyChangeEvent -= OnEfficiencyChange;
            GameEvents.EfficiencyDropEvent -= OnEfficiencyDrop;
            base.OnDisable();
        }

        private void OnEfficiencyDrop(GameEvents.EfficiencyDrop drop ) {
            if(manager != null  ) {
                OnEfficiencyChanged(drop.Value, drop.Manager);
            }
        }

        private void OnEfficiencyChange(GameEvents.EfficiencyChange change) {
            if(manager != null ) {
                OnEfficiencyChanged(change.NewEfficiency - change.OldEfficiency, change.Manager);
            }
        }

        private void CreateConstSecretaryAnimObject() {
            if(manager != null) {
                int secretaryCount = Services.SecretaryService.GetSecretaryCount(manager.Id);
                if(secretaryView == null ) {
                    if(secretaryCount > 0 ) {
                        secretaryView = Instantiate<GameObject>(secretaryObjectPrefab);
                        secretaryView.GetComponent<RectTransform>().SetParent(animParent, false);
                        secretaryView.GetComponent<ConstSecretaryAnimObject>().Setup(Services.SecretaryService.GetSecretaryInfo(manager.Id));
                    }
                }
            }
        }
        


        private void OnReportsCountChanged(int oldCount, int newCount, ReportInfo reportInfo) {
            if(manager != null ) {
                UpdateReportsCountText(manager.Id);
            }
        }

        private void OnEfficiencyChanged(double efficiencyChange, ManagerInfo targetManager) {
            if(manager != null ) {
                if(manager.Id == targetManager.Id) {
                    UpdateManagerEfficiency();
                }
            }
        }

        private void OnSecretaryCountChanged(int oldCount, int newCount, SecretaryInfo reportInfo) {
            if (manager != null) {
                if (manager.Id == reportInfo.GeneratorId) {
                    UpdateSecretaryCountText();
                    UpdateSecretaryPriceText();
                    CreateConstSecretaryAnimObject();
                }
            }

        }

        private void OnCoinsChanged(int oldCoins, int newCoins) {
            UpdateBuyButtonState();
        }
        private void UpdateSecretaryPriceText() {
            if(manager != null ) {
                secretaryPriceText.text = Services.GetService<ISecretaryService>().GetNextSecretaryPrice(manager.Id).ToString();
            } else {
                secretaryPriceText.text = string.Empty;
            }
        }

        private void UpdateBuyButtonState() {
            if (manager != null) {
                int nextSecretaryPrice = Services.GetService<ISecretaryService>().GetNextSecretaryPrice(manager.Id);
                var currency = Currency.CreateCoins(nextSecretaryPrice);
                //buySecretaryButton.interactable = Services.SecretaryService.IsAllowBuySecretary(manager.Id); //Services.PlayerService.IsEnough(currency) && Services.ManagerService.IsHired(manager.Id);
                BosError error;
                Services.SecretaryService.IsAllowBuySecretary(manager.Id, out error);
                if(error == BosError.Ok || error == BosError.NoEnoughCoins ) {
                    buySecretaryButton.interactable = true;
                } else {
                    buySecretaryButton.interactable = false;
                }
            }
        }

        private void UpdateManagerEfficiency() {
            ILocalizationRepository localization = Services.ResourceService.Localization;
            if (manager != null) {
                if (manager.IsMaxEfficiency(Services)) {
                    managerEfficiencyText.text = string.Format(localization.GetString("fmt_mgr_eff"), manager.EfficiencyPercent(Services));
                } else {
                    managerEfficiencyText.text = string.Format(localization.GetString("fmt_mgr_eff_drop"), manager.EfficiencyPercent(Services).ToString().Colored(ConsoleTextColor.red));
                }
            }
        }
        private void UpdateReportsCountText(int managerId) {
            ISecretaryService secretaryService = Services.SecretaryService;
            ILocalizationRepository localization = Services.ResourceService.Localization;
            int reportCount = secretaryService.GetReportCount(managerId);
            if (reportCount == 0) {
                reportCountText.text = localization.GetString("lbl_no_reports");
            } else {
                reportCountText.text = string.Format(localization.GetString("fmt_reports_count"), reportCount.ToString().Colored(ConsoleTextColor.red));
            }
        }

        private void UpdateSecretaryCountText() {
            if(manager != null ) {
                secretaryCountText.text = Services.GetService<ISecretaryService>().GetSecretaryCount(manager.Id).ToString();
            } else {
                secretaryCountText.text = string.Empty;
            }
        }
    }

}