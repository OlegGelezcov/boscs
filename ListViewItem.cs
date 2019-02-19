using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ListViewItem
{
    private object _context;

    public object Context
    {
        get { return _context; }
        set
        {
            OnContextChanged(value);
        }
    }

    public GameObject Item { get; private set; }
    private Vector3 _position; // issue of unity reset postion after awake
    public float Height { get; internal set; }

    public ListViewItem(GameObject item)
    {
        Item = item;
        _position = item.transform.localPosition;
    }

    private void OnContextChanged(object value)
    {
        if (Item == null)
            return;

        _context = value;

        if (value == null)
        {
            Item.SetActive(false);
        }
        else
        {
            Item.SetActive(true);
           // Item.transform.localPosition = _position;
            PerformBindings();
        }
    }


    private void PerformBindings()
    {
        var bindings = Item.GetComponentsInChildren<Binding>();
        foreach (var x in bindings)
        {
            x.SetContext(_context);
        }
    }
}
