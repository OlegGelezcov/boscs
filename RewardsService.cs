namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class RewardsService : SaveableGameBehaviour, IRewardsService {

        public int AvailableRewards { get; private set; }

        public void AddAvailableRewards(int count) {
            int oldCount = AvailableRewards;
            AvailableRewards += count;
            if(oldCount != AvailableRewards ) {
                GameEvents.OnAvailableRewardsChanged(oldCount, AvailableRewards);
            }
        }

        public void RemoveAvailableRewards(int count ) {
            int oldCount = AvailableRewards;
            AvailableRewards -= count;
            if(AvailableRewards < 0 ) {
                AvailableRewards = 0;
            }

            if (oldCount != AvailableRewards) {
                GameEvents.OnAvailableRewardsChanged(oldCount, AvailableRewards);
            }
        }


        public override string SaveKey => "rewards_service";

        public override Type SaveType => typeof(RewardsServiceSave);

        public override object GetSave() {
            return new RewardsServiceSave {
                availableRewards = AvailableRewards
            };
        }

        public override void LoadDefaults() {
            AvailableRewards = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            RewardsServiceSave save = obj as RewardsServiceSave;
            if(save != null ) {
                AvailableRewards = save.availableRewards;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public void Setup(object data = null) {
            
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(RateService)}.{nameof(UpdateResume)}() => {pause}");

    }

    public interface IRewardsService : IGameService {
        int AvailableRewards { get; }
        void AddAvailableRewards(int count);
        void RemoveAvailableRewards(int count);
    }

    [System.Serializable]
    public class RewardsServiceSave {
        public int availableRewards;
    }
}