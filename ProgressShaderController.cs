namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ProgressShaderController : MonoBehaviour {

        public Image image;
        public float fillAmount = 0;
        
        private int rectPropertyId;
        private int fillAmountPropertyId;

        private bool isInitialized = false;
        
        private void Start() {
            Setup();
        }

        private void OnEnable() {
            Setup();
        }

        private void Setup() {
            if (!isInitialized) {
                rectPropertyId = Shader.PropertyToID("_Rect");
                fillAmountPropertyId = Shader.PropertyToID("_FillAmount");
                Sprite sprite = image.sprite;
                Vector4 result = new Vector4(sprite.textureRect.xMin ,
                    sprite.textureRect.yMin ,
                    sprite.textureRect.width,
                     sprite.textureRect.height);
                image.materialForRendering.SetVector(rectPropertyId, result);
                isInitialized = true;
            }
        }

        private void Update() {
            //image.materialForRendering.SetFloat(fillAmountPropertyId, Mathf.Clamp01(fillAmount));
        }

        public void SetFillAmount(float val) {
            Setup();
            fillAmount = val;
            image.materialForRendering.SetFloat(fillAmountPropertyId, Mathf.Clamp01(fillAmount));
        }
    }
}


