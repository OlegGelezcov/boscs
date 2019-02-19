namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
    using Bos.Debug;
    using UnityEngine;
	using UnityEngine.UI;

	public class ImageAspectFitter : MonoBehaviour {

		/* 
//#if UNITY_ANDROID
		private bool isFitted = false;

		void Start() {
			if(!isFitted) {
				OnEnable();
			}
		}

		void OnEnable() {
			if(!isFitted) {
				
				Debug.Log("FIT To ")
				RectTransform rectTransform = GetComponent<RectTransform>();
				var result = FitTo(rectTransform, new Vector2(Screen.width, Screen.height));
				var scaler = GetComponentInParent<CanvasScaler>();
				if(scaler != null ) {
					var scalerResolution = scaler.referenceResolution;
					if(result.x < scalerResolution.x || result.y < scalerResolution.y ) {
						FitTo(rectTransform, scalerResolution);
					}
				}
				isFitted = true;
			}
		}

		private Vector2 FitTo(RectTransform rectTranform, Vector2 targetSize) {
				float targetWidth = targetSize.x; 
				float targetHeight = targetSize.y; 
				
				float imageWidth = rectTranform.sizeDelta.x;
				float imageHeight = rectTranform.sizeDelta.y;
				

				

				float ratioY = targetHeight / imageHeight;

				float newWidth = rectTranform.sizeDelta.x * ratioY;
				if(newWidth < targetWidth) {
					targetHeight *= (targetWidth / newWidth);
					newWidth = targetWidth;					
				}
				rectTranform.sizeDelta = new Vector2(newWidth, targetHeight);

				
				return rectTranform.sizeDelta;
		}

//#endif
*/

	}

}


