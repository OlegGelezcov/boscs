using System;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TreasureHuntService : SaveableGameBehaviour, ITreasureHuntService {

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public void Setup(object data = null) {
            updateTimer.Setup(2.0f, (dt) => {
                if (IsLoaded) {
                    if (!HasTries) {
                        if (NextTriesUpdateTime < Services.TimeService.UnixTimeInt) {
                            SetTries(MaxTries);
                            GameEvents.OnTreasureHuntReload();
                        }
                    }
                }
            }, true);
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(TreasureHuntService)}.{nameof(UpdateResume)}() => {pause}");


        public override void Update() {
            base.Update();
            updateTimer.Update();
        }

        #region SaveableGameBehaviour overrides
        public override object GetSave() {
            return new TreasureHuntServiceSave {
                triesCount = TriesCount,
                nextTriesUpdateTime = NextTriesUpdateTime,
                maxTries = MaxTries,
                openCounter = OpenCounter
            };
        }

        public override void LoadDefaults() {
            MaxTries = Services.ResourceService.Defaults.startTriesTreasureHunt;
            TriesCount = MaxTries;
            NextTriesUpdateTime = 0;
            OpenCounter = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            TreasureHuntServiceSave save = obj as TreasureHuntServiceSave;
            if (save != null) {
                TriesCount = save.triesCount;
                NextTriesUpdateTime = save.nextTriesUpdateTime;
                MaxTries = save.maxTries;
                OpenCounter = save.openCounter;
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
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override string SaveKey => "treasure_hunt_service";

        public override Type SaveType => typeof(TreasureHuntServiceSave);
        

        #endregion

        public int TriesCount { get; private set; }
        public int NextTriesUpdateTime { get; private set; }
        public int MaxTries { get; private set; }
        public int OpenCounter { get; private set; }

        public void AddMaxTries(int count) {
            if (!CanBuyTries())
            {
                UnityEngine.Debug.LogError("Try buy maxties for treasure hunt, but has max");
                return;
            }

            int oldCount = MaxTries;
            MaxTries += count;
            if (oldCount != MaxTries) {
                GameEvents.OnTreasureHuntMaxTriesChanged(oldCount, MaxTries);
                SetTries(MaxTries);
                GameEvents.OnTreasureHuntReload();
            }
        }

        public void AddTries(int count) {
            int oldCount = TriesCount;
            TriesCount += count;
            if (oldCount != TriesCount) {
                GameEvents.OnTreasureHuntTriesChanged(oldCount, TriesCount);
            }
        }

        public void SetTries(int count) {
            int oldCount = TriesCount;
            TriesCount = count;
            if (oldCount != TriesCount) {
                GameEvents.OnTreasureHuntTriesChanged(oldCount, TriesCount);
            }
        }

        public void RemoveTries(int count) {
            int oldCount = TriesCount;
            TriesCount -= count;
            OpenCounter += count;
            if (oldCount != TriesCount) {
                GameEvents.OnTreasureHuntTriesChanged(oldCount, TriesCount);
            }
            UpdateNextTriesTime();
        }

        public void UpdateNextTriesTime() {
            NextTriesUpdateTime = Services.TimeService.UnixTimeInt + ResourceService.Defaults.treasure_cooldown;
        }

        public void ResetNextTriesTime() {
            NextTriesUpdateTime = Services.TimeService.UnixTimeInt;
        }

        public bool CanBuyTries()
        {
            return MaxTries < Services.ResourceService.Defaults.maxTriesTreasureHunt;
        }

        public bool HasTries
            => TriesCount > 0;
        
    }

    public interface ITreasureHuntService : IGameService {
        int TriesCount { get; }
        int NextTriesUpdateTime { get; }
        int MaxTries { get; }
        int OpenCounter { get; }
        void AddMaxTries(int count);
        void AddTries(int count);
        void RemoveTries(int count);
        void SetTries(int count);
        bool HasTries { get; }
        void ResetNextTriesTime();
        bool CanBuyTries();

    }

    [System.Serializable]
    public class TreasureHuntServiceSave {
        public int triesCount;
        public int nextTriesUpdateTime;
        public int maxTries;
        public int openCounter;
        
    }
}
