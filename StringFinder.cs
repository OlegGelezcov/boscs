using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Bos;
using System.Linq;

public class StringFinder : EditorWindow {

    private static readonly List<StringInfo> strings = new List<StringInfo>();


    private static readonly string[] stringFiles = new string[] {
        "Data/localization",
        "Data/localization2"
    };

    private string keyPattern = string.Empty;
    private string valuePattern = string.Empty;
    private List<StringInfo> results = new List<StringInfo>();
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Tools/String Finder")]
    private static void ShowView() {
        var view = GetWindow<StringFinder>();
        view.ReloadStrings();
        view.Show();
    }

    public void ReloadStrings() {
        strings.Clear();
        foreach(string file in stringFiles) {
            strings.AddRange(
                JsonConvert.DeserializeObject<List<StringInfo>>(
                    Resources.Load<TextAsset>(file).text));
        }
    }

    private void OnGUI() {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        string oldKeyPattern = keyPattern;
        keyPattern = EditorGUILayout.TextField("Key Pattern", keyPattern);

        string oldValuePattern = valuePattern;
        valuePattern = EditorGUILayout.TextField("Value Pattern", valuePattern);


        EditorGUILayout.EndHorizontal();

        if(oldKeyPattern != keyPattern || oldValuePattern != valuePattern) {
            if(!string.IsNullOrEmpty(keyPattern) || !string.IsNullOrEmpty(valuePattern)) {
                results = CollectResults(keyPattern, valuePattern).ToList();
            }
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach(var info in results ) {
            DrawResult(info);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

    }

    private void DrawResult(StringInfo info) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.SelectableLabel(info.id, GUILayout.MinWidth(200));
        EditorGUILayout.SelectableLabel(info.en, GUILayout.MinWidth(300));
        EditorGUILayout.SelectableLabel(info.ru, GUILayout.MinWidth(300));
        EditorGUILayout.EndHorizontal();
    }

    private IEnumerable<StringInfo> CollectResults(string keyPattern, string valuePattern) {
        foreach(var info in strings) {
            if(keyPattern.IsValid()) {
                if(info.IsMatchById(keyPattern)) {
                    yield return info;
                }
            }
            if(valuePattern.IsValid()) {
                if(info.IsMatchByValue(valuePattern)) {
                    yield return info;
                }
            }
        }
    }
}


public class StringInfo {
    public string id;
    public string en;
    public string ru;

    public bool IsMatchById(string pattern) {
        pattern = pattern.Trim();
        return id.ToLower().Contains(pattern.ToLower());
    }

    public bool IsMatchByValue(string pattern) {
        pattern = pattern.Trim();
        return en.ToLower().Contains(pattern.ToLower()) ||
            ru.ToLower().Contains(pattern.ToLower());
    }
}