namespace Bos {
    using Bos.Data;
    using System;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    #region Stub Service
    public class StubManagerService : IManagerService {

        public bool HasAvailableManager => false;

        public int HiredCount => 0;

        public float MinManagerEfficiency => 0.05f;

        public Dictionary<int, ManagerInfo> Managers { get; private set; } = new Dictionary<int, ManagerInfo>();

        public bool IsLoaded => true;

        public KickbackInfo CurrentRollbackInfo => null;

        public IEnumerable<ManagerInfo> HiredManagers {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsAnyHired {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsWakeUpCompleted {
            get {
                throw new NotImplementedException();
            }
        }

        public void AddCashOnHand(int managerId, double value) {
            //throw new NotImplementedException();
        }

        public ManagerInfo GetManager(int id) {
            ManagerData managerData = GameServices.Instance.ResourceService.Managers.GetManagerData(id);
            GeneratorData generatorData = GameServices.Instance.ResourceService.Generators.GetGeneratorData(id);
            return new ManagerInfo(managerData, generatorData);
        }

        public double GetManagerFatigueEfficiencySpeed(int managerId) {
            return 0f;
        }

        public void Hire(int managerId) {
            //throw new NotImplementedException();
        }

        public bool IsHired(int managerId) {
            return false;
        }

        public double KickBackManager(int managerId, double kickBackKoefficient) {
            //throw new NotImplementedException();
            return 1f;
        }

        public void OnEfficiencyChanged(double efficiencyChange, ManagerInfo manager) {
            //throw new NotImplementedException();
        }

        public void SetCashOnHand(int managerId, double value) {
            //throw new NotImplementedException();
        }

        public void Setup(object data = null) {
            //throw new NotImplementedException();
        }

        public ManagerTransactionState BuyEfficiencyLevel(int managerId) {
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState BuyRollbackLevel(int managerId) {
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState BuyMegaUpgrade(int managerId)
        {
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState GetBuyEfficiencyLevelStatus(int managerId) {
            return ManagerTransactionState.Success;
        }

        public ManagerTransactionState GetBuyRollbackLevelStatus(int managerId) {
            return ManagerTransactionState.Success;
        }

        public void StartRollback(int managerId, int planetId) {
            throw new NotImplementedException();
        }

        public double FinishRollback() {
            throw new NotImplementedException();
        }

        public double GetManagerPrice(int generatorId) {
            throw new NotImplementedException();
        }

        public void Enhance(GeneratorInfo generator) {
            throw new NotImplementedException();
        }

        public bool IsRallbackAllowed(int managerId) {
            throw new NotImplementedException();
        }

        public void HireManager(int manId, bool free = false) {
            throw new NotImplementedException();
        }

        public void HireManagerFree(int genId) {
            throw new NotImplementedException();
        }

        public int GetNextEfficiencyLevel(int managerId) {
            throw new NotImplementedException();
        }

        public int GetNextRollbackLevel(int managerId) {
            throw new NotImplementedException();
        }

        public ManagerEfficiencyRollbackLevel GetManagerEfficiencyRollbackLevel(int managerId) {
            throw new NotImplementedException();
        }

        public bool IsMegaEfficiencyLevel(ManagerInfo manager) {
            throw new NotImplementedException();
        }

        public bool IsMegaRollbackLevel(ManagerInfo manager) {
            throw new NotImplementedException();
        }

        public void UpdateResume(bool pause) { }

        public double CurrentEfficiency(int managerId) {
            throw new NotImplementedException();
        }

        public double MaxEfficiency(int managerId) {
            throw new NotImplementedException();
        }
    }
    #endregion


  
}