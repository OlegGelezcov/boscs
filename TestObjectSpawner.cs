namespace Bos {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TestObjectSpawner : MonoBehaviour {

        public GameObject prefab;
        public RectTransform cp1;
        public RectTransform cp2;

        public float offsetRadius = 300;

        void Update() {
            if (Input.GetKeyUp(KeyCode.A)) {
                GameObject obj = Instantiate(prefab);
                obj.GetComponent<RectTransform>().SetParent(transform, false);
                EndGameCoin c = obj.GetComponent<EndGameCoin>();
                c.cp1 = cp1.anchoredPosition + GetRandomOffset();
                c.cp2 = cp2.anchoredPosition + GetRandomOffset();
                c.Setup();
            }
        }

        private Vector2 GetRandomOffset() {
            return Random.insideUnitCircle.normalized * offsetRadius;
        }
    }


}