
namespace Bos {
    using Bos.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Bos.UI;
    using Ozh.Tools.Functional;
    using UniRx;

    public class BankService : SaveableGameBehaviour, IBankService {

        public int CurrentBankLevel { get; private set; } = 0;
        public float ProfitTimer { get; private set; } = 0f;
        public float TimerFromLastCollect { get; private set; } = 0f;
        public int CoinsAccumulatedCount { get; private set; } = 0;
   

        private bool _notifyShowed = false;
        private BankLevelData bankLevelData = null;

        private bool isNeedUpdateOnResume = true;

        #region SaveableGameBehaviour override
        public override string SaveKey => "bank_service";

        public override Type SaveType => typeof(BankServiceSave);

        public override object GetSave() {
            return new BankServiceSave {
                coinsAccumulated = CoinsAccumulatedCount,
                currentBankLevel = CurrentBankLevel,
                profitTimer = ProfitTimer,
                timerFromLastCollect = TimerFromLastCollect
            };
        }


        public override void ResetByPlanets() {
            //
        }
        public override void ResetByInvestors() {
            //
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            CurrentBankLevel = 0;
            CoinsAccumulatedCount = 0;
            ProfitTimer = 0f;
            TimerFromLastCollect = 0f;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            BankServiceSave save = obj as BankServiceSave;
            if(save != null ) {
                CurrentBankLevel = save.currentBankLevel;
                CoinsAccumulatedCount = save.coinsAccumulated;
                ProfitTimer = save.profitTimer;
                TimerFromLastCollect = save.timerFromLastCollect;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }
        #endregion


        #region IBankServiceSave
        public void Setup(object data = null) {

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                if(IsLoaded && ResourceService.IsLoaded && GameMode.IsGame ) {
                    UpdateBank(1f);
                }
            }).AddTo(gameObject);
        }

        private void OnApplicationPause(bool pause) {
            UpdateResume(pause);
        }

        private void OnApplicationFocus(bool focus) {
            UpdateResume(!focus);
        }

        public void UpdateResume(bool pause)
            => UpdateOnResume(pause);

        private void UpdateOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateOnResumeImpl() {
            yield return new WaitUntil(() => Services.ResourceService.IsLoaded && IsLoaded);
            yield return new WaitUntil(() => Services.SleepService.IsRunning);
            yield return new WaitUntil(() => Services.TimeChangeService.IsLoaded);
            UpdateBank(Services.SleepService.SleepInterval);
        }


        public BankLevelData LevelData {
            get {
                if((bankLevelData == null) || (bankLevelData.Level != CurrentBankLevel)) {
                    bankLevelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(CurrentBankLevel);
                }
                return bankLevelData;
            }
        }

        public bool IsOpened
            => CurrentBankLevel > 0;

        public bool IsFull 
            => CoinsAccumulatedCount >= MaxCapacity;

        public void Accumulate(int coins) {
            int oldCoins = CoinsAccumulatedCount;
            CoinsAccumulatedCount += coins;
            if(IsFull) {
                
                CoinsAccumulatedCount = MaxCapacity;

                if (!_notifyShowed && GameMode.GameModeName == GameModeName.Game && ViewService.ModalCount == 0)
                {

                    Services.ExecuteWhen(() => {
                        ViewService.Show(ViewType.BankNotify);
                        _notifyShowed = true;
                    }, () => {
                        return GameMode.GameModeName == GameModeName.Game && 
                            ViewService.ModalCount == 0 &&
                            !_notifyShowed && 
                            IsLoaded;
                    });
                    
                }
                
            }
            if(oldCoins != CoinsAccumulatedCount) {
                GameEvents.OnBankAccumulatedCoinsChanged(oldCoins, CoinsAccumulatedCount);
            }
        }


        public int MaxCapacity {
            get { 
                var bankLevelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(CurrentBankLevel);
                if (bankLevelData != null) {
                    
                    int maxCpc = (int)bankLevelData.ProfitPerInterval(TimeSpan.FromSeconds(ResourceService.Defaults.bankWorkInterval));
                    //UnityEngine.Debug.Log($"Max Bank Capacity => {maxCpc}");
                    return maxCpc;
                } else {
                    //UnityEngine.Debug.LogError($"not found bank level data for level: {CurrentBankLevel}");
                }
                return 0;
            }
        }
        
        public TimeSpan SpanWhenBankBeFull()
        {
            BankLevelData bankLevelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(CurrentBankLevel);
            var diff = MaxCapacity - CoinsAccumulatedCount;
            if (diff > 0 && bankLevelData != null)
            {
                var totalSeconds = diff / bankLevelData.Profit * 3600;
                return TimeSpan.FromSeconds(totalSeconds);
            }
            return TimeSpan.Zero;
        }
        
        
        private void RemoveAccumulatedCoins(int count) {
            int oldCoins = CoinsAccumulatedCount;
            CoinsAccumulatedCount -= count;
            if(CoinsAccumulatedCount < 0 ) { CoinsAccumulatedCount = 0; }
            if(oldCoins != CoinsAccumulatedCount) {
                GameEvents.OnBankAccumulatedCoinsChanged(oldCoins, CoinsAccumulatedCount);
            }
        }

        public void CollectAccumulated(int count) {
            if(CoinsAccumulatedCount > 0 && count <= CoinsAccumulatedCount) {
                Services.PlayerService.AddCoins(count);
                RemoveAccumulatedCoins(count);
                //here effect of accumulation
                TimerFromLastCollect = 0;
                //SetAccumulated(0);     
                _notifyShowed = false;
            }
        }

        public void OpenBank() {
            if(!IsOpened) {
                var levelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(1);
                if(Services.PlayerService.Coins >= levelData.LevelPriceCoins ) {
                    Services.PlayerService.RemoveCoins(levelData.LevelPriceCoins);
                    SetBankLevel(1);
                }
            }
        }


        public bool IsAvailableNextLevel {
            get {
                int playerCoins = Services.PlayerService.Coins;
                if(IsOpened ) {
                    if(IsMaxLevel(CurrentBankLevel)) {
                        return false;
                    } else {
                        var levelData = ResourceService.BankLevelRepository.GetBankLevelData(NextLevel);
                        return (playerCoins >= levelData.LevelPriceCoins);
                    }
                } else {
                    var levelData = ResourceService.BankLevelRepository.GetBankLevelData(1);
                    return (playerCoins >= levelData.LevelPriceCoins);
                }
            }
        }

        public void OpenNextLevel() {
            if(CurrentBankLevel >= 1) {
                if(!IsMaxLevel(CurrentBankLevel)) {
                    var nextLevelData = Services.ResourceService.BankLevelRepository.GetBankLevelData(NextLevel);
                    if(Services.PlayerService.Coins >= nextLevelData.LevelPriceCoins) {
                        Services.PlayerService.RemoveCoins(nextLevelData.LevelPriceCoins);
                        ProfitTimer = 0f;
                        SetBankLevel(NextLevel);
                    }
                }
            }
        }

        private void SetBankLevel(int level) {
            int oldLevel = CurrentBankLevel;
            CurrentBankLevel = level;
            if(oldLevel != CurrentBankLevel) {
                GameEvents.OnBankLevelChanged(oldLevel, CurrentBankLevel);
            }
        }

        public int TimeToNextCoin {
            get {
                float remainInterval = LevelData.TimeOfProfitOneCoin - ProfitTimer;
                if(remainInterval < 0f ) {
                    remainInterval = 0f;
                }
                return Mathf.RoundToInt(remainInterval);
            }
        }

        public bool IsMaxLevel(int level) {
            IBankLevelReporitory bankRepository = Services.ResourceService.BankLevelRepository;
            int maxLevel = bankRepository.BankLevelCollection.Max(bLevel => bLevel.Level);
            return (level >= maxLevel);
        }

        public int NextLevel
            => CurrentBankLevel + 1;


        public bool HasAccumulatedCoins
            => CoinsAccumulatedCount > 0;
        
        #endregion


        public Option<TimeSpan> TimeToFullBank {
            get {
                if(!IsOpened) {
                    return F.None;
                }
                if(IsFull) {
                    return F.Some(TimeSpan.FromHours(1));
                }

                float interval = (MaxCapacity - CoinsAccumulatedCount + 1) * LevelData.TimeOfProfitOneCoin;
                return F.Some(TimeSpan.FromSeconds(interval));
            }
        }

        private void UpdateBank(float deltaTime) {
            if (IsOpened) {
                float deltaTimeModified = deltaTime * Services.TimeChangeService.TimeMult;

                ProfitTimer += deltaTimeModified;
                TimerFromLastCollect += deltaTimeModified;
                if(ProfitTimer >= LevelData.TimeOfProfitOneCoin) {
                    int count = (int)(ProfitTimer / LevelData.TimeOfProfitOneCoin);
                    ProfitTimer -= LevelData.TimeOfProfitOneCoin * count;
                    Accumulate(count);
                }
            }
        }

        private void SetAccumulated(int coins) {
            int oldCount = CoinsAccumulatedCount;
            CoinsAccumulatedCount = coins;
            if(oldCount != CoinsAccumulatedCount) {
                GameEvents.OnBankAccumulatedCoinsChanged(oldCount, CoinsAccumulatedCount);
            }
        }
 
    }

    [Serializable]
    public class BankServiceSave {
        public int currentBankLevel;
        public float profitTimer;
        public float timerFromLastCollect;
        public int coinsAccumulated;
    }

    public interface IBankService : IGameService {
        int CurrentBankLevel { get; }
        BankLevelData LevelData { get; }
        bool IsOpened { get; }
        bool IsFull { get; }
        void Accumulate(int coins);
        void CollectAccumulated(int count);
        int CoinsAccumulatedCount { get; }
        void OpenBank();
        void OpenNextLevel();
        int TimeToNextCoin { get; }
        float TimerFromLastCollect {
            get;
        }
        bool IsMaxLevel(int level);
        int NextLevel { get; }
        bool HasAccumulatedCoins { get; }
        TimeSpan SpanWhenBankBeFull();
        float ProfitTimer {
            get;
        }

        /// <summary>
        /// Return interval when bank is filled
        /// </summary>
        Option<TimeSpan> TimeToFullBank { get; }
        bool IsAvailableNextLevel { get; }
    }
}