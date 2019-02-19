namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using Bos.Debug;
    using UniRx;
    using UnityEngine.SocialPlatforms;


    public class AchievmentService : SaveableGameBehaviour, IAchievmentServcie {

        public float reportInterval = 5;
        private int authCount = 0;

        public Sprite LevelUpIcon;
        private bool IsInitialized {get; set;}

        public Dictionary<int, GeneratorAchievmentCollection> GeneratorAchievments { get; } = 
            new Dictionary<int, GeneratorAchievmentCollection>();
        public List<string> CompletedAchievments { get; } = 
            new List<string>();
        
        public int AchievmentPoints { get; private set; } = 0;

        public int CompletedAchievmentsCount
            => CompletedAchievments.Count;

        public void AddCompletedAchievment(string achievmentId ) {
            if(!IsAchievmentCompleted(achievmentId)) {
                CompletedAchievments.Add(achievmentId);
            }
        }

        public bool IsAchievmentCompleted(string id) {
            return CompletedAchievments.Contains(id);
        }



        public void AddAchievmentPoints(int count ) {
            int oldCount = AchievmentPoints;
            AchievmentPoints += count;
            if(oldCount != AchievmentPoints) {
                GameEvents.OnAchievmentPointsChanged(oldCount, AchievmentPoints);
            }
        }

        public void ShowAchievementUI()
        {
            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate(success =>
                {
                    UnityEngine.Debug.Log("Auth = " + success);
                    if(success) {
                        Social.ShowAchievementsUI();
                    }
                });
            }  else {
                Social.ShowAchievementsUI();
            }       
        }

        private void TryReport(System.Action reportAction ){
            if(Social.localUser.authenticated) {
                reportAction();
            } else {
                if(authCount < 1 ) {
                    Social.localUser.Authenticate(success => {
                        if(success) {
                            reportAction();
                        }
                        authCount = 1;
                    });
                }
            }
        }

        #region IAchievmentService
        public void Setup(object data = null) {
            StartCoroutine(LoadDb());

            if(!IsInitialized) {
                Observable.Interval(TimeSpan.FromSeconds(reportInterval)).Subscribe(_=>{
                    if(IsLoaded && StoreAchievementDB.IsLoaded) {
                        StoreAchievementItem targetAchievment = StoreAchievementDB
                        .StoreAchievements.FirstOrDefault(a => a.Check());
                        if(targetAchievment != null ) {
                            #if !UNITY_EDITOR
                            TryReport(()=>{
                                Social.ReportProgress(achievementID: targetAchievment.AchievementId,
                                    progress: 100,
                                    callback: success => 
                                    UnityEngine.Debug.Log($"{targetAchievment.AchievementId} reported {success}"));
                            });
                            #endif
                            StoreAchievementDB.StoreAchievements.Remove(targetAchievment);
                            AddCompletedAchievment(targetAchievment.AchievementId);
                            UnityEngine.Debug.Log($"achievment was completed: {targetAchievment.AchievementId}");
                        }
                    }
                }).AddTo(gameObject);

                Observable.Interval(TimeSpan.FromHours(6)).Subscribe(_ => {
                    authCount = 0;
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        private IEnumerator LoadDb() {
            yield return new WaitUntil(() => IsLoaded);

            StoreAchievementDB.Load();
        }


        public void UpdateResume(bool pause) { }

        public float GetProgressForGenerator(int generatorId, int unitsCount ) {
            int maxAchievmentIndex = GetMaxAchievmentIdForGenerator(generatorId);
            if(maxAchievmentIndex < 0 ) {
                return Mathf.Clamp01((float)unitsCount / (float)LevelDb.GetLevel(0).NextLevelCount);
            }

            Level nextLevel = LevelDb.GetLevel(maxAchievmentIndex + 1);
            if(nextLevel != null ) {
                Level curLevel = LevelDb.GetLevel(maxAchievmentIndex);
                return Mathf.Clamp01((float)(unitsCount - curLevel.NextLevelCount) / (float)(nextLevel.NextLevelCount - curLevel.NextLevelCount));
            }

            return 1.0f;
        }

        private int GetMaxAchievmentIdForGenerator(int generatorId) {

            List<int> allAchievments = GetAchievmentsForGenerator(generatorId);
            if(allAchievments.Count == 0 ) {
                return -1;
            } else {
                return allAchievments.Max();
            }

        }

        public List<int> GetAchievmentsForGenerator(int generatorId) {
            if(false == GeneratorAchievments.ContainsKey(generatorId)) {
                GeneratorAchievments.Add(generatorId, new GeneratorAchievmentCollection(generatorId));
            }
            return GeneratorAchievments[generatorId].ReceivedAchievments;
        }

        public void AddAchievment(int generatorId, int achievmentId ) {
            if(false == GeneratorAchievments.ContainsKey(generatorId)) {
                GeneratorAchievments.Add(generatorId, new GeneratorAchievmentCollection(generatorId));
            }
            GeneratorAchievments[generatorId].AddAchievment(achievmentId);
        }

        private IEnumerable<Level> FilterForGenerator(int generatorId, IEnumerable<Level> levels) {
            var generatorAchievments = GetAchievmentsForGenerator(generatorId);
            foreach(var level in levels) {
                if(false == generatorAchievments.Contains(level.Id)) {
                    yield return level;
                }
            }
        }

        public void UpdateAchievments(GeneratorInfo[] generators) {
            foreach(GeneratorInfo generator in generators) {
                int unitCount = Services.TransportService.GetUnitTotalCount(generator.GeneratorId);
                IEnumerable<Level> achievmentLevels = LevelDb.GetLevelsForCount(unitCount);

                List<Level> validLevels = FilterForGenerator(generator.GeneratorId, achievmentLevels).ToList();
                if(validLevels.Count > 0 ) {
                    List<ExtendedAchievmentInfo> extendedList = new List<ExtendedAchievmentInfo>();
                    foreach(Level level in validLevels) {
                        ExtendedAchievmentInfo info = new ExtendedAchievmentInfo(level.Id, $"{FirstSimbolUpper(generator.Data.Name)} Level Up",
                            LevelUpIcon, 5, level.RewardType, level.RewardValue);
                        extendedList.Add(info);
                        AddAchievment(generator.GeneratorId, level.Id);
                        AddAchievmentPoints(5);
                        RewardAchievment(generator.GeneratorId, level);
                    }
                    GameEvents.OnGeneratorAchievmentsReceived(generator.GeneratorId, extendedList);
                }
            }
        }
        #endregion

        private string FirstSimbolUpper(string str) {
            if (string.IsNullOrEmpty(str)) return "";

            var firstSibol = str.Substring(0, 1).ToUpper();
            str = str.ToLower();
            str = str.Substring(1, str.Length - 1);
            return firstSibol + str;
        }

        private void RewardAchievment(int generatorId, Level level ) {
            switch (level.RewardType) {
                case RewardType.SpeedUpgrade:
                    GenerationService.Generators.AddTimeBoost(
                        generatorId: generatorId,
                        boost: BoostInfo.CreateTemp($"reward_speed_{generatorId}_".GuidSuffix(5), level.RewardValue));
                    break;
                case RewardType.ProfitUpgrade: {
                        GenerationService.Generators.AddProfitBoost(
                            generatorId: generatorId,
                            boost: BoostInfo.CreateTemp(
                                id: $"reward_profit_{generatorId}_{level.Id}_".GuidSuffix(5), 
                                value: level.RewardValue));
                    }
                    break;
                case RewardType.None:
                    break;
                default:
                    break;
            }
        }

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "achievment_service";

        public override Type SaveType => typeof(AchievmentServiceSave);

        public override object GetSave() {
            List<string> completedAchievments = new List<string>(CompletedAchievments);
            //List<int> badges = new List<int>(Badges);

            return new AchievmentServiceSave {
                generatorAchievments = GeneratorAchievments
                .Select(kvp => kvp.Value.GetSave())
                .ToDictionary(sv => sv.generatorId, sv => sv),
                completedAchievments = completedAchievments,
                //badges = badges,
                achievmentPoints = AchievmentPoints
            };
        }


        public override void ResetByInvestors() {
            GeneratorAchievments.Clear();
            //Badges.Clear();
            AchievmentPoints = 0;
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            GeneratorAchievments.Clear();
            //Badges.Clear();
            AchievmentPoints = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }
        
        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            GeneratorAchievments.Clear();
            CompletedAchievments.Clear();
            AchievmentPoints = 0;
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            AchievmentServiceSave save = obj as AchievmentServiceSave;
            if(save != null ) {
                if(save.generatorAchievments != null ) {
                    GeneratorAchievments.Clear();
                    foreach(var kvp in save.generatorAchievments) {
                        GeneratorAchievments.Add(kvp.Key, new GeneratorAchievmentCollection(kvp.Value));
                    }

                    CompletedAchievments.Clear();
                    if(save.completedAchievments != null ) {
                        CompletedAchievments.AddRange(save.completedAchievments);
                    }

                    //Badges.Clear();
                    //if(save.badges != null ) {
                    //    Badges.AddRange(save.badges);
                    //}

                    this.AchievmentPoints = save.achievmentPoints;

                    IsLoaded = true;
                } else {
                    LoadDefaults();
                }
            } else {
                LoadDefaults();
            }
        }
        #endregion
    }

    public interface IAchievmentServcie : IGameService {
        List<int> GetAchievmentsForGenerator(int generatorId);
        void AddAchievment(int generatorId, int achievmentId);
        void UpdateAchievments(GeneratorInfo[] generators);
        //int GetMaxAchievmentIdForGenerator(int generatorId);
        float GetProgressForGenerator(int generatorId, int unitsCount);
        int CompletedAchievmentsCount { get; }
        void AddCompletedAchievment(string achievmentId);
        bool IsAchievmentCompleted(string id);

        void AddAchievmentPoints(int count);
        int AchievmentPoints { get; }

        void ShowAchievementUI();
    }

    public class AchievmentServiceSave {
        public Dictionary<int, GeneratorAchievmentCollectionSave> generatorAchievments;
        public List<string> completedAchievments;
        //public List<int> badges;
        public int achievmentPoints;
    }

    public class GeneratorAchievmentCollection {
        public int GeneratorId { get; private set; }
        public List<int> ReceivedAchievments { get; private set; }

        public GeneratorAchievmentCollection(int generatorId) {
            GeneratorId = generatorId;
            ReceivedAchievments = new List<int>();
        }

        public GeneratorAchievmentCollection(GeneratorAchievmentCollectionSave save) {
            GeneratorId = save.generatorId;
            ReceivedAchievments = save.receivedAchievments != null ? save.receivedAchievments : new List<int>();
        }

        public void AddAchievment(int achievmentId ) {
            if(!ReceivedAchievments.Contains(achievmentId)) {
                ReceivedAchievments.Add(achievmentId);
            }
        }

        public GeneratorAchievmentCollectionSave GetSave()
            => new GeneratorAchievmentCollectionSave {
                generatorId = GeneratorId,
                receivedAchievments = ReceivedAchievments
            };
    }

    public class GeneratorAchievmentCollectionSave {
        public int generatorId;
        public List<int> receivedAchievments;
    }


    public class ExtendedAchievmentInfo : AchievmentInfo {
        public ExtendedAchievmentInfo(int id, string name, 
            Sprite iconSprite, int points, 
            RewardType rewardType, int rewardValue)
            : base(id, name) {
            Icon = iconSprite;
            Points = points;
            RewardType = rewardType;
            RewardValue = rewardValue;
        }

        public Sprite Icon { get; private set; }
        public int Points { get; private set; }
        public RewardType RewardType { get; private set; }
        public int RewardValue { get; private set; }
        public string CustomText { get; set; } = string.Empty;

        public string RewardText {
            get {
                switch (RewardType) {
                    case RewardType.SpeedUpgrade:
                        return string.Format("X.SPEED".GetLocalizedString(), RewardValue);
                    case RewardType.ProfitUpgrade:
                        return string.Format("X.PROFIT".GetLocalizedString(), RewardValue);
                    default:
                        return CustomText;
                }
            }
        }
    }


    public class StoreAchievementItem : GameElement
{
    public string AchievementId;
    public Func<bool> IsAttained;

    public virtual bool Check()
    {
        return IsAttained();
    }
}

public class GeneratorCountSAI : StoreAchievementItem
{
    private int _id;
    private int _count;

    public GeneratorCountSAI(int id, int count)
    {
        _id = id;
        _count = count;
    }

    public override bool Check()
    {
        if (false == Services.TransportService.HasUnits(_id))
            return false;

        return Services.TransportService.GetUnitTotalCount(_id) >= _count;
    }
}

public class OwnedUpgradeSAI : StoreAchievementItem
{
    private int _upId;

    public OwnedUpgradeSAI(int upgradeId)
    {
        _upId = upgradeId;
    }

    public override bool Check()
    {
        return Services.UpgradeService.IsBoughtCoinsUpgrade(_upId); //GlobalRefs.PlayerData.BoughtTransportUpgrade.Contains(_upId);
    }
}

public class OwnedGeneratorSAI : StoreAchievementItem
{
    private int _id;

    public OwnedGeneratorSAI(int Id)
    {
        _id = Id;
    }

    public override bool Check()
    {
        return Services.TransportService.HasUnits(_id); //GlobalRefs.PlayerData.OwnedGenerators.ContainsKey(_id);
    }
}

public class AllUpgradesOwnedSAI : StoreAchievementItem
{
    private int _genId;

    public AllUpgradesOwnedSAI(int genId)
    {
        _genId = genId;
    }

    public override bool Check()
    {
        /*
        var ups = GlobalRefs.CashUVMs;
        if (ups == null)
            return false;

        var allUpForGen = ups.Where(x => x.GeneratorIdToUpgrade == _genId).Select(x => x.Id);*/
        List<int> allUpgradeForGenerators = Services.ResourceService.CashUpgrades.UpgradeList.Where(u => u.GeneratorId == _genId)
            .Select(u => u.Id)
            .ToList();
        allUpgradeForGenerators.AddRange(Services.ResourceService.SecuritiesUpgrades.UpgradeList.Where(u2 => u2.GeneratorId == _genId).Select(u2 => u2.Id));

        var sec = Services.UpgradeService.BoughtCashAndSecuritiesUpgrades.Intersect(allUpgradeForGenerators);
        return sec.Count() == allUpgradeForGenerators.Count();
    }
}

public class CashEarnedSAI : StoreAchievementItem
{
    private double _amount;

    public CashEarnedSAI(double amount)
    {
        _amount = amount;
    }

    public override bool Check()
    {
        return Services.PlayerService.LifetimeEarnings >= _amount;
    }
}

public class HasManagerSAI : StoreAchievementItem
{
    private int _id;

    public HasManagerSAI(int id)
    {
        _id = id;
    }

    public override bool Check()
    {
        if (false == Services.ManagerService.IsHired(_id))
            return false;

        return true;
    }
}

public class StoreAchievementDB
{
    public static List<StoreAchievementItem> StoreAchievements;
    public static bool IsLoaded { get; private set; } = false;

    public static void Load()
    {
        StoreAchievements = new List<StoreAchievementItem>();

        LoadUnlocks2();
        LoadUnlocks3();
        LoadUnlocks4();

        LoadUpgrades1();
        LoadUpgrades2();

        LoadCash1();

        LoadRewards1();

        LoadInvestorsUpgraddes1();
        LoadManagers1();
        
        LoadGenerator();

        int removedAchievmentsCountOnStart = 0;

        foreach (var ac in StoreAchievements.ToList())
        {
            if (GameServices.Instance.GetService<IAchievmentServcie>().IsAchievmentCompleted(ac.AchievementId))
            {
                StoreAchievements.Remove(ac);
                removedAchievmentsCountOnStart++;
            }
        }
        UnityEngine.Debug.Log($"Removed achievments count on start => {removedAchievmentsCountOnStart}".Colored(ConsoleTextColor.yellow));

        IsLoaded = true;
    }

    private static void LoadInvestorsUpgraddes1()
    {
        StoreAchievements.Add(new StoreAchievementItem() {
            AchievementId = LeaderboardConstants.achievement_reset_the_game_and_claim_your_investors,
            IsAttained = () => GameServices.Instance.GameModeService.ResetCount >= 1 });
        StoreAchievements.Add(new StoreAchievementItem() {
            AchievementId = LeaderboardConstants.achievement_reset_the_game_and_claim_your_investors_20_times,
            IsAttained = () => GameServices.Instance.GameModeService.ResetCount >= 20 });
        StoreAchievements.Add(new StoreAchievementItem() {
            AchievementId = LeaderboardConstants.achievement_reach_1_million_investors,
            IsAttained = () => GameServices.Instance.PlayerService.Securities.Value >= 1e6 });
        StoreAchievements.Add(new StoreAchievementItem() {
            AchievementId = LeaderboardConstants.achievement_reach_1_septillion_investors,
            IsAttained = () => GameServices.Instance.PlayerService.Securities.Value >= 1e15 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_reach_1_quadrillion_investors, IsAttained = () => GameServices.Instance.PlayerService.Securities.Value >= 1e24 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_purchase_all_available_investor_upgrades, IsAttained = () => GameServices.Instance.UpgradeService.BoughtSecuritiesUpgrades.Count == GameServices.Instance.ResourceService.SecuritiesUpgrades.Count });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_purchase_an_investor_upgrade, IsAttained = () => GameServices.Instance.UpgradeService.BoughtSecuritiesUpgrades.Count > 0 });
    }

    private static void LoadManagers1()
    {
        StoreAchievements.Add(new HasManagerSAI(0) { AchievementId = LeaderboardConstants.achievement_unlock_wheeliam });
        StoreAchievements.Add(new HasManagerSAI(1) { AchievementId = LeaderboardConstants.achievement_unlock_din_viesel });
        StoreAchievements.Add(new HasManagerSAI(2) { AchievementId = LeaderboardConstants.achievement_unlock_motto_man });
        StoreAchievements.Add(new HasManagerSAI(3) { AchievementId = LeaderboardConstants.achievement_unlock_tom_tanks });
        StoreAchievements.Add(new HasManagerSAI(4) { AchievementId = LeaderboardConstants.achievement_unlock_ricardo_suave });
        StoreAchievements.Add(new HasManagerSAI(5) { AchievementId = LeaderboardConstants.achievement_unlock_rick_mave });
        StoreAchievements.Add(new HasManagerSAI(6) { AchievementId = LeaderboardConstants.achievement_unlock_nu_clear_jim });
        StoreAchievements.Add(new HasManagerSAI(7) { AchievementId = LeaderboardConstants.achievement_unlock_captain_h_burg });
        StoreAchievements.Add(new HasManagerSAI(8) { AchievementId = LeaderboardConstants.achievement_unlock_captain_bames_t_cork });
        StoreAchievements.Add(new HasManagerSAI(9) { AchievementId = LeaderboardConstants.achievement_unlock_marvin });
    }

    private static void LoadRewards1()
    {
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_win_at_the_slot_machine, IsAttained = () => GameServices.Instance.GameModeService.SlotGameWonCount >= 1 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_win_at_the_slot_machine_10_times, IsAttained = () => GameServices.Instance.GameModeService.SlotGameWonCount >= 10 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_win_at_the_slot_machine_20_times, IsAttained = () => GameServices.Instance.GameModeService.SlotGameWonCount >= 20 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_win_at_the_slot_machine_50_times, IsAttained = () => GameServices.Instance.GameModeService.SlotGameWonCount >= 50 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_win_at_the_slot_machine_100_times, IsAttained = () => GameServices.Instance.GameModeService.SlotGameWonCount >= 100 });

        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_open_your_daily_reward, IsAttained = () => GameServices.Instance.PlayerService.LegacyPlayerData.DailyBonusGathered });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_open_your_daily_reward_for_7_consecutive_days, IsAttained = () => GameServices.Instance.PlayerService.LegacyPlayerData.ConsecutiveDaysEntered == 6 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_level_up, IsAttained = () => GameServices.Instance.PlayerService.Level >= 2 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_reach_level_50, IsAttained = () => GameServices.Instance.PlayerService.Level >= 50 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_reach_level_100, IsAttained = () => GameServices.Instance.PlayerService.Level >= 100 });
        StoreAchievements.Add(new StoreAchievementItem() { AchievementId = LeaderboardConstants.achievement_complete_the_game, IsAttained = () => GameServices.Instance.PlayerService.LegacyPlayerData.GameFinished >= 1 });
    }

    private static void LoadCash1()
    {
        StoreAchievements.Add(new CashEarnedSAI(1e6) { AchievementId = LeaderboardConstants.achievement_millionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e9) { AchievementId = LeaderboardConstants.achievement_billionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e15) { AchievementId = LeaderboardConstants.achievement_quadrillionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e18) { AchievementId = LeaderboardConstants.achievement_quintillionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e33) { AchievementId = LeaderboardConstants.achievement_decillionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e42) { AchievementId = LeaderboardConstants.achievement_tredecillionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e54) { AchievementId = LeaderboardConstants.achievement_septendecillionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e60) { AchievementId = LeaderboardConstants.achievement_novemdecillionaire });
        StoreAchievements.Add(new CashEarnedSAI(1e303) { AchievementId = LeaderboardConstants.achievement_toomuchonaire });
    }

    private static void LoadUpgrades2()
    {
        StoreAchievements.Add(new AllUpgradesOwnedSAI(0) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_rickshaw });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(1) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_taxi });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(2) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_bus });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(3) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_tram });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(4) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_limo });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(5) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_plane });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(6) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_submarine });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(7) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_zeppelin });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(8) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_shuttle });
        StoreAchievements.Add(new AllUpgradesOwnedSAI(9) { AchievementId = LeaderboardConstants.achievement_unlock_all_the_upgrades_for_the_teleporter });
    }
    private static void LoadUpgrades1()
    {
        StoreAchievements.Add(new OwnedUpgradeSAI(0) { AchievementId = LeaderboardConstants.achievement_upgrade_the_rickshaw  });
        StoreAchievements.Add(new OwnedUpgradeSAI(1) { AchievementId = LeaderboardConstants.achievement_upgrade_the_taxi });
        StoreAchievements.Add(new OwnedUpgradeSAI(2) { AchievementId = LeaderboardConstants.achievement_upgrade_the_bus });
        StoreAchievements.Add(new OwnedUpgradeSAI(3) { AchievementId = LeaderboardConstants.achievement_upgrade_the_tram });
        StoreAchievements.Add(new OwnedUpgradeSAI(4) { AchievementId = LeaderboardConstants.achievement_upgrade_the_limo });
        StoreAchievements.Add(new OwnedUpgradeSAI(5) { AchievementId = LeaderboardConstants.achievement_upgrade_the_plane });
        StoreAchievements.Add(new OwnedUpgradeSAI(6) { AchievementId = LeaderboardConstants.achievement_upgrade_the_submarine});
        StoreAchievements.Add(new OwnedUpgradeSAI(7) { AchievementId = LeaderboardConstants.achievement_upgrade_the_zeppelin });
        StoreAchievements.Add(new OwnedUpgradeSAI(8) { AchievementId = LeaderboardConstants.achievement_upgrade_the_shuttle });
        StoreAchievements.Add(new OwnedUpgradeSAI(9) { AchievementId = LeaderboardConstants.achievement_upgrade_the_teleporter });
    }

    private static void LoadUnlocks4()
    {
        StoreAchievements.Add(new GeneratorCountSAI(0, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_rickshaws });
        StoreAchievements.Add(new GeneratorCountSAI(1, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_taxis });
        StoreAchievements.Add(new GeneratorCountSAI(2, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_buses });
        StoreAchievements.Add(new GeneratorCountSAI(3, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_trams });
        StoreAchievements.Add(new GeneratorCountSAI(4, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_limos });
        StoreAchievements.Add(new GeneratorCountSAI(5, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_planes });
        StoreAchievements.Add(new GeneratorCountSAI(6, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_submarines });
        StoreAchievements.Add(new GeneratorCountSAI(7, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_zeppelins });
        StoreAchievements.Add(new GeneratorCountSAI(8, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_shuttles });
        StoreAchievements.Add(new GeneratorCountSAI(9, 5000) { AchievementId = LeaderboardConstants.achievement_own_5000_teleporters });
    }
    private static void LoadUnlocks3()
    {
        StoreAchievements.Add(new GeneratorCountSAI(0, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_rickshaws });
        StoreAchievements.Add(new GeneratorCountSAI(1, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_taxis });
        StoreAchievements.Add(new GeneratorCountSAI(2, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_buses });
        StoreAchievements.Add(new GeneratorCountSAI(3, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_trams });
        StoreAchievements.Add(new GeneratorCountSAI(4, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_limos });
        StoreAchievements.Add(new GeneratorCountSAI(5, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_planes });
        StoreAchievements.Add(new GeneratorCountSAI(6, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_submarines });
        StoreAchievements.Add(new GeneratorCountSAI(7, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_zeppelins });
        StoreAchievements.Add(new GeneratorCountSAI(8, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_shuttles });
        StoreAchievements.Add(new GeneratorCountSAI(9, 1000) { AchievementId = LeaderboardConstants.achievement_own_1000_teleporters });
    }
    private static void LoadUnlocks2()
    {
        StoreAchievements.Add(new GeneratorCountSAI(0, 100) { AchievementId = LeaderboardConstants.achievement_own_100_rickshaws });
        StoreAchievements.Add(new GeneratorCountSAI(1, 100) { AchievementId = LeaderboardConstants.achievement_own_100_taxis });
        StoreAchievements.Add(new GeneratorCountSAI(2, 100) { AchievementId = LeaderboardConstants.achievement_own_100_buses });
        StoreAchievements.Add(new GeneratorCountSAI(3, 100) { AchievementId = LeaderboardConstants.achievement_own_100_trams });
        StoreAchievements.Add(new GeneratorCountSAI(4, 100) { AchievementId = LeaderboardConstants.achievement_own_100_limos });
        StoreAchievements.Add(new GeneratorCountSAI(5, 100) { AchievementId = LeaderboardConstants.achievement_own_100_planes });
        StoreAchievements.Add(new GeneratorCountSAI(6, 100) { AchievementId = LeaderboardConstants.achievement_own_100_sumbarines });
        StoreAchievements.Add(new GeneratorCountSAI(7, 100) { AchievementId = LeaderboardConstants.achievement_own_100_zeppelins });
        StoreAchievements.Add(new GeneratorCountSAI(8, 100) { AchievementId = LeaderboardConstants.achievement_own_100_shuttles });
        StoreAchievements.Add(new GeneratorCountSAI(9, 100) { AchievementId = LeaderboardConstants.achievement_own_100_teleporters });
    }
    
    private static void LoadGenerator()
    {
        StoreAchievements.Add(new OwnedGeneratorSAI(0) { AchievementId = LeaderboardConstants.achievement_your_first_rickshaw });
        StoreAchievements.Add(new OwnedGeneratorSAI(1) { AchievementId = LeaderboardConstants.achievement_your_first_taxi });
        StoreAchievements.Add(new OwnedGeneratorSAI(2) { AchievementId = LeaderboardConstants.achievement_your_first_bus });
        StoreAchievements.Add(new OwnedGeneratorSAI(3) { AchievementId = LeaderboardConstants.achievement_your_first_tram });
        StoreAchievements.Add(new OwnedGeneratorSAI(4) { AchievementId = LeaderboardConstants.achievement_your_first_limo });
        StoreAchievements.Add(new OwnedGeneratorSAI(5) { AchievementId = LeaderboardConstants.achievement_your_first_plane });
        StoreAchievements.Add(new OwnedGeneratorSAI(6) { AchievementId = LeaderboardConstants.achievement_your_first_submarine });
        StoreAchievements.Add(new OwnedGeneratorSAI(7) { AchievementId = LeaderboardConstants.achievement_your_first_zeppelin });
        StoreAchievements.Add(new OwnedGeneratorSAI(8) { AchievementId = LeaderboardConstants.achievement_your_first_shuttle });
        StoreAchievements.Add(new OwnedGeneratorSAI(9) { AchievementId = LeaderboardConstants.achievement_your_first_teleporter });
    }
}

}