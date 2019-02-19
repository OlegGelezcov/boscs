using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AndroidSettings = UnityEditor.PlayerSettings.Android;
using Ozh.Tools.Functional;

public class AndroidSettingsTest : EditorWindow {

    [MenuItem("Bos/Show Android Settings Test")]
    private static void ShowAndroidSettingsTest() {
        var window = GetWindow<AndroidSettingsTest>();
        window.position = new Rect(100, 100, 1000, 1000);
        window.Show();
    }
    
    private Vector2 scrollPosition = Vector2.zero;
    private void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach(string propStr in GetSettingsStrings()) {
            EditorGUILayout.LabelField(propStr);
        }
        EditorGUILayout.EndScrollView();
    }

    private IEnumerable<string> GetSettingsStrings() {
        List<string> settings = new List<string>();
        settings.Add($"{nameof(AndroidSettings.androidIsGame)}: {AndroidSettings.androidIsGame}");
        settings.Add($"{nameof(AndroidSettings.androidTVCompatibility)}: {AndroidSettings.androidTVCompatibility}");
        settings.Add($"{nameof(AndroidSettings.blitType)}: {AndroidSettings.blitType}");
        settings.Add($"{nameof(AndroidSettings.bundleVersionCode)}: {AndroidSettings.bundleVersionCode}");
        settings.Add($"{nameof(AndroidSettings.disableDepthAndStencilBuffers)}: {AndroidSettings.disableDepthAndStencilBuffers}");
        settings.Add($"{nameof(AndroidSettings.forceInternetPermission)}: {AndroidSettings.forceInternetPermission}");
        settings.Add($"{nameof(AndroidSettings.forceSDCardPermission)}: {AndroidSettings.forceSDCardPermission}");
        settings.Add($"{nameof(AndroidSettings.keyaliasName)}: {AndroidSettings.keyaliasName}");
        settings.Add($"{nameof(AndroidSettings.keyaliasPass)}: {AndroidSettings.keyaliasPass}");
        settings.Add($"{nameof(AndroidSettings.keystoreName)}: {AndroidSettings.keystoreName}");
        settings.Add($"{nameof(AndroidSettings.keystorePass)}: {AndroidSettings.keystorePass}");
        settings.Add($"{nameof(AndroidSettings.licenseVerification)}: {AndroidSettings.licenseVerification}");
        settings.Add($"{nameof(AndroidSettings.maxAspectRatio)}: {AndroidSettings.maxAspectRatio}");
        settings.Add($"{nameof(AndroidSettings.minSdkVersion)}: {AndroidSettings.minSdkVersion}");
        settings.Add($"{nameof(AndroidSettings.preferredInstallLocation)}: {AndroidSettings.preferredInstallLocation}");
        settings.Add($"{nameof(AndroidSettings.showActivityIndicatorOnLoading)}: {AndroidSettings.showActivityIndicatorOnLoading}");
        settings.Add($"{nameof(AndroidSettings.splashScreenScale)}: {AndroidSettings.splashScreenScale}");
        settings.Add($"{nameof(AndroidSettings.targetArchitectures)}: {AndroidSettings.targetArchitectures}");
        settings.Add($"{nameof(AndroidSettings.targetSdkVersion)}: {AndroidSettings.targetSdkVersion}");
        settings.Add($"{nameof(AndroidSettings.useAPKExpansionFiles)}: {AndroidSettings.useAPKExpansionFiles}");
        settings.Add("*********************************************");
        
        return settings;
    }

    
    
}
