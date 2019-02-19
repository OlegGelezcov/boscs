namespace Bos.UI {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    public class DebugView : TypedView {

        public Button closeButton;
        public Text logText;
        public InputField commandInput;
        public Button runButton;
        public GameObject[] views;

        public Button x1000CompanyCashButton;
        public Button x1000PlayerCashButton;

        public Button x1000CoinsButton;
        public Button nextPlanetButton;

        public Button startSpecialOfferButton;


        public override ViewType Type
            => ViewType.DebugView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 400;

        private readonly Subject<string> logSubject = new Subject<string>();

        private bool isInitialized = false;

        public override void OnEnable() {
            base.OnEnable();
            Application.logMessageReceived += OnLogMessage;
            Application.logMessageReceivedThreaded += OnLogMessage;
        }

        public override void OnDisable() {
            Application.logMessageReceived -= OnLogMessage;
            Application.logMessageReceivedThreaded -= OnLogMessage;
            base.OnDisable();
        }

        public override void Setup(ViewData data) {
            base.Setup(data);

            runButton.SetListener(() => {
                string cmd = GetCommand();
                bool isHandled = false;
                foreach(GameObject view in views ) {
                    if(view.name.Trim().ToLower() == cmd ) {
                        view.Activate();
                        isHandled = true;
                    } else {
                        view.Deactivate();
                    }
                }

                if(false == isHandled) {
                    Services.GetService<IConsoleService>().RunCommand(cmd);
                }
            });
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.DebugView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });

            if (!isInitialized) {
                logSubject.ObserveOnMainThread().SubscribeOnMainThread().Subscribe(str => {
                    logText.text = str;
                }).AddTo(gameObject);
                isInitialized = true;
            }

            x1000CompanyCashButton.SetListener(() => {
                double maxCompanyCash = Player.MaxCompanyCash;
                if(maxCompanyCash.Approximately(0.0)) {
                    maxCompanyCash = 1000.0;
                }
                Player.AddCompanyCash(maxCompanyCash * 1000);
            });

            x1000PlayerCashButton.SetListener(() => {
                double maxPlayerCash = Player.MaxPlayerCash;
                if(maxPlayerCash.Approximately(0.0)) {
                    maxPlayerCash = 1000.0;
                }
                Player.AddPlayerCash(new CurrencyNumber(maxPlayerCash * 1000));
            });

            x1000CoinsButton.SetListener(() => {
                Player.AddCoins(1000);
            });

            nextPlanetButton.SetListener(() => {
                DebugUtils.MoveOnNextPlanet();
            });

            startSpecialOfferButton.SetListener(() => {
                Services.GetService<ISpecialOfferService>().ForceStart();
            });

        }

        private string GetCommand()
            => commandInput.text.Trim().ToLower();



        private void OnLogMessage(string logString, string stackTrace, LogType type) {
            if(type == LogType.Exception || type == LogType.Error || type == LogType.Assert ) {
                //logText.text = logString + "\n" + stackTrace;
                logSubject.OnNext(logString + "\n" + stackTrace);
            }
        }
    }



}