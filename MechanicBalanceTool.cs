using Bos.Data;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MechanicBalanceTool : EditorWindow {

    private Vector2 scrollPosition = Vector2.zero;
    private readonly List<MechanicJsonData> localMechanicData = new List<MechanicJsonData>();
    private readonly List<MechanicJsonData> remoteMechanicData = new List<MechanicJsonData>();
    private readonly List<SecretaryJsonData> localSecretaryData = new List<SecretaryJsonData>();
    private readonly List<SecretaryJsonData> remoteSecretaryData = new List<SecretaryJsonData>();
    private AnalyzeType analyzeType = AnalyzeType.Mechanic;

    [MenuItem("Tools/Mechanic Balance Tool")]
    private static void ShowWindow() {
        MechanicBalanceTool tool = GetWindow<MechanicBalanceTool>();
        tool.Show();
    }

    private void LoadMechanicLocal() {
        var items = JsonConvert.DeserializeObject<List<MechanicJsonData>>(Resources.Load<TextAsset>("Data/mechanics").text);
        localMechanicData.Clear();
        localMechanicData.AddRange(items);
    }

    private void LoadMechanicRemote() {
        var items = BosUtils.LoadMechanicsRemote();
        remoteMechanicData.Clear();
        remoteMechanicData.AddRange(items);
    }

    private void LoadSecretaryLocal() {
        var items = JsonConvert.DeserializeObject<List<SecretaryJsonData>>(Resources.Load<TextAsset>("Data/secretaries").text);
        localSecretaryData.Clear();
        localSecretaryData.AddRange(items);
    }

    private void LoadSecretaryRemote() {
        var items = BosUtils.GetSecretariesRemote();
        remoteSecretaryData.Clear();
        remoteSecretaryData.AddRange(items);
    }

    private List<IDataDifference> GetMechanicDifference(List<MechanicJsonData> localData, List<MechanicJsonData> remoteData) {
        List<IDataDifference> difference = new List<IDataDifference>();
        foreach (MechanicJsonData lData in localData) {
            var rData = remoteData.FirstOrDefault(d => lData.planetId == d.planetId);
            if (rData == null) {
                //GUILayout.Label($"Not found remote data for planet, planet id = {lData.planetId}");
            } else {
                difference.Add(lData.GetDifference(rData));
            }
        }
        return difference;
    }

    private List<IDataDifference> GetSecretaryDifference(List<SecretaryJsonData> localData, List<SecretaryJsonData> remoteData) {
        List<IDataDifference> difference = new List<IDataDifference>();
        localData.ForEach(lData => {
            var rData = remoteData.FirstOrDefault(d => lData.planetId == d.planetId);
            if (rData != null) {
                difference.Add(lData.GetDifference(rData));
            }
        });
        return difference;
    }


    private void AnalyzeDifference(List<IDataDifference> difference) {

        GUILayout.Label("Differences:");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach(var d in difference ) {
            string str = d.ToString();
            if(!string.IsNullOrEmpty(str)) {
                EditorGUILayout.LabelField(str, GUILayout.MinHeight(100));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void OnGUI() {
        EditorGUILayout.BeginVertical();
        analyzeType = (AnalyzeType)EditorGUILayout.EnumPopup(analyzeType);
        if(GUILayout.Button("Analyze")) {
            LoadLocal();
        }

        if(RemouteCount() > 0 ) {
            if(GUILayout.Button("Save remote to local")) {
                SaveRemoteToLocal();
            }
        }

        switch (analyzeType) {
            case AnalyzeType.Mechanic: {
                    AnalyzeDifference(GetMechanicDifference(localMechanicData, remoteMechanicData));
                }
                break;
            case AnalyzeType.Secretary: {
                    AnalyzeDifference(GetSecretaryDifference(localSecretaryData, remoteSecretaryData));
                }
                break;
        }
        

        EditorGUILayout.EndVertical();

    }

    private int RemouteCount() {
        switch(analyzeType) {
            case AnalyzeType.Mechanic:
                return remoteMechanicData.Count;
            case AnalyzeType.Secretary:
                return remoteSecretaryData.Count;
        }
        return 0;
    }

    private void LoadLocal() {
        switch(analyzeType) {
            case AnalyzeType.Mechanic: {
                    LoadMechanicLocal();
                    LoadMechanicRemote();
                }
                break;
            case AnalyzeType.Secretary: {
                    LoadSecretaryLocal();
                    LoadSecretaryRemote();
                }
                break;
        }
    }

    private void SaveRemoteToLocal() {
        switch(analyzeType) {
            case AnalyzeType.Mechanic: {
                    BosUtils.SaveMechanicsToLocal(remoteMechanicData);
                }
                break;
            case AnalyzeType.Secretary: {
                    BosUtils.SaveSecretariesLocal(remoteSecretaryData);
                }
                break;
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    public enum AnalyzeType { Mechanic, Secretary }
}
