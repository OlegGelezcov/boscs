namespace Bos {
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class X20BoostAlternativeService : SaveableGameBehaviour, IX20BoostService {

        private const float kFirstChargerInterval = 60;
        private const float kCooldownInterval = 10 * 60;
        private const float kProfitBoostInterval = 2.0f;
        private const float kActiveInterval = 30f;

        public BoostState State { get; private set; } = BoostState.FirstCharger;
        public float ActiveTimer { get; private set; } = 0f;
        public float CooldownTimer { get; private set; } = 0f;
        public float FirstChargerTimer { get; private set; } = 0f;

        private bool checkSleep = false;

        private string tempProfitBoostId = string.Empty;
        private bool isTempBoostRunning = false;
        private float profitMultTimer = 0.0f;

        public bool IsAdStarted { get; private set; }

        public void SetAdStarted(bool value) {
            IsAdStarted = true;
        }

        public void Setup(object data = null) {

        }

        public void ApplyBoost() {
            if(State == BoostState.Active ) {
                if(!isTempBoostRunning) {
                    isTempBoostRunning = true;
                    tempProfitBoostId = Guid.NewGuid().ToString();
                    profitMultTimer = kProfitBoostInterval;
                    //Services.GenerationService.Generators.AddTempProfitMult(new GeneratorMult(tempProfitBoostId, 20));
                    Services.GenerationService.Generators.AddProfitBoost(
                        boost: BoostInfo.CreateTemp(
                            id: "x20boost",
                            value: 20));
                    GameEvents.OnX20BoostMultStarted(true);
                }
            }
        }

        public bool IsBoostRunning
            => isTempBoostRunning;

        public float TempBoostProgress
            => 1f - Mathf.Clamp01(profitMultTimer / kProfitBoostInterval);
        public float FirstChargerProgress
            => 1f - Mathf.Clamp01(FirstChargerTimer / kFirstChargerInterval);
        public float AtiveProgress
            => Mathf.Clamp01(ActiveTimer / kActiveInterval);
        public float LockedProgress
            => 1f - Mathf.Clamp01(CooldownTimer / kCooldownInterval);

        public void Activate() {
            if(State == BoostState.ReadyToActivate) {
                SetState(BoostState.Active);
            }
        }

        public void UnlockForAd() {
            SetState(BoostState.Active);
        }

        public void RemoveFromActiveTimer(float deltaTime ) {
            if(State == BoostState.Active ) {
                ActiveTimer -= deltaTime;
            }
        }

        #region Unity Events
        public override void Update() {
            base.Update();
            UpdateState(Time.deltaTime);

            if(IsLoaded && checkSleep ) {
                StartCoroutine(AddSleepIntervalImpl());
                checkSleep = false;
            }

            if(IsLoaded ) {
                if(IsBoostRunning) {
                    profitMultTimer -= Time.deltaTime;
                    if(profitMultTimer <= 0f) {
                        //Services.GenerationService.Generators.RemoveTempProfitMult(tempProfitBoostId);
                        Services.GenerationService.Generators.RemoveProfitBoost("x20boost");
                        isTempBoostRunning = false;
                        tempProfitBoostId = string.Empty;
                        GameEvents.OnX20BoostMultStarted(false);
                    }
                }
            }
        }

        private void UpdateState(float deltaTime) {
            switch(State) {
                case BoostState.FirstCharger: {
                        FirstChargerTimer -= deltaTime;
                        if(FirstChargerTimer <= 0f ) { SetState(BoostState.ReadyToActivate); }
                    }
                    break;
                case BoostState.Active: {
                        ActiveTimer -= deltaTime;
                        if(ActiveTimer <= 0f ) { SetState(BoostState.Locked); }
                    }
                    break;
                case BoostState.Locked: {
                        CooldownTimer -= deltaTime;
                        if(CooldownTimer <= 0f ) { SetState(BoostState.ReadyToActivate); }
                    }
                    break;
            }
        }
        private void OnApplicationPause(bool isPause) {
            UpdateResume(isPause);
        }
        private void OnApplicationFocus(bool isFocus) {
            UpdateResume(!isFocus);
        }

        public void UpdateResume(bool pause) {
            //UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");
            checkSleep = !pause;
        }

        private IEnumerator AddSleepIntervalImpl() {
            yield return new WaitUntil(() => Services.SleepService.IsRunning);
            yield return new WaitUntil(() => IsLoaded);
            if (!IsAdStarted) {
                UpdateWithSleepInterval(Services.SleepService.SleepInterval);
            } else {
                IsAdStarted = false;
            }
        }

        private void UpdateWithSleepInterval(float sleepInterval ) {
            switch(State) {
                case BoostState.FirstCharger: {
                        FirstChargerTimer -= sleepInterval;
                        if(FirstChargerTimer <= 0f ) { SetState(BoostState.ReadyToActivate); }
                    }
                    break;
                case BoostState.Active: {
                        ActiveTimer -= sleepInterval;
                        if(ActiveTimer <= 0f) {
                            float remain = Mathf.Abs(ActiveTimer);
                            CooldownTimer = kCooldownInterval;
                            if(remain >= CooldownTimer ) {
                                CooldownTimer = 0f;
                                State = BoostState.ReadyToActivate;
                                GameEvents.OnBoostX20StateChanged(BoostState.Active, BoostState.ReadyToActivate);
                            } else {
                                CooldownTimer -= remain;
                                State = BoostState.Locked;
                                GameEvents.OnBoostX20StateChanged(BoostState.Active, State);
                            }
                        }
                    }
                    break;
                case BoostState.Locked: {
                        CooldownTimer -= sleepInterval;
                        if(CooldownTimer <= 0f ) { SetState(BoostState.ReadyToActivate); }
                    }
                    break;
            }
        }
        #endregion

        private void SetState(BoostState newState ) {
            if(State != newState ) {
                BoostState oldState = State;
                State = newState;
                switch(State) {
                    case BoostState.ReadyToActivate: { }
                        break;
                    case BoostState.Active: {
                            ActiveTimer = kActiveInterval;
                        }
                        break;
                    case BoostState.Locked: {
                            CooldownTimer = kCooldownInterval;
                        }
                        break;
                }
                GameEvents.OnBoostX20StateChanged(oldState, State);
            }
        }

        #region Saveable overrides
        public override string SaveKey => "x20_boost_service_alternative";

        public override Type SaveType => typeof(X20BoostAlternativeSave);

        public override object GetSave() {
            return new X20BoostAlternativeSave {
                activeTimer = ActiveTimer,
                cooldownTimer = CooldownTimer,
                state = State,
                firstChargerTimer = FirstChargerTimer
            };
        }

        public override void LoadDefaults() {
            State = BoostState.FirstCharger;
            CooldownTimer = kCooldownInterval;
            FirstChargerTimer = kFirstChargerInterval;
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            X20BoostAlternativeSave save = obj as X20BoostAlternativeSave;
            if(save != null ) {
                ActiveTimer = save.activeTimer;
                CooldownTimer = save.cooldownTimer;
                State = save.state;
                FirstChargerTimer = save.firstChargerTimer;
                IsLoaded = true;
            } else { LoadDefaults(); }
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByPlanets() {

        }

        public override void ResetByInvestors() {

        }

        public override void ResetByWinGame() {
            LoadDefaults();
        }

        #endregion
    }


    [Serializable]
    public class X20BoostAlternativeSave {

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public BoostState state;

        public float activeTimer;

        public float cooldownTimer;

        public float firstChargerTimer;
    }
}