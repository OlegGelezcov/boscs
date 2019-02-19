using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ListView : MonoBehaviour
{
    private RectTransform _transform;
    private object _dataSource;
    private RectTransform _contentRoot;
    private ScrollRect _scrollRect;
    private Scrollbar _vertScroll;
    private List<ListViewItem> _items;

    private List<GameObject> _trash = new List<GameObject>();

    [SerializeField]
    private TypeTemplateMapping[] _templateMapping;

    [SerializeField]
    private bool _virtualize = true;

    public object DataSource
    {
        get { return _dataSource; }
        set
        {
            if (_contentRoot == null)
            {
                _scrollRect = GetComponentInChildren<ScrollRect>();
                _contentRoot = _scrollRect.content;
            }

            OnDataSourceChanged(value);
        }
    }
    public IListViewTemplateSelector TemplateSelector { get; set; }


    public void Clear()
    {
        if (_trash.Count == 0) return;

        foreach (var trashObject in _trash.ToList())
           Destroy(trashObject);
        
        _items.Clear();
        _trash.Clear();
    }

    public ListView()
    {
        TemplateSelector = new DefaultListViewTemplateSelector();
        _items = new List<ListViewItem>();
    }

    private void Awake()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _vertScroll = _scrollRect.verticalScrollbar;
        _vertScroll.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>(verticalScroll_ValueChanged));

        _contentRoot = _scrollRect.content;

        _transform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (_vertScroll == null)
            return;
        
        var sum = _items.Sum(x => x.Height);

        if (sum < Screen.height)
            verticalScroll_ValueChanged(_vertScroll.value);
        
        ResetScroll();
    }

    private void UpdateItems()
    {
        float h = 0;
        if (_dataSource is IEnumerable)
        {
            foreach (var dataItem in ((IEnumerable)_dataSource))
            {
                var tm = TemplateSelector.SelectTemplateCore(_templateMapping, dataItem);

                if (tm == null)
                    continue;

                var go = Instantiate(tm.Template);
                AddChild(go);
                var rt = go.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(0, rt.offsetMin.y);
                rt.offsetMax = new Vector2(0, rt.offsetMax.y);
                rt.localPosition = new Vector3(0, -h, 0);
                rt.localScale = new Vector3(1, 1, 1);

                ListViewItem lvi = new ListViewItem(go);
                lvi.Height = tm.Template.GetComponent<RectTransform>().rect.height;
                lvi.Context = _virtualize ? null : dataItem;

                _items.Add(lvi);

                h += lvi.Height;
            }
        }

        _contentRoot.sizeDelta = new Vector2(_contentRoot.sizeDelta.x, h);
    }

    private IEnumerator MakeSize(float h)
    {
        yield return new WaitForEndOfFrame();
    }


    public void ResetScroll()
    {
        _scrollRect.verticalNormalizedPosition = 1.03f;
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        float h = float.MaxValue;
        foreach (var lvi in _items)
        {
            var go = lvi.Item;
            if (h == float.MaxValue)
            {
                h = lvi.Height;
            }
            var rt = go.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(0, -h);
            rt.offsetMax = new Vector2(0, -h + lvi.Height);
            rt.localScale = new Vector3(1, 1, 1);

            h += lvi.Height;
        }

        _contentRoot.sizeDelta = new Vector2(_contentRoot.sizeDelta.x, h);
        verticalScroll_ValueChanged(_vertScroll.value);
    }

    private void verticalScroll_ValueChanged(float e)
    {
        if (!_virtualize || _dataSource == null)
            return;

        var qarr = ((IEnumerable)_dataSource).OfType<object>().ToArray();

        var curH = _transform.rect.height;

        var scrollSize = _vertScroll.size * _contentRoot.sizeDelta.y;

        var scrollPos = (1 - e) * _contentRoot.sizeDelta.y;

        var qstart = _contentRoot.anchoredPosition.y - curH / 2;
        var qend = _contentRoot.anchoredPosition.y + curH;

        float curItemH = 0;
        int i = 0;
        foreach (var x in _items)
        {
            if (qstart < curItemH && curItemH < qend && qarr.Length > i)
            {
        
                x.Context = qarr[i];
            }
            else
            {
                x.Context = null;
            }

            curItemH += x.Height;
            i++;
        }
    }

    private void LoadViewportData()
    {
        var d = _dataSource as IEnumerable;

        int i = 0;
        foreach (var x in d)
        {
            _items[i].Context = x;

            i++;
        }
    }

    private void AddChild(GameObject go)
    {
        //go.transform.parent = _contentRoot;
        go.transform.SetParent(_contentRoot, false);
        _trash.Add(go);
    }

    private void OnDataSourceChanged(object value)
    {
        if (_dataSource != null)
        {
            if (_dataSource is INotifyCollectionChanged)
            {
                (_dataSource as INotifyCollectionChanged).CollectionChanged -= dataSource_CollectionChanged;
            }
        }

        _dataSource = value;

        if (_dataSource is INotifyCollectionChanged)
        {
            (_dataSource as INotifyCollectionChanged).CollectionChanged += dataSource_CollectionChanged;
        }

        UpdateItems();
    }

    private ListViewItem GetItemFromContext(object context)
    {
        return _items.FirstOrDefault(x => x.Context == context);
    }

    private void dataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                dataSource_CollectionChanged_Add(sender, e);
                return;
            case NotifyCollectionChangedAction.Remove:
                dataSource_CollectionChanged_Remove(sender, e);
                return;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                break;
            default:
                break;
        }
    }

    private void dataSource_CollectionChanged_Remove(object sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var x in e.OldItems)
        {
            var lvi = GetItemFromContext(x);

            lvi.Context = null;
            Destroy(lvi.Item);

            _items.Remove(lvi);
        }

        ApplyLayout();

    }
    private void dataSource_CollectionChanged_Add(object sender, NotifyCollectionChangedEventArgs e)
    {
        float h = 0;
        foreach (var x in e.NewItems)
        {
            var tm = TemplateSelector.SelectTemplateCore(_templateMapping, x);

            if (tm == null)
                continue;

            var go = Instantiate(tm.Template);
            AddChild(go);
            var rt = go.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(0, rt.offsetMin.y);
            rt.offsetMax = new Vector2(0, rt.offsetMax.y);
            rt.localPosition = new Vector3(0, -h, 0);
            rt.localScale = new Vector3(1, 1, 1);

            ListViewItem lvi = new ListViewItem(go);
            lvi.Height = tm.Template.GetComponent<RectTransform>().rect.height;
            if (_virtualize)
                lvi.Context = null; //start off with null content
            else
                lvi.Context = x;

            _items.Add(lvi);

            h += lvi.Height;
        }
        
        ApplyLayout();
    }
    
}