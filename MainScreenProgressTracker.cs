using System;
using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenProgressTracker : GameBehaviour
{
    public Image ProgressBar;
    public Text ProgressXPText;
    private float _curFill;

    public override void Start()
    {
        ProgressBar.fillAmount = 0;
        UpdateXPText();
    }

    public override void OnEnable() {
        GameEvents.XPChanged += OnXPChanged;
    }

    public override void OnDisable() {
        GameEvents.XPChanged -= OnXPChanged;
    }
    
    private void OnXPChanged(int oldXP, int newXP)
        => UpdateXPText();

    private void UpdateXPText(){
        IPlayerService player = Services.PlayerService;

        var q = (player.CurrentXP / (float)player.XPLevelLimit);
        if (_curFill != q)
        {
            Hashtable ht = iTween.Hash(
                "from", ProgressBar.fillAmount, 
                "to", q, 
                "time", .5f, 
                "onupdate", "UpdateProgressFill", 
                "easetype", iTween.EaseType.easeInOutCubic);

            iTween.ValueTo(gameObject, ht);
            _curFill = q;
        }

        ProgressXPText.text = $"{player.CurrentXP} / {player.XPLevelLimit} XP";
    }

    private void UpdateProgressFill(float newValue)
    {
        ProgressBar.fillAmount = newValue;
    }
}
