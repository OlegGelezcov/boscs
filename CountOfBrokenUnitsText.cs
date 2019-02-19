namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class CountOfBrokenUnitsText : GameBehaviour {

        public Text countText;

        private int managerId = -100;

        public void Setup(int managerId) {
            this.managerId = managerId;
            int brokenedCount = Services.TransportService.GetUnitBrokenedCount(managerId);
            UpdateText(brokenedCount);
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GeneratorUnitsCountChanged += OnUnitsCountChanged;
        }

        public override void OnDisable() {
            GameEvents.GeneratorUnitsCountChanged -= OnUnitsCountChanged;
            base.OnDisable();
        }

        private void OnUnitsCountChanged(TransportUnitInfo info) {
            if (managerId == info.GeneratorId) {
                UpdateText(info.BrokenedCount);
            }
        }

        private void UpdateText(int count) {
            countText.text = (count > 0) ? count.ToString() : string.Empty;
        }
    }

}