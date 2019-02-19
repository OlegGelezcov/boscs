using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisableWhenNotActiveScene : MonoBehaviour
{
    public int BelongsToScene = 0;
    public GameObject[] Targets;

    private bool _wasSet = false;
    private int _lastCompare;

    private void Update() {
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex != BelongsToScene) {
            if (_lastCompare != activeScene.buildIndex || !_wasSet) {
                foreach (var target in Targets) {
                    target.SetActive(false);
                }

                _wasSet = true;
                _lastCompare = activeScene.buildIndex;
            }
        } else {
            if (_lastCompare != activeScene.buildIndex || !_wasSet) {
                foreach (var target in Targets) {
                    target.SetActive(true);
                }

                _wasSet = true;
                _lastCompare = activeScene.buildIndex;

            }
        }
    }

}
