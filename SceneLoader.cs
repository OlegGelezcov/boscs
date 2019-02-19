using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneLoader : EditorWindow {

    [MenuItem("Bos/Scene Loader")]
    private static void OpenView() {
        var window = GetWindow<SceneLoader>();
        window.titleContent = new GUIContent("Scene Loader");
        window.Show();
    }

    private void OnGUI() {
        EditorGUILayout.BeginVertical();
        if(GUILayout.Button("Loading Scene")) {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Game/Scenes/LoadingScene.unity");
        }
        if(GUILayout.Button("Game Scene")) {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Game/Scenes/GameScene.unity");
        }
        if(GUILayout.Button("Test Scene")) {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Game/Scenes/test.unity");
        }
        if(GUILayout.Button("Break Lines")) {
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene("Assets/Game/Scenes/SplitLiner.unity");
            }
        }
        EditorGUILayout.EndVertical();
    }
}
