using System.ComponentModel;
using UnityEngine;

public abstract class Binding : MonoBehaviour
{
    protected object _context;

    public string PropertyName;

    public virtual void SetContext(object context)
    {
        if (_context == context)
            return;

        if (_context != null && _context is INotifyPropertyChanged)
        {
            var q = _context as INotifyPropertyChanged;
            q.PropertyChanged -= context_PropertyChanged;
        }

        _context = context;

        if (_context != null && _context is INotifyPropertyChanged)
        {
            var q = _context as INotifyPropertyChanged;
            q.PropertyChanged += context_PropertyChanged;
        }
    }

    protected abstract void OnPropertyChanged(object newValue);

    private void context_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != PropertyName)
            return;

        var val = GetPropertyValue(sender, e.PropertyName);
        OnPropertyChanged(val);
    }

    protected object GetPropertyValue(object context, string propName)
    {
        var t = context.GetType();
        var propinfo = t.GetProperty(propName);
        return propinfo.GetValue(context);
    }
}
