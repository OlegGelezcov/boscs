using UnityEngine;
using UnityEngine.UI;
using Bos;
using Bos.UI;

public abstract class EventView : GameBehaviour
{
    public override void Start()
    {
        //gameObject.SetActive(false);
    }

    public virtual void Show(LocalEvent evt)
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        ViewService.Remove(ViewType.BosEventView);
    }
}
