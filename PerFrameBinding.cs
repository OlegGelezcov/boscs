using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerFrameBinding : GeneralBinding
{
    private void Update()
    {
        var val = GetPropertyValue(_context, PropertyName);
        Proc(val);
    }
}
