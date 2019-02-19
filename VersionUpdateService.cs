using System;
using System.Collections;
using System.Collections.Generic;
using Bos;
using Newtonsoft.Json;
using UnityEngine;
using UDBG = UnityEngine.Debug;

namespace Bos
{
    public class VersionUpdateService : SaveableGameBehaviour, IVersionUpdateService
    {
        public Dictionary<string, bool> UpdateFlags { get; } = new Dictionary<string, bool>();

        private readonly string UPDATE_1_KEY = "UPDATE_1_KEY";
        public void CheckUpdate()
        {
            Update1();
        }

        public void Update1()
        {
            if (!UpdateFlags.ContainsKey(UPDATE_1_KEY) || !UpdateFlags[UPDATE_1_KEY])
            {
                var json = PlayerPrefs.GetString("playerData");// "{\"ResearchedGenerators\":{\"0\":true,\"1\":true,\"2\":true,\"3\":true,\"4\":true,\"5\":true,\"6\":true,\"7\":true},\"OwnedGenerators\":{\"0\":19,\"1\":43,\"2\":107},\"EnhancedGenerators\":[0,1],\"TimeMultipliers\":{\"1\":2.0,\"2\":4.0},\"ProfitMultipliers\":{\"0\":62.5,\"2\":4.0,\"7\":15625.0},\"PermanentTimeMultipliers\":{},\"PermanentProfitMultipliers\":{\"0\":125.0,\"7\":15625.0},\"HiredManagers\":{\"0\":true},\"HiredManagerType\":{},\"Efficiencies\":{},\"Speeds\":{},\"CompletedAchievements\":[],\"EarnedBadges\":[4,7,9,10,11,12],\"BoughtUpgrades\":[],\"BoughtTransportUpgrade\":[],\"ProfitUpgradeLevel\":{},\"SpeedUpgradeLevel\":{},\"adsTimers\":{\"0\":\"2018-07-04T08:50:01\",\"1\":\"2018-07-04T08:50:01\",\"2\":\"2018-07-04T08:50:01\",\"3\":\"2018-07-04T08:50:01\",\"4\":\"2018-07-04T08:50:01\",\"5\":\"2018-07-04T08:50:01\"},\"investorNextClaimDate\":\"0001-01-01T00:00:00\",\"managers\":[{\"Id\":0,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":2.7070836213746637E+24,\"CashLifeTime\":2.7070836213746637E+24,\"KickBacksPayed\":0.0,\"Efficienty\":0.5,\"KickBackCoef\":0.25,\"Name\":\"Wheelliam\",\"Desc\":\"Rickshaw Manager\",\"hasKickBacks\":false,\"BaseCost\":50.0},{\"Id\":1,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.5,\"KickBackCoef\":0.25,\"Name\":\"Din Visel\",\"Desc\":\"Taxi Manager\",\"hasKickBacks\":false,\"BaseCost\":4000.0},{\"Id\":2,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.475,\"KickBackCoef\":0.25,\"Name\":\"Motto Man\",\"Desc\":\"Bus Manager\",\"hasKickBacks\":false,\"BaseCost\":95000.0},{\"Id\":3,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.45,\"KickBackCoef\":0.25,\"Name\":\"Tom Tanks\",\"Desc\":\"Tram Manager\",\"hasKickBacks\":false,\"BaseCost\":1000000.0},{\"Id\":4,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.425,\"KickBackCoef\":0.25,\"Name\":\"Ricardo Suave\",\"Desc\":\"Limo Manager\",\"hasKickBacks\":false,\"BaseCost\":54000000.0},{\"Id\":5,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.4,\"KickBackCoef\":0.25,\"Name\":\"Rick Mave\",\"Desc\":\"Plane Manager\",\"hasKickBacks\":false,\"BaseCost\":75000000000.0},{\"Id\":6,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.375,\"KickBackCoef\":0.25,\"Name\":\"N.U. 'Clear' Jim\",\"Desc\":\"Submarine Manager\",\"hasKickBacks\":false,\"BaseCost\":5E+15},{\"Id\":7,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.35,\"KickBackCoef\":0.25,\"Name\":\"Capt. H. Burg\",\"Desc\":\"Zeppelin Manager\",\"hasKickBacks\":false,\"BaseCost\":2E+23},{\"Id\":8,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.325,\"KickBackCoef\":0.25,\"Name\":\"Captain Bames T. Cork\",\"Desc\":\"Shuttle Manager\",\"hasKickBacks\":false,\"BaseCost\":4.3E+33},{\"Id\":9,\"NextKickBack\":\"0001-01-01T00:00:00\",\"CashOnhand\":0.0,\"CashLifeTime\":0.0,\"KickBacksPayed\":0.0,\"Efficienty\":0.3,\"KickBackCoef\":0.25,\"Name\":\"Marvin\",\"Desc\":\"Teleporter Manager\",\"hasKickBacks\":false,\"BaseCost\":7.2E+52}],\"AchiPoints\":25,\"Investors\":0.0,\"LifeTimeInvestors\":0.0,\"GlobalProfitMultiplier\":35184372088832.0,\"GlobalSpeedMultiplier\":32.0,\"PermanentProfitMultiplier\":65536.0,\"PermanentSpeedMultiplier\":1.0,\"Level\":5,\"CurrentXP\":990,\"XPLevelLimit\":4000,\"IsQuickBuyResearched\":false,\"ConsecutiveDaysEntered\":0,\"CurrentOfferExpires\":\"2018-11-12T00:00:00+07:00\",\"DailyBonusGathered\":false,\"DateOfferClicked\":\"0001-01-01T00:00:00\",\"WatchAdStarted\":\"0001-01-01T00:00:00\",\"InvestorEffectiveness\":0.01,\"InvestorNeedForBonus\":1000,\"FirstManagerCashOnHand\":1000,\"AvailableRewards\":4,\"MaxTriesCasino\":3,\"MaxTriesRace\":3,\"HasMicromanager\":true,\"SessionCount\":4,\"UserRated\":false,\"CurrentPromotionIndex\":0,\"CurrentFlashSale\":null,\"FreeCoinsFB\":false,\"FreeCoinsTW\":false,\"LifetimeEarnings\":5.8741193493362806E+46,\"StoredBalance\":5.8741193493362806E+46,\"StoredMaxBalance\":5.8741193493362806E+46,\"LastPauseBalance\":6901.0,\"LocalDataPorted\":true,\"TempMultiplier\":1.0,\"GameFinished\":0}"; //
                if (string.IsNullOrWhiteSpace(json))
                {
                    UpdateFlags[UPDATE_1_KEY] = true;
                    UnityEngine.Debug.Log("Update1 oldPlayerData not exist");
                    return;
                }

                var coin = PlayerPrefs.GetInt("Coin") + 100;
                var data = JsonConvert.DeserializeObject<Update1Data>(json);

                if (data == null)
                {
                    UpdateFlags[UPDATE_1_KEY] = true;
                    UnityEngine.Debug.Log("Update1 data is null");
                    return;
                }

                StartCoroutine(CorrectOldSavesImpl(data, coin));
                UpdateFlags[UPDATE_1_KEY] = true;
            }
        }

        private IEnumerator CorrectOldSavesImpl(Update1Data data, int coins) {


            yield return new WaitUntil(() => Services.PlayerService.IsLoaded);
            Services.PlayerService.SetCompanyCash(data.StoredBalance);
            Services.PlayerService.SetSecurities(data.Investors);
            Services.PlayerService.SetCoins(coins);


            yield return new WaitUntil(() => Services.GenerationService.IsLoaded);
            Services.GenerationService.Generators.AddProfitBoost(
                boost: BoostInfo.CreatePersist(
                    id: "legacy_profit",
                    value: data.PermanentProfitMultiplier));
            Services.GenerationService.Generators.AddTimeBoost(
                boost: BoostInfo.CreatePersist(
                    id: "legacy_time",
                    value: data.PermanentSpeedMultiplier));

            //Services.GenerationService.Generators.SetPermanentProfit(data.PermanentProfitMultiplier);
            //Services.GenerationService.Generators.SetPermanentTime(data.PermanentSpeedMultiplier);
            if(data.PermanentProfitMultipliers != null ) {
                foreach(var kvp in data.PermanentProfitMultipliers) {
                    //Services.GenerationService.Generators.SetPermanentProfit(kvp.Key, kvp.Value);
                    Services.GenerationService.Generators.AddProfitBoost(
                        generatorId: kvp.Key,
                        boost: BoostInfo.CreatePersist(
                            id: "legacy_profit",
                            value: kvp.Value));
                }
            }
            if(data.PermanentTimeMultipliers != null ) {
                foreach(var kvp in data.PermanentTimeMultipliers ) {
                    //Services.GenerationService.Generators.SetPermanentTime(kvp.Key, kvp.Value);
                    Services.GenerationService.Generators.AddTimeBoost(
                        generatorId: kvp.Key,
                        boost: BoostInfo.CreatePersist(
                            id: "legacy_time",
                            value: kvp.Value));
                }
            }

            if (data.EnhancedGenerators != null) {
                foreach (var generator in data.EnhancedGenerators)
                {
                    Services.GenerationService.Enhance(generator);
                }
            }

            if (data.HasMicromanager && !Services.PlayerService.IsHasMicromanager)
                Services.PlayerService.SetHasMicromanager(true);
            
            if (data.IsQuickBuyResearched && !Services.UpgradeService.IsQuickBuyResearched)
                Services.UpgradeService.SetQuickBuyResearched(true);

            Services.RewardsService.AddAvailableRewards(data.AvailableRewards);

            if (data.EarnedBadges != null)
            {
                foreach (var badgeId in data.EarnedBadges)
                {
                    var badge = Services.BadgeService.GetUniqueData(badgeId);
                    if (badge != null && !Services.BadgeService.IsBadgeEarned(badge))
                        Services.BadgeService.EarnBadge(badge); 
                }
            }

            var globalProfit = data.GlobalProfitMultiplier < 1 ? 1 : data.GlobalProfitMultiplier;
            var globalTime = data.GlobalTimeMultiplier < 1 ? 1 : data.GlobalTimeMultiplier;
            Services.GenerationService.Generators.AddProfitBoost(
                boost: BoostInfo.CreateTemp("global_legacy_profit", globalProfit));
            Services.GenerationService.Generators.AddTimeBoost(
                boost: BoostInfo.CreateTemp("global_legacy_time", globalTime));
            //var generatorBonusMult = new GeneratorBonusMult(globalProfit, globalTime);
            //Services.GenerationService.Generators.ApplyGlobal(generatorBonusMult);
            
            UnityEngine.Debug.Log("Update1 complete");

        }


        public void Setup(object data = null)
        {
            StartCoroutine(CheckUpdateWaiterImpl());
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        private IEnumerator CheckUpdateWaiterImpl() {
            yield return new WaitUntil(() => IsLoaded);
            CheckUpdate();
        }
        

        public class Update1Data
        {
            public double StoredBalance;
            public double Investors;
            public double GlobalProfitMultiplier;
            public double GlobalTimeMultiplier;
            public double PermanentProfitMultiplier;
            public double PermanentSpeedMultiplier;
            public bool HasMicromanager;
            public bool IsQuickBuyResearched;
            public int AvailableRewards;
            public int Level;
            public int CurrentXP;
            public List<int> EnhancedGenerators = new List<int>();
            public Dictionary<int, double> TimeMultiplier;
            public Dictionary<int, double> ProfitMultipliers;
            public Dictionary<int, double> PermanentTimeMultipliers;
            public Dictionary<int, double> PermanentProfitMultipliers;
            public List<int> EarnedBadges = new List<int>();
        }


        public override void ResetByWinGame()
        {

        }

        public override void LoadSave(object obj)
        {
            VersionUpdateServiceSave save = obj as VersionUpdateServiceSave;
            if (save != null)
            {
                save.Validate();
                UpdateFlags.Clear();
                UpdateFlags.CopyFrom(save.updateFlags);
                foreach(var kvp in save.updateFlags ) {
                    //UDBG.Log($"update flag: {kvp.Key} => {kvp.Value}");
                }
                IsLoaded = true;
            }
            else
            {
                LoadDefaults();
            }
        }


        public override void LoadDefaults()
        {
            UpdateFlags.Clear();
            IsLoaded = true;
        }

        public override void ResetFull()
        {

        }

        public override void ResetByInvestors()
        {

        }

        public override void ResetByPlanets()
        {

        }
        
        public override string SaveKey => "version_update_service";
        public override Type SaveType => typeof(VersionUpdateServiceSave);
        public override object GetSave()
        {
            return new VersionUpdateServiceSave
            {
                updateFlags = UpdateFlags,
            };
        }

        [Serializable]
        public class VersionUpdateServiceSave
        {
            public Dictionary<string, bool> updateFlags;

            public void Validate() {
                if(updateFlags == null ) {
                    updateFlags = new Dictionary<string, bool>();
                }
            }
        }
    }

    public interface IVersionUpdateService : IGameService
    {
        Dictionary<string, bool> UpdateFlags { get; }
    }
}
