using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChildrenChangeNotifier : MonoBehaviour
{

    public UnityEvent ChildPositionChanged;

    private void OnTransformChildrenChanged()
    {
        ChildPositionChanged.Invoke();
    }
}
