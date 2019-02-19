namespace Bos {
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SpaceShipState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.SpaceShip;

        private bool isDialogShowed = false;

        public override bool IsValid(IBosServiceCollection services) {

            bool isModuleNotOpened = IsModuleNotOpened(services);
            bool isEnoughCurrencyToBuyModule = IsEnoughCurrencyForModule(services);

            return  isEnoughCurrencyToBuyModule && isModuleNotOpened;
        }

        private bool IsEnoughCurrencyForModule(IBosServiceCollection services ) {
            var moduleData = services.ResourceService.ShipModuleRepository.GetModule(0);
            return services.PlayerService.IsEnough(moduleData.Currency);
        }
        private bool IsModuleNotOpened(IBosServiceCollection services) {
            return services.Modules.IsOpened(0) == false;
        }

        public override string GetValidationDescription(IBosServiceCollection services) {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is Module not opened => {IsModuleNotOpened(services)}");
            sb.AppendLine($"Is Enough currencies for module => {IsEnoughCurrencyForModule(services)}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {
                if (!isDialogShowed) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_53"),
                        OnOk = () => {
                            //Finger(context, "modules", context.ResourceService.Localization.GetString("lbl_tut_53"),
                            //    new Vector2(110, -34), 1.34f, 500, 20);
                            Finger(context, "modules", 20);
                        }
                    });

                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (isDialogShowed) {
                    if (data.EventName == TutorialEventName.ViewOpened) {
                        if ((ViewType)data.UserData == ViewType.BuyModuleView) {
                            RemoveFinger(context, "modules");
                            ForceTutorialDialog(context, new TutorialDialogData {
                                Texts = GetLocalizationStrings(context, "lbl_tut_54"),
                                OnOk = () => CompleteSelf(context)
                            });
                        }
                    }
                }
            }
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
        }

        protected override void OnExit(IBosServiceCollection context) {
        }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }
    }


}