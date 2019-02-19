using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class BosImageScaler : EditorWindow {

	[MenuItem("Bos/Image Size Mult")]
	private static void ImageSizeMult() {
		BosImageScaler window = EditorWindow.GetWindow<BosImageScaler>();
		window.Show();
	}

	private float sizeMult = 1.0f;

	private void OnGUI(){
		EditorGUILayout.BeginVertical();
		sizeMult = EditorGUILayout.FloatField("Mult", sizeMult);
		if(GUILayout.Button("Apply")) {
			ApplySizeMult();
		}
		EditorGUILayout.EndVertical();
	}

	private void ApplySizeMult() {
		foreach(GameObject obj in Selection.gameObjects) {			
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			if(rectTransform != null ) {
				Vector2 oldSize = rectTransform.GetComponent<RectTransform>().sizeDelta;
				rectTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(oldSize.x * sizeMult, oldSize.y * sizeMult);
			}

			Text text = obj.GetComponent<Text>();
			if(text != null ) {
				if(text.resizeTextForBestFit){
					text.resizeTextMaxSize = Mathf.RoundToInt(sizeMult * text.resizeTextMaxSize);
				}
				int newFontSize = Mathf.RoundToInt(text.fontSize * sizeMult);
				text.fontSize = newFontSize;
			}

            LayoutElement layoutElement = obj.GetComponent<LayoutElement>();
            if(layoutElement != null ) {
                layoutElement.preferredWidth *= sizeMult;
                layoutElement.preferredHeight *= sizeMult;
            }
		}
	}
}
