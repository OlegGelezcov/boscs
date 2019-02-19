using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScrollResetter : MonoBehaviour
{

    private void Start()
    {
        GetComponent<RectTransform>().localPosition = new Vector3();
    }

}
