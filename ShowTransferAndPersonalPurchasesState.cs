using Bos.UI;
using Ozh.Tools.Functional;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using UDBG = UnityEngine.Debug;
    using Bos.Debug;

    public class ShowTransferAndPersonalPurchasesState : TutorialState {

        public const int START_STAGE = 0;
        public const int DIALOG_FIRST_STAGE = 5;
        public const int FINGER_ON_PROFILE_SHOWED_STAGE = 10;
        public const int SHOW_FINGER_ON_TRANSFER_TAB = 15;
        public const int NEED_SHOW_LEGAL_TRANSFER = 17;
        public const int WAIT_FOR_LEGAL_TRANSFER = 20;
        public const int WAIT_FOR_FIRST_ILLEGAL_TRANSFER = 30;
        public const int WAIT_FOR_SECOND_ILLEGAL_TRANSFER = 40;
        public const int DIALOG_SECOND_STAGE = 45;
        public const int WAIT_FOR_OPEN_STATUS_GOODS = 50;
        public const int DIALOG_THIRD_STAGE = 55;
        public const int WAIT_FOR_PURCHASE_PERSONAL_PRODUCT = 60;
        public const int END_STAGE = 1000;


        private readonly HighlightParams LegalHighlightPosition 
            = HighlightParams.CreateDefaultAnchored(new Vector2(0, -631), new Vector2(700, 400));
        private readonly HighlightParams IllegalHighlightPosition
            = HighlightParams.CreateDefaultAnchored(new Vector2(0, -85.4f), new Vector2(700, 400));
        private readonly HighlightParams GoogsHighlightPosition
            = HighlightParams.Create(new Vector2(386, -490), new Vector2(700, 400),
                                        new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        private readonly HighlightParams TransferHighlightPosition
            = HighlightParams.Create(new Vector2(40, -490), new Vector2(700, 400), new Vector2(0.5f, 1), new Vector2(0.5f, 1));



        private IBosServiceCollection services;

        public override TutorialStateName Name => TutorialStateName.ShowTransferAnsPersonalPurchasesState;


        private bool isTransferWarningClosed = false;

        public override void Load(TutorialStateSave save) {
            base.Load(save);
            UDBG.Log($"{nameof(ShowTransferAndPersonalPurchasesState)} loaded");
        }

        private bool isNeedShowFirstDialog = true;


        protected override void OnStageChanged(int oldStage, int newStage) {
            base.OnStageChanged(oldStage, newStage);

            var context = services;

            context.TutorialService.RemoveHighlightRegion();

            switch(newStage) {
                case DIALOG_FIRST_STAGE: {
                        ShowFirstDialog(context);
                    }
                    break;
                case WAIT_FOR_LEGAL_TRANSFER: {
                        context.TutorialService.CreateHighlightRegion(LegalHighlightPosition);
                    }
                    break;
                case WAIT_FOR_FIRST_ILLEGAL_TRANSFER:
                case WAIT_FOR_SECOND_ILLEGAL_TRANSFER: {
                        context.TutorialService.CreateHighlightRegion(IllegalHighlightPosition);
                        if(newStage == WAIT_FOR_SECOND_ILLEGAL_TRANSFER) {
                            isTransferWarningClosed = false;
                        }
                    }
                    break;
                case SHOW_FINGER_ON_TRANSFER_TAB: {
                        Finger(context, "transfer_tab");
                        context.TutorialService.CreateHighlightRegion(TransferHighlightPosition);
                    }
                    break;
                case NEED_SHOW_LEGAL_TRANSFER: {
                        //remove finger on profile
                        context.TutorialService.RemoveFinger("profile");
                        //show finger on legal button
                        Finger(context, "legal", 40);
                        //set stage waiting for legal transfer
                        SetStage(WAIT_FOR_LEGAL_TRANSFER);
                    }
                    break;
                case DIALOG_THIRD_STAGE: {
                        context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                            UserData = new TutorialDialogData {
                                Texts = GetLocalizationStrings(context, "lbl_tut_29"),
                                OnOk = () => {
                                    SetStage(WAIT_FOR_PURCHASE_PERSONAL_PRODUCT);
                                    context.RunCoroutine(ShowFingerOnBuyPersonalProduct());
                                }
                            }
                        });
                    }
                    break;
            }
        }



        private void ShowFirstDialog(IBosServiceCollection context) {
            ShowTutorialDialog(context, new TutorialDialogData {
                Texts = new List<string> {
                                         context.ResourceService.Localization.GetString("lbl_tut_21"),
                                         context.ResourceService.Localization.GetString("lbl_tut_22")
                                     },
                OnOk = () => {
                    Finger(context, "profile", context.ResourceService.Localization.GetString("lbl_tut_23"),
                        new Vector2(74, -81), 1.34f, 500, 10);
                    SetStage(FINGER_ON_PROFILE_SHOWED_STAGE);
                }
            });
            isNeedShowFirstDialog = false;
        }

        public ShowTransferAndPersonalPurchasesState(IBosServiceCollection context) {
            services = context;
            AddLocked(WAIT_FOR_LEGAL_TRANSFER, new List<string> { "currency", "gender", "close", "office", "goods", "illegal" });
            AddLocked(WAIT_FOR_FIRST_ILLEGAL_TRANSFER, new List<string> { "currency", "gender", "close", "office", "goods", "legal" });
            AddLocked(WAIT_FOR_SECOND_ILLEGAL_TRANSFER, new List<string> { "currency", "gender", "close", "office", "goods", "legal" });
            AddLocked(WAIT_FOR_OPEN_STATUS_GOODS, new List<string> { "currency", "gender", "close", "office", "transfer" });
            AddLocked(SHOW_FINGER_ON_TRANSFER_TAB, new List<string> { "currency", "gender", "close", "office", "goods", "badge", "stats" });

        }

        private IEnumerator ShowHighlightWhenClosedTransferWarning(IBosServiceCollection context) {
            //wait until close illegal transfer fail view
            yield return new WaitUntil(() => isTransferWarningClosed && (Stage == DIALOG_SECOND_STAGE));

            context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                UserData = new TutorialDialogData {
                    Texts = GetLocalizationStrings(context, "lbl_tut_27", "lbl_tut_28"),
                    OnOk = () => {
                        FingerDelayed(context, "status_goods", string.Empty, Vector2.zero, 1, 1, 10);
                        //show finger on goods
                        //.....
                        SetStage(WAIT_FOR_OPEN_STATUS_GOODS);
                        context.TutorialService.CreateHighlightRegion(GoogsHighlightPosition);
                    }
                }
            });

           
        }

        public override bool IsValid(IBosServiceCollection context) {
            context.PlayerService.ProductNotifier.AvailableProduct.Match(() => {
                IsProductAvailable = false;
                return F.None;
            }, prod => {
                IsProductAvailable = true;
                return F.Some(prod);
            });
            return IsProductAvailable;
        }

        public override string GetValidationDescription(IBosServiceCollection services)
        {
            var sb = GetBaseValidationDescription();
            sb.AppendLine($"Product Notifier Available Product(IsProductAvailable): {IsProductAvailable}");
            return sb.ToString();
        }

        public bool IsProductAvailable { get; private set; }

        public override void OnEnter(IBosServiceCollection context) {
            if(!IsOnEntered) {
                IsOnEntered = true;

            }
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData eventData) {
            if(IsActive) {

                if(eventData.EventName == TutorialEventName.ViewHided) {
                    ViewType viewType = (ViewType)eventData.UserData;
                    if(viewType == ViewType.TransferWarningView ) {
                        isTransferWarningClosed = true;
                        if(Stage == DIALOG_SECOND_STAGE ) {
                            context.RunCoroutine(ShowHighlightWhenClosedTransferWarning(context));
                        }
                    }
                }

                if(eventData.EventName == TutorialEventName.ViewOpened ) {
                    ViewType viewType = (ViewType)eventData.UserData;
                    if(viewType == ViewType.ProfileView ) {
                        context.TutorialService.RemoveFinger("profile");
                    }
                }

                if(Stage == SHOW_FINGER_ON_TRANSFER_TAB ) {
                    if(eventData.EventName == TutorialEventName.TransferTabOpened ) {
                        context.TutorialService.RemoveFinger("transfer_tab");
                        context.ViewService.Show(ViewType.TutorialDialogView, new ViewData {
                            UserData = new TutorialDialogData {
                                Texts = new List<string> {
                                     context.ResourceService.Localization.GetString("lbl_tut_25"),
                                     context.ResourceService.Localization.GetString("lbl_tut_26")
                                 },
                                OnOk = () => {
                                    SetStage(NEED_SHOW_LEGAL_TRANSFER);
                                }
                            }
                        });
                        context.TutorialService.RemoveHighlightRegion();
                    }
                }


                if (Stage == FINGER_ON_PROFILE_SHOWED_STAGE) {
                    if (eventData.EventName == TutorialEventName.TransferTabOpened) {
                        //this code moved in NEED_SHOW_LEGAL_TRANSFER
                    }
                } else if (Stage == WAIT_FOR_LEGAL_TRANSFER) {
                    if (eventData.EventName == TutorialEventName.LegalTransferCompleted) {
                        context.TutorialService.RemoveFinger("legal");
                        Finger(context, "illegal", 100);
                        SetStage(WAIT_FOR_FIRST_ILLEGAL_TRANSFER);
                    }
                } else if (Stage == WAIT_FOR_FIRST_ILLEGAL_TRANSFER) {
                    if (eventData.EventName == TutorialEventName.IllegalTransferCompleted) {
                        context.TutorialService.RemoveFinger("illegal");
                        Finger(context, "illegal", 100);
                        SetStage(WAIT_FOR_SECOND_ILLEGAL_TRANSFER);
                    }
                } else if (Stage == WAIT_FOR_SECOND_ILLEGAL_TRANSFER) {
                    if (eventData.EventName == TutorialEventName.IllegalTransferCompleted) {
                        context.TutorialService.RemoveFinger("illegal");

                        SetStage(DIALOG_SECOND_STAGE);

                        /////
                        ///


                    }
                } else if (Stage == WAIT_FOR_OPEN_STATUS_GOODS) {
                    if (eventData.EventName == TutorialEventName.StatusGoodsOpened) {
                        context.TutorialService.RemoveFinger("status_goods");

                        SetStage(DIALOG_THIRD_STAGE);

                        /////////////////
                        //SetStage(WAIT_FOR_PURCHASE_PERSONAL_PRODUCT);
                        //context.RunCoroutine(ShowFingerOnBuyPersonalProduct());
                        
                    }
                } else if (Stage == WAIT_FOR_PURCHASE_PERSONAL_PRODUCT) {
                    if (eventData.EventName == TutorialEventName.PersonalProductPurchased) {
                        context.TutorialService.RemoveFinger("buy_product");                      
                        CompleteSelf(context);
                        //UDBG.Log($"complete self state => {IsCompleted}");
                        context.RunCoroutine(WaitForComplete());
                    }
                } 
            }
        }

        private IEnumerator WaitForComplete() {
            yield return new WaitUntil(() => IsCompleted);
            UnityEngine.Debug.Log($"{nameof(ShowTransferAndPersonalPurchasesState)}: It's completed!".Bold().Colored(ConsoleTextColor.green));
        }

        private IEnumerator ShowFingerOnBuyPersonalProduct() {
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(0.2f);
                bool isFounded = false;
                FindProductViewForAvailableProduct().Match(
                    () => { isFounded = false; }, 
                    view => {                        
                        Transform buttonTransform = view.buyButton?.GetComponent<RectTransform>() ?? null;
                        if (buttonTransform != null) {
                            services.TutorialService.CreateFinger(buttonTransform, new TutorialFingerData() {
                                Id = "buy_product",
                                IsTooltipVisible = false,
                                Position = Vector2.zero,
                                Timeout = 10,
                            });
                            isFounded = true;
                        }
                });
                if (isFounded) {
                    yield break;
                }
            }
        }

        private Option<ProductView> FindProductViewForAvailableProduct() {
            return services?.PlayerService.AvailablePersonalProduct.Bind(data => {
                var views = GameObject.FindObjectsOfType<ProductView>();
                foreach (var view in views) {
                    if (view.Data != null) {
                        if (view.Data.id == data.id) {
                            return F.Some(view);
                        }
                    }
                }
                return F.None;
            }) ?? F.None;
        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
            if(IsActive) {
                if(Stage == START_STAGE ) {
                    if (IsValid(context)) {
                        /*
                        Finger(context, "profile", 10); 
                        SetStage(FINGER_ON_PROFILE_SHOWED_STAGE);*/

                        //show first dialog
                        SetStage(DIALOG_FIRST_STAGE);
                    }
                } else if(Stage == DIALOG_FIRST_STAGE ) {
                    if(isNeedShowFirstDialog) {
                        if (context.GameModeService.IsGame) {
                            ShowFirstDialog(services);
                        }
                    }
                }
            }
        }

        protected override void OnExit(IBosServiceCollection context) {

        }


    }

}