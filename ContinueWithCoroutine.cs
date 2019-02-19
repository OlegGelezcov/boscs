namespace Bos.Debug {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ContinueWithCoroutine : MonoBehaviour {

        private int index = 0;

        private void Start() {
            StartCoroutine(TestCoroutine());
        }
        
        private IEnumerator TestCoroutine() {
            while (true) {
                print($"start waiting on index: {index}");
                yield return new WaitForSeconds(2);

                if (index < 5) {
                    index++;
                    continue;
                }

                index++;
                print($"after continue {index}");
                if (index >= 10) {
                    yield break;
                }
            }
        }
    }


}