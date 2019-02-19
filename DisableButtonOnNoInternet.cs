using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DisableButtonOnNoInternet : MonoBehaviour
{
    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    private void FixedUpdate()
    {
        _button.interactable = Application.internetReachability != NetworkReachability.NotReachable;
    }
}
