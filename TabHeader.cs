using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabHeader : MonoBehaviour
{
    public Button[] Tabs;
    public GameObject[] TabContent;
    public Image[] SelectedBackgrounds;

    public UnityIntEvent TabChanged;

    private void Start()
    {
        foreach (var x in SelectedBackgrounds)
        {
            if (x == null)
                continue;
            x.gameObject.SetActive(false);
        }

        for (int i = 0; i < Tabs.Length; i++)
        {
            var x = Tabs[i];
            if (x != null)
            {
                var t = i - 1; // why do i do this you ask? because shit mono implementation of lambdas that's why
                x.onClick.AddListener(() => Proc(t + 1));
            }
        }

        foreach (var x in TabContent)
        {
            if (x == null)
                continue;
            x.SetActive(false);
        }

        if (TabContent.Length != 0)
            TabContent[0].SetActive(true);

        if (SelectedBackgrounds.Length != 0)
            SelectedBackgrounds[0].gameObject.SetActive(true);

        if (TabChanged != null)
            TabChanged.Invoke(0);
    }

    public void Proc(int i)
    {
        var content = TabContent[i];

        foreach (var x in TabContent)
        {
            if (x == null)
                continue;
            x.SetActive(false);
        }

        foreach (var x in SelectedBackgrounds)
        {
            if (x == null)
                continue;
            x.gameObject.SetActive(false);
        }

        content.SetActive(true);
        SelectedBackgrounds[i].gameObject.SetActive(true);

        if (TabChanged != null)
            TabChanged.Invoke(i);
    }
}

[Serializable]
public class UnityIntEvent : UnityEvent<int>
{
}