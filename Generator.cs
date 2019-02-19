/* 
using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Generator : GameBehaviour, IGenerator {
    [Header("Identification")]
    public int Id;
    public string Name;
    public double BaseCost;
    //public ReactiveProperty<GeneratorState> State = new ReactiveProperty<GeneratorState>();
    public int CoinPrice = 0;
    public int EnhancePrice = 60;


    [Header("Generation Options")]
    private double IncrementFactor;
    public double IncrementFactorTime;
    //public double ProfitIncrementFactor;
    //public double BaseGeneration;
    //public ReactiveProperty<bool> AutomaticGeneration = new ReactiveProperty<bool>(false);
    //public float Efficency = 1.0f;

    [Header("Time Options")]
    //private float TimeToGenerate;
    public int[] CountForHalfing = new int[] { 25, 50, 100, 200, 300, 400 };


    [Header("Unlock Options")]
    public int RequiredOwnedGenerator = 0;
    public bool IsDependent = false;

    [Header("Achievement integration")]
    public bool WillOverrideIcon;
    //public Sprite OverrideSource;
    public Level CurrentLevel;

    public string ManagerIcon;

    //public ReactiveProperty<int> BuyMultiplier = new ReactiveProperty<int>(1);

    private IPlanetService planetService = null;
    private GeneratorInfo generatorInfo = null;


    private int buyMultiplier = 1;

    public void SetBuyMultiplier(int value) { 
        int oldValue = buyMultiplier;
        buyMultiplier = value;
        if(oldValue != buyMultiplier) {
            GameEvents.OnLegacyBuyMultiplierChanged(GeneratorId, value);
        }
    }

    public int BuyMultiplier
        => this.buyMultiplier;

    private IPlanetService PlanetService {
        get {
            if(planetService == null ) {
                planetService = Services.GetService<IPlanetService>();
            }
            return planetService;
        }
    }

    private GeneratorInfo GeneratorInfo {
        get {
            return (generatorInfo != null) ? generatorInfo : (generatorInfo = Services.GenerationService.Generators.GetGeneratorInfo(GeneratorId)); 
        }
    }

    public override void Awake() {
        var prototype = Services.ResourceService.Generators.GetGeneratorData(Id); //GameData.instance.GetGenerator(Id);
        Name = prototype.Name;
        BaseCost = prototype.BaseCost;
        CoinPrice = prototype.CoinPrice;
        EnhancePrice = prototype.EnhancePrice;
        IncrementFactor = prototype.IncrementFactor;
        ManagerIcon = prototype.ManagerIconId;
    }

    //public double GeneratorUnitBuyKoefficient 
    //    => (PlanetService?.CurrentPlanet?.Data?.TransportUnityPriceMult ?? 1.0) * IncrementFactor;



    //public double CalculatePrice(int count, int owned = 0) {
    //    double price = 0.0;

    //    if (GeneratorInfo.State == GeneratorState.Unlockable || GeneratorInfo.State == GeneratorState.Locked) {
    //        price = BaseCost;
    //    } else if (GeneratorInfo.State == GeneratorState.Active) {
    //        price = BaseCost * ((Math.Pow(GeneratorUnitBuyKoefficient, owned) * 
    //            (Math.Pow(GeneratorUnitBuyKoefficient, count) - 1.0f)) / (GeneratorUnitBuyKoefficient - 1.0f));
    //    }


    //    return price;
    //}

    //public float CalculateGenerationTime() {
    //    return TimeToGenerate;
    //}



    //public int GetMaxNumberBuyable(double companyCash, int countOfOwnedGenerators) {
    //    //var k = IncrementFactor;
    //    double baseCost = BaseCost;
    //    //double multiplier = (PlanetService?.CurrentPlanet?.Data?.TransportUnityPriceMult ?? 1.0) * IncrementFactor;
    //    return (int)Math.Floor(Math.Log(((companyCash * (GeneratorUnitBuyKoefficient - 1)) / (baseCost * Math.Pow(GeneratorUnitBuyKoefficient, countOfOwnedGenerators)) + 1), GeneratorUnitBuyKoefficient));
    //}


    #region IGenerator
    public int GeneratorId => Id;

    //public bool IsAutomatic => AutomaticGeneration.Value;

    //public bool IsManual => (!IsAutomatic);

    //public GeneratorState GeneratorState => State.Value;

    #endregion
}*/


