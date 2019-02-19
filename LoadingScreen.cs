using Bos;
using Bos.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : TypedView
{
    public RectTransform progressParent;
    public Image ProgressImage;
    public Text loadingText;
    public Image backImage;
    public LoadingScreenSprite[] loadingBackSprites;
    public CanvasGroup canvasGroup;

    public override ViewType Type => ViewType.StartView;

    public override CanvasType CanvasType => CanvasType.UI;

    public override bool IsModal => false;

    public override int ViewDepth => 1000;

    public override void Setup(ViewData data) {
        base.Setup(data);
        SetupLoadingBackSprite();
        loadingText.text = string.Empty;
        ProgressImage.fillAmount = 0;
    }

    private void SetupLoadingBackSprite() {
        int planetId = Services.LoadingPlanet;
        for(int i = 0; i < loadingBackSprites.Length; i++ ) {
            if(planetId == loadingBackSprites[i].planetId) {
                backImage.overrideSprite = loadingBackSprites[i].loadingSprite;
                break;
            }
        }
    }

    public override void OnEnable() {
        base.OnEnable();
        NavigateToGame.ReportLoadProgress += OnReportProgress;
        GameEvents.GameModeChanged += OnGameModeChanged;
    }

    public override void OnDisable() {
        NavigateToGame.ReportLoadProgress -= OnReportProgress;
        GameEvents.GameModeChanged -= OnGameModeChanged;
        base.OnDisable();
    }

    private void FillWithValue(float value) {
        var fillAnimator = ProgressImage.gameObject.GetOrAdd<FloatAnimator>();
        float time = Mathf.Abs(value - ProgressImage.fillAmount);
        if(Mathf.Approximately(time, 0)) {
            //time = 0.05f;
            return;
        }

        //print($"fill from => {ProgressImage.fillAmount} to {value}");
        if (fillAnimator.IsBusy) {
            fillAnimator.StartAnimation(new FloatAnimationData {
                Duration = time,
                StartValue = ProgressImage.fillAmount,
                EndValue = value,
                EaseType = EaseType.EaseInOutQuad,
                Target = ProgressImage.gameObject,
                OnStart = (v, _) => ProgressImage.fillAmount = v,
                OnUpdate = (v, _1, _2) => ProgressImage.fillAmount = v,
                OnEnd = (v, _) => ProgressImage.fillAmount = v
            });
        }
    }


    private void OnReportProgress(float progress) {
        //ProgressImage.fillAmount = progress;
        if (progress < 0.9) {
            FillWithValue(progress);
            //Debug.Log($"OnReportProgress() => {progress}");
        }


        if(progress >= 0.9f ) {
            ProgressImage.fillAmount = 1;
            ProgressImage.gameObject.GetOrAdd<FloatAnimator>().StartAnimation(new FloatAnimationData {
                StartValue = ProgressImage.fillAmount,
                EndValue = 1,
                Duration = 0.5f,
                EaseType = EaseType.EaseInOutQuad,
                OnStart = (v, go1) => ProgressImage.fillAmount = v,
                OnUpdate = (v, t1, go1) => ProgressImage.fillAmount = v,
                OnEnd = (v, go1) => {
                    ProgressImage.fillAmount = v;
                    progressParent.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(
                    new Vector3AnimationData {
                        StartValue = Vector3.one,
                        EndValue = Vector3.zero,
                        Duration = 0.5f,
                        EaseType = EaseType.EaseInOutQuad,
                        OnStart = (scale, go) => progressParent.localScale = scale,
                        OnUpdate = (scale, t, go) => progressParent.localScale = scale,
                        OnEnd = (scale, go) => {
                            progressParent.localScale = scale;
                        
                    }
                    });
                }
            });
        }
    }

    private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode ) {
        if(newGameMode == GameModeName.Game ) {
            var alphaAnimator = canvasGroup.gameObject.GetOrAdd<FloatAnimator>();
            alphaAnimator.StartAnimation(new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = 0.4f,
                EaseType = EaseType.EaseInOutQuad,
                Target = canvasGroup.gameObject,
                OnStart = canvasGroup.UpdateAlphaFunctor(),
                OnUpdate = canvasGroup.UpdateAlphaTimedFunctor(),
                OnEnd = canvasGroup.UpdateAlphaFunctor(() => {
                    ViewService.Remove(ViewType.StartView);
                })
            });
        }
    }
}

[System.Serializable]
public class LoadingScreenSprite {
    public Sprite loadingSprite;
    public int planetId;
}
