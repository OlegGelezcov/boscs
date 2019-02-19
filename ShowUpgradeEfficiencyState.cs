namespace Bos {
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public class ShowUpgradeEfficiencyState : TutorialState {

        public const int START_STAGE = 0;
        public const int DIALOG_FIRST = 10;
        public const int FINGER_ON_UPGRADE_ROLLBACK_BUTTON = 20;
        public const int FINGER_ON_UPGRADE_EFFICIENCY_BUTTON = 30;
        public const int END_DIALOG = 40;

        private const int RICKSHAW_MANAGER_ID = 0;

        private readonly string rollbackFingerPosition = "add_rollback";
        private readonly string efficiencyFingerPosition = "add_efficiency";

        private readonly HighlightParams rollbackHighlightParams =
            HighlightParams.CreateDefaultAnchored(new Vector2(284, -125), new Vector2(700, 400));
        private readonly HighlightParams efficiencyHighlightParams =
            HighlightParams.CreateDefaultAnchored(new Vector2(284, 135.5f), new Vector2(700, 400));


        private IBosServiceCollection services;
        private float updateTimer = 0f;

        public ShowUpgradeEfficiencyState(IBosServiceCollection services) {
            this.services = services;

            AddLocked(FINGER_ON_UPGRADE_ROLLBACK_BUTTON, new List<string> {
                "currency", "account", "garage", "office", "close", "slide", "addeff", "stuff"
            });
            AddLocked(FINGER_ON_UPGRADE_EFFICIENCY_BUTTON, new List<string> {
                "currency", "account", "garage", "office", "close", "slide", "addrollback", "stuff"
            });

            TutorialStageObservable.Subscribe(stage => {
                switch(stage) {
                    case DIALOG_FIRST: {
                            services.ExecuteWhen(() => {
                                ForceTutorialDialog(services, new TutorialDialogData {
                                    Texts = new List<string> {
                                services.ResourceService.Localization.GetString("lbl_tut_55")
                              },
                                    OnOk = () => {
                                        SetStage(FINGER_ON_UPGRADE_ROLLBACK_BUTTON);
                                    }
                                });
                            }, () => IsValid(services));
                        }
                        break;
                    case FINGER_ON_UPGRADE_ROLLBACK_BUTTON: {
                            services.Execute(() => {
                                services.TutorialService.CreateHighlightRegion(rollbackHighlightParams);
                                Finger(services, rollbackFingerPosition, 20);
                                int nextRollbackLevel = services.ManagerService.GetNextRollbackLevel(managerId: 0);
                                var coins = services.ResourceService.ManagerImprovements.GetRollbackImproveData(level: nextRollbackLevel).CoinsPrice;
                                services.PlayerService.AddCoins(coins);
                            }, 0.5f);
                        }
                        break;
                    case FINGER_ON_UPGRADE_EFFICIENCY_BUTTON: {
                            services.Execute(() => {
                                services.TutorialService.CreateHighlightRegion(efficiencyHighlightParams);
                                Finger(services, efficiencyFingerPosition, 20);
                                int nextEfficiencyLevel = services.ManagerService.GetNextEfficiencyLevel(0);
                                var coins = services.ResourceService.ManagerImprovements.GetEfficiencyImproveData(nextEfficiencyLevel).CoinsPrice;
                                services.PlayerService.AddCoins(coins);
                            }, 1);
                        }
                        break;
                    case END_DIALOG: {
                            ForceTutorialDialog(services, new TutorialDialogData {
                                Texts = new List<string> {
                                     services.ResourceService.Localization.GetString("lbl_tut_56")
                                 },
                                OnOk = () => {
                                    SetCompleted(services, true);
                                }
                            });
                        }
                        break;
                }
            }).AddTo(services.Disposables);

            
        }

        public override void OnEfficiencyLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel level) {
            base.OnEfficiencyLevelChanged(oldLevel, newLevel, level);
            if (IsActive) {
                RemoveFinger(services, efficiencyFingerPosition);
                services.TutorialService.RemoveHighlightRegion();
                SetStage(END_DIALOG, 0.7f);
            }
        }

        public override void OnRollbackLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel level) {
            base.OnRollbackLevelChanged(oldLevel, newLevel, level);
            if (Stage == FINGER_ON_UPGRADE_ROLLBACK_BUTTON) {
                RemoveFinger(services, rollbackFingerPosition);
                services.TutorialService.RemoveHighlightRegion();
                SetStage(FINGER_ON_UPGRADE_EFFICIENCY_BUTTON);
            }
        }

        public override TutorialStateName Name
            => TutorialStateName.ShowUpgradeEfficiencyState;


        public override bool IsValid(IBosServiceCollection context) {
            bool preCondition =  context.TutorialService.IsStateCompleted(TutorialStateName.MakeRollbackState) &&
                context.ViewService.Exists(UI.ViewType.ManagementView);

            var view = context.ViewService.FindView<ManagementView>(ViewType.ManagementView);
            bool isManagerTab = false;
            if(view != null && view.CurrentActiveTab == ManagementView.ActiveTab.Office && view.ManagerId == 0) {
                isManagerTab = true;
            }
            return preCondition && isManagerTab;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"MakeRollbackState Coompleted: {services.TutorialService.IsStateCompleted(TutorialStateName.MakeRollbackState)}");
            sb.AppendLine($"Exists view: {services.ViewService.Exists(ViewType.ManagementView)}");
            var view = services.ViewService.FindView<ManagementView>(ViewType.ManagementView);
            sb.AppendLine($"Management view != null: {view != null}");
            if(view != null)
            {
                sb.AppendLine($"Current tab of management view is Office: {view.CurrentActiveTab == ManagementView.ActiveTab.Office}");
                sb.AppendLine($"We are on generator tab: {view.ManagerId == 0}");
            }
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {
                if (Stage == START_STAGE) {
                    SetStage(DIALOG_FIRST);
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {

        }

        public override void OnUpdate(IBosServiceCollection services, float deltaTime) {

            updateTimer += deltaTime;

            if (updateTimer >= 0.5f) {
                updateTimer -= 0.5f;

                //fix error when don't enough coins to rollback tutorial
                if (IsActive && Stage == FINGER_ON_UPGRADE_ROLLBACK_BUTTON) {
                    if (services.ViewService.Exists(ViewType.ManagementView)) {
                        var view = services.ViewService.FindView<ManagementView>(ViewType.ManagementView);
                        if (view != null) {
                            if (view.CurrentActiveTab == ManagementView.ActiveTab.Office && view.ManagerId == RICKSHAW_MANAGER_ID) {
                                int nextRollbackLevel = services.ManagerService.GetNextRollbackLevel(managerId: RICKSHAW_MANAGER_ID);
                                var coins = services.ResourceService.ManagerImprovements.GetRollbackImproveData(level: nextRollbackLevel).CoinsPrice;
                                if (services.PlayerService.Coins < coins) {
                                    services.PlayerService.AddCoins(coins);
                                }
                            }
                        }
                    }
                }

                if (IsActive && Stage == FINGER_ON_UPGRADE_EFFICIENCY_BUTTON) {
                    if (services.ViewService.Exists(ViewType.ManagementView)) {
                        var view = services.ViewService.FindView<ManagementView>(ViewType.ManagementView);
                        if (view != null) {
                            if (view.CurrentActiveTab == ManagementView.ActiveTab.Office && view.ManagerId == RICKSHAW_MANAGER_ID) {
                                int nextEfficiencyLevel = services.ManagerService.GetNextEfficiencyLevel(managerId: RICKSHAW_MANAGER_ID);
                                var coins = services.ResourceService.ManagerImprovements.GetEfficiencyImproveData(nextEfficiencyLevel).CoinsPrice;
                                if (services.PlayerService.Coins < coins) {
                                    services.PlayerService.AddCoins(coins);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) {

        }
    }

}