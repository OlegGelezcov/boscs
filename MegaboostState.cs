namespace Bos {
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public class MegaBoostState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.MegaBoost;

        private const int STAGE_START = 0;
        public const int STAGE_SHOW_FINGER_OB_BUTTON = 10;
        private const int STAGE_END = 1000;

        private bool isDialogShowed = false;
        private const string kMegaBoostPosition = "mega_boost";
        public const string kTapMegaBoostPosition = "tap_mega_boost";

        public override bool IsValid(IBosServiceCollection context) {
            var boostState = context.GetService<IX20BoostService>().State;

            return (context.TutorialService.IsStateCompleted(TutorialStateName.BuyBus) &&
                (boostState == BoostState.Active || boostState == BoostState.ReadyToActivate));
        }

        public override string GetValidationDescription(IBosServiceCollection services) {
            var sb = GetBaseValidationDescription();
            var boostState = services.GetService<IX20BoostService>().State;
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.BuyBus);
            sb.AppendLine($"x20 boost is Active or ReadyToActivate: { boostState == BoostState.Active || boostState == BoostState.ReadyToActivate}");
            return sb.ToString();
        }

        private bool IsInitialized { get; set; }

        public override void Setup(IBosServiceCollection services) {
            base.Setup(services);
            if(!IsInitialized) {
                GameEvents.X20BoostStateChangedObservable.Subscribe(args => {
                    if(IsActive) {
                        if(Stage == STAGE_START ) {
                            if(args.NewState == BoostState.Active || args.NewState == BoostState.ReadyToActivate) {
                                if(IsOnEntered && isDialogShowed ) {
                                    
                                    MoveToRepeatFingerState(services);
                                }
                            }
                        }
                    }
                }).AddTo(services.Disposables);
                IsInitialized = true;
            }
        }

        protected override void OnStageChanged(int oldStage, int newStage) {
            base.OnStageChanged(oldStage, newStage);
            if(newStage == STAGE_SHOW_FINGER_OB_BUTTON ) {
                GameServices.Instance.TutorialService.SetPausedOnInterval(60, () => {
                    SetStage(STAGE_END);
                });
            } else if(newStage == STAGE_END ) {
                CompleteSelf(GameServices.Instance);
            }
        }

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {

                if (!isDialogShowed) {
                    isDialogShowed = true;
                    context.Execute(() => {
                        context.ViewService.Show(ViewType.TutorialDialogView,
                            () => {
                                return (context.ViewService.ModalCount == 0) && (context.ViewService.LegacyCount == 0) &&
                                (!context.ViewService.Exists(ViewType.TutorialDialogView));
                            },
                            (viewObj) => { },
                            new ViewData {
                                UserData = new TutorialDialogData {
                                    Texts = context.ResourceService.Localization.GetString("lbl_tut_19").WrapToList(),
                                    OnOk = () => {
                                        context.TutorialService.CreateFinger(kMegaBoostPosition, new TutorialFingerData {
                                            Id = kMegaBoostPosition,
                                            IsTooltipVisible = true,
                                            Position = Vector2.zero,
                                            TooltipPosition = new Vector2(-196, -95),
                                            TooltipScale = new Vector3(1.34f, 1.34f, 1),
                                            TooltipText = context.ResourceService.Localization.GetString("lbl_tut_20"),
                                            TooltipWidth = 500,
                                            Timeout = 7
                                        });
                                    }
                                }
                            });
                    }, 1f);
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (isDialogShowed) {
                    if (data.EventName == TutorialEventName.MegaBoostActivated) {
                        MoveToRepeatFingerState(context);
                    } else if (data.EventName == TutorialEventName.MegaboostStateChanged) {
                        BoostState state = (BoostState)data.UserData;
                        if (state == BoostState.ReadyToActivate || state == BoostState.Active) {
                            MoveToRepeatFingerState(context);
                        }
                    }
                }
            }
        }

        private void MoveToRepeatFingerState(IBosServiceCollection services) {
            services.TutorialService.RemoveFinger(kMegaBoostPosition);
            //CompleteSelf(context);
            if (Stage == STAGE_START) {
                UnityEngine.Debug.Log($"Set MegaboostState Stage to VALID".Attrib(bold: true, italic: true, color: "g", size: 20));
                SetStage(STAGE_SHOW_FINGER_OB_BUTTON);
            }
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) { }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }
    }


}