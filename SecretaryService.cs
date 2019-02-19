namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;
    using UDebug = UnityEngine.Debug;

    public class SecretaryService : SaveableGameBehaviour, ISecretaryService {
        private Dictionary<int, ReportInfo> Reports { get; } = new Dictionary<int, ReportInfo>();
        private Dictionary<int, SecretaryInfo> Secretaries { get; } = new Dictionary<int, SecretaryInfo>();
        public int HandledViewReportsCount { get; private set; }

        private readonly UpdateTimer secretaryTimer = new UpdateTimer();

        private bool isNeedUpdateOnResume = true;
        private bool isInitialized = false;

        public Lazy<IAuditorService> AuditorService = new Lazy<IAuditorService>(() => {
            return GameServices.Instance.AuditorService;
        });

        public void Setup(object obj) {

            secretaryTimer.Setup(1.0f, (delta) => {
                if (IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame && Services.ManagerService.IsLoaded) {
                    if (IsLoaded && Services.ResourceService.IsLoaded) {
                        UpdateSecretaries(delta);
                    }
                }
            });

            if(!isInitialized) {
                /*
                GameEvents.EfficiencyDropObservable.Subscribe(drop => {
                    OnManagerEfficiencyChanged(drop.Value, drop.Manager);
                }).AddTo(gameObject);*/
                isInitialized = true;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            //GameEvents.EfficiencyChanged += OnManagerEfficiencyChanged;
            GameEvents.SecretaryReportHandled += OnSecretaryHandledReports;
            GameEvents.SecretaryWorkCircleCompleted += OnWorkCompleted;
            GameEvents.ViewShowed += OnViewShowed;
            GameEvents.ViewHided += OnViewHided;
        }

        public override void OnDisable() {
            //GameEvents.EfficiencyChanged -= OnManagerEfficiencyChanged;
            GameEvents.SecretaryReportHandled -= OnSecretaryHandledReports;
            GameEvents.SecretaryWorkCircleCompleted -= OnWorkCompleted;
            GameEvents.ViewShowed -= OnViewShowed;
            GameEvents.ViewHided -= OnViewHided;
            base.OnDisable();
        }

        private void OnWorkCompleted(SecretaryInfo secretary, int count ) {
            int reportCount = GetReportCount(secretary.GeneratorId);
            if(reportCount > 0 ) {
                if(secretary.Count > 0 ) {
                    int handleCount = Mathf.Min(reportCount, count * secretary.Count);
                    int removed = GetReportInfo(secretary.GeneratorId).RemoveReports(handleCount);
                    UDebug.Log($"reports handled => {removed}");
                }
            }
        }

        private void OnViewShowed(ViewType viewType) {
            if(viewType == ViewType.ManagementView) {
                secretaryTimer.SetInterval(0.0167f);
            }
        }

        private void OnViewHided(ViewType viewType ) {
            if(viewType == ViewType.ManagementView ) {
                secretaryTimer.SetInterval(1);
            }
        }

        public override void Update() {
            base.Update();
            if(IsLoaded && Services.ResourceService.IsLoaded && Services.GameModeService.IsGame) {
                secretaryTimer.Update();
            }
        }

        private void OnApplicationPause(bool pause) 
            => UpdateResume(pause);

        private void OnApplicationFocus(bool focus) 
            => UpdateResume(!focus);

        public void UpdateResume(bool pause) {
            //UDebug.Log($"{nameof(UpdateResume)}.{nameof(UpdateResume)}() => {pause}");
            UpdateOnResume(pause);
        }

        private void OnSecretaryHandledReports(int count)
            => AddHandledViewReportsCount(count);

        private void UpdateOnResume(bool isPause) {
            if (isPause) {
                isNeedUpdateOnResume = true;
            } else {
                if (isNeedUpdateOnResume) {
                    isNeedUpdateOnResume = false;
                    StartCoroutine(UpdateOnResumeImpl());
                }
            }
        }

        private IEnumerator UpdateOnResumeImpl() {
            yield return new WaitUntil(() => Services.ResourceService.IsLoaded && IsLoaded && Services.GameModeService.IsGame);
            yield return new WaitUntil(() => Services.ManagerService.IsLoaded && Services.TimeChangeService.IsLoaded && Services.ManagerService.IsWakeUpCompleted);
            ISleepService sleepService = Services.GetService<ISleepService>();
            yield return new WaitUntil(() => sleepService.IsRunning);
            UpdateSecretaries(sleepService.SleepInterval);
            Services.GetService<IConsoleService>().AddOutput("Secretaries resumed", ConsoleTextColor.green, true);
        }

        /*
        private void OnManagerEfficiencyChanged(double change, ManagerInfo manager) {

            //generate reports only for normal transport (planet transport excludes)
            GeneratorData generatorData = Services.ResourceService.Generators.GetGeneratorData(manager.Id);
            if(generatorData.Type == GeneratorType.Normal  && change < 0) {
                GetReportInfo(manager.Id).UpdateReports(GetReportsAdded(manager.Id, change));
            }
        }*/

        public void AddHandledViewReportsCount(int count)
            => HandledViewReportsCount += count;

        public void ResetHandledViewReportsCount()
            => HandledViewReportsCount = 0;

        #region ISecretaryService


        private void UpdateSecretaries(float deltaTime) {
            foreach(var secretary in Secretaries) {
                secretary.Value.Update(deltaTime, AuditorService.Value);
            }
        }

        /*
        public double GetReportsAdded(int generatorId, double managerEfficiencyChange) {

            
            if(managerEfficiencyChange < 0) {

                UnitStrengthData strengthData = Services.ResourceService.UnitStrengthDataRepository.GetStrengthData(generatorId);
                int unitCount = Services.TransportService.GetUnitTotalCount(generatorId);
                if(unitCount == 0 ) {
                    unitCount = 1;
                }

                if(strengthData != null ) {
                    if(generatorId == 0 ) {
                        UDebug.Log($"handle adding report eff change: {managerEfficiencyChange}, ch * 2 * str * uCnt: {Math.Abs(managerEfficiencyChange) * 3 * strengthData.Strength * unitCount}".Attrib(bold: true, color: "cyan", italic: true));
                    }
                    return Math.Abs(managerEfficiencyChange) * 3.0 * strengthData.Strength * unitCount;
                } else {
                    UDebug.Log($"strength data for unit => {generatorId} not found".Colored(ConsoleTextColor.red));
                }
            }
            return 0.0; 

            IManagerService mgrService = Services.ManagerService;
            ReportGenerationData data = new ReportGenerationData {
                CurrentEfficiency = mgrService.CurrentEfficiency(generatorId),
                MinEfficiency = mgrService.MinManagerEfficiency,
                MaxEfficiency = mgrService.MaxEfficiency(generatorId),
                TransportCount = Services.TransportService.GetUnitTotalCount(generatorId),
                CurrentReportCount = GetReportCount(generatorId)
            };
            return ReportUtils.GetGeneratedReports(data);
        }*/

        public double GetEfficiencyChangeForOneReport(int generatorId ) {

            int unitCount = Services.TransportService.GetUnitTotalCount(generatorId);
            if(unitCount == 0 ) { unitCount = 1; }
            IManagerService mgrService = Services.ManagerService;
            EfficiencySpeedData data = new EfficiencySpeedData {
                MinEfficiency = mgrService.MinManagerEfficiency,
                MaxEfficiency = mgrService.MaxEfficiency(generatorId),
                TransportCount = unitCount
            };
            return ReportUtils.GetEfficiencyChangeForOneReport(data);


            /*
            UnitStrengthData strengthData = null;

            try {
                strengthData = Services.ResourceService.UnitStrengthDataRepository.GetStrengthData(generatorId);
                int unitCount = Services.TransportService.GetUnitTotalCount(generatorId);
                if (unitCount == 0) {
                    unitCount = 1;
                }

                if (strengthData != null) {
                    return 1.0 / (3.0 * strengthData.Strength * unitCount);
                } else {
                    return 0;
                }

            } catch(Exception exc) {
                UDebug.Log($"exception strength is null { strengthData == null } for gen {generatorId}");
                return 0;
            }*/
        }

        
        public double GetRestoredEfficiency(int managerId, int reportCompletedCount ) {

            UnitStrengthData strengthData = Services.ResourceService.UnitStrengthDataRepository.GetStrengthData(managerId);

            int unitCount = Services.TransportService.GetUnitTotalCount(managerId);

            if(unitCount == 0 ) {
                unitCount = 1;
            }

            if (strengthData != null) {
                return reportCompletedCount / (3.0 * strengthData.Strength * unitCount);
            } else {
                UDebug.Log($"strength  data for manager => {managerId} is NULL".BoldItalic().Colored(ConsoleTextColor.brown));
            }
            return 0.0;
        }

        public ReportInfo GetReportInfo(int managerId) {
            if(!Reports.ContainsKey(managerId)) {
                Reports.Add(managerId, new ReportInfo(managerId));
            }
            return Reports[managerId];
        }

        public int GetReportCount(int managerId)
            => GetReportInfo(managerId).ReportCount;

        private int CurrentPlanetId
            => Services.GetService<IPlanetService>().CurrentPlanet.Id;

        public float GetSecretarySpeed(int generatorId) {
            SecretaryData secretaryData = Services.ResourceService.SecretaryDataRepository.GetSecretaryData(Services.PlanetService.CurrentPlanetId);
            int reportCount = secretaryData.ReportCountPerSecretary;
            return (float)reportCount / 60.0f;
        }

        public SecretaryInfo GetSecretaryInfo(int generatorId) {
            if(!Secretaries.ContainsKey(generatorId)) {
                Secretaries.Add(generatorId, new SecretaryInfo(generatorId));
            }
            return Secretaries[generatorId];
        }

        public int GetSecretaryCount(int generatorId) {
            return GetSecretaryInfo(generatorId).Count;
        }

        public int GetNextSecretaryPrice(int generatorId) {
            SecretaryData secretaryData = Services.ResourceService.SecretaryDataRepository.GetSecretaryData(Services.PlanetService.CurrentPlanetId);
            return secretaryData.PriceForFirstSecretary + GetSecretaryCount(generatorId) * secretaryData.PriceIncreasingForNextSecretary;
        }

        public BosError BuySecretary(int generatorId) {
            int price = GetNextSecretaryPrice(generatorId);
            Currency currency = Currency.CreateCoins(price);
            if(Services.PlayerService.IsEnough(currency)) {
                Services.PlayerService.RemoveCurrency(currency);
                SecretaryInfo info = GetSecretaryInfo(generatorId);
                int oldCount = info.Count;
                info.AddSecretary(1);
                if(oldCount != info.Count) {
                    GameEvents.OnSecretaryCountChanged(oldCount, info.Count, info);
                }
                return BosError.Ok;
            } else {
                return BosError.NoEnoughCoins;
            }
        }

        public bool IsAllowBuySecretary(int managerId, out BosError error) {
            bool isManagerHired = Services.ManagerService.IsHired(managerId);
            if(!isManagerHired) {
                error = BosError.ManagerNotHired;
                return false;
            }

            int planetId = Services.PlanetService.CurrentPlanet.Id;
            Bos.Data.Currency price = Bos.Data.Currency.CreateCoins(GetNextSecretaryPrice(managerId));
            bool isEnoughCurrency = Services.PlayerService.IsEnough(price);
            if(!isEnoughCurrency) {
                error = BosError.NoEnoughCoins;
                return false;
            }
            error = BosError.Ok;
            return isManagerHired && isEnoughCurrency;
        }

        public void ForceAddReports(int managerId, int reportsCount) 
            => GetReportInfo(managerId: managerId).AddReports(count: reportsCount);

        public bool TryAddReports(int managerId, int reportCount ) {
            if (reportCount > 0) {
                var generatorData = ResourceService.Generators.GetGeneratorData(managerId);
                if (generatorData.Type == GeneratorType.Normal) {
                    var report = GetReportInfo(managerId);
                    if (report != null) {
                        report.AddReports(count: reportCount);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handle countToHandle reports instantly
        /// </summary>
        /// <param name="managerId">Target manager</param>
        /// <param name="countToHandle">Count of reports to handle</param>
        /// <returns>Count of handled reports</returns>
        public int ForceHandle(int managerId, int countToHandle)
            => GetReportInfo(managerId)?.RemoveReports(countToHandle) ?? 0;

        #endregion

        #region SaveableGameBehaviour overrides
        public override string SaveKey => "secretary_service";

        public override Type SaveType => typeof(SecretaryServiceSave);

        public override object GetSave() {
            List<ReportInfoSave> reports = Reports.Values.Select(r => r.GetSave()).ToList();
            List<SecretaryInfoSave> secretaries = Secretaries.Values.Select(s => s.GetSave()).ToList();

            return new SecretaryServiceSave {
                reports = reports,
                secretaries = secretaries,
                handledViewReportsCount = HandledViewReportsCount
            };
        }


        public override void ResetByInvestors() {
            Reports.Clear();
            HandledViewReportsCount = 0;
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            ResetFull();
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void LoadDefaults() {
            Reports.Clear();
            Secretaries.Clear();
            HandledViewReportsCount = 0;
            IsLoaded = true;
        }
        
        public override void ResetByWinGame() {
            LoadDefaults();   
        }

        public override void LoadSave(object obj) {
            SecretaryServiceSave save = obj as SecretaryServiceSave;
            if(save != null ) {
                if(save.reports == null) {
                    save.reports = new List<ReportInfoSave>();
                }
                if(save.secretaries == null) {
                    save.secretaries = new List<SecretaryInfoSave>();
                }

                Reports.Clear();
                save.reports.ForEach(reportSave => Reports.Add(reportSave.managerId, new ReportInfo(reportSave)));
                Secretaries.Clear();
                save.secretaries.ForEach(secSave => Secretaries.Add(secSave.generatorId, new SecretaryInfo(secSave)));
                HandledViewReportsCount = save.handledViewReportsCount;
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        } 
        #endregion
    }


    public interface ISecretaryService : IGameService {
        //double GetReportsAdded(int generatorId, double managerEfficiencyChange);

        ReportInfo GetReportInfo(int managerId);

        int GetReportCount(int managerId);

        float GetSecretarySpeed(int generatorId);

        int GetNextSecretaryPrice(int generatorId);

        SecretaryInfo GetSecretaryInfo(int generatorId);

        int GetSecretaryCount(int generatorId);

        BosError BuySecretary(int generatorId);
        bool IsAllowBuySecretary(int managerId, out BosError error);
        double GetRestoredEfficiency(int managerId, int reportCompletedCount);

        int HandledViewReportsCount { get; }
        void AddHandledViewReportsCount(int count);
        void ResetHandledViewReportsCount();
        void ForceAddReports(int managerId, int reportsCount);
        double GetEfficiencyChangeForOneReport(int generatorId);
        int ForceHandle(int managerId, int countToHandle);
        bool TryAddReports(int managerId, int reportCount);

        bool IsLoaded { get; }
    }

    public class SecretaryServiceSave {
        public List<ReportInfoSave> reports;
        public List<SecretaryInfoSave> secretaries;
        public int handledViewReportsCount;
    }










}