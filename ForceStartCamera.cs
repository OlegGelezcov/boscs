using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceStartCamera : MonoBehaviour
{
    public GameObject Camera;

    private void Update() {
        if (!Camera.activeInHierarchy) {
            Camera.SetActive(true);
        }
    }
}
