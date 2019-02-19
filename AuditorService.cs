namespace Bos {
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;
    using UDBG = UnityEngine.Debug;

    public class AuditorService : SaveableGameBehaviour, IAuditorService {

        public Dictionary<int, Dictionary<string, Auditor>> Auditors { get; } = new Dictionary<int, Dictionary<string, Auditor>>();
        private readonly UpdateTimer updateTimer = new UpdateTimer();
        private bool isNeedUpdateOnResume = true;
        public Dictionary<int, int> SpeedMults { get; } = new Dictionary<int, int>();
        private bool IsInitialized { get; set; } = false;

        public void Setup(object obj) {
            updateTimer.Setup(1f, dt => {
                if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                    UpdateOnInterval(dt);
                }
            });
            if(!IsInitialized) {

                GameEvents.AuditorReportHandleObservable.Subscribe(info => {
                    int repairedCount = Services.SecretaryService.GetReportInfo(info.Auditor.GeneratorId).RemoveReports(info.RepairCount);
                    UDBG.Log($"repaied count => {repairedCount}");
                }).AddTo(gameObject);

                IsInitialized = true;
            }
        }

        private void UpdateOnInterval(float deltaTime ) {
            foreach(var kvp in Auditors ) {
                foreach(var kvp2 in kvp.Value) {
                    kvp2.Value.Update(deltaTime, this);
                }
            }
            RemoveCompletedAuditors();
        }

        private void RemoveCompletedAuditors() {
            foreach(var kvp in Auditors) {
                RemoveCompletedAuditors(kvp.Value);
            }
        }

        private int RemoveCompletedAuditors(Dictionary<string, Auditor> auditors) {
            int removeCount = 0;
            List<string> completedIds = auditors.Where(kvp => kvp.Value.IsCompleted).Select(kvp => kvp.Key).ToList();
            List<Auditor> removedAuditors = new List<Auditor>();
            foreach(string id in completedIds) {
                if(auditors.ContainsKey(id)) {
                    var toRemove = auditors[id];
                    if(auditors.Remove(id)) {
                        removeCount++;
                        removedAuditors.Add(toRemove);
                    }
                }
            }
            if(removeCount > 0 ) {
                GameEvents.OnAuditorsRemoved(removedAuditors);
            }
            return removeCount;
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.AuditorStateChanged += OnAuditorStateChanged;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
        }

        public override void OnDisable() {
            GameEvents.AuditorStateChanged -= OnAuditorStateChanged;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            base.OnDisable();
        }

        public override void Update() {
            base.Update();
            if(IsLoaded) {
                updateTimer.Update();
            }
        }

        private void OnApplicationPause(bool pause) {
            UpdateResume(pause);
        }

        private void OnApplicationFocus(bool focus) {
            UpdateResume(!focus);
        }

        public void UpdateResume(bool pause)
            => UpdateAuditorsOnResume(pause);

        private void UpdateAuditorsOnResume(bool isPause) {
            if(isPause) {
                isNeedUpdateOnResume = true;
            } else {
                if(isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateAuditorsOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateAuditorsOnResumeImpl() {
            yield return new WaitUntil(() =>
                Services.ResourceService.IsLoaded &&
                Services.GameModeService.IsGame &&
                Services.SleepService.IsRunning &&
                IsLoaded &&
                Services.TransportService.IsLoaded &&
                Services.TimeChangeService.IsLoaded);
            UpdateOnInterval(Services.SleepService.SleepInterval);
        }

        public bool IsConditionsForBuyValid(GeneratorInfo generator) {
            bool isManagerHired = Services.ManagerService.IsHired(generator.GeneratorId);
            if (!isManagerHired) {
                return false;
            }
            int reportCount = Services.SecretaryService.GetReportCount(generator.GeneratorId);
            if(reportCount <= 0 ) {
                return false;
            }

            double price = GetAuditorPrice(generator, reportCount);
            var currency = Bos.Data.Currency.CreateCompanyCash(price);
            if(!Services.PlayerService.IsEnough(currency)) {
                return false;
            }
            return true;
        }

        public double GetAuditorPrice(GeneratorInfo generator, int reportCount ) {
            var secretaryData = ResourceService.SecretaryDataRepository.GetSecretaryData(Planets.CurrentPlanetId);
            if(secretaryData == null ) {
                return 0;
            }
            double ratio = (double)reportCount / (double)secretaryData.ReportCountProcessedPer10Seconds;
            if(ratio.Approximately(0.0)) {
                ratio = 1.0;
            }
            return ratio * GetAuditorPriceBase(generator);
        }

        private double GetAuditorPriceBase(GeneratorInfo generator) {
            var secretaryData = ResourceService.SecretaryDataRepository.GetSecretaryData(Planets.CurrentPlanetId);
            double pricePerOneReport = MechanicSecretaryHelper.GetUnitPriceForMechanicSecretaryPrice(generator, (int)(secretaryData?.AuditCashPrice ?? 2.0)) / secretaryData.ReportCountProcessedPer10Seconds;
            return pricePerOneReport * Mathf.Min(secretaryData.ReportCountProcessedPer10Seconds, Services.SecretaryService.GetReportCount(generator.GeneratorId));
        }

        public BosError Buy(GeneratorInfo generator ) {
            int reportCount = Services.SecretaryService.GetReportCount(generator.GeneratorId);
            if(reportCount <= 0 ) {
                return BosError.NoReports;
            }
            double price = GetAuditorPrice(generator, reportCount);
            var currency = Bos.Data.Currency.CreateCompanyCash(price);
            if (!Services.PlayerService.IsEnough(currency)) {
                return BosError.NotEnoughCompanyCash;
            }

            var secretaryData = Services.ResourceService.SecretaryDataRepository.GetSecretaryData(Services.PlanetService.CurrentPlanetId);
            Services.PlayerService.RemoveCurrency(currency);    

            //int handledReportCount = Mathf.Min(reportCount, secretaryData.ReportCountProcessedPer10Seconds);
            Auditor newAuditor = new Auditor(generator.GeneratorId, reportCount, secretaryData.AuditorSpeed);
            AddAuditor(newAuditor);
            return BosError.Ok;
        }

        public bool IsBusy(GeneratorInfo generator, out float progress) {
            progress = 0;
            var auditors = GetAuditors(generator.GeneratorId);
            if (auditors.Count > 0) {
                var busyAuditor = auditors.Values.FirstOrDefault(a => !a.IsCompleted);
                if (busyAuditor != null) {
                    progress = busyAuditor.TotalProgress;
                    return true;
                }
            }

            return false;
        }


        public bool IsAdApplicable(int generatorId)
            => Planets.IsMoonOpened && Services.ManagerService.IsHired(generatorId);

        public void ApplyAd(int generatorId ) {
            if (IsAdApplicable(generatorId)) {
                MultSpeed(generatorId, 2);
            }
        }

        public int GetSpeedMult(int generatorId) {
            if (!SpeedMults.ContainsKey(generatorId)) {
                SpeedMults.Add(generatorId, 1);
            }
            return SpeedMults[generatorId];
        }

        private void MultSpeed(int generatorId, int val) {
            if (!SpeedMults.ContainsKey(generatorId)) {
                SpeedMults.Add(generatorId, 1);
            }
            SpeedMults[generatorId] *= val;
            UniRx.MessageBroker.Default.Publish<SpeedMultChangedArgs>(new SpeedMultChangedArgs { SpeedModifier = this, Name = "secretary", GeneratorId = generatorId });
        }

        public Dictionary<string, Auditor> GetAuditors(int generatorId ) {
            if(!Auditors.ContainsKey(generatorId)) {
                Auditors.Add(generatorId, new Dictionary<string, Auditor>());
            }
            return Auditors[generatorId];
        }

        private void AddAuditor(Auditor auditor) {
            if(Auditors.ContainsKey(auditor.GeneratorId)) {
                Auditors[auditor.GeneratorId].Add(auditor.Id, auditor);
            } else {
                Dictionary<string, Auditor> generatorAuditors = new Dictionary<string, Auditor> {
                    [auditor.Id] = auditor
                };
                Auditors.Add(auditor.GeneratorId, generatorAuditors);
            }
            auditor.StartAuditor();
            GameEvents.OnAuditorAdded(auditor);
        }

        private int GetCountOfActiveAuditors(int generatorId ) {
            if(Auditors.ContainsKey(generatorId)) {
                return Auditors[generatorId].Where(kvp => !kvp.Value.IsCompleted).Count();
            }
            return 0;
        }

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "auditor_service";

        public override Type SaveType => typeof(AuditorServiceSave);

        public override object GetSave() {
            Dictionary<int, Dictionary<string, AuditorSave>> auditorSaves = new Dictionary<int, Dictionary<string, AuditorSave>>();
            foreach (var kvp in Auditors) {
                if (kvp.Value.Count > 0) {
                    Dictionary<string, AuditorSave> generatorAuditors = new Dictionary<string, AuditorSave>();
                    foreach (var kvp2 in kvp.Value) {
                        AuditorSave saveObj = kvp2.Value.GetSave();
                        generatorAuditors.Add(saveObj.id, saveObj);
                    }
                    auditorSaves.Add(kvp.Key, generatorAuditors);
                }
            }
            return new AuditorServiceSave {
                auditors = auditorSaves,
                speedMults = SpeedMults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }

        public override void LoadDefaults() {
            Auditors.Clear();
            SpeedMults.Clear();
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj) {
            AuditorServiceSave save = obj as AuditorServiceSave;
            if (save != null) {
                Auditors.Clear();
                save.Validate();
                foreach (var kvp in save.auditors) {
                    Dictionary<string, Auditor> generatorAuditors = new Dictionary<string, Auditor>();
                    foreach (var kvp2 in kvp.Value) {
                        generatorAuditors.Add(kvp2.Key, new Auditor(kvp2.Value));
                    }
                    Auditors.Add(kvp.Key, generatorAuditors);
                }
                SpeedMults.Clear();
                SpeedMults.CopyFrom(save.speedMults);
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public override void ResetByInvestors() {
            LoadDefaults();
        }

        public override void ResetByPlanets() {
            LoadDefaults();
        }

        public override void ResetFull() {
            LoadDefaults();
        }
        #endregion

        #region GameEvents
        private void OnAuditorStateChanged(AuditorState oldState, AuditorState newState, Auditor auditor ) {
            if(newState == AuditorState.Completed) {
                //int removedReports = Services.SecretaryService.GetReportInfo(auditor.GeneratorId).RemoveReports(auditor.Count);
                //UDBG.Log($"reports handled count => {removedReports}");
            }
        }

        private void OnViewShowed(ViewType type ) {
            if(type == ViewType.ManagementView) {
                updateTimer.SetInterval(0.005f);
            } 
        }

        private void OnViewHided(ViewType type) {
            if(type == ViewType.ManagementView) {
                updateTimer.SetInterval(1);
            }
        }
        #endregion
    }

    public class SpeedMultChangedArgs {
        public ISpeedModifier SpeedModifier { get; set; }
        public string Name { get; set; }
        public int GeneratorId {get; set;}
    }

    public interface IAuditorService : IGameService, ISpeedModifier {
        bool IsConditionsForBuyValid(GeneratorInfo generator);
        BosError Buy(GeneratorInfo generator);
        double GetAuditorPrice(GeneratorInfo generator, int reportCount);
        //bool IsAdValid(int generatorId);
        bool IsAdApplicable(int generatorId);
        void ApplyAd(int generatorId);
        Dictionary<string, Auditor> GetAuditors(int generatorId);
        bool IsBusy(GeneratorInfo generator, out float progress);
    }

    public class AuditorServiceSave {
        public Dictionary<int, Dictionary<string, AuditorSave>> auditors;
        public Dictionary<int, int> speedMults;

        public void Validate() {
            if(auditors == null ) {
                auditors = new Dictionary<int, Dictionary<string, AuditorSave>>();
            }
            if(speedMults == null ) {
                speedMults = new Dictionary<int, int>();
            }
        }
    }

}