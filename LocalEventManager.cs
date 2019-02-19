using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalEventManager : GameBehaviour
{
    private float _currentEventChance;
    private AllManagers _allManRef;

    public float BaseEventPercent = 0.2f;
    public float PerHourIncrement = 0.05f;

    public double EventTriggerThreshold = 1000000;

    public LocalEvent[] Events;

    [Header("Manager References")]
    //public IAPManager IAPManager;
    public GameUI UIManager;
    public GameManager GameManager;
    private bool _eventsCanOccur;

    public bool IsEventActive
    {
        get { return System.Convert.ToBoolean(PlayerPrefs.GetInt("IsEventActive", 0)); }
        set { PlayerPrefs.SetInt("IsEventActive", System.Convert.ToInt32(value)); }
    }

    public int ActiveEventIndex
    {
        get { return PlayerPrefs.GetInt("ActiveEventIndex", 0); }
        set { PlayerPrefs.SetInt("ActiveEventIndex", value); }
    }

    public static System.DateTime LastEventUpdate
    {
        get { return System.DateTime.Parse(PlayerPrefs.GetString("LastEventUpdate", "1337/3/3")); }
        set { PlayerPrefs.SetString("LastEventUpdate", value.ToString()); }
    }

    public override void Start()
    {
        
        _allManRef = new AllManagers()
        {
            GameManager = GameManager,
            //IAPManager = IAPManager,
            UIManager = UIManager
        };

        foreach (var x in Events)
        {
            x.AllManagers = _allManRef;
        }

        if(Services.GameModeService.IsFirstTimeLaunch) {
            return;
        }


        var gameSvc = ServiceLocator.Instance.Resolve<IGameService>();

        var serverDT = gameSvc.GetServerDateTime();

        if (IsEventActive)
        {
            var evt = Events[ActiveEventIndex];
            var evtDiff = serverDT - LastEventUpdate;

            if (evtDiff.TotalSeconds < evt.DurationInSeconds)
            {
                evt.EventStay();
                return;
            }

            IsEventActive = false;
            ActiveEventIndex = -1;

            evt.EventExit();
            //return; // TODO: check if pass throgh or skip this session
        }
    }

    private void FixedUpdate()
    {
        _eventsCanOccur = Player.LifetimeEarnings > EventTriggerThreshold;

        if (_eventsCanOccur && DateTime.Now > LocalData.NextEventTime)
        {
            TriggerEvent();

            var t = TimeSpan.FromMinutes(45 + 15 * UnityEngine.Random.Range(0f,1f));
            LocalData.NextEventTime = DateTime.Now.Add(t);
        }
    }

    public void TriggerEvent()
    {
        var rnd = UnityEngine.Random.Range(0, Events.Length);
        var evt = Events[rnd];
        if (evt.name.Contains("Nuclear"))
            return;

        evt.EventEnter();
    }
}

[System.Serializable]
public class AllManagers
{
    //public IAPManager IAPManager;
    public GameUI UIManager;
    public GameManager GameManager;
};