namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class CountOfReportsText : GameBehaviour {

        public Text reportsCountText;

        private int managerId = -100;

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ReportCountChanged += OnReportCountChanged;
        }

        public override void OnDisable() {
            GameEvents.ReportCountChanged -= OnReportCountChanged;
            base.OnDisable();
        }

        private void OnReportCountChanged(int oldReport, int newReport, ReportInfo report) {
            if(report.ManagerId == managerId) {
                UpdateReportsCountText(report);
            }
        }

        public void Setup(int managerId) {
            this.managerId = managerId;
            var report = Services.GetService<ISecretaryService>().GetReportInfo(managerId);
            UpdateReportsCountText(report);
        }

        private void UpdateReportsCountText(ReportInfo report) {
            reportsCountText.text = (report.ReportCount != 0) ? report.ReportCount.ToString() : string.Empty;
        }
    }

}