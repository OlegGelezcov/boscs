namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UDBG = UnityEngine.Debug;
    using Bos.Debug;

    public class TimeChangeService : SaveableGameBehaviour, ITimeChangeService  {


        public override void Update() {
            base.Update();
            if(IsLoaded) {
                RealTime += Time.deltaTime * TimeMult;
            }
        }

        #region ITimeChangeService implementation
        public void Setup(object data = null) {
            UDBG.Log($"{nameof(TimeChangeService):Setup()}".Colored(ConsoleTextColor.green).Bold());
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        public bool IsEnabled { get; private set; }
        public float RealTime { get; private set; }

        public int TimeMult
            => IsEnabled ? 60 : 1;

        public void SetEnabled(bool value) {
            bool oldValue = IsEnabled;
            IsEnabled = value;
            GameEvents.OnTimeChangeServiceStateChanged(oldValue, IsEnabled, () => oldValue != IsEnabled);
        }

        #endregion

        #region SaveableGameBehaviour override
        public override string SaveKey => "time_change_service";

        public override Type SaveType => typeof(TimeChangeServiceSave);

        public override object GetSave() {
            return new TimeChangeServiceSave { isEnabled = IsEnabled, realTime = RealTime };
        }

        public override void LoadDefaults() {
            IsEnabled = false;
            RealTime = 0f;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();   
        }

        public override void LoadSave(object obj) {
            TimeChangeServiceSave save = obj as TimeChangeServiceSave;
            if(save == null ) {
                LoadDefaults();
            } else {
                IsEnabled = save.isEnabled;
                RealTime = save.realTime;
                IsLoaded = true;
            }
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            RealTime = 0f;
            IsLoaded = true;
        }

        public override void ResetFull() {
            LoadDefaults();
        }


        #endregion
    }

    public interface ITimeChangeService : IGameService {
        int TimeMult { get; }
        float RealTime { get; }
        bool IsEnabled { get; }
        void SetEnabled(bool value);
        bool IsLoaded { get; }
    }

    public class TimeChangeServiceSave {
        public bool isEnabled;
        public float realTime;
    }
}