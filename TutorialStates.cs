namespace Bos {
    using Bos.Debug;
    using Bos.UI;
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using UDBG = UnityEngine.Debug;





    public class NoneTutorialState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.None;

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {
                context.GetService<IConsoleService>().AddOnScreenText($"enter tutorial state => {GetType().Name}");
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
        }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {

        }

        public override bool IsValid(IBosServiceCollection context) {
            return false;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Always invalid...");
            return sb.ToString();
        }

        public override bool IsActive => false;
        public override bool IsCompleted => true;

        public override TutorialStateSave GetSave() {
            return new TutorialStateSave {
                isActive = false,
                isCompleted = true,
                stateName = Name
            };
        }

        public override void Load(TutorialStateSave save) {
            
        }

        public override void Reset() {
            
        }
    }

    public class HelloTextState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.HelloText;

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {
                context.GetService<IConsoleService>().AddOnScreenText($"enter tutorial state => {GetType().Name}");

                context.ViewService.Show(UI.ViewType.TutorialDialogView, () => {
                    return IsStandardDialogCondition(context);
                }, go => { },
                new UI.ViewData {
                    UserData = new TutorialDialogData {
                        Texts = context.ResourceService.Localization.GetString("FIRSTTIME.TITLE").WrapToList(),
                        OnOk = () => {
                            context.TutorialService.CompleteState(Name);
                        }
                    }
                });
                IsOnEntered = true;
            }
        }



        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) { }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) { }

        public override bool IsValid(IBosServiceCollection context) {
            return context.GameModeService.GameModeName == GameModeName.Game;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is Game Mode(gamemodename == game): {services.GameModeService.GameModeName == GameModeName.Game}");
            return sb.ToString();
        }
    }

    public class HintBuyRickshawState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.HintBuyRickshaw;

        private const string kPositionObjectName = "rickshaw_unlock";

        public override void OnEnter(IBosServiceCollection context) {
            if (context.TransportService.HasUnits(0)) {
                ForceSetCompleted();
                return;
            }
            if (!IsOnEntered) {


                context.GetService<IConsoleService>().AddOnScreenText($"enter tutorial state => {GetType().Name}");
                var tutorialObject = context.TutorialService.FindPositionObject(kPositionObjectName);
                if (tutorialObject != null) {
                    context.TutorialService.CreateFinger(tutorialObject.transform, new TutorialFingerData {
                        Id = kPositionObjectName,
                        IsTooltipVisible = true,
                        Position = Vector2.zero,
                        TooltipWidth = 575,
                        TooltipPosition = new Vector2(-710, -47),
                        TooltipText = context.ResourceService.Localization.GetString("lbl_tut_1"),
                        TooltipScale = new Vector3(1.18f, 1.18f, 1)
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (data.EventName == TutorialEventName.UnitCountChanged) {
                    context.TutorialService.RemoveFinger(kPositionObjectName);
                    context.Execute(() => {
                        CompleteSelf(context);
                    }, 0.2f);
                }
            }
        }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.HelloText)
                && !context.TutorialService.IsPaused ;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is State 'HelloText' completed: {services.TutorialService.IsStateCompleted(TutorialStateName.HelloText)}");
            sb.AppendLine($"TutorialService not paused: {false == services.TutorialService.IsPaused}");
            return sb.ToString();
        }

        protected override void OnExit(IBosServiceCollection context) {

        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {

        }
    }

    public class ClickGenerateRickshawState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.ClickGenerateRickshaw;

        private const string kGenerateRickshawPositionObjectName = "generate_rickshaw";
        private const int kRickshawGeneratorId = 0;

        public override void OnEnter(IBosServiceCollection context) {

            if(context.GenerationService.GetGetenerator(0).IsAutomatic ) {
                ForceSetCompleted();
                return;
            }

            if (!IsOnEntered) {
                context.GetService<IConsoleService>().AddOnScreenText($"enter tutorial state => {GetType().Name}");
                var positionObject = context.TutorialService.FindPositionObject(kGenerateRickshawPositionObjectName);
                if (positionObject != null) {
                    context.TutorialService.CreateFinger(positionObject.transform, new TutorialFingerData {
                        Id = kGenerateRickshawPositionObjectName,
                        IsTooltipVisible = true,
                        Position = Vector2.zero,
                        TooltipPosition = new Vector2(-257, -97),
                        TooltipScale = new Vector3(1.18f, 1.18f, 1),
                        TooltipWidth = 500,
                        TooltipText = context.ResourceService.Localization.GetString("lbl_tut_2")
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (data.EventName == TutorialEventName.GenerationButtonClicked) {
                    int generatorId = (int)data.UserData;
                    if (generatorId == kRickshawGeneratorId) {
                        context.TutorialService.RemoveFinger(kGenerateRickshawPositionObjectName);
                        context.Execute(() => {
                            CompleteSelf(context);
                        }, .3f);
                    }
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) { }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.HintBuyRickshaw);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is TutorialStateName.HintBuyRickshaw is completed: {services.TutorialService.IsStateCompleted(TutorialStateName.HintBuyRickshaw)}");
            return sb.ToString();
        }

    }

    public class WaitForCashOnSecondRickshawState : TutorialState {

        private const string kBuyRickshawPositionObjectName = "buy_rickshaw";

        public override TutorialStateName Name => TutorialStateName.WaitForCashOnSecondRickshaw;

        public override void OnEnter(IBosServiceCollection context) {
            if(context.TransportService.GetUnitTotalCount(0) > 1) {
                ForceSetCompleted();
                return;
            }
            if(context.ManagerService.IsHired(0)) {
                ForceSetCompleted();
                return;
            }
            IsOnEntered = true;
        }

        private bool isDialogWasShowed = false;

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (data.EventName == TutorialEventName.CopanyCashChanged) {
                    double cash = (double)data.UserData;
                    double price = context.GenerationService.CalculatePrice(1, context.TransportService.GetUnitTotalCount(0), context.GenerationService.GetGetenerator(0));

                    if (cash >= price) {
                        if (!isDialogWasShowed) {
                            context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                                UserData = new TutorialDialogData {
                                    Texts = context.ResourceService.Localization.GetString("lbl_tut_3").WrapToList(),
                                    OnOk = () => {
                                        var positionObject = context.TutorialService.FindPositionObject(kBuyRickshawPositionObjectName);
                                        if (positionObject) {
                                            context.TutorialService.CreateFinger(positionObject.transform, new TutorialFingerData {
                                                Id = kBuyRickshawPositionObjectName,
                                                IsTooltipVisible = false,
                                                Position = Vector2.zero,
                                            });
                                        }
                                    }
                                }
                            });
                            isDialogWasShowed = true;
                        }
                    }
                } else if (data.EventName == TutorialEventName.UnitCountChanged) {
                    TransportUnitInfo unit = data.UserData as TransportUnitInfo;
                    if (unit.GeneratorId == 0 && unit.TotalCount >= 2) {
                        context.TutorialService.RemoveFinger(kBuyRickshawPositionObjectName);
                        context.Execute(() => CompleteSelf(context), 0.3f);
                    }
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) { }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.ClickGenerateRickshaw);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is TutorialStateName.ClickGenerateRickshaw completed: {services.TutorialService.IsStateCompleted(TutorialStateName.ClickGenerateRickshaw)}");
            return sb.ToString();
        }

        public override void Reset() {
            base.Reset();
            isDialogWasShowed = false;
        }
    }

    public class WaitCashForFirstManagerState : TutorialState {

        private bool isDialogShowed = false;

        private const string kBuyManagerPositionObjectName = "buy_manager";
        private const string kBuyManagerButtonPositionObjectName = "buy_manager_button";

        public override TutorialStateName Name => TutorialStateName.WaitCashForFirstManager;


        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public override void OnEnter(IBosServiceCollection context) {
            if(context.ManagerService.IsHired(0)) {
                ForceSetCompleted();
                return;
            }

            if (!IsOnEntered) {
                updateTimer.Setup(1, dt => {
                    if(IsActive) {
                        if(context.ViewService.LegacyCount == 0 && 
                        context.ViewService.ModalCount == 0 ) {
                            if(context.ManagerService.IsHired(0)) {
                                CompleteSelf(context);
                            }
                        }
                    }
                });
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (data.EventName == TutorialEventName.CopanyCashChanged) {
                    if (!isDialogShowed) {
                        double rickshawManagerPrice = context.ManagerService.GetManagerPrice(0);
                        double cash = (double)data.UserData;
                        if (cash >= rickshawManagerPrice) {
                            isDialogShowed = true;

                            ShowTutorialDialog(context, new TutorialDialogData {
                                Texts = context.ResourceService.Localization.GetString("lbl_tut_4").WrapToList(),
                                OnOk = () => {
                                    context.TutorialService.CreateFinger(kBuyManagerPositionObjectName, new TutorialFingerData {
                                        Id = kBuyManagerPositionObjectName,
                                        IsTooltipVisible = true,
                                        Position = Vector2.zero,
                                        TooltipPosition = new Vector2(78, -43),
                                        TooltipScale = new Vector3(1.18f, 1.18f),
                                        TooltipText = context.ResourceService.Localization.GetString("lbl_tut_5"),
                                        TooltipWidth = 500,
                                        Timeout = 7
                                    });
                                }
                            });
                        }
                    }
                } else if (data.EventName == TutorialEventName.ViewOpened) {
                    ViewType viewType = (ViewType)data.UserData;
                    if (viewType == ViewType.ManagementView && isDialogShowed) {
                        context.TutorialService.RemoveFinger(kBuyManagerPositionObjectName);
                    }
                } else if (data.EventName == TutorialEventName.ManagementViewOpenedForManager) {
                    int managerId = (int)data.UserData;
                    UnityEngine.Debug.Log($"management view opened => {managerId}");
                    if (managerId == 0) {
                        if (!context.ManagerService.IsHired(0)) {
                            if (isDialogShowed) {
                                double price = context.ManagerService.GetManagerPrice(0);
                                if (context.PlayerService.CompanyCash.Value >= price) {
                                    UnityEngine.Debug.Log($"try show finger => {kBuyManagerButtonPositionObjectName}");
                                    context.TutorialService.CreateFinger(kBuyManagerButtonPositionObjectName, new TutorialFingerData {
                                        Id = kBuyManagerButtonPositionObjectName,
                                        IsTooltipVisible = true,
                                        Position = Vector2.zero,
                                        TooltipPosition = new Vector2(-250, 135),
                                        TooltipScale = Vector3.one,
                                        TooltipText = context.ResourceService.Localization.GetString("lbl_tut_6"),
                                        TooltipWidth = 500
                                    });
                                }
                            }
                        }
                    }
                } else if (data.EventName == TutorialEventName.ManagerHired) {
                    //int managerId = (int)data.UserData;
                    //if (managerId == 0) {

                    //}
                    context.TutorialService.RemoveFinger(kBuyManagerButtonPositionObjectName);
                    context.Execute(() => CompleteSelf(context), .5f);
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if(IsActive) {
                updateTimer.Update(deltaTime);
            }
        }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.WaitForCashOnSecondRickshaw);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is TutorialStateName.WaitForCashOnSecondRickshaw completed: {services.TutorialService.IsStateCompleted(TutorialStateName.WaitForCashOnSecondRickshaw)}");
            return sb.ToString();
        }
    }

    public class MakeRollbackState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.MakeRollbackState;

        private bool isDialogShowed = false;
        private const string kMakeRollbackPositionName = "make_rollback";

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {
                
                if(context.TutorialService.IsWasAnyRollback) {
                    ForceSetCompleted();
                    return;
                }

                

                updateTimer.Setup(.5f, dt => {
                    if (!isDialogShowed) {
                        TryShowDialog(context);
                    }
                }, false);

                TryShowDialog(context);
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (data.EventName == TutorialEventName.TutorialPositionObjectActivated) {
                    string positionName = (string)data.UserData;
                    if (positionName == kMakeRollbackPositionName) {
                        var managerView = GameObject.FindObjectOfType<ManagerView>();
                        if(managerView == null ) {
                            ForceSetCompleted();
                            return;
                        } else {
                            if (!context.ManagerService.IsRallbackAllowed(managerView.ManagerId)) {
                                ForceSetCompleted();
                                return;
                            }
                        }
                        TryShowDialog(context);
                    }
                } else if (data.EventName == TutorialEventName.ManagerRollback) {

                    context.TutorialService.RemoveFinger(kMakeRollbackPositionName);
                    context.Execute(() => CompleteSelf(context), 0.5f);
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if (IsActive) {
                updateTimer.Update(deltaTime);
            }
        }

        private void TryShowDialog(IBosServiceCollection context) {
            if (!isDialogShowed) {
                if (context.TutorialService.IsHasTutorialPositionObject(kMakeRollbackPositionName)) {
                    if (context.ManagerService.IsHired(0) && !context.TutorialService.IsWasAnyRollback) {
                        isDialogShowed = true;
                        context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                            UserData = new TutorialDialogData {
                                Texts = context.ResourceService.Localization.GetString("lbl_tut_7").WrapToList(),
                                OnOk = () => {
                                    context.TutorialService.CreateFinger(kMakeRollbackPositionName, new TutorialFingerData {
                                        Id = kMakeRollbackPositionName,
                                        IsTooltipVisible = true,
                                        Position = Vector2.zero,
                                        TooltipPosition = new Vector2(-250, 147),
                                        TooltipScale = Vector3.one,
                                        TooltipText = context.ResourceService.Localization.GetString("lbl_tut_8"),
                                        TooltipWidth = 500
                                    });
                                }
                            }
                        });
                    }
                }
            }
        }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.WaitCashForFirstManager);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is TutorialStateName.WaitCashForFirstManager completed: {services.TutorialService.IsStateCompleted(TutorialStateName.WaitCashForFirstManager)}");
            return sb.ToString();
        }
    }

    public class BuyTaxiState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.BuyTaxi;

        private const int kTaxiGeneratorId = 1;
        private bool isDialogShowed = false;
        private const string kPositionNameUnlockTaxy = "taxi_unlock";

        private readonly UpdateTimer timer = new UpdateTimer();

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {

                if(context.TransportService.GetUnitTotalCount(kTaxiGeneratorId) > 0) {
                    ForceSetCompleted();
                    return;
                }

                timer.Setup(.5f, dt => {

                    bool isNeedShow = false;
                    if (!isDialogShowed) {
                        if (context.GameModeService.GameModeName == GameModeName.Game) {
                            if (context.ViewService.ModalCount == 0) {
                                double taxiUnlockPrice = context.GenerationService.GetGeneratorUnlockPrice(generatorId: kTaxiGeneratorId);
                                double cash = context.PlayerService.CompanyCash.Value;
                                if (cash >= taxiUnlockPrice) {
                                    isNeedShow = true;
                                }
                            }
                        }
                    }

                    if (!isDialogShowed) {
                        if (isNeedShow) {
                            isDialogShowed = true;
                            context.ViewService.Show(ViewType.TutorialDialogView, () => { return IsStandardDialogCondition(context); },
                                (go) => {
                                    UnityEngine.Debug.Log($"view => {go.name} showed");
                                }, new ViewData {
                                    UserData = new TutorialDialogData {
                                        Texts = context.ResourceService.Localization.GetString("lbl_tut_9").WrapToList(),
                                        OnOk = () => {
                                        //show finger
                                        //and go to next stage
                                        context.TutorialService.CreateFinger(kPositionNameUnlockTaxy, new TutorialFingerData {
                                                Id = kPositionNameUnlockTaxy,
                                                IsTooltipVisible = true,
                                                Position = Vector2.zero,
                                                TooltipPosition = new Vector2(-533, -95),
                                                TooltipScale = new Vector3(1.18f, 1.18f, 1),
                                                TooltipText = context.ResourceService.Localization.GetString("lbl_tut_10"),
                                                TooltipWidth = 500
                                            });
                                        }
                                    }
                                });
                        }
                    }
                });


                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (data.EventName == TutorialEventName.UnitCountChanged) {
                    if (isDialogShowed) {
                        int generatorId = (data.UserData as TransportUnitInfo).GeneratorId;
                        if (generatorId == kTaxiGeneratorId) {
                            context.TutorialService.RemoveFinger(kPositionNameUnlockTaxy);
                            //change state
                            CompleteSelf(context);
                        }
                    }
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) {

        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if (IsActive) {
                timer.Update(deltaTime);
            }
        }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.ShowUpgradeEfficiencyState); //&&
                //context.TutorialService.IsStateCompleted(TutorialStateName.SpaceShip);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Is TutorialStateName.ShowUpgradeEfficiencyState completed: {services.TutorialService.IsStateCompleted(TutorialStateName.ShowUpgradeEfficiencyState)}");
            sb.AppendLine($"Is TutorialStateName.SpaceShip completed: {services.TutorialService.IsStateCompleted(TutorialStateName.SpaceShip)}");
            return sb.ToString();
        }
    }

    public class BuyBusState : TutorialState {

        private const int kBusId = 2;

        public override TutorialStateName Name => TutorialStateName.BuyBus;
        private const string kResearchBusPosition = "research_bus";
        private const string kUnlockBusPosition = "unlock_bus";

        private bool isDialogShowed = false;


        public override bool IsValid(IBosServiceCollection context) {
            //valid if  enough cah for unlock and enough coins for research
            bool isResearchEnoughCoins = context.ResourceService.GeneratorLocalData.GetLocalData(kBusId).GetResearchPrice(
                context.PlanetService.CurrentPlanetId.Id).Match(() => false, pr => context.PlayerService.Coins >= pr.price);

            double cash = context.PlayerService.CompanyCash.Value;
            double price = context.GenerationService.GetGeneratorUnlockPrice(kBusId);
            if (cash >= price && isResearchEnoughCoins) {
                if (context.TutorialService.IsStateCompleted(TutorialStateName.BuyTaxi)) {
                    if (context.TransportService.HasUnits(1)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            bool isResearchEnoughCoins = services.ResourceService.GeneratorLocalData.GetLocalData(kBusId).GetResearchPrice(
                services.PlanetService.CurrentPlanetId.Id).Match(() => false, pr => services.PlayerService.Coins >= pr.price);
            double cash = services.PlayerService.CompanyCash.Value;
            double price = services.GenerationService.GetGeneratorUnlockPrice(kBusId);
            bool isEnoughCashForUnlockBus = cash >= price;
            bool isBuyTaxiStateCompleted = services.TutorialService.IsStateCompleted(TutorialStateName.BuyTaxi);
            bool isTaxiHasUnits = services.TransportService.HasUnits(1);

            sb.AppendLine($"Is enough coins for research bus: {isResearchEnoughCoins}");
            sb.AppendLine($"Is enough cash for unlock bus: {isEnoughCashForUnlockBus}");
            sb.AppendLine($"Is TutorialStateName.BuyTaxi completed: {isBuyTaxiStateCompleted}");
            sb.AppendLine($"Is has taxi units: {isTaxiHasUnits}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {

                if(context.GenerationService.IsResearched(kBusId)) {
                    ForceSetCompleted();
                    return;
                }


                if (!isDialogShowed) {
                    isDialogShowed = true;

                    context.ViewService.Show(ViewType.TutorialDialogView,
                    () => {
                        return IsStandardDialogCondition(context);

                    }, (viewObj) => {

                    }, new ViewData {
                        UserData = new TutorialDialogData {
                            Texts = context.ResourceService.Localization.GetString("lbl_tut_16").WrapToList(),
                            OnOk = () => {
                                context.TutorialService.CreateFinger(kResearchBusPosition, new TutorialFingerData {
                                    Id = kResearchBusPosition,
                                    IsTooltipVisible = true,
                                    Position = Vector2.zero,
                                    TooltipPosition = new Vector2(-705, -47),
                                    TooltipScale = new Vector3(1.34f, 1.34f),
                                    TooltipWidth = 500,
                                    TooltipText = context.ResourceService.Localization.GetString("lbl_tut_17")
                                });
                            }
                        }
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if (IsActive) {
                if (isDialogShowed) {
                    if (data.EventName == TutorialEventName.GeneratorResearched) {
                        GeneratorInfo generator = data.UserData as GeneratorInfo;
                        if (generator.GeneratorId == kBusId) {
                            context.TutorialService.RemoveFinger(kResearchBusPosition);
                            context.TutorialService.CreateFinger(kUnlockBusPosition, new TutorialFingerData {
                                Id = kUnlockBusPosition,
                                IsTooltipVisible = true,
                                Position = Vector2.zero,
                                TooltipPosition = new Vector2(-705, -47),
                                TooltipScale = new Vector3(1.34f, 1.34f, 1),
                                TooltipWidth = 500,
                                TooltipText = context.ResourceService.Localization.GetString("lbl_tut_18")
                            });
                        }
                    } else if (data.EventName == TutorialEventName.UnitCountChanged) {
                        var unit = data.UserData as TransportUnitInfo;
                        if (unit.GeneratorId == kBusId) {
                            context.TutorialService.RemoveFinger(kUnlockBusPosition);
                            CompleteSelf(context);
                        }
                    }
                }
            }
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) { }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }
    }




    public class BuyUpgradeState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.BuyUpgrade;

        private const string kEmpirePosition = "empire_button";
        private const string kUpgradeButtonPosition = "upgrade_button";

        private bool isDialogShowed = false;

        public override bool IsValid(IBosServiceCollection context) {
            return context.UpgradeService.HasAvailableUpgrades && 
                (context.TutorialService.IsStateCompleted(TutorialStateName.PlaySlot));
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Has available upgrades: {services.UpgradeService.HasAvailableUpgrades}");
            sb.AppendLine($"Is TutorialStateName.PlaySlot completed: {services.TutorialService.IsStateCompleted(TutorialStateName.PlaySlot)}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                if (!isDialogShowed) {
                    isDialogShowed = true;
                    UDBG.Log($"OnEnter() => {Name}".Colored(ConsoleTextColor.lightblue));
                    context.ViewService.Show(ViewType.TutorialDialogView, () => {
                        return IsStandardDialogCondition(context);
                    }, (viewObj) => {
                        UDBG.Log($"opened => {viewObj.name}".Colored(ConsoleTextColor.lightblue));
                    }, new ViewData {
                        UserData = new TutorialDialogData {
                            Texts = context.ResourceService.Localization.GetString("lbl_tut_12").WrapToList(),
                            OnOk = () => {
                                context.TutorialService.CreateFinger(kEmpirePosition, new TutorialFingerData {
                                    Id = kEmpirePosition,
                                    IsTooltipVisible = false,
                                    Position = Vector2.zero
                                });
                            }
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
                        ViewType viewType = (ViewType)data.UserData;
                        if(viewType == ViewType.MainView ) {
                            context.TutorialService.RemoveFinger(kEmpirePosition);
                            context.TutorialService.CreateFinger(kUpgradeButtonPosition, new TutorialFingerData {
                                Id = kUpgradeButtonPosition,
                                IsTooltipVisible = false,
                                Position = Vector2.zero
                            });
                        } else if(viewType == ViewType.UpgradesView ) {
                            context.TutorialService.RemoveFinger(kUpgradeButtonPosition);
                            context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                                UserData = new TutorialDialogData {
                                    Texts = context.ResourceService.Localization.GetString("lbl_tut_13").WrapToList(),
                                    OnOk = () => {
                                        CompleteSelf(context);
                                    }
                                }
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

    public class DailyBonusState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.DailyBonus;
        private bool isDialogShowed = false;
        private const string kDailyBonusPosition = "daily_bonus";

        public override bool IsValid(IBosServiceCollection context) {
            return (!context.TutorialService.IsAnyDailyBonusClaimed) &&
                context.TutorialService.IsStateCompleted(TutorialStateName.BuyUpgrade); 

        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.BuyUpgrade)
                .AppendLine($"Is not claimed any daily bonus: {false == services.TutorialService.IsAnyDailyBonusClaimed}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {

                if(!isDialogShowed) {
                    isDialogShowed = true;
                    context.ViewService.Show(ViewType.TutorialDialogView, () => {
                        return IsStandardDialogCondition(context);
                    }, (viewObj) => {
                        UDBG.Log($"OnEnter(): {Name} => {viewObj.name}");
                    }, new ViewData {
                        UserData = new TutorialDialogData {
                             Texts = context.ResourceService.Localization.GetString("lbl_tut_14").WrapToList(),
                             OnOk = () => {
                                 context.TutorialService.CreateFinger(kDailyBonusPosition, new TutorialFingerData {
                                     Id = kDailyBonusPosition,
                                     IsTooltipVisible = true,
                                     Position = Vector2.zero,
                                     TooltipText = context.ResourceService.Localization.GetString("lbl_tut_15"),
                                     TooltipPosition = new Vector2(-552, 95),
                                     TooltipScale = Vector3.one,
                                     TooltipWidth = 500
                                 });
                             }
                        }
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if(IsActive) {
                if(isDialogShowed ) {
                    if(data.EventName == TutorialEventName.DailyBonusOpened) {
                        context.TutorialService.RemoveFinger(kDailyBonusPosition);
                        CompleteSelf(context);
                    }
                }
            }
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) { }

        protected override void OnExit(IBosServiceCollection context) { }

        public override void Reset() {
            base.Reset();
            isDialogShowed = false;
        }
    }



    /*
    public class TransferPersonalCashState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.TransferPersonalCash;

        private bool isFirstDialogShowed = false;
        private bool isSecondDialogShowed = false;

        private const string kPositionProfile = "profile";
        private const string kPositionTransferTab = "transfer_tab";

        public override bool IsValid(IBosServiceCollection context) {
            //return context.TutorialService.IsWasSoldToInvestors;
            return context.PlayerService.CompanyCash.Value > 1e6 && context.TutorialService.IsStateCompleted(TutorialStateName.BuyBus);
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                if(!isFirstDialogShowed) {
                    isFirstDialogShowed = true;
                    context.Execute(() => {
                        context.ViewService.Show(ViewType.TutorialDialogView,
                            () => {
                                return (context.ViewService.LegacyCount == 0) &&
                                (context.ViewService.ModalCount == 0) &&
                                (!context.ViewService.Exists(ViewType.TutorialDialogView));
                            },
                            (viewObj) => { },
                            new ViewData {
                                UserData = new TutorialDialogData {
                                     Texts = new List<string> {
                                         context.ResourceService.Localization.GetString("lbl_tut_21"),
                                         context.ResourceService.Localization.GetString("lbl_tut_22")
                                     },
                                     OnOk = () => {
                                         context.TutorialService.CreateFinger(kPositionProfile, new TutorialFingerData {
                                             Id = kPositionProfile,
                                             IsTooltipVisible = true,
                                             Position = Vector2.zero,
                                             TooltipText = context.ResourceService.Localization.GetString("lbl_tut_23"),
                                             TooltipPosition = new Vector2(74, -81),
                                             TooltipScale = new Vector3(1.34f, 1.34f, 1),
                                             TooltipWidth = 500
                                         });
                                     }
                                }
                            });
                    }, 15f);
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if(IsActive) {
                if(data.EventName == TutorialEventName.ViewOpened) {
                    ViewType viewType = (ViewType)data.UserData;
                    if(viewType== ViewType.ProfileView) {
                        if(isFirstDialogShowed) {
                            context.TutorialService.RemoveFinger(kPositionProfile);
                            context.TutorialService.CreateFinger(kPositionTransferTab, new TutorialFingerData {
                                Id = kPositionTransferTab,
                                IsTooltipVisible = true,
                                Position = Vector2.zero,
                                TooltipText = context.ResourceService.Localization.GetString("lbl_tut_24"),
                                TooltipPosition = new Vector2(-535, -27),
                                TooltipScale = Vector3.one,
                                TooltipWidth = 500
                            });
                        }
                    }
                } else if(data.EventName == TutorialEventName.TransferTabOpened ) {
                    if(isFirstDialogShowed && !isSecondDialogShowed) {
                        isSecondDialogShowed = true;
                        context.TutorialService.RemoveFinger(kPositionTransferTab);
                        context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                            UserData = new TutorialDialogData {
                                Texts = new List<string> {
                                     context.ResourceService.Localization.GetString("lbl_tut_25"),
                                     context.ResourceService.Localization.GetString("lbl_tut_26")
                                 },
                                OnOk = () => {
                                    CompleteSelf(context);
                                }
                            }
                        });
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
            isFirstDialogShowed = false;
            isSecondDialogShowed = false;
        }
    }
    */

        /*
    public class BuyPersonalProductState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.BuyPersonalProduct;

        private bool isDialogShowed = false;
        private readonly string positionStatusGoods = "status_goods";

        public override bool IsValid(IBosServiceCollection context) {

            //condition: was any transfer and enough player cash for any personal product...

            bool isValid = context.TutorialService.IsWasTransfer;
            bool hasProduct = context.PlayerService.AvailablePersonalProduct.Match(() => false, p => true);
            return context.TutorialService.IsWasTransfer && hasProduct;
        }

        public override void OnEnter(IBosServiceCollection context) {

            if(!IsOnEntered) {
                if(!isDialogShowed) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_27", "lbl_tut_28"),
                        OnOk = () => {
                            Finger(context, "profile", 5);
                        }
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            //if already has purchased product - we force complete this step
            if(context.PlayerService.HasPurchasedProducts()) {
                ForceSetCompleted();
                return;
            }

            if(IsActive) {
                if(isDialogShowed) {
                    if(data.EventName == TutorialEventName.ViewOpened) {
                        if((ViewType)data.UserData == ViewType.ProfileView ) {
                            RemoveFinger(context, "profile");
                            Finger(context, positionStatusGoods);
                        }
                    } else if(data.EventName == TutorialEventName.StatusGoodsOpened ) {
                        RemoveFinger(context, positionStatusGoods);
                        ForceTutorialDialog(context, new TutorialDialogData {
                            Texts = GetLocalizationStrings(context, "lbl_tut_29"),
                            OnOk = () => CompleteSelf(context)
                        });
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
    }*/


    public class EnhanceManagerState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.TransportInfoView;

        private bool isDialogShowed = false;
        private float timeoutTimer = 0;

        private readonly UpdateTimer guardTimer = new UpdateTimer();

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.BuyBus);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.BuyBus);
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                if (!isDialogShowed) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_30"),
                        OnOk = () => {
                            Finger(context, "enhance",
                                context.ResourceService.Localization.GetString("lbl_tut_31"),
                                new Vector2(88, -55), 1.34f, 500, 10);
                            
                        }
                    });

                    timeoutTimer = 0f;
                    guardTimer.Setup(1, dt => {
                        if (IsActive) {
                            if (isDialogShowed) {
                                timeoutTimer += dt;
                                if(timeoutTimer > 10 ) {
                                    CompleteSelf(context);
                                }
                            }
                        }
                    });

                    IsOnEntered = true;
                }
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {

            BosUtils.If(
                () => IsActive && isDialogShowed && data.EventName == TutorialEventName.ViewOpened,
                trueAction: () => {
                    if ((ViewType)data.UserData == ViewType.TransportInfoView) {
                        RemoveFinger(context, "enhance");
                        CompleteSelf(context);
                    }
                },
                falseAction: () => { });


        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if(IsActive) {
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

    public class BankState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.Bank;

        private bool isDialogShowed = false;

        private readonly UpdateTimer guardTimer = new UpdateTimer();

        public override bool IsValid(IBosServiceCollection context) {
            bool isValid =  context.TutorialService.IsStateCompleted(TutorialStateName.ShowTransferAnsPersonalPurchasesState) &&
                context.TutorialService.IsStateCompleted(TutorialStateName.PlaySlot);

            //UDBG.Log($"{nameof(BankState)} is valid: {isValid}".Colored(ConsoleTextColor.lightblue));
            return isValid;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.ShowTransferAnsPersonalPurchasesState)
                .AppendTutorialStateCompletedCondition(TutorialStateName.PlaySlot);
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                if(!isDialogShowed) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_32"),
                        OnOk = () => {
                            Finger(context, "bank", context.ResourceService.Localization.GetString("lbl_tut_33"), new Vector2(67, 212), 1, 500, 10);
                        }
                    });
                }

                guardTimer.Setup(1, dt => {
                    if (IsActive) {
                        if (context.BankService.IsOpened) {
                            RemoveFinger(context, "bank");
                            CompleteSelf(context);
                        }
                    }
                });

                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            BosUtils.If(() => IsActive && isDialogShowed && data.EventName == TutorialEventName.ViewOpened,
                trueAction: () => {
                    if ((ViewType)data.UserData == ViewType.BankView) {
                        RemoveFinger(context, "bank");
                        ForceTutorialDialog(context, new TutorialDialogData {
                            Texts = GetLocalizationStrings(context, "lbl_tut_34"),
                            OnOk = () => CompleteSelf(context)
                        });
                    }
                },
                falseAction: () => { });
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if(IsActive) {
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


    public class PlanetTransportState : TutorialState {

        public override TutorialStateName Name => TutorialStateName.PlanetTransport;
        private bool isDialogShowed = false;

        public override bool IsValid(IBosServiceCollection context) {
            return context.PlanetService.IsOpened(1) &&
                context.TutorialService.IsStateCompleted(TutorialStateName.Moon);
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.Moon)
                .AppendLine($"Is Moon Opened: {services.PlanetService.IsOpened(1)}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                if(!isDialogShowed) {
                    isDialogShowed = true;
                    context.RunCoroutine(ShowDialogImpl(context));
                }
                IsOnEntered = true;
            }
        }

        private IEnumerator ShowDialogImpl(IBosServiceCollection context) {
            yield return new WaitForSeconds(5);
            yield return new WaitUntil(() => context.GameModeService.IsGame);
            ShowTutorialDialog(context, new TutorialDialogData {
                Texts = GetLocalizationStrings(context, "lbl_tut_38"),
                OnOk = () => {
                    GameScrollView gameScroll = GameObject.FindObjectOfType<GameScrollView>();
                    if (gameScroll != null) {
                        gameScroll.Scroll.verticalNormalizedPosition = 1;
                    }
                    Finger(context, "earth", 4);
                    context.Execute(() => CompleteSelf(context), 4);
                }
            });
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {

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

    public class ReportState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.Report;

        private bool isDialogShowed = false;

        public override bool IsValid(IBosServiceCollection context) {
            return context.SecretaryService.GetReportCount(0) > 0;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Has reports on Rickshaw: {services.SecretaryService.GetReportCount(0) > 0}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered ) {
                if(!isDialogShowed) {
                    isDialogShowed = true;

                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_39", "lbl_tut_40"),
                        OnOk = () => {
                            Finger(context, "sec_manager", context.ResourceService.Localization.GetString("lbl_tut_41"),
                                new Vector2(67, -82), 1.34f, 500, 10);
                        }
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if(IsActive) {
                if(isDialogShowed) {
                    if(data.EventName == TutorialEventName.ViewOpened) {
                        if((ViewType)data.UserData == ViewType.ManagementView ) {
                            RemoveFinger(context, "sec_manager");
                            Finger(context, "secretary", context.ResourceService.Localization.GetString("lbl_tut_42"), new Vector2(-200, -95), 1, 500, 10);

                        }
                    } else if(data.EventName == TutorialEventName.SecretariesOpened ) {
                        RemoveFinger(context, "secretary");
                        ForceTutorialDialog(context, new TutorialDialogData {
                            Texts = GetLocalizationStrings(context, "lbl_tut_43"),
                            OnOk = () => {
                                CompleteSelf(context);
                            }
                        });
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

    public class BreakLinesState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.BreakLines;

        private bool isDialogShowed = false;

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.Moon)
                && context.PlanetService.IsMoonOpened;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.Moon)
                .AppendLine($"Is Moon Opened: {services.PlanetService.IsMoonOpened}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                if(!isDialogShowed ) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_44"),
                        OnOk = () => {
                            //show finger on mini games
                            Finger(context, "mini_games", 10);
                        }
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if(IsActive) {
                if(isDialogShowed) {
                    if(data.EventName == TutorialEventName.MiniGameOpened ) {
                        RemoveFinger(context, "mini_games");
                        FingerTimeoutAction(context, "break_lines", 10, () => { CompleteSelf(context); });
                    } else if(data.EventName == TutorialEventName.BreakLinesOpened) {
                        RemoveFinger(context, "break_lines");
                        CompleteSelf(context);
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

    public class TeleportState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.Teleport;

        public override bool IsValid(IBosServiceCollection context) {
            return context.TutorialService.IsStateCompleted(TutorialStateName.BreakLines) &&
                context.GenerationService.GetGetenerator(9).IsResearched == false;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendTutorialStateCompletedCondition(TutorialStateName.BreakLines)
                .AppendLine($"Is Teleport not researched: {services.GenerationService.GetGetenerator(9).IsResearched == false }");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered ) {
                FingerDelayed(context,
                    "teleport",
                    context.ResourceService.Localization.GetString("lbl_tut_46"),
                    new Vector2(-620, 314),
                    1.34f,
                    400,
                    4,
                    () => {
                        var gameScroll = GameObject.FindObjectOfType<GameScrollView>();
                        if(gameScroll != null ) {
                            gameScroll.Scroll.verticalNormalizedPosition = 0;
                        }
                        context.Execute(() => CompleteSelf(context), 4);
                    });
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {

        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {

        }

        protected override void OnExit(IBosServiceCollection context) {

        }
    }

    public class MarsState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.Mars;

        public override bool IsValid(IBosServiceCollection context) {
            var planetData = context.ResourceService.Planets.GetPlanet(2);
            return context.PlayerService.IsEnoughCurrencies(planetData.Prices) &&
             !context.PlanetService.IsMarsOpened &&
             context.PlanetService.GetPlanet(2).State == PlanetState.Closed; 

        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            bool isEnoughCurrenciesForMars = services.PlayerService.IsEnoughCurrencies(services.ResourceService.Planets.GetPlanet(2).Prices);
            bool isMarsNotOpened = false == services.PlanetService.IsMarsOpened;
            bool isMarsStateClosed = services.PlanetService.GetPlanet(2).State == PlanetState.Closed;
            sb.AppendLine($"Is enough currencies for opening Mars: {isEnoughCurrenciesForMars}");
            sb.AppendLine($"Is Mars not opened: {isMarsNotOpened}");
            sb.AppendLine($"Is Mars State closed: {isMarsStateClosed}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered ) {
                if (context.PlanetService.IsMarsOpened) {
                    CompleteSelf(context);
                } else {
                    FingerDelayed(context, "planets", context.ResourceService.Localization.GetString("lbl_tut_45"),
                        new Vector2(-670, -95), 1.34f, 500, 10, () => {
                            var gameScroll = GameObject.FindObjectOfType<GameScrollView>();
                            if (gameScroll != null) {
                                gameScroll.Scroll.verticalNormalizedPosition = 1;
                            }
                        });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if(IsActive) {
                if(data.EventName == TutorialEventName.ViewOpened ) {
                    if((ViewType)data.EventName == ViewType.PlanetsView) {
                        RemoveFinger(context, "planets");
                        Finger(context, "mars_button", 10);
                    } 
                } else if(data.EventName == TutorialEventName.OpenPlanetClicked ) {
                    int planetId = (int)data.UserData;
                    if(planetId == PlanetConst.MARS_ID ) {
                        RemoveFinger(context, "mars_button");
                        CompleteSelf(context);
                    }
                }
            }
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {

        }

        protected override void OnExit(IBosServiceCollection context) {

        }
    }

    public class TutorialMechanicState : TutorialState {
        public override TutorialStateName Name => TutorialStateName.Mechanic;

        private bool isDialogShowed = false;

        public override bool IsValid(IBosServiceCollection context) {
            int brokenCount = context.TransportService.GetUnitBrokenedCount(0);
            return brokenCount > 0;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Has brokened Rickshaw transport: {services.TransportService.GetUnitBrokenedCount(0) > 0}");
            return sb.ToString();
        }

        public override void OnEnter(IBosServiceCollection context) {
            if (!IsOnEntered) {
                if(!isDialogShowed) {
                    isDialogShowed = true;
                    ShowTutorialDialog(context, new TutorialDialogData {
                        Texts = GetLocalizationStrings(context, "lbl_tut_47", "lbl_tut_48"),
                        OnOk = () => {
                            Finger(context, "buy_manager", context.ResourceService.Localization.GetString("lbl_tut_49"), new Vector2(67, -69), 1.34f, 500, 20);
                        }
                    });
                }
                IsOnEntered = true;
            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {
            if(IsActive) {
                if(isDialogShowed) {
                    if(data.EventName == TutorialEventName.ViewOpened ) {
                        if((ViewType)data.UserData == ViewType.ManagementView ) {
                            RemoveFinger(context, "buy_manager");
                            Finger(context, "mechanic", context.ResourceService.Localization.GetString("lbl_tut_50"),
                                new Vector2(-554, -70), 1, 500, 20);
                        }
                    } else if(data.EventName == TutorialEventName.MechanicOpened ) {
                        RemoveFinger(context, "mechanic");
                        ForceTutorialDialog(context, new TutorialDialogData {
                            Texts = GetLocalizationStrings(context, "lbl_tut_51"),
                            OnOk = () => { CompleteSelf(context); }
                        });
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