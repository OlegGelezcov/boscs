using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StateSwitcher))]
public class StateSwitcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var q = (StateSwitcher)target;

        if (GUILayout.Button("Change To Unlocked"))
        {
            for (int i = 0; i < q.UnlockedStates.Length; i++)
            {
                var a = q.UnlockedStates[i];
                var b = q.LockedStates[i];

                a.SetActive(true);
                b.SetActive(false);
            }
        }

        if (GUILayout.Button("Change To Locked"))
        {
            for (int i = 0; i < q.UnlockedStates.Length; i++)
            {
                var a = q.UnlockedStates[i];
                var b = q.LockedStates[i];

                a.SetActive(false);
                b.SetActive(true);
            }
        }
    }
}
