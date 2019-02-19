using System.Linq;

namespace Bos {
    using Bos.Data;
    using System;
    using UnityEngine;
    using UDBG = UnityEngine.Debug;

    public class ManagerInfo : GameElement {
        public int Id { get; private set; }
        public DateTime NextKickBackTime { get; private set; }
        public double CashOnHand { get; private set; }
        public double CashLifeTime { get; private set; }
        public double KickBacksPayed { get; private set; }

        public double Efficiency(IBosServiceCollection services) {
            if(!services.SecretaryService.IsLoaded) {
                return MaxEfficiency;
            }

            double eff = MaxEfficiency - 
                services.SecretaryService.GetReportCount(Id) * Math.Abs(services.SecretaryService.GetEfficiencyChangeForOneReport(Id));
            if(eff < services.ManagerService.MinManagerEfficiency) {
                eff = services.ManagerService.MinManagerEfficiency;
            }
            return eff;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool HasKickBacks { get; private set; }
        public bool IsHired { get; private set; }
        public double MaxEfficiency { get; private set; }
        public double MaxRollback { get; private set; }
        private double fatigueValue = 0;
        private ManagerData data;

        private double PrevFrameEfficiency = 0;







        public bool IsHireAllowed(double playerCash)
            => playerCash >= Cost;

        public string ManagerProfitMultId
            => $"manager_{Id}";

        public bool IsMaxEfficiency(IBosServiceCollection services)
            => Efficiency(services).Approximately(MaxEfficiency);

        public int EfficiencyPercent(IBosServiceCollection services)
            => (int)(Efficiency(services) * 100);

        public int RollbackPercent
            => (int)(MaxRollback * 100);

        public double BaseCost
            => data?.BaseCost ?? 0;


        public double Cost {
            get
            {
                var currentPlanetId = Services.PlanetService.CurrentPlanetId.Id;
                var planetData = Services.ResourceService.Planets.PlanetCollection.FirstOrDefault(val => val.Id == currentPlanetId);
                if (planetData != null && Id < planetData.ManagersMult.Count)
                {
                    return BaseCost * planetData.ManagersMult[Id];
                }
                return BaseCost;
            }
        }


        private GeneratorData generatorData = null;

        public void ResetByInvestors() {
            CashLifeTime = 0;
            CashOnHand = 0;
            KickBacksPayed = 0;
            //IsHired = false;
        }

        public void UpdateData(ManagerData data) {
            this.data = data;
            MaxEfficiency = data.Coef;
            MaxRollback = data.KickBackCoef;
            UnityEngine.Debug.Log($"Update manager data: {data.Id}");
        }

        public ManagerInfo(ManagerData data, GeneratorData generatorData) {
            this.data = data;
            Id = data.Id;
            MaxEfficiency = data.Coef;          
            MaxRollback = data.KickBackCoef;
            Name = data.Name;
            Description = data.Description;

            PrevFrameEfficiency = MaxEfficiency;
            CashLifeTime = 0;
            CashOnHand = 0;
            KickBacksPayed = 0;
            NextKickBackTime = DateTime.MinValue;
            HasKickBacks = false;
            IsHired = false;
            this.generatorData = generatorData;
        }

        public ManagerInfo(ManagerData data, GeneratorData genData, ManagerInfoSave save)
            : this(data, genData) {
            CashLifeTime = save.cashLifeTime;
            CashOnHand = save.cashOnHand;
            KickBacksPayed = save.kickBacksPayed;
            NextKickBackTime = save.nextKickBackTime;
            HasKickBacks = save.hasKickbacks;
            IsHired = save.isHired;
            fatigueValue = save.fatigueValue;
            if (save.maxEfficiency > 0.0) {
                this.MaxEfficiency = save.maxEfficiency;
            }
            if (save.maxRollback > 0.0) {
                this.MaxRollback = save.maxRollback;
            }
            PrevFrameEfficiency = MaxEfficiency;
        }



        public ManagerInfoSave GetSave()
            => new ManagerInfoSave {
                id = Id,
                cashLifeTime = CashLifeTime,
                cashOnHand = CashOnHand,
                kickBacksPayed = KickBacksPayed,
                nextKickBackTime = NextKickBackTime,
                hasKickbacks = HasKickBacks,
                isHired = IsHired,
                fatigueValue = fatigueValue,
                maxEfficiency = MaxEfficiency,
                maxRollback = MaxRollback
            };

        public void SetCashOnHand(double value) {
            CashOnHand = value;
        }

        public void AddCash(double value, bool isGenerateEvent = false) {
            double oldValue = CashOnHand;
            CashOnHand += value;
            CashLifeTime += value;
            if (isGenerateEvent) {
                if (oldValue != CashOnHand) {
                    GameEvents.OnManagerCashOnHandChanged(oldValue, CashOnHand, this);
                }
            }
        }

        public void SetHasKickBacks(bool isHasKickBacks) {
            HasKickBacks = isHasKickBacks;
        }

        public void ResetNextKickBackTime() {
            NextKickBackTime = GameServices.Instance.TimeService.Now.AddHours(3);
        }

        public double KickBack(double kickBackKoefficient, out bool isFirstKickBack) {
            isFirstKickBack = false;
            if (!HasKickBacks) {
                SetHasKickBacks(true);
                isFirstKickBack = true;
            }

            double payed = CashOnHand * kickBackKoefficient;
            KickBacksPayed += payed;
            SetCashOnHand(0);
            ResetNextKickBackTime();
            return payed;
        }

        public void Hire() {
            if (!IsHired) {
                IsHired = true;
            }
        }



        private void PatchEfficiency() {
        }

        public void AddMaxEfficiency(double value, IBosServiceCollection services) {
            double oldValue = MaxEfficiency;
            MaxEfficiency += value;
            if (oldValue != MaxEfficiency) {
                GameEvents.OnMaxEfficiencyChanged(oldValue, MaxEfficiency, this);
            }
        }

        public void AddMaxRollback(double value) {
            double oldValue = MaxRollback;
            MaxRollback += value;
            if (MaxRollback != oldValue) {
                GameEvents.OnMaxRollbackChanged(oldValue, MaxRollback, this);
            }
        }

        public class LogManagerEfficiencyData {
            public int Id { get; set; }
            public double FatigueSpeed { get; set; }
            public double FatigueForReport { get; set; }
            public double SingleReportInterval => FatigueForReport / FatigueSpeed;
            public double CurrentFatigue { get; set; }

            public override string ToString() {
                return $"ID: {Id}, Fatig. SPEED: {FatigueSpeed}, Fatig. For Report: {FatigueForReport}, Curr. Fatig.: {CurrentFatigue}, Single Report Interval: {SingleReportInterval}";
            }
        }

        private void LogFatigueState(LogManagerEfficiencyData data) {
            //UnityEngine.Debug.Log(data.ToString().Attrib(bold: true, italic: true, color: "cyan", size: 10));
        }

        public void UpdateEfficiency(float deltaTime, IBosServiceCollection services) {

            IManagerService service = services.ManagerService;

            if (generatorData.Type == GeneratorType.Normal && services.PlanetService.IsMoonOpened) {

                if (services.SecretaryService.IsLoaded == false) {
                    return;
                }

                //when efficiency dropped to minimum values than we stop fatig accumulation
                if (BosMath.Approximately(Efficiency(services), service.MinManagerEfficiency)) {
                    fatigueValue = 0;
                    return;
                }




                double managerFatigueSpeed = service.GetManagerFatigueEfficiencySpeed(Id);
                double changeForOneReport = Services.SecretaryService.GetEfficiencyChangeForOneReport(Id);
                double deltaFatig = deltaTime * managerFatigueSpeed;
                fatigueValue += deltaFatig; 
                double oldEfficiency = Efficiency(services);
                double removedTotal = 0;
                LogFatigueState(new LogManagerEfficiencyData { CurrentFatigue = fatigueValue, FatigueForReport = changeForOneReport, FatigueSpeed = managerFatigueSpeed, Id = Id });

                if (fatigueValue >= changeForOneReport ) {
                    
                    int loops = (int)(fatigueValue / changeForOneReport);
                    removedTotal = loops * changeForOneReport;
                    fatigueValue -= Math.Abs(removedTotal);
                    services.SecretaryService.TryAddReports(Id, loops);
                }

                if(removedTotal > 0 ) {
                    var drop = new GameEvents.EfficiencyDrop {
                        Value = -removedTotal,
                        Manager = this
                    };

                    GameEvents.EfficiencyDropObservable.OnNext(drop);
                    GameEvents.OnEfficiencyDrop(drop);
                }
            }
        }

        public void TrackEfficiencyChange(IBosServiceCollection services) {
            double currEff = Efficiency(services);
            if(currEff != PrevFrameEfficiency ) {
                var change = new GameEvents.EfficiencyChange {
                    OldEfficiency = PrevFrameEfficiency,
                    NewEfficiency = currEff,
                    Manager = this
                };
                GameEvents.EfficiencyChangeObservanle.OnNext(change);
                GameEvents.OnEfficiencyChangedEvent(change);
            }
            PrevFrameEfficiency = currEff;
        }

    }

    public class ManagerInfoSave {
        public int id;
        public double cashLifeTime;
        public double cashOnHand;
        public double kickBacksPayed;
        public DateTime nextKickBackTime;
        public bool hasKickbacks;
        public bool isHired;
        //public double efficiency;
        public double fatigueValue;
        public double maxEfficiency;
        public double maxRollback;
    }

    public class ManagerEfficiencyRollbackLevel {

        public int Id { get; private set; }
        public int EfficiencyImproveLevel { get; private set; }
        public int RollbackImrpoveLevel { get; private set; }

        public bool IsMega { get; private set; }
        
        public ManagerEfficiencyRollbackLevel(int id) {
            Id = id;
        }

        public ManagerEfficiencyRollbackLevel(ManagerEfficiencyRollbackLevelSave save) {
            Id = save.id;
            Load(save);
        }

        public void Load(ManagerEfficiencyRollbackLevelSave save) {
            EfficiencyImproveLevel = save.efficiencyImproveLevel;
            RollbackImrpoveLevel = save.rollbackImproveLevel;
            IsMega = save.isMega;
        }

        public ManagerEfficiencyRollbackLevelSave GetSave()
            => new ManagerEfficiencyRollbackLevelSave {
                id = Id,
                efficiencyImproveLevel = EfficiencyImproveLevel,
                rollbackImproveLevel = RollbackImrpoveLevel,
                isMega = IsMega
            };
        

        public void SetEfficiencyLevel(int value) {
            int oldLevel = EfficiencyImproveLevel;
            EfficiencyImproveLevel = value;
            if (EfficiencyImproveLevel != oldLevel) {
                GameEvents.OnEfficiencyImproveLevelChanged(oldLevel, EfficiencyImproveLevel, this);
            }
        }

        public void SetRollbackImproveLevel(int value) {
            int oldLevel = RollbackImrpoveLevel;
            RollbackImrpoveLevel = value;
            if (RollbackImrpoveLevel != oldLevel) {
                GameEvents.OnRollbackImproveLevelChanged(oldLevel, RollbackImrpoveLevel, this);
            }
        }

        public bool IsEfficiencyMaxLevel(IManagerImprovementRepository improveRepo)
            => EfficiencyImproveLevel >= improveRepo.MaxLevel;

        public bool IsRollbackMaxLevel(IManagerImprovementRepository improveRepo)
            => RollbackImrpoveLevel >= improveRepo.MaxLevel;

        public void ApplyMega()
        {
            IsMega = true;
            GameEvents.OnMegaImproveChanged(IsMega, this);
        }
    }

    public class ManagerEfficiencyRollbackLevelSave {
        public int id;
        public int efficiencyImproveLevel;
        public int rollbackImproveLevel;
        public bool isMega;


    }

}