using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLineFollow : MonoBehaviour
{

    private LineRenderer _lr;

    public Transform[] Objects;

    private void Start()
    {
        _lr = GetComponent<LineRenderer>();

        _lr.positionCount = Objects.Length;
        for (int i = 0; i < Objects.Length; i++)
        {
            _lr.SetPosition(i, Objects[i].position);
        }
    }

    private void Update()
    {
        for (int i = 0; i < Objects.Length; i++)
        {
            _lr.SetPosition(i, Objects[i].position);
        }
    }
}
