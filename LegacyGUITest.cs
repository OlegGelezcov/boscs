using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyGUITest : MonoBehaviour {

    private readonly string textStyleName = "testtext";

    public GUISkin skin;

    private float ReferenceWidth => 1080;
    private float ReferenceHeight => 1920;

    private Vector2 scrollPosition;
    private bool isToggle = true;

    private void OnGUI() {
        GUI.skin = skin;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight, 1));

        //GUI.Label(new Rect(ReferenceWidth * 0.5f, ReferenceHeight * 0.5f, 0, 0), "sample text", skin.GetStyle(textStyleName));

        float y = 0;
        scrollPosition = GUI.BeginScrollView(new Rect(50, 50, 200, 400), scrollPosition, new Rect(0, 0, 400, 800), false, false);
        for(int i = 0; i < 30; i++  ) {
            GUI.Label(new Rect(0, i * 45, 0, 0), "sample text for testing scroll rect", skin.GetStyle(textStyleName));
        }
        GUI.EndScrollView();

        isToggle = GUI.Toggle(new Rect(10, ReferenceHeight / 2, 60, 60), isToggle, "ON/OFF VALUE");

        GUI.matrix = oldMatrix;
    }
}
