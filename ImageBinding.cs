using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageBinding : Binding
{
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public override void SetContext(object context)
    {
        base.SetContext(context);

        var val = GetPropertyValue(context, PropertyName);
        SetValue(val);
    }

    protected override void OnPropertyChanged(object newValue)
    {
        SetValue(newValue);
    }

    private void SetValue(object newValue)
    {
        if (newValue == null)
            _image.sprite = null;
        else
            _image.sprite = newValue as Sprite;
    }
}