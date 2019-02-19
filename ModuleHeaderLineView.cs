namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ModuleHeaderLineView : GameBehaviour {

        public EventTrigger trigger;

        public override void OnEnable() {
            base.OnEnable();
            Setup();
        }

        private void Setup() {
            trigger.SetPointerListeners((dp) => {
                ScaleIn();
            }, (cp) => {
                Services.GetService<IViewService>().ShowDelayed(ViewType.BuyModuleView, 0.15f);
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            }, (up) => {
                ScaleOut();
            });
        }

        private void ScaleIn()
            => gameObject.GetOrAdd<Vector2Animator>().StartAnimation(
                AnimUtils.GetScaleAnimData(1, 0.95f, 0.15f, EaseType.EaseInOutCubic, gameObject.GetComponent<RectTransform>())
               );
        private void ScaleOut()
            => gameObject.GetOrAdd<Vector2Animator>().StartAnimation(
                AnimUtils.GetScaleAnimData(0.95f, 1, 0.15f, EaseType.EaseInOutCubic, gameObject.GetComponent<RectTransform>())
                );
    }

}