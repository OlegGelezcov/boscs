namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Analytics;
    using UnityEngine.UI;

    public class ResearchGeneratorView : GameBehaviour {

        private const int kBusId = 2;

        public Button researchButton;
        public ParticleSystem transitionParticles;
        //public GameObject notEnoughCoinsPopup;
        public Image generatorIconImage;
        public Image bg;
        public Text priceText;
        public GameObject coinIconObject;
        public Text generatorNameText;
        public GameObject marsLockObject;

        private GeneratorPlanetIconData generatorPlanetIconData = null;


        private void AutoResearch() {
            Services.GenerationService.Research(generator.GeneratorId);
            transitionParticles.Play();
            Analytics.CustomEvent(AnalyticsStrings.RESEARCH_ZEPPELIN);
            Services.GetService<ISoundService>().PlayOneShot(SoundName.Poof);
        }

        private GeneratorInfo generator = null;
        public void Setup(int generatorId ) {
            generator = Services.GenerationService.Generators.GetGeneratorInfo(generatorId);
            int researchPrice = generator.ResearchPrice(Planets);

            researchButton.SetListener(() => {
                IPlayerService playerService = Services.PlayerService;

                if(IsNeedShowDependView()) {
                    Sounds.PlayOneShot(SoundName.click);

                    ViewService.Show(ViewType.DependGeneratorView, new ViewData {
                        ViewDepth = ViewService.NextViewDepth,
                        UserData = generator
                    });
                } else if(playerService.IsEnoughCoins(researchPrice)) {
                    playerService.RemoveCoins(researchPrice);
                    Services.GenerationService.Research(generator.GeneratorId);
                    transitionParticles.Play();
                    Analytics.CustomEvent(AnalyticsStrings.RESEARCH_ZEPPELIN);
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.Poof);
                } else {
                    //notEnoughCoinsPopup.GetComponent<NotEnoughCoinsScreen>().Show(generator.Data.CoinPrice);
                    Sounds.PlayOneShot(SoundName.click);
                    Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                        UserData = researchPrice
                    });
                }
            });
            /*
            if(generator.Data.Type == GeneratorType.Planet ) {
                PlanetNameData planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(generator.PlanetId);
                generatorNameText.text = Services.ResourceService.Localization.GetString(planetNameData?.name) ?? string.Empty;
                generatorIconImage.overrideSprite = Services.ResourceService.Sprites.GetObject(planetNameData.icon);
            } else {
                generatorNameText.text = Services.ResourceService.Localization.GetString(generator.Data.Name);
                
            }*/
            ViewService.Utils.ApplyGeneratorName(generatorNameText, generator);

            if(generator.Data.Type == GeneratorType.Normal ) {
                UpdateGeneratorIcon();
            }
            var currentPlanetData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(Services.PlanetService.CurrentPlanet.Id);
            bg.overrideSprite = Services.ResourceService.GetSpriteByKey(currentPlanetData.research_bg);
            
            priceText.text = (researchPrice != 0) ? researchPrice.ToString() : string.Empty;
            if(researchPrice != 0 ) {
                coinIconObject?.Activate();
            } else {
                coinIconObject.Deactivate();
            }

            
            UpdateResearchButtonInteractability();

            //autoresearch planets
            if (generator.Data.Type == GeneratorType.Planet) {
                AutoResearch();
            }
        }



        public override void OnEnable(){
            base.OnEnable();
            GameEvents.GeneratorStateChanged += OnGeneratorStateChanged;
            GameEvents.CoinsChanged += OnPlayerCoinsChanged;
            GameEvents.TutorialStateActivityChanged += OnTutorialStateActivityChanged;
            GameEvents.TutorialStateCompletedChanged += OnTutorialStateCompletedChanged;
        }

        public override void OnDisable(){
            GameEvents.GeneratorStateChanged -= OnGeneratorStateChanged;
            GameEvents.CoinsChanged -= OnPlayerCoinsChanged;
            GameEvents.TutorialStateActivityChanged -= OnTutorialStateActivityChanged;
            GameEvents.TutorialStateCompletedChanged -= OnTutorialStateCompletedChanged;
            base.OnDisable();
        }

        private void OnTutorialStateActivityChanged(TutorialState state) {
            if(generator != null && generator.GeneratorId == kBusId ) {
                UpdateResearchButtonInteractability();
            }
        }

        private void OnTutorialStateCompletedChanged(TutorialState state ) {
            if (generator != null && generator.GeneratorId == kBusId) {
                UpdateResearchButtonInteractability();
            }
        }

        private void OnGeneratorStateChanged(GeneratorState oldState, GeneratorState newState, GeneratorInfo targetGenerator) {
            if(generator != null ) {
                if(generator.IsDependent) {
                    if(generator.RequiredGeneratorId == targetGenerator.GeneratorId) {
                        UpdateResearchButtonInteractability();
                    }
                }
            }
        }

        private void OnPlayerCoinsChanged(int oldCount, int newCount) {
            UpdateResearchButtonInteractability();
        }

        private bool IsResearchInteractableBuyTutorial() {
            /*
            if(generator != null ) {
                if(generator.GeneratorId == kBusId) {
                    if(Services.TutorialService.IsStateCompleted(TutorialStateName.BuyBus) ||
                        Services.TutorialService.IsStateActive(TutorialStateName.BuyBus)) {
                        return true;
                    } else {
                        return false;
                    }
                } else {
                    return true;
                }
            } else {
                return false;
            }*/
            return true;
        }

        private void SetResearchButtonInteractability(bool isInputInteractable ) {

            bool isInteractable = isInputInteractable;
            if(isInteractable) {
                isInteractable = IsResearchInteractableBuyTutorial();
            }

            researchButton.interactable = isInteractable;
            if(isInteractable) {
                researchButton.GetComponent<Image>().color = Color.white;
            } else {
                researchButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            }
        }

        private bool IsNeedShowDependView() {
            int researchPrice = generator.ResearchPrice(Planets);
            if(Services.PlayerService.IsEnoughCoins(researchPrice)) {
                if(generator.IsDependent) {
                    var requiredGenerator = Services.GenerationService.Generators.GetGeneratorInfo(generator.RequiredGeneratorId);
                    if(requiredGenerator != null ) {
                        if(requiredGenerator.State != GeneratorState.Active ) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateResearchButtonInteractability() {
            if(generator != null) {
                if (!Services.GenerationService.IsLockedByMars(generator)) {
                    marsLockObject?.Deactivate();

                    int researchPrice = generator.ResearchPrice(Planets);
                    if (Services.PlayerService.IsEnoughCoins(researchPrice)) {
                        if (generator.IsDependent) {
                            var requiredGenerator = Services.GenerationService.Generators.GetGeneratorInfo(generator.RequiredGeneratorId);
                            if (requiredGenerator != null) {
                                if (requiredGenerator.State == GeneratorState.Active) {
                                    SetResearchButtonInteractability(true);
                                } else {
                                    SetResearchButtonInteractability(true);

                                }
                            } else {
                                Debug.LogError($"required generator with id => {generator.RequiredGeneratorId} not founded");
                                SetResearchButtonInteractability(true);
                            }
                        } else {
                            SetResearchButtonInteractability(true);
                        }
                    } else {
                        SetResearchButtonInteractability(true);
                    }
                } else {
                    SetResearchButtonInteractability(false);
                    marsLockObject?.Activate();
                }
            }
        }

        private void UpdateGeneratorIcon() {
            if(generatorPlanetIconData == null || (generatorPlanetIconData.planet_id != Services.PlanetService.CurrentPlanet.Id)) {
                generatorPlanetIconData = Services.ResourceService.GeneratorLocalData.GetLocalData(generator.GeneratorId).GetIconData(Services.PlanetService.CurrentPlanet.Id);
                if(generatorPlanetIconData != null ) {
                    generatorIconImage.Activate();
                    if(generatorPlanetIconData.icon_id.IsValid()) {
                        var sprite = Services.ResourceService.GetSpriteByKey(generatorPlanetIconData.icon_id);
                        if(sprite != null ) {
                            generatorIconImage.overrideSprite = sprite;
                        } else {
                            Debug.LogError($"Not found generator {generator.GeneratorId} icon for planet => {Services.PlanetService.CurrentPlanet.Id}");
                            generatorIconImage.MakeTransparent();
                        }
                    } else {
                        generatorIconImage.MakeTransparent();
                    }
                }
            }
        }
    }

}