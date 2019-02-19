namespace Bos {
    using Bos.Data;
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GameBehaviour : MonoBehaviour {

        public virtual void Awake() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void OnDestroy() { }

        protected IBosServiceCollection Services
            => GameServices.Instance;

        protected IGameModeService GameMode
            => Services.GameModeService;

        protected IResourceService ResourceService
            => Services.ResourceService;

        protected ILocalizationRepository LocalizationObj
            => ResourceService.Localization;

        protected IViewService ViewService
            => Services.ViewService;

        protected IPlayerService Player
            => Services.PlayerService;

        protected IPlanetService Planets
            => Services.PlanetService;

        protected ITimeService TimeService
            => Services.TimeService;

        protected ISoundService Sounds
            => Services.SoundService;

        protected IGenerationService GenerationService
            => Services.GenerationService;
    }

}