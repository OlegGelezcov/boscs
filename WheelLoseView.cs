namespace Bos.UI {
    public class WheelLoseView : GameBehaviour {
        public override void OnDisable() {
            base.OnDisable();
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.WheelCompleted));
        }
    }

}