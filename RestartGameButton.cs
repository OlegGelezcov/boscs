namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class RestartGameButton : GameBehaviour {

        public override void OnEnable() {
            GetComponent<Button>().SetListener(() => {
                SceneManager.LoadScene("Restart");
            });
        }

        public override void Start() {
            base.Start();
#if !BOSDEBUG
            gameObject.Deactivate();
#endif
        }

    }

}