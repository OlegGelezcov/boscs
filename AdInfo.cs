namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AdInfo : GameElement {
        public int RemainingAdsCount { get; private set; }
        public int AdResetTime { get; private set; }
        public int CurrentMultiplier { get; private set; } = 1;

        public int MaxCountOfAds { get; }

        private readonly UpdateTimer updateTimer = new UpdateTimer();

        private void SetupTimer() {
            updateTimer.Setup(1f, (deltaTime) => {
                if ((Services.TimeService.UnixTimeInt >= AdResetTime) && (AdResetTime > 0)) {
                    Reset();
                }
            }, invokeImmediatly: true);
        }
        public AdInfo(int maxCountOfAds) {
            MaxCountOfAds = maxCountOfAds;
            Reset();
            SetupTimer();
        }

        public AdInfo(int maxCountOfAds, AddInfoSave save) {
            MaxCountOfAds = maxCountOfAds;
            Load(save);
            SetupTimer();
        }

        public void Load(AddInfoSave save) {
            RemainingAdsCount = save.remainingAdsCount;
            AdResetTime = save.adResetTime;
            CurrentMultiplier = save.currentMultiplier;
        }


        public void Reset() {
            RemainingAdsCount = MaxCountOfAds;
            CurrentMultiplier = 1;
            AdResetTime = 0;
        }

        public void Update(float deltaTime) {
            updateTimer.Update(deltaTime);
            //UDebug.Log($"AD INFO UPDATE".Colored(ConsoleTextColor.white).Bold());
        }

        public void Apply() {
            if (IsValidForApply) {
                CurrentMultiplier *= 2;
                RemainingAdsCount--;
                if (CurrentMultiplier == 2) {
                    AdResetTime = Services.TimeService.UnixTimeInt + 2 * 60 * 60;
                }
            }
        }

        public bool IsValidForApply
            => (Services.TimeService.UnixTimeInt > AdResetTime) || (RemainingAdsCount > 0);

        public AddInfoSave GetSave()
            => new AddInfoSave {
                remainingAdsCount = RemainingAdsCount,
                adResetTime = AdResetTime,
                currentMultiplier = CurrentMultiplier
            };

        public override string ToString() {
            return $"Max Count => {MaxCountOfAds}, Current Count => {RemainingAdsCount}{System.Environment.NewLine}" +
                $"Current Mult => {CurrentMultiplier}, Ad Reset Time => {AdResetTime}";
        }
    }

    [System.Serializable]
    public class AddInfoSave {
        public int remainingAdsCount;
        public int adResetTime;
        public int currentMultiplier;

    }

}