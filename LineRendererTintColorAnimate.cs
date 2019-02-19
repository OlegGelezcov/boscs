namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LineRendererTintColorAnimate : MonoBehaviour {

        public float loopSpeed = 1.0f;
        public float minAlpha = 0.5f;

        private Material material;
        private int colorId;

        void Start() {
            material = GetComponent<LineRenderer>().material;
            colorId = Shader.PropertyToID("_TintColor");
        }

        void Update() {
            float deltaAlpha = Mathf.PingPong(Time.time * loopSpeed, 0.5f);
            float alpha = minAlpha + deltaAlpha;
            Color color = material.GetColor(colorId);
            color.a = alpha;
            material.SetColor(colorId, color);
        }
    }

}