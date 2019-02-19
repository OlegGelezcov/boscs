namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class DebugViewButton : GameBehaviour {
        public override void Start() {
            base.Start();

            GetComponent<Button>().SetListener(() => {
                ViewService.Show(ViewType.DebugView, new ViewData {
                    ViewDepth = ViewService.NextViewDepth
                });
                Sounds.PlayOneShot(SoundName.click);
            });

#if !BOSDEBUG
            gameObject.Deactivate();
#endif


        }
    }

}