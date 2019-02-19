namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StatusProgressShaderController : MonoBehaviour {

        public float speed = 1;
        public Image image;

        public bool isEnabled = true;
        public float shineWidth = 0;

        private int enabledPropertyId;
        private int shinePropertyId;
        private int rectPropertyId;
        private bool isInitialized = false;

        void Start() {
            Setup();
        }

        private void OnEnable() {
            Setup();
        }

        private void Setup() {
            if (!isInitialized) {
                Sprite sprite = image.sprite;
                enabledPropertyId = Shader.PropertyToID("_Enabled");
                rectPropertyId = Shader.PropertyToID("_Rect");
                shinePropertyId = Shader.PropertyToID("_ShineWidth");

                Vector4 result = new Vector4(
                    sprite.textureRect.min.x / sprite.texture.width,
                    sprite.textureRect.min.y / sprite.texture.height,
                    sprite.textureRect.max.x / sprite.texture.width,
                    sprite.textureRect.max.y / sprite.texture.height);
                image.materialForRendering.SetVector(rectPropertyId, result);
                ToggleEffect(false);
                isInitialized = true;
            }
        }

        private void Update() {
            if (isEnabled) {
                shineWidth = Mathf.PingPong(Time.time * speed, 1);
                image.materialForRendering.SetFloat(shinePropertyId, shineWidth);
            }

            if(Input.GetKeyUp(KeyCode.Q)) {
                ToggleEffect(!isEnabled);
            }
        }

        public void ToggleEffect(bool isEnabled) {
            this.isEnabled = isEnabled;
            image.materialForRendering.SetFloat(enabledPropertyId, isEnabled ? 1.0f : 0.0f);
        }
    }





}