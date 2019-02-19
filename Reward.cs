using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Reward : GameBehaviour
{
    public string Name;
    public Sprite Icon;
    public float WinChance;

    public abstract void Apply(AllManagers bman);
}
