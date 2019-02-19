using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonActionBinding : Binding
{
    private Button _button;

    public string MethodName;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Click()
    {
        var t = _context.GetType();
        var mi = t.GetMethod(MethodName);
        mi.Invoke(_context, null);
    }

    protected override void OnPropertyChanged(object newValue)
    {
        // not necesary
    }
}
