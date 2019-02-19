namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class TriggerAnimation : GameBehaviour {

        public string triggerName;


        public override void OnEnable() {
            base.OnEnable();
            var animator = GetComponent<Animator>();

            if (triggerName.IsNonEmpty()) {
                
                animator?.SetTrigger(triggerName);
            }

            EventTrigger eventTrigger = GetComponent<EventTrigger>();
            if(eventTrigger != null ) {
                eventTrigger.SetPointerListeners((bev) => {
                    animator.SetLayerWeight(animator.GetLayerIndex("click"), 1);
                    animator?.SetTrigger("down");
                    Services.GetService<ISoundService>().PlayOneShot(SoundName.buyCoinUpgrade);
                }, (bev2) => { }, (bev3) => {
                    animator?.SetTrigger("up");
                    animator.SetLayerWeight(animator.GetLayerIndex("click"), 0);
                });
            }
        }


    }

}