using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromoCodeResultScreen : MonoBehaviour
{
    public Text TitleText;
    public Text DescText;
    public GameObject ProgressCircle;
    public GameObject ContinueButton;

    public void Show(string title, string desc, bool showProgress = false, bool showContinue = false)
    {
        TitleText.text = title;
        DescText.text = desc;

        ProgressCircle.SetActive(showProgress);
        ContinueButton.SetActive(showContinue);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
