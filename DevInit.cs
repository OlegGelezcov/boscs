using GhostCore.MVVM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevInit : MonoBehaviour
{

    public ListView ListView;

    private void Start()
    {
        List<GenericItem> items = new List<GenericItem>();
        for (int i = 0; i < 200; i++)
        {
            items.Add(new LeftItem() { Content = "Left Content Lorem Ipsum dolor sit amet blabla caca " + i, Title = "Left Item " + i, ImageSprite = SpriteDB.SpriteRefs["gold"] });
            items.Add(new RightItem() { Content = "Right Content  Lorem Ipsum dolor sit amet blabla caca " + i, Title = "Right Item " + i, ImageSprite = SpriteDB.SpriteRefs["silver"] });
        }

        ListView.DataSource = items;
    }
}

public abstract class GenericItem : ViewModelBase
{
    private string _title;
    private object _content;
    private Sprite _image;
    private float _prog;
    private bool _visibility;

    public bool ContentVisibility
    {
        get { return _visibility; }
        set { _visibility = value; OnPropertyChanged(nameof(ContentVisibility)); }
    }

    public float ProgressValue
    {
        get { return _prog; }
        set { _prog = value; OnPropertyChanged(nameof(ProgressValue)); }
    }

    public string Title
    {
        get { return _title; }
        set { _title = value; OnPropertyChanged("Title"); }
    }

    public object Content
    {
        get { return _content; }
        set { _content = value; OnPropertyChanged("Content"); }
    }

    public Sprite ImageSprite
    {
        get { return _image; }
        set { _image = value; OnPropertyChanged("ImageSprite"); }
    }

    public abstract void DoSomething();
}

public class LeftItem : GenericItem
{
    public override void DoSomething()
    {
        Title = Random.Range(234, 648).ToString();
        Content = Random.Range(100, 200);
        ImageSprite = SpriteDB.SpriteRefs["silver"];
        ProgressValue = Random.Range(0, 1f);
        ContentVisibility = !ContentVisibility;
    }
}

public class RightItem : GenericItem
{
    public override void DoSomething()
    {
        Debug.Log("Right Item");
    }
}