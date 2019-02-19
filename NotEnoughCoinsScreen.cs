/*
using Bos;
using Bos.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotEnoughCoinsScreen : GameBehaviour
{
    public Text RequiredCoinsText;
    public static NotEnoughCoinsScreen Instance;
    public GameObject content;
    public Button buyButton;

    public override void Awake()
    {
        Instance = this;
    }

    public override void OnEnable() {
        base.OnEnable();
        buyButton.SetListener(() => {
            Services.SoundService.PlayOneShot(SoundName.buyGenerator);
            content.Deactivate();
            Services.ViewService.ShowDelayed(Bos.UI.ViewType.StoreView, BosUISettings.Instance.ViewShowDelay);
        });
    }

    public void Show(int reqCoins)
    {
        content.SetActive(true);
        RequiredCoinsText.text = reqCoins.ToString();
    }


}
*/
