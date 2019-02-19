namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlanetsView : TypedViewWithCloseButton, IPlanetViewContext {

        public TMPro.TMP_ColorGradient selectedColorGradient;
        public TMPro.TMP_ColorGradient notSelectedColorGradient;
        public ScrollRect parentScroll;
        //public Sprite emptyRocket;
        //public Sprite filledRocket;
        public Button startShipButton;


       public override bool IsModal => true;

        private List<PlanetView> planetViews = null;

        public List<PlanetView> PlanetViews {
            get {
                if(planetViews == null ) {
                    var views = GetComponentsInChildren<PlanetView>();
                    planetViews = new List<PlanetView>(views);
                }
                return planetViews;
            }
        }

        public TMP_ColorGradient SelectedColorGradient => selectedColorGradient;

        public TMP_ColorGradient NotSelectedColorGradient => notSelectedColorGradient;

        //public Sprite StartOpeningSprite => emptyRocket;

        //public Sprite EndOpenedSprite => filledRocket;

        public override ViewType Type => ViewType.PlanetsView;

        public override void Setup(ViewData data) {
            base.Setup(data);
            PlanetViews.ForEach(view => view.Setup(this));
            Debug.Log($"Planet count => {PlanetViews.Count}");
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.PlanetsView, BosUISettings.Instance.ViewCloseDelay);
                closeButton.interactable = false;
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });
            if(parentScroll != null ) {
                parentScroll.verticalNormalizedPosition = 1;
            }
            UpdateShip();
        }

        public override int ViewDepth => 10;

        public override CanvasType CanvasType => CanvasType.UI;



        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ShipModuleStateChanged += OnShipModuleStateChanged;
        }

        public override void OnDisable() {
            GameEvents.ShipModuleStateChanged -= OnShipModuleStateChanged;
            base.OnDisable();
        }

        private void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState, ShipModuleInfo module) {
            UpdateShip();
        }

        
        private void UpdateShip() {
            IShipModuleService moduleService = Services.GetService<IShipModuleService>();
            if (moduleService.IsAllModulesOpened) {

                startShipButton.interactable = true;
                startShipButton.SetListener(() => {
                    startShipButton.Deactivate();
                    Services.GameModeService.StartWinGame();
                    Services.ViewService.Remove(ViewType.PlanetsView, BosUISettings.Instance.ViewCloseDelay);
                    Services.SoundService.PlayOneShot(SoundName.slotWin);
                });
            } else {
                startShipButton.interactable = false;
            }
        }

    }

    public interface IPlanetViewContext {
        List<PlanetView> PlanetViews { get; }
        TMPro.TMP_ColorGradient SelectedColorGradient { get; }
        TMPro.TMP_ColorGradient NotSelectedColorGradient { get; }
        //Sprite StartOpeningSprite { get; }
        //Sprite EndOpenedSprite { get; }
    }

}