using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDownEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 Offset;

    private Vector2[] _h;
    private bool _hasEntered;
    private bool _isHolding;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hasEntered && _isHolding)
        {
            OnPointerDown(eventData);
            return;
        }

        var c = transform.childCount;
        _h = new Vector2[c];
        for (int i = 0; i < c; i++)
        {
            var child = transform.GetChild(i);
            var rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
                _h[i] = rectTransform.anchoredPosition;
        }

        _hasEntered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            var child = transform.GetChild(i);
            var rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.anchoredPosition = _h[i];
        }

        _hasEntered = false;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        var c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            var child = transform.GetChild(i);
            var rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.anchoredPosition = _h[i];
        }

        _isHolding = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            var child = transform.GetChild(i);
            var rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.anchoredPosition = _h[i] + Offset;
        }

        _isHolding = true;
    }


    private void Start()
    {
        var c = transform.childCount;
        _h = new Vector2[c];
    }

}
