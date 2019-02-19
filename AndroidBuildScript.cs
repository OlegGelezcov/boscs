using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Reflection;

public class AndroidBuildScript: MonoBehaviour {

    [MenuItem("Bos/Build Android Debug")]
    public static void BuildDebug() {

        IncrementVersion.IncrementVersionMenu();
       
        PlayerSettings.bundleVersion = "1.0." + PlayerSettings.Android.bundleVersionCode.ToString();
        string options = PlayerSettings.aotOptions;
        if (!options.Contains("BOSDEBUG")) {
            options += ";BOSDEBUG";
            PlayerSettings.aotOptions = options;
        }

        string[] scenes = new[] {
            "Assets/Game/Scenes/LoadingScene.unity",
            "Assets/Game/Scenes/GameScene.unity",
            "Assets/Game/Scenes/MoneySink_SlotMachine.unity",
            "Assets/Game/Scenes/MoneySink_RacingGame.unity",
            "Assets/Game/Scenes/ManagerSlot.unity",
            "Assets/Game/Scenes/Restart.unity",
            "Assets/Game/Scenes/SplitLiner.unity",
            "Assets/Game/Scenes/TransferSlot.unity"
        };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = LocationPathName;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
        WriteConsoleInfo();


        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        Debug.Log($"summary: {summary}");
    }

    private static string LocationPathName
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "bos-2/AutoAndroidBuild");

    private static void WriteConsoleInfo() {
        try {
            Console.WriteLine(PlayerSettings.applicationIdentifier);
            Console.WriteLine($"version: {PlayerSettings.bundleVersion}");
            Console.WriteLine($"version code: {PlayerSettings.Android.bundleVersionCode}");

            PrintPublicStaticProperties(typeof(PlayerSettings), 0);
            PrintPublicStaticProperties(typeof(PlayerSettings.Android), 1);

        } catch(Exception ex) {
            Debug.Log(ex.Message);
        }
    }

    private static void PrintPublicStaticProperties(Type type, int indentation) {
        Console.WriteLine(type.Name + ":");
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

        string indentString = (indentation > 0) ? new string('\t', indentation) : string.Empty;
        foreach (PropertyInfo propertyInfo in properties) {
            Console.WriteLine($"{indentString}{propertyInfo.Name}: {propertyInfo.GetValue(null)}");
        }
    }

}
