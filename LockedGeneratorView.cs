namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class LockedGeneratorView : GameBehaviour {

        private GeneratorInfo generator = null;

        public Button unlockButton;
        public ParticleSystem unlockParticles;
        public Text unlockPriceText;
        public Text generatorNameText;


        public Image lockIcon;
        private GeneratorPlanetIconData generatorPlanetIconData = null;

        public void Setup(int generatorId) {
            generator = Services.GenerationService.Generators.GetGeneratorInfo(generatorId);

            unlockButton.SetListener(() => {
                Services.GenerationService.BuyGenerator(generator);
                unlockParticles.Play();
                Sounds.PlayOneShot(SoundName.Poof);
            });

            string priceString = Services.Currency.CreatePriceString(Services.GenerationService.GetGeneratorUnlockPrice(generatorId), false, " ");
            unlockPriceText.text = priceString;

            /*
            if(generator.Data.Type == GeneratorType.Planet) {
                PlanetNameData planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(generator.PlanetId);
                generatorNameText.text = Services.ResourceService.Localization.GetString(planetNameData.name);
            } else {
                generatorNameText.text = Services.ResourceService.Localization.GetString(generator.Data.Name);
            }*/
            Services.ViewService.Utils.ApplyGeneratorName(generatorNameText, generator);

            UpdateUnlockButtonView(generator);
            OnGeneratorStateChanged(generator.State, generator.State, generator);
            UpdateGeneratorIcon();

            //unlock planets automatically
            if (generator.Data.Type == GeneratorType.Planet) {
                Services.GenerationService.BuyGenerator(generator, true);
                unlockParticles.Play();
                Services.GetService<ISoundService>().PlayOneShot(SoundName.Poof);
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GeneratorStateChanged += OnGeneratorStateChanged;
            GameEvents.GeneratorEnhanced += OnGeneratorEnhanced;
        }

        public override void OnDisable() {
            
            GameEvents.GeneratorStateChanged -= OnGeneratorStateChanged;
            GameEvents.GeneratorEnhanced -= OnGeneratorEnhanced;
            base.OnDisable();
        }

        private void OnGeneratorEnhanced(GeneratorInfo info) {
            UpdateUnlockButtonView(info);
        }

        private void UpdateUnlockButtonView(GeneratorInfo info) {
            if (generator != null && (generator.GeneratorId == info.GeneratorId)) {
                if (info.IsEnhanced) {
                    unlockButton.GetComponent<Image>().sprite = SpriteDB.SpriteRefs["unlock_enhanced"];
                    unlockButton.GetComponent<Image>().overrideSprite = SpriteDB.SpriteRefs["unlock_enhanced"];
                    unlockButton.spriteState = new SpriteState {
                        disabledSprite = unlockButton.spriteState.disabledSprite,
                        pressedSprite = SpriteDB.SpriteRefs["unlock_enhanced_pressed"],
                        highlightedSprite = SpriteDB.SpriteRefs["unlock_enhanced"]
                    };
                }
            }
        }

        private void OnGeneratorStateChanged(GeneratorState oldState, 
            GeneratorState newState, GeneratorInfo info ) {
            if(generator != null && (generator.GeneratorId == info.GeneratorId)) {
                switch(newState) {
                    case GeneratorState.Unlockable: {
                            unlockButton.interactable = true;
                        }
                        break;
                    case GeneratorState.Locked: {
                            unlockButton.interactable = false;
                        }
                        break;
                }
            }
        }


        private void UpdateGeneratorIcon()
        {
            if (lockIcon == null) return;
            if (generatorPlanetIconData == null || (generatorPlanetIconData.planet_id != Services.PlanetService.CurrentPlanet.Id))
            {
                generatorPlanetIconData = Services.ResourceService.GeneratorLocalData.GetLocalData(generator.GeneratorId)
                    .GetIconData(Services.PlanetService.CurrentPlanet.Id);
                if (generatorPlanetIconData != null)
                {
                    lockIcon.Activate();
                    if (generatorPlanetIconData.icon_id.IsValid())
                    {
                        var sprite = Services.ResourceService.GetSpriteByKey(generatorPlanetIconData.lock_icon);
                        if (sprite != null)
                        {
                            lockIcon.overrideSprite = sprite;
                        }
                        else
                        {
                            Debug.LogError($"Not found generator {generator.GeneratorId} icon for planet => {Services.PlanetService.CurrentPlanet.Id}");
                            lockIcon.MakeTransparent();
                        }
                    }
                    else
                    {
                        lockIcon.MakeTransparent();
                    }
                }
            }
        }
    }

}