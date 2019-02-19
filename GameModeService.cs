using Bos.UI;

namespace Bos {
    using Bos.Debug;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UDBG = UnityEngine.Debug;

    public class GameModeService : SaveableGameBehaviour, IGameModeService {

        private GameModeName gameModeName = GameModeName.Loading;
        public int WinCount { get; private set; } = 0;

        public GameModeName GameModeName {
            get {
                return gameModeName;
            }
        }

        public bool IsGame 
            => GameModeName == GameModeName.Game || 
               GameModeName == GameModeName.ManagerSlot || 
               GameModeName == GameModeName.RaceGame || 
               GameModeName == GameModeName.SlotGame || 
               GameModeName == GameModeName.SplitLiner;

        public int ResetCount { get; private set; } = 0;
        public int SlotGameWonCount { get; private set; } = 0;
        
        public bool IsFirstTimeLaunch { get; private set; } = true;

        public int NextWinCoinReward => 
            (int) (Services.ResourceService.Defaults.baseWinCoinReward * Math.Pow(2, WinCount));

        public int GameStartTime { get; private set; }

        public void Setup(object data = null) {
            StartCoroutine(SetGameStartTimeImpl());
        }

        public void UpdateResume(bool pause)
            => UDBG.Log($"{nameof(GameModeService)}.{nameof(UpdateResume)}() => {pause}");

        private IEnumerator SetGameStartTimeImpl() {
            yield return new WaitUntil(() => {
                return IsLoaded && Services.TimeService.IsValid && Services.SleepService.IsRunning;
            });
            if(GameStartTime == 0 ) {
                GameStartTime = TimeService.UnixTimeInt;
            }
            //UDBG.Log($"GameModeService:GameStartTime => {GameStartTime}".Colored(ConsoleTextColor.aqua));
        }
        public void StartWinGame() {
            int rewardCoins = NextWinCoinReward;
            WinCount++;
            GameEvents.OnWinGame(WinCount, rewardCoins);
            Services.ViewService.ShowDelayed(ViewType.EndGameView, BosUISettings.Instance.ViewShowDelay, new ViewData() {
                UserData = rewardCoins
            });
        }

        public void EndWinGame(int count) {
            //play coins effect
            Services.ViewService.Utils.CreateEndGameCoins(() => {
                Services.SaveService.ResetByWinGame();
                Services.PlayerService.AddCoins(count);
                Services.SaveService.SaveAll();
                Services.ViewService.Show(ViewType.LoadingView, new ViewData() {
                    UserData = new LoadSceneData() {
                        BuildIndex = 0,
                        Mode = LoadSceneMode.Single,
                        LoadAction = () => { }
                    }
                });
            });
        }

        public void SetGameMode(GameModeName newGameModeName) {
            var prevGameMode = gameModeName;
            gameModeName = newGameModeName;
            if(prevGameMode != gameModeName) {
                GameEvents.OnGameModeChanged(prevGameMode, gameModeName);
            }
        }



        public void Restart() {
            int coins = Services.PlayerService.Coins;
            Services.GenerationService.MakeGeneratorsManual();
            Services.PlayerService.SetCoins(coins);
            Services.PlayerService.SetSecurities(0);
            Services.PlayerService.SetCompanyCash(Services.ResourceService.Defaults.startCompanyCash);
            SceneManager.LoadScene("LoadingScene");
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.Pause += OnPause;
            GameEvents.Quit += OnQuit;
            GameEvents.GameModeChanged += OnGameModeChanged;
        }

        public override void OnDisable() {       
            GameEvents.Pause -= OnPause;
            GameEvents.Quit -= OnQuit;
            GameEvents.GameModeChanged -= OnGameModeChanged;
            base.OnDisable();
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode ) {
            if(oldGameMode == GameModeName.Loading && newGameMode == GameModeName.Game ) {
                Services.ResourceService.Sprites.RemovePlanetContainersExcept(Planets.CurrentPlanet.Id);
            }
        }

        private void OnPause() {
            if(IsLoaded) {
                IsFirstTimeLaunch = false;
                Services.SaveService.Save(this, true);
                //UnityEngine.Debug.Log("game mode service is paused...");
            }
        }

        private void OnQuit() {
            if(IsLoaded) {
                IsFirstTimeLaunch = false;
                Services.SaveService.Save(this, true);
                //UnityEngine.Debug.Log("game mode service is quitted...");
            }
        }

        public void AddResetCount(int count) {
            ResetCount += count;
        }

        public void AddSlotGameWonCount(int count)
            => SlotGameWonCount += count;

        public void SetIsFirstLaunchGame(bool value) {
            IsFirstTimeLaunch = value;
        }
        

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "game_mode_service";

        public override Type SaveType => typeof(GameModeServiceSave);

        public override void ResetFull() {
            ResetCount = 0;
            SlotGameWonCount = 0;
            IsLoaded = true;
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void LoadDefaults() {
            ResetCount = 0;
            SlotGameWonCount = 0;
            IsFirstTimeLaunch = true;
            WinCount = 0;
            GameStartTime = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            ResetCount = 0;
            SlotGameWonCount = 0;
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            GameModeServiceSave save = obj as GameModeServiceSave;
            if(save != null ) {
                ResetCount = save.resetCount;
                SlotGameWonCount = save.gameWonCount;
                IsFirstTimeLaunch = save.isFirstTimeLaunch;
                WinCount = save.winCount;
                GameStartTime = save.gameStartTime;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override object GetSave() {
            return new GameModeServiceSave {
                resetCount = ResetCount,
                gameWonCount = SlotGameWonCount,
                isFirstTimeLaunch = IsFirstTimeLaunch,
                winCount = WinCount,
                gameStartTime = GameStartTime
            };
        }
        #endregion
    }


    public interface IGameModeService : IGameService {
        GameModeName GameModeName { get; }
        void SetGameMode(GameModeName newGameModeName);
        void Restart();
        bool IsGame { get; }
        int ResetCount { get; }
        void AddResetCount(int count);
        int SlotGameWonCount { get; }
        void AddSlotGameWonCount(int count);
        bool IsFirstTimeLaunch {
            get;
        }
        void SetIsFirstLaunchGame(bool value);
        bool IsLoaded { get; }

        int WinCount { get; }
        int NextWinCoinReward { get; }
        void StartWinGame();
        void EndWinGame(int coinCount);
        int GameStartTime { get; }
    }

    public enum GameModeName : byte {
        None,
        Loading,
        Game,
        ManagerSlot,
        SlotGame,
        RaceGame,
        SplitLiner
    }

    [System.Serializable]
    public class GameModeServiceSave {
        public int resetCount;
        public int gameWonCount;
        public bool isFirstTimeLaunch;
        public int winCount;
        public int gameStartTime;
    }
}