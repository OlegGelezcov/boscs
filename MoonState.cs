namespace Bos {
    using Bos.Data;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;


   
    public class MoonState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.Moon;

        private bool isDialogShowed = false;

        private readonly UpdateTimer guardTimer = new UpdateTimer();

        public override bool IsValid(IBosServiceCollection context) {
            return IsEnoughCurrenciesForPlanet(context) && IsModuleOpened(context) &&
                context.TutorialService.IsStateCompleted(TutorialStateName.SpaceShip) &&
                IsPlanetLocked(context);
        }

        private bool IsPlanetLocked(IBosServiceCollection services) {
            return services.PlanetService.GetPlanet(1).State == PlanetState.Closed;
        }

        private bool IsEnoughCurrenciesForPlanet(IBosServiceCollection services) {
            var planetData = services.ResourceService.Planets.GetPlanet(1);
            return services.PlayerService.IsEnoughCurrencies(planetData.Prices);
        }

        private bool IsModuleOpened(IBosServiceCollection services) {
            return services.Modules.IsOpened(0);
        }

        


        public override string GetValidationDescription(IBosServiceCollection services) {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"is enough currencies for planet: {IsEnoughCurrenciesForPlanet(services)}");
            sb.AppendLine($"is module opened: {IsModuleOpened(services)}");
            sb.AppendLine($"is planet closed: {IsPlanetLocked(services)}");
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.SpaceShip);
            return sb.ToString();
        }

        private bool IsMoonOpening(IBosServiceCollection services) {
            IPlanetService planets = services.PlanetService;
            var moonPlanet = planets.GetPlanet(PlanetConst.MOON_ID);
            return (moonPlanet.State == PlanetState.Opening || 
                moonPlanet.State == PlanetState.ReadyToOpen);
        }

        public override void OnEnter(IBosServiceCollection context) {
            //handle state if we already on moon
            if (context.PlanetService.IsMoonOpened || IsMoonOpening(context)) {
                ForceSetCompleted();
                return;
            }

            if (!IsOnEntered) {
                if (!isDialogShowed) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_35"),
                        OnOk = () => {
                            GameScrollView scrollView = GameObject.FindObjectOfType<GameScrollView>();
                            if (scrollView != null) {
                                scrollView.Scroll.verticalNormalizedPosition = 1;
                            }
                            Finger(context, "planets", context.ResourceService.Localization.GetString("lbl_tut_36"), new Vector2(-717, -72), 1.34f, 500, 10);
                        }
                    });
                }

                guardTimer.Setup(1, dt => {
                    if (IsActive) {
                        if (isDialogShowed) {
                            if (context.PlanetService.IsOpened(1)) {
                                CompleteSelf(context);
                            }
                        }
                    }
                });

                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            Func<bool> isValid = () => IsActive && isDialogShowed && data.EventName == TutorialEventName.ViewOpened;
            BosUtils.If(isValid,
                trueAction: () => {
                    if ((ViewType)data.UserData == ViewType.PlanetsView) {
                        RemoveFinger(context, "planets");
                        ForceTutorialDialog(context, new TutorialDialogData {
                            Texts = GetLocalizationStrings(context, "lbl_tut_37"),
                            OnOk = () => {
                                Finger(context, "moon", 10);
                            }
                        });
                    }
                },
                falseAction: () => {
                });

            BosUtils.If(() => IsActive && isDialogShowed && data.EventName == TutorialEventName.OpenPlanetClicked,
                trueAction: () => {
                    if ((int)data.UserData == 1) {
                        RemoveFinger(context, "moon");
                        CompleteSelf(context);
                    }
                },
                falseAction: () => { });
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if (IsActive) {
                guardTimer.Update(deltaTime);
            }
        }

        protected override void OnExit(IBosServiceCollection context) {

        }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }
    }
    


    /*
    public class MoonStateVer2 : TutorialState {
        
        private bool isDialogShowed = false;

        public override TutorialStateName Name => TutorialStateName.Moon;

        private bool IsModuleNotOpenedAndMoonLocked(IBosServiceCollection services, int moduleId) {
            bool isModuleNotOpened = services.Modules.IsOpened(moduleId) == false;
            bool isMoonLocked = services.PlanetService.GetPlanet(PlanetConst.MOON_ID).State == PlanetState.Closed;
            return isModuleNotOpened && isMoonLocked;
        }
        public override bool IsValid(IBosServiceCollection services) {

            var planetData = services.ResourceService.Planets.GetPlanet(PlanetConst.MOON_ID);
            var localData = services.PlanetService.GetPlanet(id: PlanetConst.MOON_ID).LocalData;
            var moduleData = services.ResourceService.ShipModuleRepository.GetModule(localData.module_id);

            if (IsModuleNotOpenedAndMoonLocked(services, localData.module_id)) {
                var prices = BosUtils.CombineCurrencies(
                    planetData.Prices, 
                    new Currency[] { moduleData.Currency });
                if (services.PlayerService.IsEnoughCurrencies(prices.ToArray())) {
                    return true;
                }
            }
            return false;
        }


    }*/
}