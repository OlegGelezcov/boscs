using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityBinding : Binding
{
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
        gameObject.SetActive((bool)newValue);
    }
}
