using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPNotificationHandler : MonoBehaviour
{
    public GameObject Notif;
    public Text DisplayText;

    private void Start()
    {
        Notif.SetActive(false);
    }

    public void ProcNotification(int xp)
    {
        StartCoroutine(ProcNotification_Internal(xp));
    }

    private IEnumerator ProcNotification_Internal(int xp)
    {
        Notif.SetActive(false);
        yield return new WaitForEndOfFrame();

        DisplayText.text = $"+{xp} XP";

        Notif.SetActive(true);
    }
}
