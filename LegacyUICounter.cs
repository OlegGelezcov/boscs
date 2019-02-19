namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LegacyUICounter : GameBehaviour {

        public override void OnEnable() {
            base.OnEnable();
            Services?.ViewService?.PushLegacy(name);
        }

        public override void OnDisable() {
            Services?.ViewService?.PopLegacy(name);
            base.OnDisable();
        }
    }

}