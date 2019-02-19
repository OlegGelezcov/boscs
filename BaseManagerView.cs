namespace Bos.UI {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    public class BaseManagerView : GameBehaviour {

        public Image iconImage;
        public Image transportIconImage;
        public Text efficiencyValueText;
        public Text kickBackValueText;
        public Text profitText;

        public GameObject megaTextObject;

        public GameObject[] additionalMegaObjects;

        private ManagerInfo manager;
        private string dollarsString = null;

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        private bool isInitialized = false;

        public void Setup(ManagerInfo manager) {
            this.manager = manager;
            

            GeneratorInfo generator = Services.GenerationService.GetGetenerator(manager.Id);

            updateTimer.Setup(1.0f, (delta) => {

                ProfitResult profitResult = generator.ConstructProfitResult(Services.GenerationService.Generators); //Services.GenerationService.CalculateProfitPerSecond(generator,Services.TransportService.GetUnitLiveCount(manager.Id));
                CurrencyNumber number = profitResult.ValuePerSecond.ToCurrencyNumber();
                profitText.text = FormatProfit(profitResult.ValuePerSecond.ToCurrencyNumber());
                UpdateEfficiencyText();

            }, invokeImmediatly: true);

            UpdateManagerIcon();
            UpdateMegaTextObject();
            UpdateRollbackChanged();

            if(!isInitialized ) {
                isInitialized = true;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            //GameEvents.EfficiencyChanged += OnEfficiencyChanged;
            GameEvents.TransportManagerHired += OnManagerHired;
            GameEvents.EfficiencyLevelChanged += OnEfficiencyLevelChanged;
            GameEvents.RollbackLevelChanged += OnRollbackLevelChanged;
            GameEvents.MaxRollbackChanged += OnMaxRollBackChanged;
            GameEvents.EfficiencyDropEvent += OnEfficiencyDrop;
        }

        public override void OnDisable() {
            //GameEvents.EfficiencyChanged -= OnEfficiencyChanged;
            GameEvents.TransportManagerHired -= OnManagerHired;
            GameEvents.EfficiencyLevelChanged -= OnEfficiencyLevelChanged;
            GameEvents.RollbackLevelChanged -= OnRollbackLevelChanged;
            GameEvents.MaxRollbackChanged -= OnMaxRollBackChanged;
            GameEvents.EfficiencyDropEvent -= OnEfficiencyDrop;
            base.OnDisable();
        }

        private void OnEfficiencyDrop(GameEvents.EfficiencyDrop drop) {
            UpdateEfficiencyText();
        }

        private void OnMaxRollBackChanged(double oldValue, double newValue, ManagerInfo mgr ) {
            UpdateRollbackChanged();
        }

        private void UpdateRollbackChanged() {
            kickBackValueText.text = string.Format(Services.ResourceService.Localization.GetString("fmt_kickback_percent"), manager.RollbackPercent);
        }

        private void UpdateManagerIcon() {
            if (manager != null) {
                int currentPlanetId = Services.PlanetService.CurrentPlanet.Id;
                bool isHired = Services.ManagerService.IsHired(manager.Id);
                Debug.Log($"update icon for manager => {manager.Id}");
                var spriteData = Services.ResourceService.ManagerLocalDataRepository.GetIconData(manager.Id, currentPlanetId, isHired);
                
                var generatorIconData = Services.ResourceService.GeneratorLocalData.GetLocalData(manager.Id).GetIconData(Services.PlanetService.CurrentPlanet.Id);
                transportIconImage.overrideSprite = Services.ResourceService.GetSpriteByKey(generatorIconData.icon_id);
                
                if (spriteData != null) {
                    Debug.Log($"set sprite from sprite data => {spriteData.ToString().Colored(ConsoleTextColor.yellow)}");
                    var img = Services.ResourceService.GetSprite(spriteData);
                    if (img != null) {
                        iconImage.overrideSprite = img;
                    }
                } else {
                    Debug.Log($"sprite data not found... set fallback");
                    spriteData = ResourceService.ManagerLocalDataRepository.GetIconData(manager.Id, 0, isHired);
                    if(spriteData != null ) {
                        Debug.Log("set earth sprite data...");
                        iconImage.overrideSprite = ResourceService.GetSprite(spriteData);
                    } else {
                        Debug.Log("earth sprite is null...");
                        iconImage.overrideSprite = ResourceService.Sprites.FallbackSprite;
                    }
                }

            }
        }

        private string FormatProfit(CurrencyNumber number) {
            string[] components = number.AbbreviationComponents();
            if (!string.IsNullOrEmpty(components[1])) {
                return $"${components[0]} {components[1]}/SEC";
            } else {
                return $"${components[0]} {DollarsString}/SEC";
            }
        }

        private string DollarsString
            => (dollarsString != null) ? dollarsString
    :           (dollarsString = Services.ResourceService.Localization.GetFrequentString("DOLLARS"));

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }



        private void OnRollbackLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel otherManager) {
            if(manager != null && (manager.Id == otherManager.Id)) {
                UpdateMegaTextObject();
            }
        }
        private void OnEfficiencyLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel otherManager) {
            if(manager != null && (manager.Id == otherManager.Id)) {
                UpdateMegaTextObject();
            }
        }

        private void UpdateEfficiencyText()
            => efficiencyValueText.text = $"{manager.EfficiencyPercent(Services)}%";

        private void OnEfficiencyChanged(double change, ManagerInfo targetManager) {
            if ((manager != null) && (manager.Id == targetManager.Id)) {
                UpdateEfficiencyText();
            }
        }

        private void OnManagerHired(ManagerInfo man) {
            if(manager != null ) {
                if(manager.Id == man.Id) {
                    UpdateManagerIcon();
                }
            }
        }

        private void UpdateMegaTextObject(){
            if(manager != null ) {
                var mgrLevel = Services.ManagerService.GetManagerEfficiencyRollbackLevel(manager.Id);
                if(mgrLevel.IsMega) {
                    megaTextObject?.Activate();
                    if(megaTextObject != null ) {
                        var scaleData1 = AnimUtils.GetScaleAnimData(1, 1.2f, 0.5f, EaseType.EaseInOutQuad, megaTextObject.GetComponent<RectTransform>(), ()=>{
                            var scaleData2 = AnimUtils.GetScaleAnimData(1.2f, 1, 0.5f, EaseType.EaseInOutQuad, megaTextObject.GetComponent<RectTransform>());
                            megaTextObject.GetOrAdd<Vector2Animator>().StartAnimation(scaleData2);
                        });
                        megaTextObject.GetOrAdd<Vector2Animator>().StartAnimation(scaleData1);
                    }
                    ToggleAdditionalObjects(true);
                } else {
                    megaTextObject?.Deactivate();
                    ToggleAdditionalObjects(false);
                }
            } else {
                megaTextObject?.Deactivate();
                ToggleAdditionalObjects(false);
            }
        }

        private void ToggleAdditionalObjects(bool isActive) {
            foreach(var obj in additionalMegaObjects) {
                if(isActive) {
                    obj.Activate();
                    ColorAnimator colorAnimator = obj.GetComponent<ColorAnimator>();
                    colorAnimator?.StartAnimation(AnimUtils.GetColorAnimData(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0), 2,
                         EaseType.EaseInOutQuad, obj.GetComponent<RectTransform>(), BosAnimationMode.PingPong));
                } else {
                    obj.Deactivate();
                }
            }
        }
    }

}