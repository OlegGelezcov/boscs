using System;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PrizeWheelGameService : SaveableGameBehaviour, IPrizeWheelGameService {

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public void Setup(object data = null) {
            updateTimer.Setup(2.0f, (dt) => {
                if (IsLoaded) {
                    if (!HasTries) {
                        if (NextTriesUpdateTime < Services.TimeService.UnixTimeInt) {
                            SetTries(MaxTries);
                        }
                    }
                }
            }, true);
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(PrizeWheelGameService)}.{nameof(UpdateResume)}() => {pause}");


        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

        #region SaveableGAmeBehaviour overrides
        public override object GetSave() {
            return new SlotsGameServiceSave {
                triesCount = TriesCount,
                nextTriesUpdateTime = NextTriesUpdateTime,
                maxTries = MaxTries,
                spinCounter = SpinCounter
            };
        }

        public override void LoadDefaults() {
            MaxTries = Services.ResourceService.Defaults.maxTriesPrizeWheel;
            TriesCount = MaxTries;
            NextTriesUpdateTime = 0;
            SpinCounter = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            SlotsGameServiceSave save = obj as SlotsGameServiceSave;
            if (save != null) {
                TriesCount = save.triesCount;
                NextTriesUpdateTime = save.nextTriesUpdateTime;
                MaxTries = save.maxTries;
                SpinCounter = save.spinCounter;
                IsLoaded = true;
            }
            else {
                LoadDefaults();
            }
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByInvestors() {

            //we remove resetting state after sold to investors
            /*
            TriesCount = MaxTries;
            NextTriesUpdateTime = 0;
            */
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            /*
            TriesCount = MaxTries;
            NextTriesUpdateTime = 0;*/

            IsLoaded = true;
        }

        public override string SaveKey => "slots_game_service";

        public override Type SaveType => typeof(SlotsGameServiceSave);
        

        #endregion

        public int TriesCount { get; private set; }
        public int NextTriesUpdateTime { get; private set; }
        public int MaxTries { get; private set; }
        public int SpinCounter { get; private set; }

        public void AddMaxTries(int count) {
            int oldCount = MaxTries;
            MaxTries += count;
            if (oldCount != MaxTries) {
                GameEvents.OnPrizeWheelMaxTriesChanged(oldCount, MaxTries);
            }
        }

        public void AddTries(int count) {
            int oldCount = TriesCount;
            TriesCount += count;
            if (oldCount != TriesCount) {
                GameEvents.OnPrizeWheelTriesChanged(oldCount, TriesCount);
            }
        }

        public void SetTries(int count) {
            int oldCount = TriesCount;
            TriesCount = count;
            if (oldCount != TriesCount) {
                GameEvents.OnPrizeWheelTriesChanged(oldCount, TriesCount);
            }
        }

        public void RemoveTries(int count) {
            int oldCount = TriesCount;
            TriesCount -= count;
            SpinCounter += count;
            if (oldCount != TriesCount) {
                GameEvents.OnPrizeWheelTriesChanged(oldCount, TriesCount);
            }
            UpdateNextTriesTime();
        }

        public void UpdateNextTriesTime() {
            NextTriesUpdateTime = Services.TimeService.UnixTimeInt + ResourceService.Defaults.slots_cooldown;
        }

        public void ResetNextTriesTime() {
            NextTriesUpdateTime = Services.TimeService.UnixTimeInt;
        }

        public bool HasTries
            => TriesCount > 0;
        
    }

    public interface IPrizeWheelGameService : IGameService {
        int TriesCount { get; }
        int NextTriesUpdateTime { get; }
        int MaxTries { get; }
        int SpinCounter { get; }
        void AddMaxTries(int count);
        void AddTries(int count);
        void RemoveTries(int count);
        void SetTries(int count);
        bool HasTries { get; }
        void ResetNextTriesTime();

    }

    [System.Serializable]
    public class SlotsGameServiceSave {
        public int triesCount;
        public int nextTriesUpdateTime;
        public int maxTries;
        public int spinCounter;
        
    }

}
