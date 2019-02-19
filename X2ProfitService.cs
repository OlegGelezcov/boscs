namespace Bos {
    using System;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using System.Collections;

    public class X2ProfitService : SaveableGameBehaviour, IX2ProfitService
    {

        public const int kMaxAds = 6;
        public const int kBoostTime = 2 * 60 * 60;
        private int[] EndBoostTimes { get; } = new int[kMaxAds];


        
        public void Setup(object data = null) {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(value => {
                if (IsLoaded && GameMode.GameModeName == GameModeName.Game && Services.GenerationService.IsLoaded) {
                    Services.GenerationService.Generators.AddProfitBoost(BoostInfo.CreateTemp(
                        "x2_profit_service",
                        ProfitMult));
                    //UnityEngine.Debug.Log($"update x2 profit service boost to: {ProfitMult}");
                }
            }).AddTo(gameObject);
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");


        public int FreeSlotsCount
            => EndBoostTimes.Count(time => time < Services.TimeService.UnixTimeInt);

        public int UsedSlotsCount
            => kMaxAds - FreeSlotsCount;


        public bool HasFreeSlots
            => FreeSlotsCount > 0;

        public bool HasUsedSlots
            => UsedSlotsCount > 0;

        private int FreeSlotIndex 
            => HasFreeSlots ?  Array.FindIndex(EndBoostTimes, time => time < Services.TimeService.UnixTimeInt) : -1;

        private int MinEndTimeIndex {
            get {
                int index = 0;
                int time = EndBoostTimes[0];

                for(int i = 1; i < EndBoostTimes.Length; i++ ) {
                    if(EndBoostTimes[i] < time) {
                        index = i;
                        time = EndBoostTimes[i];
                    }
                }
                return index;
            }
        }


        public void SetNewProfit() {
            int index = FreeSlotIndex;
            if(index >= 0 ) {
                EndBoostTimes[index] = Mathf.Max(EndBoostTimes.Max(), Services.TimeService.UnixTimeInt) + kBoostTime;
               
            } else {
                UnityEngine.Debug.LogError("no free slots");
            }

        }

        public int AvailableAfterInterval {
            get {
                int val = EndBoostTimes.Max() - Services.TimeService.UnixTimeInt;
                if (val < 0) {
                    //UnityEngine.Debug.LogError($"logic error in x2profitservice");
                    val = 0;
                }
                return val;
            }
        }

        public int ProfitMult
            => HasUsedSlots ? 2 : 1;

        
        #region SaveableGameBehaviour
        public override object GetSave()
        {
            return new X2ProfitServiceSave {
                endTimes = EndBoostTimes.Select(t => t).ToArray(),
            };
        }

        public override void LoadDefaults()
        {
            for(int i = 0; i < EndBoostTimes.Length; i++ ) {
                EndBoostTimes[i] = 0;
            }
            IsLoaded = true;
        }

        public override void LoadSave(object obj)
        {
            X2ProfitServiceSave save = obj as X2ProfitServiceSave;
            if(save != null ) {
                save.Guard();
                for(int i = 0; i < save.endTimes.Length; i++ ) {
                    EndBoostTimes[i] = save.endTimes[i];
                }
  
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override void ResetFull()
        {
            LoadDefaults();
        }

        public override void ResetByInvestors()
        {
            IsLoaded = true;
        }

        public override void ResetByPlanets()
        {
            IsLoaded = true;
        }

        public override void ResetByWinGame()
        {
            IsLoaded = true;
        }

        public override string SaveKey => "x2_profit_service";

        public override Type SaveType => typeof(X2ProfitServiceSave);
        #endregion

    }

    public class X2ProfitServiceSave {
        public int[] endTimes;

        public void Guard() {
            if(endTimes == null || endTimes.Length != X2ProfitService.kMaxAds) {
                endTimes = new int[X2ProfitService.kMaxAds];
            }
        }
    }

    public interface IX2ProfitService : IGameService {
        int ProfitMult { get; }
        int AvailableAfterInterval { get; }
        int FreeSlotsCount { get; }
        bool HasFreeSlots { get; }
        void SetNewProfit();
        bool HasUsedSlots { get; }
        //int ResetTime { get; }
    }
}