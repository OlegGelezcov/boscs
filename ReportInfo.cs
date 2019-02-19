namespace Bos {
    using Bos.Debug;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ReportInfo {
        public int ManagerId { get; private set; }
        public int ReportCount { get; private set; }

        //private double reportTimer;

        public ReportInfo(int managerId) {
            this.ManagerId = managerId;
            ReportCount = 0;
            //reportTimer = 0;
        }

        public ReportInfo(ReportInfoSave save) {
            ManagerId = save.managerId;
            ReportCount = save.reportCount;
           // reportTimer = save.reportTimer;
        }

        /*
        public void UpdateReports(double deltaReport) {
            reportTimer += deltaReport;
            if (ManagerId == 0) {
                UnityEngine.Debug.Log($"report timer for 0 gen is => {reportTimer}".Colored(ConsoleTextColor.yellow));
            }
            int count = (int)Math.Floor(reportTimer);
            if (count > 0) {
                AddReports(count);
                reportTimer -= count;
            }
        }*/

        public void AddReports(int count) {
            int oldReports = ReportCount;
            ReportCount += count;
            if (oldReports != ReportCount) {
                GameEvents.OnReportCountChanged(oldReports, ReportCount, this);
            }
        }

        public int RemoveReports(int count) {
            int oldCount = ReportCount;
            ReportCount -= count;
            if (ReportCount < 0) {
                ReportCount = 0;
            }
            if (oldCount != ReportCount) {
                GameEvents.OnReportCountChanged(oldCount, ReportCount, this);
                GameEvents.OnSecretaryReportHandled(Mathf.Abs(ReportCount - oldCount));
            }
            return oldCount - ReportCount;
        }

        public ReportInfoSave GetSave()
            => new ReportInfoSave {
                managerId = ManagerId,
                reportCount = ReportCount,
                //reportTimer = reportTimer
            };
    }

    public class ReportInfoSave {
        public int managerId;
        public int reportCount;
        //public double reportTimer;
    }

}