using Bos;
using Bos.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LocalEvent : GameBehaviour
{
    public double DurationInSeconds = 60 * 60 * 5; //default 5 hours

    public BosEventViewType eventType;
    //public EventView EventView;
    public bool IsPositive;
    public bool IsOneTimeEvent;

    public AllManagers AllManagers;

    public abstract void EventEnter();
    public abstract void EventStay();
    public abstract void EventExit();
}
