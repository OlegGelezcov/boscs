namespace Bos {
    using Bos.Debug;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class X20BoostService : SaveableGameBehaviour, IX20BoostService {

        private const float kFirstChargerInterval = 600.0f;
        private const float kProfitBoostInterval = 2.0f;

        public BoostState State { get; private set; } = BoostState.FirstCharger;
        public float ActiveTimer { get; private set; } = 0.0f;
        public float CooldownTimer { get; private set; } = 0.0f;
        public float FirstChargerTimer { get; private set; } = 0.0f;
        public int NextCooldownPowerOfTwo { get; private set; } = 0;
        public float CooldownInterval { get; private set; } = 0;

        private bool checkSleep = false;

        private string tempProfitBoostId = string.Empty;
        private bool isTempBoostRunning = false;

        private float profitMultTimer = 0.0f;

        public void ApplyBoost() {
            if (State == BoostState.Active) {
                if (false == isTempBoostRunning) {
                    isTempBoostRunning = true;
                    tempProfitBoostId = System.Guid.NewGuid().ToString();
                    profitMultTimer = kProfitBoostInterval;
                    //Services.GetService<IGenerationService>().Generators.AddTempProfitMult(new GeneratorMult(tempProfitBoostId, 20));
                    Services.GenerationService.Generators.AddProfitBoost(
                        boost: BoostInfo.CreateTemp(
                            id: "x20boost",
                            value: 20));
                    GameEvents.OnX20BoostMultStarted(true);
                }
            }
        }

        public bool IsBoostRunning => isTempBoostRunning;

        public float TempBoostProgress {
            get {
                return 1.0f - Mathf.Clamp01(profitMultTimer / kProfitBoostInterval);
            }
        }

        public float FirstChargerProgress {
            get {
                return 1.0f - Mathf.Clamp01(FirstChargerTimer / kFirstChargerInterval);
            }
        }

        public float AtiveProgress {
            get {
                return Mathf.Clamp01(ActiveTimer / Services.ResourceService.Defaults.x20BoostActiveInterval);
            }
        }

        public float LockedProgress {
            get {
                return 1.0f -  Mathf.Clamp01(CooldownTimer / CooldownInterval);
            }
        }

        #region Unity events
        public override void Update() {
            base.Update();
            UpdateState(deltaTime: Time.deltaTime);

            if (IsLoaded && checkSleep) {
                StartCoroutine(AddSleepIntervalImpl());
                checkSleep = false;
            }

            if(IsLoaded) {
                //if(State == BoostState.Active) {
                    if(isTempBoostRunning) {
                        profitMultTimer -= Time.deltaTime;
                        if(profitMultTimer <= 0.0f ) {
                        //Services.GetService<IGenerationService>().Generators.RemoveTempProfitMult(tempProfitBoostId);
                            Services.GenerationService.Generators.RemoveProfitBoost("x20boost");
                            isTempBoostRunning = false;
                            tempProfitBoostId = string.Empty;
                            GameEvents.OnX20BoostMultStarted(false);
                        }
                    }
                //}
            }
        }



        private IEnumerator AddSleepIntervalImpl() {
            ISleepService sleepService = Services.GetService<ISleepService>();
            yield return new WaitUntil(() => {
                return sleepService.IsRunning;
            });
            UpdateWithSleepInterval(sleepService.SleepInterval);
            //UnityEngine.Debug.Log($"Added sleep interval to X20 Boost {sleepService.SleepInterval}".Colored(ConsoleTextColor.magenta));
        }

        private void OnApplicationPause(bool isPause) {
            UpdateResume(isPause);
        }
        private void OnApplicationFocus(bool isFocus) {
            UpdateResume(!isFocus);
        }

        public void UpdateResume(bool pause) {
            UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");
            checkSleep = !pause;
        }

        public void Activate() {
            if(State == BoostState.ReadyToActivate ) {
                SetState(BoostState.Active);
            }
        }

        public void UnlockForAd() {
            if (State != BoostState.Active) {
                SetState(BoostState.Active);
            }
        }

        private void UpdateState(float deltaTime) {
            switch(State) {
                case BoostState.FirstCharger: {
                        FirstChargerTimer -= deltaTime;
                        if(FirstChargerTimer <= 0.0f ) {
                            SetState(BoostState.ReadyToActivate);
                        }
                    }
                    break;
                case BoostState.Active: {
                        ActiveTimer -= deltaTime;
                        if(ActiveTimer <= 0.0f ) {
                            SetState(BoostState.Locked);
                        }
                    }
                    break;
                case BoostState.Locked: {
                        CooldownTimer -= deltaTime;
                        if(CooldownTimer <= 0.0f ) {
                            SetState(BoostState.ReadyToActivate);
                        }
                    }
                    break;
            }
        }

        public void RemoveFromActiveTimer(float deltaTime) {
            if(State == BoostState.Active ) {
                ActiveTimer -= deltaTime;
            }
        }

        private void UpdateWithSleepInterval(float sleepInterval) {
            switch(State) {
                case BoostState.FirstCharger: {
                        FirstChargerTimer -= sleepInterval;
                        if(FirstChargerTimer <= 0.0f ) {
                            SetState(BoostState.ReadyToActivate);
                        }
                    }
                    break;
                case BoostState.Active: {
                        ActiveTimer -= sleepInterval;
                        if(ActiveTimer <= 0.0f ) {
                            float remain = Mathf.Abs(ActiveTimer);
                            CooldownTimer = Services.ResourceService.Defaults.x20BoostStartCooldown * Mathf.Pow(2, NextCooldownPowerOfTwo);
                            NextCooldownPowerOfTwo++;
                            CooldownInterval = CooldownTimer;
                            if(remain > CooldownTimer) {
                                CooldownTimer = 0f;
                                State = BoostState.ReadyToActivate;
                                //Game State Changed
                                GameEvents.OnBoostX20StateChanged(BoostState.Active, BoostState.ReadyToActivate);
                            } else {
                                CooldownTimer -= remain;
                                State = BoostState.Locked;
                                //Game State Chnaged
                                GameEvents.OnBoostX20StateChanged(BoostState.Active, State);
                            }
                        }
                    }
                    break;
                case BoostState.Locked: {
                        CooldownTimer -= sleepInterval;
                        if(CooldownTimer <= 0.0f ) {
                            SetState(BoostState.ReadyToActivate);
                        }
                    }
                    break;
            }
        }

        private void SetState(BoostState newState) {
            if(State != newState) {
                BoostState oldState = State;
                State = newState;
                switch(State) {
                    case BoostState.ReadyToActivate: {

                        }
                        break;
                    case BoostState.Active: {
                            ActiveTimer = Services.ResourceService.Defaults.x20BoostActiveInterval;
                        }
                        break;
                    case BoostState.Locked: {
                            CooldownTimer = Services.ResourceService.Defaults.x20BoostStartCooldown * Mathf.Pow(2, NextCooldownPowerOfTwo);
                            NextCooldownPowerOfTwo++;
                            CooldownInterval = CooldownTimer;
                        }
                        break;

                }

                GameEvents.OnBoostX20StateChanged(oldState, State);
            }
        }
        #endregion

        #region IX20BoostService
        public void Setup(object data = null) {

        } 
        #endregion

        #region Saveable overrides
        public override string SaveKey => "x20_boost_service";

        public override Type SaveType => typeof(X20BoostSave);

        public override object GetSave() {
            return new X20BoostSave {
                activeTimer = ActiveTimer,
                cooldownTimer = CooldownTimer,
                state = State,
                nextCooldownPowerOfTwo = NextCooldownPowerOfTwo,
                firstChargerTimer = FirstChargerTimer,
                cooldownInterval = CooldownInterval
            };
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByPlanets() {
            
        }

        public override void ResetByInvestors() {
            
        }

        public override void LoadDefaults() {
            State = BoostState.FirstCharger;
            CooldownTimer = Services.ResourceService.Defaults.x20BoostStartCooldown;
            NextCooldownPowerOfTwo = 0;
            CooldownInterval = CooldownTimer;
            FirstChargerTimer = kFirstChargerInterval;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            X20BoostSave save = obj as X20BoostSave;
            if(save != null ) {
                ActiveTimer = save.activeTimer;
                CooldownTimer = save.cooldownTimer;
                State = save.state;
                NextCooldownPowerOfTwo = save.nextCooldownPowerOfTwo;
                FirstChargerTimer = save.firstChargerTimer;
                CooldownInterval = save.cooldownInterval;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }


        #endregion

        public void SetAdStarted(bool value) { }
    }

    public interface IX20BoostService : IGameService {
        BoostState State {
            get;
        }
        float ActiveTimer { get; }
        float CooldownTimer { get; }
        float FirstChargerTimer { get; }


        float FirstChargerProgress { get; }
        float AtiveProgress { get; }
        float LockedProgress { get; }

        void Activate();
        void ApplyBoost();
        float TempBoostProgress { get; }
        bool IsBoostRunning { get; }
        void UnlockForAd();
        void RemoveFromActiveTimer(float deltaTime);
        void SetAdStarted(bool value);
    }

    public enum BoostState : byte {
        FirstCharger,
        ReadyToActivate,
        Active,
        Locked
    }

    [Serializable]
    public class X20BoostSave {
        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public BoostState state;

        public float activeTimer;

        public float cooldownTimer;

        public float firstChargerTimer;

        public int nextCooldownPowerOfTwo;

        public float cooldownInterval;
    }
}