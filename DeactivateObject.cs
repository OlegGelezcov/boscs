namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DeactivateObject : GameBehaviour {

        public float timeout = 1.5f;

        public override void OnEnable() {
            base.OnEnable();
            StartCoroutine(DeactivateImpl());
        }

        private IEnumerator DeactivateImpl() {
            yield return new WaitForSeconds(timeout);
            gameObject.Deactivate();
        }


    }

}