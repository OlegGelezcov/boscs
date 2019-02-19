using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;
using Ozh.Tools.Functional;

public class IncrementVersion : MonoBehaviour {

    [MenuItem("Bos/Increment Version")]
    public static void IncrementVersionMenu() {
        AndroidVersion.
            FromPlayerSettingsIncremented.
            UpdatePlayerSettings().
            UpdateAndroidManifest(AndroidManifestFilePath);

        AssetDatabase.Refresh(ImportAssetOptions.Default);

        ApplyPassword();

    }


    private static void ApplyPassword() {
#if UNITY_ANDROID
        string passwordPath = Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "password.txt");
        string password = File.ReadAllText(passwordPath);
        password = password.Trim();
        PlayerSettings.Android.keyaliasPass = password;
        PlayerSettings.Android.keystorePass = password;
        print($"password: {password} applied");
#endif
    }


    private static string AndroidManifestFilePath
        => Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");



}

public static class AndroidVersionExtension {

    public static Option<AndroidVersion> UpdatePlayerSettings(this Option<AndroidVersion> version ) {
        return version.Match(() => {
            return F.None;
        }, ver => {
            PlayerSettings.bundleVersion = ver.ToString();
            PlayerSettings.Android.bundleVersionCode = ver.VersionCode;
            return F.Some(ver);
        });
    }

    public static Option<AndroidVersion> UpdateAndroidManifest(this Option<AndroidVersion> version, string manifestPath ) {
        return version.Match(() => F.None, ver => {
            string text = File.ReadAllText(manifestPath);
            string versionNamePattern = "android:versionName=\"\\d+\\.\\d+\\.\\d+\"";
            string versionCodePattern = "android:versionCode=\"\\d+\"";
            text = Regex.Replace(text, versionNamePattern, (match) => ver.VersionNameString);
            text = Regex.Replace(text, versionCodePattern, (match) => ver.VersionCodeString);
            Debug.Log(text);
            File.WriteAllText(manifestPath, text);
            return F.Some(ver);
        });
    }
}

public class AndroidVersion {
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int VersionCode { get; private set; }

    public override string ToString()
        => $"{Major}.{Minor}.{VersionCode}";

    public AndroidVersion(string sourceVersion ) {
        string[] tokens = sourceVersion.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        int major = 1;
        if (tokens.Length > 0) {
            Int.Parse(tokens[0]).Match(() => major = 1, num => major = num);           
        }
        int minor = 0;
        if(tokens.Length > 1 ) {
            Int.Parse(tokens[1]).Match(() => minor = 0, num => minor = num);
        }

        int versionCode = 0;
        if(tokens.Length > 2 ) {
            Int.Parse(tokens[2]).Match(() => versionCode = 0, num => versionCode = num);
        }
        Major = major;
        Minor = minor;
        VersionCode = versionCode;
    }

    public void UpdateVersionCodeIfBigger(int otherCode) {
        if(otherCode > VersionCode ) {
            VersionCode = otherCode;
        }
    }

    public AndroidVersion UpdatedWithBiggerVersionCode(int otherVersionCode ) {
        return new AndroidVersion(Major, Minor, (otherVersionCode > VersionCode) ? otherVersionCode : VersionCode);
    }

    public AndroidVersion IncrementedVersionCode(int inrement) {
        return new AndroidVersion(Major, Minor, VersionCode + inrement);
    }

    public AndroidVersion(int major, int minor, int versionCode) {
        Major = major;
        Minor = minor;
        VersionCode = versionCode;
    }

    public string VersionNameString
        => string.Format("android:versionName=\"{0}\"", ToString());
    public string VersionCodeString
        => string.Format("android:versionCode=\"{0}\"", VersionCode.ToString());

    public static Option<AndroidVersion> FromPlayerSettingsIncremented {
        get {
            return F.Some(new AndroidVersion(PlayerSettings.bundleVersion).
                UpdatedWithBiggerVersionCode(PlayerSettings.Android.bundleVersionCode).
                IncrementedVersionCode(1));
        }
    }
}
