namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LastSiblingLockPanel : GameBehaviour {

        private RectTransform rectTransform;
        private readonly UpdateTimer updateTimer = new UpdateTimer();

        public override void OnEnable() {
            base.OnEnable();
            rectTransform = GetComponent<RectTransform>();
            updateTimer.Setup(0.3f, dt => {
                rectTransform.SetAsLastSibling();
            }, true);
        }

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }
    }

}