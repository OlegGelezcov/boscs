using Bos.Data;

namespace Bos {
    public class SplitGameService : SaveableGameBehaviour, ISplitGameService {
        private const int kFirstTries = 3;
        
        private readonly UpdateTimer updateTimer = new UpdateTimer();
        private bool isFirstTries = true;

        private int _level;    
        
        public void Setup(object data = null) {
            updateTimer.Setup(2.0f, (dt) => {
                if (IsLoaded) {
                    if (!HasTries) {
                        if (NextTriesUpdateTime < Services.TimeService.UnixTimeInt) {
                            if (isFirstTries) {
                                SetTries(kFirstTries);
                            } else {
                                SetTries(MaxTries);
                            }
                        }
                    }
                }
            }, true);
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(SplitGameService)}.{nameof(UpdateResume)}() => {pause}");

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }


        public int TriesCount { get; private set; }
        public int NextTriesUpdateTime { get; private set; }
        public int MaxTries { get; private set; }
        public int SpinCounter { get; private set; }

        public void AddMaxTries(int count) {
            int oldCount = MaxTries;
            MaxTries += count;
            if (oldCount != MaxTries) {
                GameEvents.OnSplitMaxTriesChanged(oldCount, MaxTries);
            }
        }

        public void AddTries(int count) {
            int oldCount = TriesCount;
            TriesCount += count;
            if (oldCount != TriesCount) {
                GameEvents.OnSplitTriesChanged(oldCount, TriesCount);
            }
        }

        public void SetTries(int count) {
            int oldCount = TriesCount;
            TriesCount = count;
            if (oldCount != TriesCount) {
                GameEvents.OnSplitTriesChanged(oldCount, TriesCount);
            }
        }

        public void RemoveTries(int count) {
            int oldCount = TriesCount;
            TriesCount -= count;
            SpinCounter += count;
            if (oldCount != TriesCount) {
                GameEvents.OnSplitTriesChanged(oldCount, TriesCount);
            }
            if(TriesCount <= 0 ) {
                isFirstTries = false;
            }
            UpdateNextTriesTime();
        }

        public void UpdateNextTriesTime() {
            NextTriesUpdateTime = Services.TimeService.UnixTimeInt + ResourceService.Defaults.breakline_cooldown;
        }

        public void ResetNextTriesTime() {
            NextTriesUpdateTime = Services.TimeService.UnixTimeInt;
        }

        public bool HasTries
            => TriesCount > 0;

        public void UpgradeLevel()
        {
            var nextLevel = _level + 1;
            if (nextLevel <= ResourceService.Defaults.maxRocketLevel)
            {
                var data = ResourceService.RocketUpgradeRepository.GetUpgrade(nextLevel);
                if (Player.IsEnoughCoins(data.cost))
                {
                    Player.IsEnoughCoins(data.cost);
                    _level = nextLevel;
                    GameEvents.OnSplitLevelChanged(_level - 1, _level);
                }
            }
        }

        public int GetLevel()
        {
            return _level;
        }

        public RocketUpgrade GetCurrentUpgrade()
        {
            var data = ResourceService.RocketUpgradeRepository.GetUpgrade(_level);
            return data;
        }

        public bool IsMaxLevel()
        {
            return _level >= ResourceService.Defaults.maxRocketLevel;
        }

        #region SaveableGameBehaviour overrides
        public override object GetSave() {
            return new SplitGameServiceSave {
                triesCount = TriesCount,
                nextTriesUpdateTime = NextTriesUpdateTime,
                maxTries = MaxTries,
                isFirstTries = isFirstTries,
                spinCounter = SpinCounter,
                level = _level
            };
        }

        public override void LoadDefaults() {
            MaxTries = 3;
            TriesCount = 3;
            NextTriesUpdateTime = 0;
            _level = 1;
            isFirstTries = true;
            IsLoaded = true;
        }

        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            SplitGameServiceSave save = obj as SplitGameServiceSave;
            if (save != null) {
                TriesCount = save.triesCount;
                NextTriesUpdateTime = save.nextTriesUpdateTime;
                MaxTries = save.maxTries;
                isFirstTries = save.isFirstTries;
                SpinCounter = save.spinCounter;
                _level = save.level == 0 ? 1 : save.level;
                IsLoaded = true;
            } else {
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

        public override string SaveKey => "split_game_service";

        public override System.Type SaveType => typeof(SplitGameServiceSave);


        #endregion

    }

    public interface ISplitGameService : IGameService {
        int TriesCount { get; }
        int NextTriesUpdateTime { get; }
        int MaxTries { get; }
        void AddMaxTries(int count);
        void AddTries(int count);
        void RemoveTries(int count);
        void SetTries(int count);
        bool HasTries { get; }
        void ResetNextTriesTime();   
        int SpinCounter { get; }
        bool IsMaxLevel();
        void UpgradeLevel();
        int GetLevel();
        RocketUpgrade GetCurrentUpgrade();
    }

    [System.Serializable]
    public class SplitGameServiceSave {
        public int triesCount;
        public int nextTriesUpdateTime;
        public int maxTries;
        public bool isFirstTries;
        public int spinCounter;
        public int level;
    }
}