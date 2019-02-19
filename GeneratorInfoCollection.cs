namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public class GeneratorInfoCollection : GameElement, IGenerationGlobalContext {

        public BoostCollection ProfitBoosts { get; } = new BoostCollection();
        public BoostCollection TimeBoosts { get; } = new BoostCollection();
        //private readonly List<DateTime> x2ProfitBonuses = new List<DateTime>();
        //private readonly List<DateTime> x2TimeBonuses = new List<DateTime>();


        public Dictionary<int, GeneratorInfo> Generators { get; } = new Dictionary<int, GeneratorInfo>();
        private readonly List<GeneratorInfo> generatorList = new List<GeneratorInfo>();

        /// <summary>
        /// Remove boosts of type sourceType from global collection and from generators
        /// </summary>
        /// <param name="sourceType">Type of boost to remove</param>
        /// <returns>Total count of removed boosts</returns>
        public int RemoveTypedBoosts(BoostSourceType sourceType) {
            int globalProfitCount = ProfitBoosts.Remove(sourceType);
            int globalTimeCount = TimeBoosts.Remove(sourceType);
            int generatorCount = 0;
            foreach (var kvp in Generators) {
                generatorCount += kvp.Value.RemoveProfitBoosts(sourceType);
                generatorCount += kvp.Value.RemoveTimeBoosts(sourceType);
            }

            return generatorCount + globalProfitCount + globalTimeCount;
        }

        public List<GeneratorInfo> PlanetGenerators
            => Generators.Values.Where(g => g.Data.Type == GeneratorType.Planet).ToList();

        public List<GeneratorInfo> NormalGenerators
            => Generators.Values.Where(g => g.Data.Type == GeneratorType.Normal).ToList();

        public IEnumerable<GeneratorInfo> ActiveGenerators
            => Generators.Values.Where(gen => gen.State == GeneratorState.Active);

        public double PlanetProfitBoost
            => Services.PlanetService.CurrentPlanet?.Data?.ProfitMultiplier ?? 1.0;

        public double PlanetTimeBoost
            => Services.PlanetService.CurrentPlanet?.Data?.TimeFactor ?? 1.0;

        public Dictionary<string, double> ProfitBoostComponents
            => new Dictionary<string, double> {
                ["global profit boosts"] = ProfitBoosts.Value,
                ["investor boost"] = SecuritiesBoost,
                ["planet boost"] = PlanetProfitBoost,
                ["time X60"] = TimeChangeBoost,
                //["x2 profit"] = X2Profit
            };

        public Dictionary<string, double> TimeBoostComponents
            => new Dictionary<string, double> {
                ["global time boosts"] = TimeBoosts.Value,
                ["planet time boost"] = PlanetTimeBoost,
                //["x2 time"] = X2Time
            };

        public double ProfitBoostValue
            => ProfitBoosts.Value * 
                PlanetProfitBoost * 
                SecuritiesBoost *
                TimeChangeBoost ;

        public double TimeBoostValue
            => TimeBoosts.Value * PlanetTimeBoost ;

        /*
        public double X2Profit {
            get {
                int countOfValidBonuses = x2ProfitBonuses.Count(date => date > Services.TimeService.Now);
                return (countOfValidBonuses > 0) ? countOfValidBonuses * 2 : 1;
            }
        }

        public double X2Time {
            get {
                int countOfValidBonuses = x2TimeBonuses.Count(date => date > Services.TimeService.Now);
                return (countOfValidBonuses > 0) ? countOfValidBonuses * 2 : 1.0;
            }
        }*/

        public void ClearExpiredBoosts(int currentTime) {
            ProfitBoosts.ClearExpired(currentTime);
            TimeBoosts.ClearExpired(currentTime);
            foreach (var gen in generatorList) {
                gen.ClearExpiredBoosts(currentTime);
            }
        }

        internal ProfitBoostBuilder CreateProfitBoost() {
            return new ProfitBoostBuilder(this);
        }

        internal TimeBoostBuilder CreateTimeBoost() {
            return new TimeBoostBuilder(this);
        }

        internal class TimeBoostBuilder {
            public double Value { get; private set; } = 1.0;

            private GeneratorInfoCollection context;

            public TimeBoostBuilder(GeneratorInfoCollection context) {
                this.context = context;
            }

            public TimeBoostBuilder WithGlobalTimeBoost {
                get {
                    Value *= context.TimeBoosts.Value;
                    return this;
                }
            }

            public TimeBoostBuilder WithPlanetBoost {
                get {
                    Value *= context.PlanetTimeBoost;
                    return this;
                }
            }

            /*
            public TimeBoostBuilder WithX2Time {
                get {
                    Value *= context.X2Time;
                    return this;
                }
            }*/

            public static implicit operator double(TimeBoostBuilder builder)
                => builder.Value;
        }

        internal class ProfitBoostBuilder {
            public double Value { get; private set; } = 1.0;

            private GeneratorInfoCollection context;

            public ProfitBoostBuilder(GeneratorInfoCollection context) {
                this.context = context;
            }

            public ProfitBoostBuilder WithGeneratorGlobalsBoost() {
                Value *= context.ProfitBoosts.Value;
                return this;
            }

            public ProfitBoostBuilder WithPlanetProfitBoost() {
                Value *= context.PlanetProfitBoost;
                return this;
            }

            public ProfitBoostBuilder WithInvestorBoost() {
                Value *= context.SecuritiesBoost;
                return this;
            }

            public ProfitBoostBuilder WithTimeChangeBoost() {
                Value *= context.TimeChangeBoost;
                return this;
            }

            public ProfitBoostBuilder WithGeneratorLocalBoost(GeneratorInfo generator) {
                Value *= generator.ProfitBoosts.Value;
                return this;
            }

            /*
            public ProfitBoostBuilder WithX2Profit() {
                Value *= context.X2Profit;
                return this;
            }*/

            public static implicit operator double(ProfitBoostBuilder builder)
                => builder.Value;
        }



        public int GetUnitCount(int generatorId)
            => Services.TransportService.GetUnitLiveCount(generatorId);



/*
        public void AddX2Profit(DateTime endData)
            => x2ProfitBonuses.Add(endData);

        public void AddX2Time(DateTime endData)
            => x2TimeBonuses.Add(endData);*/
        

        public void UpdateGeneratorBalance() {
            foreach(var genData in Services.ResourceService.Generators.GeneratorCollection ) {
                GetOrAdd(genData.Id).UpdateData(genData);
            }
        }

        #region public methods

        public void Update(float deltaTime) {
            for (int i = 0; i < generatorList.Count; i++) {
                var generator = generatorList[i];
                if (generator.State == GeneratorState.Active) {
                    generator.Update(deltaTime, this);
                }
            }
        }

        public void SetState(int generatorId, GeneratorState newState) {
            var generator = GetOrAdd(generatorId);
            var oldState = generator.State;
            generator.SetState(newState);
            if (oldState != newState) {
                GameEvents.OnGeneratorStateChanged(oldState, generator.State, generator);
            }
        }

        public void SetAutomatic(int generatorId, bool IsAutonatic) {
            GetGeneratorInfo(generatorId).SetAutomatic(IsAutonatic);
        }

        public void AddTimeBoost(int generatorId, BoostInfo boost) {
            GetGeneratorInfo(generatorId).AddTimeBoost(boost);
        }

        public void RemoveTimeBoost(int generatorId, string boostId) {
            GetGeneratorInfo(generatorId).RemoveTimeBoost(boostId);
        }

        public void AddTimeBoost(BoostInfo boost)
            => TimeBoosts.Add(boost);

        public bool RemoveTimeBoost(string boostId)
            => TimeBoosts.Remove(boostId);

        public void AddProfitBoost(int generatorId, BoostInfo boost) {
            GetGeneratorInfo(generatorId).AddProfitBoost(boost);
        }

        public void RemoveProfitBoost(int generatorId, string boostId) {
            GetGeneratorInfo(generatorId).RemoveProfitBoost(boostId);
        }

        public void AddProfitBoost(BoostInfo boost)
            => ProfitBoosts.Add(boost);

        public bool RemoveProfitBoost(string boostId)
            => ProfitBoosts.Remove(boostId);


        public bool IsAutomatic(int generatorId)
            => GetGeneratorInfo(generatorId).IsAutomatic;

        public bool IsManual(int generatorId)
            => GetGeneratorInfo(generatorId).IsManual;

        public GeneratorInfo GetGeneratorInfo(int generatorId) {
            return GetOrAdd(generatorId);
        }

        public bool IsEnhanced(int generatorId)
            => GetGeneratorInfo(generatorId).IsEnhanced;

        public bool IsResearched(int generatorId)
            => GetGeneratorInfo(generatorId).IsResearched;

        public void Research(int generatorId)
            => GetGeneratorInfo(generatorId).Research();

        public void Enhance(int generatorId)
            => GetGeneratorInfo(generatorId).Enhance();

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GLOBAL PROFIT BOOSTS =>");
            sb.AppendLine(ProfitBoosts.ToString());
            sb.AppendLine();
            sb.AppendLine("GLOBAL TIME BOOSTS =>");
            sb.AppendLine(TimeBoosts.ToString());
            sb.AppendLine("--------------------");

            Generators.Values.ToList().ForEach(g => {
                sb.AppendLine(g.ToString());
            });
            return sb.ToString();
        }

        public void ResetFull() {
            ClearGenerators();
            ProfitBoosts.ClearTemp();
            TimeBoosts.ClearTemp();
        }

        public void ResetByInvestors() {
            foreach (var generator in Generators) {
                generator.Value.ClearExceptPermanents(true);
                generator.Value.ClearBoostsByInvestors();
                //generator.Value.ClearTimedBoosts();
                generator.Value.ResetByInvestors();
            }

            ProfitBoosts.ClearTemp();
            TimeBoosts.ClearTemp();
            ProfitBoosts.ClearTimed();
            TimeBoosts.ClearTimed();
            
        }

        public void ResetByPlanets() {
            foreach (var generator in Generators) {
                generator.Value.ClearExceptPermanents();
                generator.Value.DropAutomatic();
                generator.Value.ClearBoostsByPlanet();
                generator.Value.ResetByPlanets();
                
            }
            ProfitBoosts.ClearTemp();
            TimeBoosts.ClearTemp();
            ProfitBoosts.ClearTimed();
            TimeBoosts.ClearTimed();
        }

        public void Clear() {
            foreach (var generator in Generators) {
                generator.Value.ClearExceptPermanents();
                generator.Value.DropAutomatic();
            }

            ProfitBoosts.ClearTemp();
            TimeBoosts.ClearTemp();

        }
        public void ClearGenerators() {
            Generators.Clear();
            generatorList.Clear();
        }
        #endregion

        #region private members

        private double TimeChangeBoost
            => Services.TimeChangeService.TimeMult;

        private double SecuritiesBoost
            => Services.InvestorService.SecuritiesProfitMult;

        public bool Contains(int generatorId)
            => Generators.ContainsKey(generatorId);

        private GeneratorInfo GetOrAdd(int generatorId) {
            if (Contains(generatorId)) {
                return Generators[generatorId];
            } else {
                var data = Services.ResourceService.Generators.GetGeneratorData(generatorId);
                var localData = Services.ResourceService.GeneratorLocalData.GetLocalData(generatorId);
                var generatorInfo = new GeneratorInfo(data, localData);
                AddGeneratorInner(generatorInfo);
                return generatorInfo;
            }
        }

        public void AddGeneratorInner(GeneratorInfo generator) {
            if (!Generators.ContainsKey(generator.GeneratorId)) {
                Generators.Add(generator.GeneratorId, generator);
                generatorList.Add(generator);
            }
        }



        #endregion

    }



    

    public interface IGenerationGlobalContext {
        double ProfitBoostValue { get; }
        double TimeBoostValue { get; }
        int GetUnitCount(int generatorId);
    }

}