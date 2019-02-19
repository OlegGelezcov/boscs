using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralBinding : Binding
{
    public Component TargetComponent;
    public string TargetProperty;

    public override void SetContext(object context)
    {
        base.SetContext(context);

        var val = GetPropertyValue(context, PropertyName);
        Proc(val);
    }

    protected override void OnPropertyChanged(object newValue)
    {
        Proc(newValue);
    }

    protected void Proc(object newValue)
    {
        var t = TargetComponent.GetType();
        var propinfo = t.GetProperty(TargetProperty);
        propinfo.SetValue(TargetComponent, newValue);
    }
}
