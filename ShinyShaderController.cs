namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ShinyShaderController : GameBehaviour {

        public float speed = 1.0f;
        public Image image;
        private bool isEnabled = true;
        private float shineWidth = 0;

        private int enabledPropertyId;
        private int shinePropertyId;
        private int rectPropertyId;
        private bool isInitialized = false;

        public override void Start() {
            Setup();
        }

        public override void OnEnable() {
            base.OnEnable();
            Setup();
        }

        private void Setup() {
            if (!isInitialized) {
                Sprite sprite = image.sprite;
                //material = image.materialForRendering;
                
                enabledPropertyId = Shader.PropertyToID("_Enabled");
                rectPropertyId = Shader.PropertyToID("_Rect");
                shinePropertyId = Shader.PropertyToID("_ShineWidth");

                Vector4 result = new Vector4(sprite.textureRect.min.x / sprite.texture.width,
                    sprite.textureRect.min.y / sprite.texture.height,
                    sprite.textureRect.max.x / sprite.texture.width,
                    sprite.textureRect.max.y / sprite.texture.height);
                if (image.materialForRendering != null) {
                    image.materialForRendering.SetVector(rectPropertyId, result);
                }
                ToggleEffect(false);
                isInitialized = true;
            }
        }


        public void ResetMaterialOnImage() {
            if (image.material != null) {
                image.material = null;
            }
        }

        public override void Update() {

            base.Update();

            if (isEnabled) {
                if (image.materialForRendering != null) {
                    shineWidth = Mathf.PingPong(Time.time * speed, 1);

                    image.materialForRendering.SetFloat(shinePropertyId, shineWidth);
                }

            }

        }

        
        public void ToggleEffect(bool isEnabled) {
            this.isEnabled = isEnabled;         
            image.materialForRendering.SetFloat(enabledPropertyId, isEnabled ? 1.0f : 0.0f);
        }
    }

}