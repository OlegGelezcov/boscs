using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemController : MonoBehaviour
{
    public static EventSystemController Instance;

    private bool _inProcess;
    
    private void Awake()
    {
        Instance = this;
    }


    public static  void OffEventSystemForSeconds(float seconds)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.OffForSecondsImpl(seconds));
        }
    }

    private IEnumerator OffForSecondsImpl(float seconds)
    {
        if (_inProcess) yield break;
        
        _inProcess = true;
        var eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.gameObject.SetActive(false);
            yield return new WaitForSeconds(seconds);
            eventSystem.gameObject.SetActive(true);
        }
        _inProcess = false;
    }
}
