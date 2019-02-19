namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TutorialPositionObject : GameBehaviour {

        public string tutorialPositionName = string.Empty;


        public override void OnEnable() {
            base.OnEnable();
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.TutorialPositionObjectActivated, tutorialPositionName));
        }
    }

}