namespace Bos.UI {
    using Bos.Data;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class AdditionalGeneratorView : GameBehaviour {

        public int generatorId;
        public GameObject x20;
        public Image levelProgressEmptyImage;
        public Image levelProgressFullImage;
        public ShinyShaderController shinyController;

        private RectTransform x20Transform;

        private RectTransform X20Rect =>
            (x20Transform != null) ? x20Transform :
            (x20Transform = x20.GetComponent<RectTransform>());


        public override void Start() {
            base.Start();
            UpdateLevelProgressesSprites();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.X20BoostMultStarted += OnBoostStarted;
            GameEvents.PlanetStateChanged += OnPlanetStateChanged;
        }

        public override void OnDisable() {
            GameEvents.X20BoostMultStarted -= OnBoostStarted;
            GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
            base.OnDisable();
           
        }

        private void OnBoostStarted(bool isStarted) {
            if(GeneratorInfo.State == GeneratorState.Active) {
                if(isStarted) {
                    x20?.Activate();
                    x20?.GetComponent<Vector3Animator>()?.StartAnimation(new Vector3AnimationData {
                        AnimationMode = BosAnimationMode.PingPong,
                        Duration = 0.4f,
                        EaseType = EaseType.EaseInOutQuad,
                        StartValue = Vector3.one,
                        EndValue = Vector3.one * 1.2f,
                        OnStart = (s, go) => X20Rect.localScale = s,
                        OnUpdate = (s, t, go) => X20Rect.localScale = s,
                        OnEnd = (s, go) => X20Rect.localScale = s,
                        Target = x20
                    });
                    if (!GeneratorInfo.IsEnhanced) {
                        shinyController?.ToggleEffect(true);
                    } else {
                        shinyController?.ResetMaterialOnImage();
                    }
                } else {
                    x20?.Deactivate();
                    x20?.GetComponent<Vector3Animator>()?.Stop();
                    if (!GeneratorInfo.IsEnhanced) {
                        shinyController?.ToggleEffect(false);
                    } else {
                        shinyController?.ResetMaterialOnImage();
                    }
                }
            }
        }

        private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
            if(newState == PlanetState.Opened) {
                UpdateLevelProgressesSprites();
            }
        }



        private GeneratorInfo generatorInfo = null;
        private GeneratorInfo GeneratorInfo {
            get {
                return (generatorInfo != null) ? 
                    generatorInfo : 
                    (generatorInfo = Services.GenerationService.Generators.GetGeneratorInfo(generatorId));
            }
        }

        private void UpdateLevelProgressesSprites() {
            IPlanetService planetService = Services.GetService<IPlanetService>();
            int currentPlanetId = planetService.CurrentPlanet.Id;
            if(currentPlanetId > PlanetConst.EARTH_ID) {
                PlanetType planetType = (PlanetType)currentPlanetId;
                string fillSpriteId = ResourceUtils.LevelProgressPlanetMap[planetType];
                levelProgressEmptyImage.overrideSprite = Services.ResourceService.GetSpriteByKey(ResourceUtils.LevelEmptyProgressForPlanets);
                levelProgressFullImage.overrideSprite = Services.ResourceService.GetSpriteByKey(fillSpriteId);
            }
        }
    }

}