namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Analytics;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class EnhanceManagerView : TypedView {

        public Image normalGeneratorIconImage;
        public Image upgradedGeneratorIconImage;
        public Text normalGeneratorNameText;
        public Text upgradedGeneratorNameText;
        public Image managerIconImage;
        public Image upgradedManagerIconImage;
        public Button closeButton;
        public Button closeBigButton;
        public Text enhancePriceText;
        public Button enhanceButton;

        public override ViewType Type => ViewType.EnahnceManagerView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 5;

        private EnhanceManagerData enhancedManagerData;
        private readonly UpdateTimer timer = new UpdateTimer();

        public override void Setup(ViewData data) {
            base.Setup(data);

            enhancedManagerData = data.UserData as EnhanceManagerData;
            normalGeneratorIconImage.overrideSprite = enhancedManagerData.normalGeneratorSprite;
            upgradedGeneratorIconImage.overrideSprite = enhancedManagerData.enhancedGeneratorSprite;
            managerIconImage.overrideSprite = enhancedManagerData.managerSprite;
            upgradedManagerIconImage.overrideSprite = enhancedManagerData.managerSprite;
            normalGeneratorNameText.text =
                upgradedGeneratorNameText.text = enhancedManagerData.generatorName;
            enhancePriceText.text = enhancedManagerData.generator.Data.EnhancePrice.ToString();
            timer.Setup(0.3f, dt => UpdateButtonInteractability(), true);
            UnityAction simpleClose = () => {
                Services.ViewService.Remove(ViewType.EnahnceManagerView);
                Services.SoundService.PlayOneShot(SoundName.click);
            };
            UnityAction closeAction = () => {
                simpleClose();
                Analytics.CustomEvent($"ENHANCE_WINDOW_{enhancedManagerData.generator.GeneratorId}_CLOSE");
            };
            closeButton.SetListener(closeAction);
            closeBigButton.SetListener(closeAction);

            var soundService = Services.SoundService;

            enhanceButton.SetListener(() => {
                soundService.PlayOneShot(SoundName.buyUpgrade);
                Services.ManagerService.Enhance(enhancedManagerData.generator);
                simpleClose();
            });

            Analytics.CustomEvent($"ENHANCE_WINDOW_{enhancedManagerData.generator.GeneratorId}_OPEN");
        }

        public override void Update() {
            base.Update();
            timer.Update();
        }


        private void UpdateButtonInteractability() {
            BosUtils.If(() => Services.PlayerService.IsEnoughCoins(enhancedManagerData.generator.Data.EnhancePrice),
                trueAction: () => enhanceButton.interactable = true,
                falseAction: () => enhanceButton.interactable = false);
        }
    }

    public class EnhanceManagerData {
        public GeneratorInfo generator;
        public Sprite normalGeneratorSprite;
        public Sprite enhancedGeneratorSprite;
        public Sprite managerSprite;
        public string generatorName;
    }

    
}