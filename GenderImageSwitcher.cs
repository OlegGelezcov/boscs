using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GenderImageSwitcher : GameBehaviour
{
    public Sprite MSprite;
    public Sprite FSprite;
    public Image _image;

    public override void Start()
    {
        UpdateIconSprite();
    }

    public override void OnEnable(){
        base.OnEnable();
        GameEvents.GenderChanged += OnGenderChanged;
    }

    public override void OnDisable() {
        GameEvents.GenderChanged -= OnGenderChanged;
        base.OnDisable();
    }

    private void OnGenderChanged(Gender oldValue, Gender newValue) 
        => UpdateIconSprite();

    private void UpdateIconSprite() {
        Gender gender = Services.PlayerService.Gender;
        if(gender == Gender.Male) {
            _image.overrideSprite = MSprite;
        } else {
            _image.overrideSprite = FSprite;
        }
    }

}