using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScreenshotHelper : MonoBehaviour
{
    [MenuItem("Tools/Take Screenshot")]
    public static void Screenshot()
    {
        var fname = DateTime.Now.TimeOfDay.TotalMilliseconds.ToString();
        ScreenCapture.CaptureScreenshot($"{fname}.png", 2);
    }
}
