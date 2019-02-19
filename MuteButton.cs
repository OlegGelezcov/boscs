using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : GameBehaviour
{
    private bool _isOn;

    //public SoundManager SoundManager;
    public Sprite MuteIcon;
    public Sprite UnmuteIcon;
    private Button _button;

    public bool IsOn
    {
        get { return Services.SoundService.IsMute; }
        set { Services.SoundService.SetMute(value); }
    }

    public override void Start()
    {
        _button = GetComponent<Button>();
        _isOn = IsOn;
    }

    private void FixedUpdate()
    {
        if (_isOn)
        {
            Services.SoundService.SetMute(true);
            _button.image.sprite = MuteIcon;
        }
        else
        {
            Services.SoundService.SetMute(false);
            _button.image.sprite = UnmuteIcon;
        }
    }

    public void Click()
    {
        _isOn = !_isOn;
        IsOn = _isOn;
    }
}
