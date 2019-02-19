namespace Bos {
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Selectable))]
    public class TutorialLocableSelectable : GameBehaviour {

        public string lockableName;
        private Selectable selectable;

        public override void OnEnable() {
            base.OnEnable();
            selectable = GetComponent<Selectable>();
            GameEvents.TutorialStateActivityChanged += OnTutorialStateChanged;
            GameEvents.TutorialStateCompletedChanged += OnTutorialStateChanged;
            GameEvents.TutorialStateStageChanged += OnTutorialStageChanged;
        }

        public override void OnDisable() {
            GameEvents.TutorialStateActivityChanged -= OnTutorialStateChanged;
            GameEvents.TutorialStateCompletedChanged -= OnTutorialStateChanged;
            GameEvents.TutorialStateStageChanged -= OnTutorialStageChanged;
            base.OnDisable();
        }

        private void UpdateSelectable() {
            if(selectable == null ) { return; }
            selectable.interactable = !Services.TutorialService.IsLockedByState(lockableName);
        }

        private void OnTutorialStateChanged(TutorialState state ) {
            UpdateSelectable();
        }

        private void OnTutorialStageChanged(TutorialState state, int oldStage, int newStage ) {
            UpdateSelectable();
        }
    }

}