using System;
using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;
using Bos.Debug;
using Bos.UI;
using UniRx;

//[ExecuteInEditMode]
public class PortraitRenderer : GameBehaviour
{
    public PlayerLevelManager LevelingManager;

    public PortraitLevelObjects[] LevelObjects;
    public Image[] LevelFrames;
    public Text LevelText;

    /// <summary>
    /// 4 2 1 3 5
    /// </summary>
    public GameObject[] Stars;
    public ColorTierShade[] ColorTiers;

    public Image BaseFrame;

    public LevelPortraitDetails PortraitDetails { get { return LevelingManager.PortraitDetails; } }

    public Button profileButton;

    public GameObject profileScreen;

    private bool IsInitialized { get; set; }

    public override void Start(){
        UpdateViews();

        if(!IsInitialized) {
            GameEvents.StatusLevelChangedObservable.Subscribe(args => {
                UpdateViews();
                LevelText.gameObject.ScaleMe(1, 1.1f, 0.3f, EaseType.EaseInCubic, () => {
                    LevelText.gameObject.ScaleMe(1.1f, 1, 0.3f, EaseType.EaseInCubic, () => { });
                });
            }).AddTo(gameObject);

            IsInitialized = true;
        }
    }

    public override void OnEnable(){
        GameEvents.LevelChanged += OnLevelChanged;

        profileButton.SetListener(() => {
            //profileScreen.Activate();
            Services.ViewService.ShowDelayed(ViewType.ProfileView, BosUISettings.Instance.ViewShowDelay);
            Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
        });
    }

    public override void OnDisable(){
        GameEvents.LevelChanged -= OnLevelChanged;
    }



    private void OnLevelChanged(int oldLevel, int newLevel) {
        Debug.Log($"NEW LEVEL => {newLevel}".Colored(ConsoleTextColor.yellow).Bold());
        UpdateViews();
    }

    private void UpdateViews() {
        int statusLevel = Player.StatusLevel;
        if (statusLevel > 0) {
            LevelText.text = Player.StatusLevel.ToString();
        } else {
            LevelText.text = string.Empty;
        }
    }

    private void ColorizeAndUpdateLevelText(int ct, int level)
    {
        if(ct >= ColorTiers.Length) {
            ct = ColorTiers.Length - 1;
        }

        var colorTier = ColorTiers[ct];
        LevelText.text = level.ToString();
        LevelText.color = colorTier.TextColor;
    }

    private void ColorizeLevelFrames(int ct)
    {
        var colorTier = ColorTiers[ct];

        foreach (var x in LevelFrames)
        {
            x.color = colorTier.StarColor;
        }
    }

    private void ColorizePortraitItems(int ct)
    {
        if (ct > ColorTiers.Length)
            ct = ColorTiers.Length - 1;

        var colorTier = ColorTiers[ct];

        int i = 0;
        foreach (var q in LevelObjects)
        {
            foreach (var x in q.Items)
            {
                if (i >= colorTier.Colors.Length)
                {
                    x.GetComponent<Image>().color = colorTier.Colors[colorTier.Colors.Length - 1];
                }
                else
                {
                    x.GetComponent<Image>().color = colorTier.Colors[i];
                }
                i++;
            }
        }

        foreach (var star in Stars)
        {
            star.GetComponent<Image>().color = colorTier.StarColor;
        }

        BaseFrame.color = colorTier.StarColor;
    }

    private void ActivatePortraitItems()
    {
        foreach (var x in LevelObjects)
        {
            x.Deactivate();
        }

        if (PortraitDetails == null)
            return;

        for (int i = 0; i < PortraitDetails.MaxElement; i++)
        {
            LevelObjects[i].Activate();
        }

        DeactivateStars();

        switch (PortraitDetails.Stars)
        {
            case 1:
                Stars[2].SetActive(true);
                break;
            case 2:
                Stars[1].SetActive(true);
                Stars[3].SetActive(true);
                break;
            case 3:
                Stars[1].SetActive(true);
                Stars[2].SetActive(true);
                Stars[3].SetActive(true);
                break;
            case 4:
                Stars[0].SetActive(true);
                Stars[1].SetActive(true);
                Stars[3].SetActive(true);
                Stars[4].SetActive(true);
                break;
            case 5:
                foreach (var x in Stars)
                {
                    x.SetActive(true);
                }
                break;
            default:
                break;
        }
    }

    private void DeactivateStars()
    {
        foreach (var x in Stars)
        {
            x.SetActive(false);
        }
    }



}

[Serializable]
public class PortraitLevelObjects
{
    public GameObject[] Items;

    public void Activate()
    {
        foreach (var x in Items)
        {
            x.SetActive(true);
        }
    }

    public void Deactivate()
    {
        foreach (var x in Items)
        {
            x.SetActive(false);
        }
    }
}

[Serializable]
public class ColorTierShade
{
    public string Name;
    public Color[] Colors;
    public Color StarColor;
    public Color TextColor;
}