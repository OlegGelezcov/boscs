namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MechanicsToggle : GameBehaviour {

        public GameObject alert;

        private int GeneratorId { get; set; } = -1;

        public void Setup(int generatorId) {
            GeneratorId = generatorId;
            UpdateAlert(Services.TransportService.GetUnit(GeneratorId));
        }


        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GeneratorUnitsCountChanged += OnUnitCountChanged;
        }

        public override void OnDisable() {
            GameEvents.GeneratorUnitsCountChanged -= OnUnitCountChanged;
            base.OnDisable();
        }

        private void OnUnitCountChanged(TransportUnitInfo unit ) {
            UpdateAlert(unit);
        }

        private void UpdateAlert(TransportUnitInfo info ) {
            if (info.GeneratorId == GeneratorId) {
                alert.ToggleActivity(() => {
                    return  info.BrokenedCount > 0;
                });
            }
        }
    }

}