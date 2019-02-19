namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ReportsToggle : GameBehaviour {

        public GameObject alert;

        private int GeneratorId { get; set; } = -1;

        public void Setup(int generatorId ) {
            GeneratorId = generatorId;
            UpdateAlert(Services.SecretaryService.GetReportInfo(GeneratorId));
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ReportCountChanged += OnReportCountChanged;
        }

        public override void OnDisable() {
            GameEvents.ReportCountChanged -= OnReportCountChanged;
            base.OnDisable();
        }

        private void OnReportCountChanged(int oldCount, int newCount, ReportInfo report ) {
            UpdateAlert(report);
        }

        private void UpdateAlert(ReportInfo report) {
            if (report.ManagerId == GeneratorId) {
                alert.ToggleActivity(() => {
                    return  report.ReportCount > 0;
                });
            }
        }


    }

}