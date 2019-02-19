namespace Bos {
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UDebug = UnityEngine.Debug;

    public class CurrencyInfo : SaveableGameElement, ICurrencyInfo {

        public const double MAX_VALUE = 1.0E+308;

        public double CompanyCash { get; private set; } = 0;
        public double PlayerCash { get; private set; } = 0;
        public int Coins { get; private set; }
        public double Securities { get; private set; } = 0;
        public double LifetimeSecurities { get; private set; } = 0;
        public double MaxCompanyCash { get; private set; } = 1000;
        public double MaxSecurities { get; private set; } = 1000;
        public double MaxPlayerCash { get; private set; } = 1000;


        private double GuardDouble(double input) {
            if(double.IsInfinity(input)) {
                return MAX_VALUE;
            }
            if(input > MAX_VALUE ) {
                return MAX_VALUE;
            }
            return input;
        }

        public void AddCompanyCash(double value) {
            CompanyCash += System.Math.Abs(value);
            CompanyCash = GuardDouble(CompanyCash);

            if (CompanyCash > MaxCompanyCash) {
                double oldMaxCompanyCash = MaxCompanyCash;
                MaxCompanyCash = CompanyCash;
                GameEvents.OnMaxCompanyCashChanged(oldMaxCompanyCash, MaxCompanyCash);
            }
        }

        public void RemoveCompanyCash(double value) {
            CompanyCash -= System.Math.Abs(value);
        }

        public void RemoveSecurities(double value) {
            Securities -= value;
        }

        public void RemovePlayerCash(double value) {
            PlayerCash -= value;
        }

        public void CheckNonNegativeCompanyCash() {
            if(CompanyCash < 0.0 ) {
                CompanyCash = 0;
            }
        }

        public void SetCompanyCash(double value) {
            CompanyCash = value;
            CompanyCash = GuardDouble(CompanyCash);

            if (MaxCompanyCash < CompanyCash) {
                double oldMaxCompanyCash = MaxCompanyCash;
                MaxCompanyCash = CompanyCash;
                GameEvents.OnMaxCompanyCashChanged(oldMaxCompanyCash, MaxCompanyCash);
            }
        }

        public void AddPlayerCash(double value) {
            PlayerCash += value;
            PlayerCash = GuardDouble(PlayerCash);

            if (MaxPlayerCash < PlayerCash) {
                double oldMaxPlayerCash = MaxPlayerCash;
                MaxPlayerCash = PlayerCash;
                GameEvents.OnMaxPlayerCashChanged(oldMaxPlayerCash, MaxPlayerCash);
            }
        }

        public void AddCoins(int value) {
            Coins += Mathf.Abs(value);
        }

        public void SetCoins(int count) {
            Coins = count;
        }

        public void RemoveCoins(int value) {
            Coins -= Mathf.Abs(value);
        }

        public void SetSecurities(double value) {
            Securities = value;
            Securities = GuardDouble(Securities);
            UDebug.Log($"Securities SET => {value}, SEC => {Securities}".Colored(ConsoleTextColor.silver));
            if (MaxSecurities < Securities) {
                double oldMaxSecurities = MaxSecurities;
                MaxSecurities = Securities;
                GameEvents.OnMaxSecuritiesChanged(oldMaxSecurities, MaxSecurities);
            }
        }

        public void AddSecurities(double value) {
            Securities += value;
            Securities = GuardDouble(Securities);
            LifetimeSecurities += value;
            UDebug.Log($"Securities ADD => {value}, SEC => {Securities}".Colored(ConsoleTextColor.silver));
            if (MaxSecurities < Securities) {
                double oldMaxSecurities = MaxSecurities;
                MaxSecurities = Securities;
                GameEvents.OnMaxSecuritiesChanged(oldMaxSecurities, MaxSecurities);
            }
        }

        public override void Register() {
            base.Register();
        }

        public override string SaveKey => "currency_info";

        public override bool IsLoaded { get; protected set; }

        public override Type SaveType => typeof(CurrencyInfoSave);

        public override object GetSave() {
            return new CurrencyInfoSave {
                companyCash = CompanyCash,
                playerCash = PlayerCash,
                coins = Coins,
                securities = Securities,
                lifeTimeSecurities = LifetimeSecurities,
                maxCompanyCash = MaxCompanyCash,
                maxPlayerCash = MaxPlayerCash,
                maxSecurities = MaxSecurities
            };
        }

        public override void ResetByPlanets()
        {
            Securities = 0;
            LifetimeSecurities = 0;

            //reset max values pf currencies to its default values
            ResetMaxValues();

            if(MaxPlayerCash < PlayerCash) {
                MaxPlayerCash = PlayerCash;
            }

            IsLoaded = true;
        }

        

        public override void ResetByInvestors() {
            CompanyCash = Services.GenerationService.CalculatePrice(1, 0, Services.GenerationService.GetGetenerator(0)); //Services.ResourceService.Defaults.startCompanyCash;
            MaxCompanyCash = Services.ResourceService.Defaults.defaultMaxCurrency;
            IsLoaded = true;
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            CompanyCash = Services.ResourceService.Defaults.startCompanyCash;
            PlayerCash = 0;
            Coins = Services.ResourceService.Defaults.startCoins;
            Securities = 0;
            LifetimeSecurities = 0;
            
            ResetMaxValues();
            IsLoaded = true;
        }

        /// <summary>
        /// Reset currencies max values to its defaults values...
        /// </summary>
        private void ResetMaxValues() {
            double defaultMaxCurrency = Services.ResourceService.Defaults.defaultMaxCurrency;
            MaxCompanyCash = defaultMaxCurrency;
            MaxPlayerCash = defaultMaxCurrency;
            MaxSecurities = defaultMaxCurrency;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            CurrencyInfoSave save = obj as CurrencyInfoSave;
            if(save != null ) {
                CompanyCash = save.companyCash;
                PlayerCash = save.playerCash;
                Coins = save.coins;
                Securities = save.securities;
                LifetimeSecurities = save.lifeTimeSecurities;
                MaxCompanyCash = save.maxCompanyCash;
                MaxPlayerCash = save.maxPlayerCash;
                MaxSecurities = save.maxSecurities;

                ValidateMaxValues();
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        private void ValidateMaxValues() {
            if (MaxCompanyCash < CompanyCash) {
                MaxCompanyCash = CompanyCash;
            }

            if (MaxSecurities < Securities) {
                MaxSecurities = Securities;
            }

            if (MaxPlayerCash < PlayerCash) {
                MaxPlayerCash = PlayerCash;
            }
        }

    }

    [Serializable]
    public class CurrencyInfoSave {
        public double companyCash;
        public double playerCash;
        public int coins;
        public double securities;
        public double lifeTimeSecurities;
        public double maxCompanyCash;
        public double maxPlayerCash;
        public double maxSecurities;
    }

    public interface ICurrencyInfo {
        double CompanyCash {
            get;
        }
        double PlayerCash {
            get;
        }
        int Coins {
            get;
        }
        double Securities {
            get;
        }
        bool IsLoaded { get; }
    }
}