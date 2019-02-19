namespace Bos.UI {
    using Ozh.Tools.Functional;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine.UI;

    public class ModuleFlightView : GameBehaviour {

        public MovingPhoneObjectSystem phone;
        public FlightShipView ship;
        public TMPro.TextMeshProUGUI titleText;
        public Text timerText;
        public Button adButton;
        public List<PlanetIdImage> planetIds = new List<PlanetIdImage>();
        

        private bool IsInitialized { get; set; }

        public void Setup()
        {
            phone.Setup();
            ship.Setup();

            if(!IsInitialized ) {

                GameEvents.ModuleStateChangedObservable.Subscribe(args => {
                    if (args.NewState == ShipModuleState.Opened) {
                        ship.Setup();
                    }
                }).AddTo(gameObject);

                Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                    Planets.GetOpeningPlanet().Match(() => F.None,
                        p => {
                            UpdateTimerText(p);
                            return F.Some(p);
                        });
                }).AddTo(gameObject);

                IsInitialized = true;
            }

            Planets.GetOpeningPlanet().Match(() => {
                SetupNoOpenedPlanet();
                return F.None;
            }, planet => {
                SetupWithPlanet(planet);
                return F.Some(planet);
            });
        }

        private void SetupNoOpenedPlanet() {
            titleText.text = string.Empty;
            timerText.text = string.Empty;
            adButton.Deactivate();
            planetIds.Select(pi => pi.icon).ToggleActivity(false);
        }

        private void SetupWithPlanet(PlanetInfo planet) {
            titleText.text = string.Format(LocalizationObj.GetString("fmt_fly2"), LocalizationObj.GetString(planet.LocalData.name));
            UpdateTimerText(planet);
            adButton.Activate();
            adButton.SetListener(() => {
                Services.AdService.WatchAd("SpeedUpPlanet", () => {
                    planet.ApplySpeedMult();
                });
                Sounds.PlayOneShot(SoundName.click);

            });
            planetIds.ForEach(pi => {
                if(pi.planetId == planet.Id) {
                    pi.icon.Activate();
                } else {
                    pi.icon.Deactivate();
                }
            });
        }

        private void UpdateTimerText(PlanetInfo planet) {
            TimeSpan ts = TimeSpan.FromSeconds(planet.OpeningRemaningTime);
            timerText.text = $"{ts.Hours.ToString("00")}:{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";
        }
    }

    [Serializable]
    public class PlanetIdImage {
        public int planetId;
        public Image icon;
    }

}