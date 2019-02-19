namespace Bos {
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public class PlaySlotstate : TutorialState {
        public override TutorialStateName Name => TutorialStateName.PlaySlot;

        private bool isDialogShowed = false;
        private const string kPositionMiniGames = "mini_games";
        private const string kCasinoButton = "casino_button";
        private const string kTutorialPositionChestTab = "chesttab";
        private const string kTutorialPositionChest = "chest";

        private const int START_STAGE = 0;
        private const int CHEST_TAB_STAGE = 10;
        private const int CHEST_CLICK_STAGE = 20;
        private const int WAIT_FOR_CHEST = 30;

        private const int END_STAGE = 1000;

        private bool IsInitialized { get; set; } = false;

        public override void Setup(IBosServiceCollection services) {
            base.Setup(services);
            UnityEngine.Debug.Log($"PlaySlot State Initialized".Attrib(bold: true, italic: true, color: "g", size: 20));
            if (!IsInitialized) {
                GameEvents.TutorialEventObservable.Subscribe(args => {
                    if(args.EventName == TutorialEventName.WheelCompleted ) {
                        if(Stage == CHEST_TAB_STAGE ) {
                            //remove previous finger
                            //show finger on CHEST TAB
                            UnityEngine.Debug.Log($"Tutorial event: {args.EventName}".Attrib(bold: true, italic: true, color: "g", size: 20));
                            Finger(services, kTutorialPositionChestTab, timeout: 10);
                            SetStage(CHEST_CLICK_STAGE);
                        }
                    } else if(args.EventName == TutorialEventName.ChestTabOpened) {
                        if(Stage == CHEST_CLICK_STAGE ) {
                            //remove previous finger
                            //show finger on CHEST
                            UnityEngine.Debug.Log($"Tutorial event: {args.EventName}".Attrib(bold: true, italic: true, color: "g", size: 20));
                            RemoveFinger(services, kTutorialPositionChestTab);
                            Finger(services, kTutorialPositionChest, timeout: 10);
                            SetStage(WAIT_FOR_CHEST);
                        }
                    } else if(args.EventName == TutorialEventName.ChestOpened ) {
                        if(Stage == WAIT_FOR_CHEST ) {
                            //remove previous finger
                            UnityEngine.Debug.Log($"Tutorial event: {args.EventName}".Attrib(bold: true, italic: true, color: "g", size: 20));
                            RemoveFinger(services, kTutorialPositionChest);
                            SetStage(END_STAGE);
                        }
                    } else if(args.EventName == TutorialEventName.ViewHided) {
                        
                        ViewType viewType = (ViewType)args.UserData;
                        if(viewType == ViewType.MiniGameView ) {
                            if(Stage > 0 ) {
                                CompleteSelf(services);
                            }
                        }
                    }
                }).AddTo(services.Disposables);
                IsInitialized = true;
            }
        }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.BuyBus);
        }

        public override string GetValidationDescription(IBosServiceCollection services) {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is TutorialStateName.BuyBus completed: {services.TutorialService.IsStateCompleted(TutorialStateName.BuyBus)}");
            return sb.ToString();
        }

        protected override void OnStageChanged(int oldStage, int newStage) {
            base.OnStageChanged(oldStage, newStage);
            if(newStage == END_STAGE ) {
                if(IsActive) {
                    CompleteSelf(GameServices.Instance);
                }
            }
        }

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {

                SetStage(START_STAGE);

                if (!isDialogShowed) {
                    isDialogShowed = true;

                    context.Execute(() => {
                        context.ViewService.Show(ViewType.TutorialDialogView,
                            () => { return IsStandardDialogCondition(context); },
                            (go) => { /* UnityEngine.Debug.Log($"view {go.name} is showed")*/ },
                            new ViewData {
                                UserData = new TutorialDialogData {
                                    Texts = context.ResourceService.Localization.GetString("lbl_tut_11").WrapToList(),
                                    OnOk = () => {
                                        context.TutorialService.CreateFinger(kPositionMiniGames, new TutorialFingerData {
                                            Id = kPositionMiniGames,
                                            IsTooltipVisible = false,
                                            Position = Vector2.zero
                                        });
                                    }
                                }
                            });
                    }, 5);
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (isDialogShowed) {
                    if (data.EventName == TutorialEventName.MiniGameOpened) {
                        context.TutorialService.RemoveFinger(kPositionMiniGames);
                        context.TutorialService.CreateFinger(kCasinoButton, new TutorialFingerData {
                            Id = kCasinoButton,
                            IsTooltipVisible = false
                        });
                    } else if (data.EventName == TutorialEventName.PlayCasinoClicked) {
                        context.TutorialService.RemoveFinger(kCasinoButton);
                        //CompleteSelf(context);
                        SetStage(CHEST_TAB_STAGE);
                    }
                }
            }
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
        }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
            SetStage(0);
        }
    }


}