namespace Bos.UI {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using System;

    public class X20BoostButton : GameBehaviour {

        public Image progressFill;
        public Image progressBack;
        public Text readyToActivateText;
        public Text activateText;
        public Text timerText;
        public Button button;
        public Image videoAdIconImage;

        public Sprite greenSprite;
        public Sprite orangeSprite;
        public RectTransform x20Image;




        private IX20BoostService boostService = null;
        private readonly UpdateTimer updateTimer = new UpdateTimer();
        private bool isCurrentlyBoostWorking = false;
        private int fingerCounter = 0;

        public override void Start() {
            base.Start();
            Setup();
            UpdateState();
            updateTimer.Setup(0.5f, (realDelta) => {
                UpdateState();
            });
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(args => {
                UpdateHelpFinger();
            }).AddTo(gameObject);
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.X20BoostStateChanged += OnBoostStateChanged;
            GameEvents.X20BoostMultStarted += OnBoostStarted;

            boostService = Services.GetService<IX20BoostService>();
            button.SetListener(() => {
                switch (boostService.State) {
                    case BoostState.Active: {
                            boostService.ApplyBoost();
                            if (false == boostService.IsBoostRunning) {
                                Sounds.PlayOneShot(SoundName.badgeUnlock);
                            }  else {
                                Sounds.PlayOneShot(SoundName.click);
                            }
                        }
                        break;
                    case BoostState.ReadyToActivate: {
                            boostService.Activate();
                            Services.GetService<ISoundService>().PlayOneShot(SoundName.Poof);
                        }
                        break;
                    case BoostState.Locked: {
                            boostService.SetAdStarted(true);
                            Services.AdService.WatchAd("MegaBoost", () => {
                                boostService.UnlockForAd();
                                //Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
                            });
                        }
                        break;
                }
            });
        }

        public override void OnDisable() {
            GameEvents.X20BoostStateChanged -= OnBoostStateChanged;
            GameEvents.X20BoostMultStarted -= OnBoostStarted;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            updateTimer.Update();

            if(boostService != null && boostService.IsBoostRunning) {
                progressFill.fillAmount = boostService.TempBoostProgress;
            }
        }

        private void OnBoostStateChanged(BoostState oldState, BoostState newState) {
            Debug.Log($"x20 state changed => {newState}".Colored(ConsoleTextColor.yellow));
            Setup();
            UpdateState();        
        }

        private void OnBoostStarted(bool isStarted ) {
            isCurrentlyBoostWorking = isStarted;
            if(!isStarted) {
                x20Image.GetComponent<Vector3Animator>().Stop();
            } else {
                x20Image.GetComponent<Vector3Animator>().StartAnimation(new Vector3AnimationData {
                    AnimationMode = BosAnimationMode.PingPong,
                    Target = x20Image.gameObject,
                    Duration = 0.4f,
                    EaseType = EaseType.EaseInOutQuad,
                    StartValue = Vector3.one,
                    EndValue = Vector3.one * 1.2f,
                    OnStart = (s, g) => x20Image.localScale = s,
                    OnUpdate = (s, t, g) => x20Image.localScale = s,
                    OnEnd = (s, g) => x20Image.localScale = s
                });
                RemoveHelpFinger();

            }
        }

        private void UpdateHelpFinger() {
            if(boostService == null ) {
                UnityEngine.Debug.Log("Boost service is null".Attrib(bold: true, color: "y", italic: true, size: 15));
                return;
            }
            if(boostService.State != BoostState.ReadyToActivate &&
                boostService.State != BoostState.Active ) {
                UnityEngine.Debug.Log($"Boost service state invalid: {boostService.State}".Attrib(bold: true, color: "y", italic: true, size: 15));
                return;
            }
            if(isCurrentlyBoostWorking) {
                UnityEngine.Debug.Log("Boost currently worked".Attrib(bold: true, color: "y", italic: true, size: 15));
                return;
            }
            if(!Services.TutorialService.IsStateActiveOnStage(TutorialStateName.MegaBoost, MegaBoostState.STAGE_SHOW_FINGER_OB_BUTTON)) {
                UnityEngine.Debug.Log($"Tutorial megaboost state in invalid Stage: {Services.TutorialService.GetState(TutorialStateName.MegaBoost).Stage}".Attrib(bold: true, color: "y", italic: true, size: 15));
                return;
            }
            if(Services.TutorialService.ExistsFinger(MegaBoostState.kTapMegaBoostPosition)) {
                UnityEngine.Debug.Log("Finger with such position already exists".Attrib(bold: true, color: "y", italic: true, size: 15));
                return;
            }
            if(fingerCounter >= 3 ) {
                UnityEngine.Debug.Log("Finger counter is greater than 3".Attrib(bold: true, color: "y", italic: true, size: 15));
                return;
            }

            UnityEngine.Debug.Log("CREATE FINGER".Attrib(bold: true, color: "y", italic: true, size: 15));
            Services.TutorialService.CreateFinger(MegaBoostState.kTapMegaBoostPosition, new TutorialFingerData {
                Id = MegaBoostState.kTapMegaBoostPosition,
                IsTooltipVisible = false,
                Timeout = 10
            });
            fingerCounter++;
        }

        private void RemoveHelpFinger() {
            if (Services.TutorialService.IsStateActiveOnStage(TutorialStateName.MegaBoost, MegaBoostState.STAGE_SHOW_FINGER_OB_BUTTON)) {
                Services.TutorialService.RemoveFinger(MegaBoostState.kTapMegaBoostPosition);
            }
        }

        private void Construct() {
            if(boostService == null ) {
                boostService = Services.GetService<IX20BoostService>();
            }
        }

        private void Setup() {
            Construct();
            switch(boostService.State) {
                case BoostState.FirstCharger: {
                        progressBack.Activate();
                        progressFill.overrideSprite = orangeSprite;
                        readyToActivateText.Deactivate();
                        activateText.Activate();
                        activateText.text = Services.ResourceService.Localization.GetString("boost");
                        timerText.Activate();
                        button.interactable = false;
                        videoAdIconImage.Deactivate();
                        x20Image.gameObject.Activate();
                    }
                    break;
                case BoostState.Active: {
                        progressBack.Activate();
                        progressFill.fillAmount = 1.0f;
                        progressFill.overrideSprite = orangeSprite;
                        readyToActivateText.Deactivate();
                        activateText.Activate();
                        activateText.text = Services.ResourceService.Localization.GetString("boost");
                        timerText.Activate();
                        button.interactable = true;
                        videoAdIconImage.Deactivate();
                        x20Image.gameObject.Activate();
                    }
                    break;
                case BoostState.ReadyToActivate: {
                        progressBack.Deactivate();
                        readyToActivateText.Activate();
                        readyToActivateText.text = Services.ResourceService.Localization.GetString("megaboost");
                        activateText.Deactivate();
                        timerText.Deactivate();
                        button.interactable = true;
                        videoAdIconImage.Deactivate();
                        x20Image.gameObject.Activate();
                    }
                    break;
                case BoostState.Locked: {
                        progressBack.Activate();
                        progressFill.overrideSprite = greenSprite;
                        readyToActivateText.Deactivate();
                        activateText.Activate();
                        activateText.text = Services.ResourceService.Localization.GetString("unlocknow");
                        timerText.Activate();
                        button.interactable = true;
                        videoAdIconImage.Activate();
                        x20Image.gameObject.Deactivate();
                    }
                    break;
            }
        }

        private void UpdateState() {

            Construct();

            //print($"megaboost state => {boostService.State}");
            switch(boostService.State) {

                case BoostState.FirstCharger: {
                        progressFill.fillAmount = boostService.FirstChargerProgress;
                        timerText.text = BosUtils.FormatTimeWithColon(boostService.FirstChargerTimer);
                    }
                    break;
                case BoostState.Active: {
                        //progressFill.fillAmount = boostService.AtiveProgress;
                        timerText.text = BosUtils.FormatTimeWithColon(boostService.ActiveTimer);
                    }
                    break;
                case BoostState.Locked: {
                        progressFill.fillAmount = boostService.LockedProgress;
                        timerText.text = BosUtils.FormatTimeWithColon(boostService.CooldownTimer);
                    }
                    break;
            }
        }
    }

}