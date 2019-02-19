namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public class BuyTransportCountState : TutorialState {

        private const int STAGE_START = 0;
        private const int STAGE_WAIT_CLICK = 10;
        private const int STAGE_END = 1000;
        private const string TUTORIAL_POSITION_GENERATOR_COUNT_BUTTON = "generator_count_button";
        private bool IsInitialized { get; set; }

        private IBosServiceCollection services;

        public override TutorialStateName Name 
            => TutorialStateName.BuyTransportCountState;

        public override string GetValidationDescription(IBosServiceCollection services) {
            var builder = base.GetBaseValidationDescription();
            builder.AppendTutorialStateCompletedCondition(TutorialStateName.MegaBoost);
            return builder.ToString();
        }

        public override bool IsValid(IBosServiceCollection context)
            => context.TutorialService.IsStateCompleted(TutorialStateName.MegaBoost);

        public override void Setup(IBosServiceCollection services) {
            base.Setup(services);

            if(!IsInitialized ) {
                GameEvents.GeneratorCountButtonClickedObservable.Subscribe(genId => {
                    if(IsActive && Stage == STAGE_WAIT_CLICK) {
                        if (genId == 0) {
                            services.TutorialService.RemoveFinger(TUTORIAL_POSITION_GENERATOR_COUNT_BUTTON);
                            SetStage(STAGE_END);
                        }
                    }
                }).AddTo(services.Disposables);
                IsInitialized = true;
            }
        }

        public override void OnEnter(IBosServiceCollection context) {
            this.services = context;
            if(!IsOnEntered ) {
                if (Stage == 0) {
                    context.RunCoroutine(ShowFingerOnRickshawCountButtonImpl());
                    SetStage(STAGE_WAIT_CLICK);
                }
                IsOnEntered = true;
            }
        }

        protected override void OnStageChanged(int oldStage, int newStage) {
            base.OnStageChanged(oldStage, newStage);
            if(newStage == STAGE_END ) {
                CompleteSelf(services);
            }
        }

        private IEnumerator ShowFingerOnRickshawCountButtonImpl() {
            yield return new WaitUntil(() => services.ViewService.ModalCount == 0);
            var gameScrollView = GameObject.FindObjectOfType<GameScrollView>();
            gameScrollView?.SetOnTop();
            Finger(services, TUTORIAL_POSITION_GENERATOR_COUNT_BUTTON,
                services.ResourceService.Localization.GetString("lbl_tut_gen_count_btn"),
                new Vector2(-734, -65.6f), 1.35f, 520, 20);
            yield return new WaitForSeconds(20);
            SetStage(STAGE_END);
        }

        public override void OnEvent(IBosServiceCollection context, TutorialEventData data) {

        }

        public override void OnUpdate(IBosServiceCollection context, float deltaTime) {
        }

        protected override void OnExit(IBosServiceCollection context) {
        }

        public override void Reset() {
            base.Reset();

        }
    }
}
