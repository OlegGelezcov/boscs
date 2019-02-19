namespace Bos.Debug.Test {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class NetServiceTest : MonoBehaviour {

        private void OnGUI() {
            GUILayout.BeginVertical();
            if(GUILayout.Button("Request")) {
                FindObjectOfType<NetService>().GetBalance((planets) => {
                    foreach(var planet in planets) {
                        Debug.Log(planet.ToString());
                    }
                }, msg => {
                    Debug.LogError(msg);
                });
            }
            GUILayout.EndVertical();
        }
    }

}