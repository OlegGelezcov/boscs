﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextBinding : Binding
{
    private Text _text;

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    public override void SetContext(object context)
    {
        base.SetContext(context);

        var val = GetPropertyValue(context, PropertyName);
        SetValue(val);
    }

    protected override void OnPropertyChanged(object newValue)
    {
        SetValue(newValue);
    }

    private void SetValue(object newValue)
    {
        if (newValue == null)
            _text.text = string.Empty;
        else
            _text.text = newValue.ToString();
    }
}
