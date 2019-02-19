using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideAndCollapse : MonoBehaviour
{
    public float Offset;
    public float AnimTime;
    public bool ToLeft = true;

    public GameObject MoveTarget;
    private bool _isAnimating;
    private Action _a;
    private RectTransform _rt;
    private RectTransform _rt2;

    private void Start()
    {
        _rt = MoveTarget.GetComponent<RectTransform>();

        if (MoveTarget == null)
            MoveTarget = gameObject;

        if (ToLeft)
            Offset *= -1;
    }

    public void Animate(Action a)
    {
        _isAnimating = true;
        _a = a;
    }

    private void Update()
    {
        if (!_isAnimating)
            return;

        var v2 = new Vector2(_rt.anchoredPosition.x, _rt.anchoredPosition.y);
        _rt.anchoredPosition += new Vector2(Offset / AnimTime * Time.deltaTime + v2.x, v2.y);

        if (v2.x > 500000 || v2.x < -500000)
        {
            _isAnimating = false;
            _a();
            _rt.anchoredPosition = new Vector2();
        }
    }




}
