namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

        private static T instance = default(T);

        public static T Instance {
            get {
                if(!instance) {
                    instance = GameObject.FindObjectOfType<T>();
                }
                return instance;
            }
        }
    }

}