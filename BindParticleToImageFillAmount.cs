using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindParticleToImageFillAmount : MonoBehaviour {

    public Image parentImage;
    public Image fillImage;

    private RectTransform rectTransform;

    private void OnEnable() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        rectTransform.anchoredPosition = Vector2.zero + Vector2.right * fillImage.fillAmount * parentImage.GetComponent<RectTransform>().sizeDelta.x;
    }
}
