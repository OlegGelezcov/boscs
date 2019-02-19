using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NStateToggleButton : MonoBehaviour
{
    public string[] States;
    public Text Content;

    public string CurrentState { get { return States[_curState]; } }
    public event EventHandler StateChanged;
    private int _curState;  
    private void Awake()
    {
        Content.text = States[_curState];

		if (StateChanged != null)
        StateChanged(this, EventArgs.Empty);
    }

    public void OnUnityButtonClick()
    {
        _curState = (_curState + 1) % States.Length;
        Content.text = States[_curState];

		if (StateChanged != null)
        StateChanged(this, EventArgs.Empty);
    }

}
