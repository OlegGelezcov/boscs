using Facebook.Unity;

namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public class GeneratorInfo : GameElement {

        public const string kEnhanceBoostName = "enhance";

        public int GeneratorId { get; private set; }
        
        public BoostCollection ProfitBoosts { get; } = new BoostCollection();
        public BoostCollection TimeBoosts { get; } = new BoostCollection();

        public void AddTimeBoost(BoostInfo boost)
            => TimeBoosts.Add(boost);

        public bool RemoveTimeBoost(string boostId)
            => TimeBoosts.Remove(boostId);

        public double GetTimeBoostValue(string boostId)
            => TimeBoosts.GetBoostValue(boostId);


        public void AddProfitBoost(BoostInfo boost)
            => ProfitBoosts.Add(boost);

        public bool RemoveProfitBoost(string boostId)
            => ProfitBoosts.Remove(boostId);

        public double GetProfitBoostValue(string boostId)
            => ProfitBoosts.GetBoostValue(boostId);

        public double TimeOfRound(IGenerationGlobalContext context) {

            double value = RealTime(context);
            if(IsTimeToProfitEnabled(value)) {
                return 0.8;
            }
            return value;
        }

        private double RealTime(IGenerationGlobalContext context) 
            => GeneratorUtils.CalculateTime(this, TimeBoosts.Value * context.TimeBoostValue);

        private bool IsTimeToProfitEnabled(double timePerRound) {
            return IsManual && timePerRound < 0.8f;
        }

        private double TimeToProfit(double time) {
            if(IsTimeToProfitEnabled(time)) {
                return 0.8 / time;
            } else {
                return 1;
            }
        }


        public double ProfitPerRound(IGenerationGlobalContext context,  bool printDetails = false) {
            double realTime = RealTime(context);
            double result = GeneratorUtils.CalculateProfitPerRound(this, context.GetUnitCount(GeneratorId), ProfitBoosts.Value * context.ProfitBoostValue) * TimeToProfit(realTime);

            if (printDetails) {
                //return generator.Data.BaseGeneration * count * profitBoost * Math.Pow(generator.Data.ProfitIncrementFactor, count);
                GeneratorInfoCollection collection = context as GeneratorInfoCollection;
                if (collection != null) {
                    Dictionary<string, double> profitFormulaComponents = new Dictionary<string, double>() {
                        ["BASE GEN"] = Data.BaseGeneration,
                        ["UNIT COUNT"] = context.GetUnitCount(GeneratorId),
                        ["LOCAL BOOSTS"] = ProfitBoosts.Value,
                        ["GLOBAL BOOST"] = context.ProfitBoostValue,
                        ["LOCBOOST * GLOBBOOST"] = ProfitBoosts.Value * context.ProfitBoostValue,
                        ["PROFIT INCR FACTOR"] = Data.ProfitIncrementFactor,

                    };

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine(collection.ProfitBoostComponents.AsString(k => k, v => v.ToString("F2")));
                    sb.AppendLine("**********************");
                    sb.AppendLine(profitFormulaComponents.AsString(k => k, v => v.ToString("F2")));
                    Services.GetService<IConsoleService>().AddOutput(sb.ToString(), ConsoleTextColor.yellow, true);
                }
            }
            return result;
        }

        public void UpdateData(GeneratorData otherData) {
            Data.UpdateFrom(otherData);
            UnityEngine.Debug.Log($"generator update from net: {otherData.Id}");
        }

        public ProfitResult ConstructProfitResult(IGenerationGlobalContext context) {
            double profitPerRound = ProfitPerRound(context);
            double timeOfRound = TimeOfRound(context);
            return new ProfitResult(profitPerRound, profitPerRound / timeOfRound, timeOfRound);
        }

        public double ProfitPerSec(IGenerationGlobalContext context)
            => ProfitPerRound(context) / TimeOfRound(context);
        

       
        private bool isGenerationStarted = false;
        private double accumulatedCash = 0.0;

        public ProfitResult ProfitResult { get; } = new ProfitResult(0, 0, 0);

        public GeneratorState State { get; private set; } = GeneratorState.Unknown;

        public bool IsAutomatic { get; private set; }

        public bool IsResearched { get; private set; }

        public bool IsEnhanced { get; private set; }

        public float GenerateTimer { get; private set; }
        public int BuyCountButtonState { get; private set; }
        public GeneratorData Data { get; private set; }
        public GeneratorLocalData LocalData { get; }


        public double Cost
        {
            get
            {
                if (GeneratorId > 9) return Data.BaseCost;
            
                var currentPlanetId = Services.PlanetService.CurrentPlanetId.Id;
                var planetData = Services.PlanetService.GetPlanet(currentPlanetId).Data;
                if (planetData != null)
                {
                    //only for debug output
                    /*if(GeneratorId == 0 ) {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"GEN COST => base cost: {Data.BaseCost}, planet mult: {planetData.GeneratorsMult[GeneratorId]}");
                        UnityEngine.Debug.Log(sb.ToString().Bold().Colored(ConsoleTextColor.cyan));

                    }*/

                    return Data.BaseCost * planetData.GeneratorsMult[GeneratorId];
                }
                return Data.BaseCost;
            }
        }


        //public void UpdateData(GeneratorData data ) {
        //    Data = data;
        //}

        #region constructors


        public GeneratorInfo(GeneratorData data, GeneratorLocalData localData) {
            this.GeneratorId = data.Id;
            IsEnhanced = false;
            Data = data;
            LocalData = localData;
            GenerateTimer = 0f;
            BuyCountButtonState = 1;
            IsResearched = GeneratorId.IsRickshawOrTaxi();

        }
        public GeneratorInfo(int generatorId, BoostInfo profitBoost, BoostInfo timeBoost,  GeneratorData data, GeneratorLocalData localData) {
            this.GeneratorId = generatorId;
            this.ProfitBoosts.Add(profitBoost);
            this.TimeBoosts.Add(timeBoost);

            this.IsEnhanced = false;
            this.Data = data;
            this.LocalData = localData;
            this.GenerateTimer = 0f;
            this.BuyCountButtonState = 1;
            this.IsResearched = generatorId.IsRickshawOrTaxi();
        }

        public GeneratorInfo(GeneratorInfoSave save,  GeneratorData data, GeneratorLocalData localData) {
            save.Guard();
            this.Data = data;
            this.LocalData = localData;
            this.GeneratorId = save.generatorId;
            this.IsResearched = save.isResearched;
            this.IsEnhanced = save.isEnhanced;
            this.GenerateTimer = save.generateTimer;
            this.isGenerationStarted = save.isGenerationStarted;
            this.IsAutomatic = save.isAutomatic;
            this.State = (GeneratorState)save.state;
            this.BuyCountButtonState = save.buyCountButtonState;
            this.ProfitBoosts.Load(save.profitBoosts);
            this.TimeBoosts.Load(save.timeBoosts);

            if(GeneratorId.IsRickshawOrTaxi()) {
                IsResearched = true;
            }
            if (IsEnhanced) {
                AddTimeBoost(BoostInfo.CreateTemp(kEnhanceBoostName, 10));
            }
        }
        #endregion

        public int ResearchPrice(IPlanetService planets)
            => LocalData.GetResearchPrice(planets.CurrentPlanetId.Id).Match(() => int.MaxValue, val => val.price);

        public double AccumulatedCash
            => accumulatedCash;

        public void AddGenerateTimer(float value) 
            => GenerateTimer += value;

        public void SetGenerateTimer(float value)
            => GenerateTimer = value;

        public bool IsManual
            => !IsAutomatic;

        public bool IsDependent
            => LocalData.dependent;

        public int RequiredGeneratorId
            => LocalData.required_id;

        public int PlanetId
            => LocalData.planet_id;
                  
        public bool IsGenerationStarted{
            get {
                if(IsAutomatic) {
                    isGenerationStarted = true;
                } 
                return isGenerationStarted;
            }
        }
        public void SetGenerationStarted(bool value){
            if(IsAutomatic) {
                isGenerationStarted = true;
            } else {
                isGenerationStarted = value;
            }
        }

        public void StartGeneration() {
            accumulatedCash = 0;
            SetGenerateTimer(0);
            SetGenerationStarted(true);
        }

        public void SetAccumulatedCash(double val) {
            accumulatedCash = val;
        }

        public void ForceFinalization() {
            SetGenerateTimer(0);
            GameEvents.OnAccumulationCompleted(this, ProfitResult);
            accumulatedCash = 0;
        }


        public void ResetByPlanets() {
            SetGenerateTimer(0);
            accumulatedCash = 0;
            if (Data.Type == GeneratorType.Normal) {
                SetAutomatic(false);
                SetGenerationStarted(false);
            }
        }

        public void ResetByInvestors() {
            SetGenerateTimer(0);
            accumulatedCash = 0;
        }


        public double UpdateAutomaticAfterSleep(float sleepTime, IGenerationGlobalContext context) {
            if (IsAutomatic) {
                if (!IsGenerationStarted) {
                    SetGenerationStarted(true);
                }
            }
            double loopInterval = TimeOfRound(context);
            double profit = ProfitPerRound(context);
            double profitPerSec = profit / loopInterval;
            int loopsCount = (int)(sleepTime / loopInterval);
            double val = loopsCount * profit;
            Services.PlayerService.AddGenerationCompanyCash(val);
            Update((float)(sleepTime - loopsCount * loopInterval), context);
           // UnityEngine.Debug.Log($"gen: {GeneratorId}, accumulated cash: {accumulatedCash}");
            return val;
        }

        public void ClearExpiredBoosts(int currentTime) {
            ProfitBoosts.ClearExpired(currentTime);
            TimeBoosts.ClearExpired(currentTime);
        }



        public int RemoveProfitBoosts(BoostSourceType sourceType)
            => ProfitBoosts.Remove(sourceType);

        public int RemoveTimeBoosts(BoostSourceType sourceType)
            => TimeBoosts.Remove(sourceType); 

        public void Update(float deltaTime, IGenerationGlobalContext context) {

            double time = TimeOfRound(context);
            double profit = ProfitPerRound(context);
            double profitPerSec = profit / time;            
            ProfitResult.UpdateFromOther(new ProfitResult(profit, profitPerSec, time));
            
            if(IsAutomatic) {
                if(!IsGenerationStarted) {
                    SetGenerationStarted(true);
                }
            }
            if(IsGenerationStarted) {
                AddGenerateTimer(deltaTime);
                double interval = AccumulateInterval;
                GameEvents.OnAccumulationProgressChanged(this, GenerateTimer, interval, ProfitResult);

                accumulatedCash += ProfitResult.ValuePerSecond * deltaTime;
                if(GenerateTimer >= interval) {
                    GameEvents.OnAccumulationCompleted(this, ProfitResult);
                    SetGenerateTimer(0f);
                    GameEvents.OnAccumulationProgressChanged(this, GenerateTimer, interval, ProfitResult);
                    accumulatedCash = 0.0;
                    if(IsManual) {
                        SetGenerationStarted(false);
                    }
                }
            }
        }

        public void FireProgressEvent() {
            GameEvents.OnAccumulationProgressChanged(this, GenerateTimer, AccumulateInterval, ProfitResult);
        }

        public double AccumulateInterval {
            get {
                if (IsManual) {
                    return this.ProfitResult.GenerationInterval;
                } else
                {
                    var settings = Services.ResourceService.Defaults;
                    if (this.ProfitResult.GenerationInterval < settings.minManualGeneratorInterval) {
                        return settings.minManualGeneratorInterval;
                    } else {
                        return this.ProfitResult.GenerationInterval;
                    }
                }
            }
        }

        public double GenerationIntervalRatio {
            get {
                if (IsAutomatic) {
                    return 1.0;
                }
                var settings = Services.ResourceService.Defaults;
                double ratio = (settings.minManualGeneratorInterval / ProfitResult.GenerationInterval);
                if (ratio < 1.0) {
                    ratio = 1.0;
                }

                return ratio;
            }
        }

        public float RemainTime
            => (float)(AccumulateInterval - GenerateTimer);

        //public void ClearTimedBoosts() {
        //    ProfitBoosts.ClearTimed();
        //    TimeBoosts.ClearTimed();
        //}

        public void ClearBoostsByInvestors() {
            ProfitBoosts.ClearTemp();
            TimeBoosts.ClearTemp(kEnhanceBoostName);
        }

        public void ClearBoostsByPlanet() {
            ProfitBoosts.ClearTemp();
            TimeBoosts.ClearTemp();
            IsEnhanced = false;
        }

        public void ClearExceptPermanents(bool research = false) {

            if(GeneratorId.IsRickshawOrTaxi()) {
                IsResearched = true;
            } else {
                IsResearched = research && IsResearched;
            }
            GenerateTimer = 0;        
        }

        public void DropAutomatic() {
            IsAutomatic = false;
            isGenerationStarted = false;
        }

        public void SetBuyCountButtonState(int state)
            => BuyCountButtonState = state;



        public void SetState(GeneratorState newState) {
            State = newState;
        }


        public void SetAutomatic(bool isAuto) {
            bool oldValue = IsAutomatic;
            IsAutomatic = isAuto;
            if(isAuto) {
                SetGenerationStarted(true);
            }
            if(oldValue != IsAutomatic) {
                GameEvents.OnAutomaticChanged(GeneratorId, IsAutomatic);
            }
        }

        public void Research() {
            if(!IsResearched) {
                IsResearched = true;
                GameEvents.OnGeneratorResearched(this);
            }
        }

        public void Enhance() {
            if(!IsEnhanced) {
                IsEnhanced = true;
                AddTimeBoost(BoostInfo.CreateTemp(kEnhanceBoostName, 10));
                GameEvents.OnGeneratorEnhanced(this);
            }
        }

        public GeneratorInfoSave GetSave() {
            return new GeneratorInfoSave {
                generatorId = GeneratorId,
                isResearched = IsResearched,
                isEnhanced = IsEnhanced,
                generateTimer = GenerateTimer,
                isGenerationStarted = IsGenerationStarted,
                isAutomatic = IsAutomatic,
                state = (int)State,
                buyCountButtonState = BuyCountButtonState,
                profitBoosts = ProfitBoosts.GetSave(),
                timeBoosts = TimeBoosts.GetSave()
            };
        }
    }
    
    

    public class GeneratorInfoSave {
        public int generatorId;

        public BoostCollectionSave profitBoosts;
        public BoostCollectionSave timeBoosts;
        
        public bool isResearched;
        public bool isEnhanced;
        public float generateTimer;
        public bool isGenerationStarted;
        public bool isAutomatic;
        public int state;
        public int buyCountButtonState = 1;

        public void Guard() {
            if (profitBoosts == null) {
                profitBoosts = new BoostCollectionSave();
            }

            if (timeBoosts == null) {
                timeBoosts = new BoostCollectionSave();
            }
        }
    }

    public static class GeneratorUtils {

        public static double CalculateTime(GeneratorInfo generator, double timeBoost ) {
            return generator.Data.TimeToGenerate / timeBoost;
        }

        public static double CalculateProfitPerRound(GeneratorInfo generator, int count, double profitBoost) {
            return generator.Data.BaseGeneration * count * profitBoost * Math.Pow(generator.Data.ProfitIncrementFactor, count);
        }

        public static double CalculateProfitOnTime(GeneratorInfo generator, int count, double profitBoost, double time, double timeBoost) {
            double profitPerRound = CalculateProfitPerRound(generator, count, profitBoost);
            double interval = CalculateTime(generator, timeBoost);
            double loops = time / interval;
            return profitPerRound * loops;
        }
    }

}
