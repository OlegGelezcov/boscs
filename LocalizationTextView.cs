using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationTextView : MonoBehaviour
{

    public string LocalizationID;
    private void Awake()
    {
       // GetComponent<Text>().text = LocalizationID.GetLocalizedString();
    }
}
