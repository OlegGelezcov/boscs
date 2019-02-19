namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FlightShipView : GameBehaviour {

        public List<ModuleItemView> modules;

        public void Setup() {
            modules.ForEach(m => {
                if (Services.Modules.IsOpened(m.moduleId)) {
                    m.Activate();
                } else {
                    m.Deactivate();
                }
            });
        }
    }

}