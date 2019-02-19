using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bos;
using Bos.Debug;
using Facebook.Unity;
using UniRx;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : GameBehaviour
{
    private Dictionary<GameObject, CanvasRenderingHelper> _disabledRenderers = new Dictionary<GameObject, CanvasRenderingHelper>();
    public GameObject MainScreen;
    public GameObject MainMenuObjects;
    public GameObject PressBackToExit;
    public Transform Dummy;

    public override void Start()
    {
        //WatchAdScreen.SetActive(false);
        PressBackToExit.SetActive(false);
        //EndgameScreen.SetActive(false);
        //_rateShowByTransport = !Services.TransportService.HasUnits(2);
    }


    //private float frameTime = 0;

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_stack.Any())
                _stack.Pop().Invoke();
        }
        //if (frameTime < 60) {
        //    frameTime += Time.deltaTime;
        //    return;
        //}
        //frameTime = 0;

        //if (GlobalRefs.PlayerData.UserRated) return;
        //var inGameSpan = DateTime.Now - LocalData.SessionStart;
        //if ((inGameSpan.TotalMinutes > 20 || Services.TransportService.HasUnits(2) && 
        //    _rateShowByTransport) && !GlobalRefs.PlayerData.UserRated && !_rateAlreadyShow) {
        //    _rateAlreadyShow = true;
        //    RateAppScreen.SetActive(true);
        //}

    }
   

    //public void Rate()
    //{
    //    if (GlobalRefs.PlayerData.UserRated)
    //    {
    //        RateAppScreen.SetActive(false);
    //        return;
    //    }



    //    GlobalRefs.PlayerData.UserRated = true;
    //    RateAppScreen.SetActive(false);
        
    //}

    public void ShowPressBackToExit()
    {
        PressBackToExit.SetActive(true);
        TransientData.CanExit = true;
        StartCoroutine(ClosePressBackInTime());
    }

    private IEnumerator ClosePressBackInTime()
    {
        yield return new WaitForSeconds(4);
        TransientData.CanExit = false;
        PressBackToExit.SetActive(false);
    }
    
    public void PlaySlotMachine(Action a = null)
    {
        StartCoroutine(LoadScene(2, a));
    }
    
    
    public void PlaySlotRacingGame(Action a = null)
    {
        if (!IsSceneLoading) {
            IsSceneLoading = true;
            StartCoroutine(LoadSceneWithDelay(3, a, 1f));
            EnableDisableCanvasRendering(CanvasTypeHide.AllDisable);
        } else {
            Debug.Log("scene already loading...");
        }
    }
    

    public void HideSlotManagerGame(Action a = null)
    {
        SceneManager.UnloadSceneAsync(2);
    }
    
    public void HideRaceGame(Action a = null)
    {
        SceneManager.UnloadSceneAsync(3);
    }

    //public void ShowWatchAd()
    //{
    //    WatchAdScreen.SetActive(true);
    //    AddBackAction(ShowWatchAd);
    //}

    //public void CloseWatchAd()
    //{
    //    WatchAdScreen.SetActive(false);
    //    RemoveLastBackAction(CloseWatchAd);
    //}

    public bool IsSceneLoading { get; private set; } = false;

    private IEnumerator LoadSceneWithDelay(int v, Action a = null, float delay = 0.0f) {
        var async = SceneManager.LoadSceneAsync(v, LoadSceneMode.Additive);
        yield return async;
        var scene = SceneManager.GetSceneByBuildIndex(v);
        SceneManager.SetActiveScene(scene);
        if(delay > 0.0f ) {
            yield return new WaitForSeconds(delay);
        }
        a?.Invoke();
        IsSceneLoading = false;
    }
    private IEnumerator LoadScene(int v, Action a = null)
    {

        var async = SceneManager.LoadSceneAsync(v, LoadSceneMode.Additive);
        yield return async;
        var scene = SceneManager.GetSceneByBuildIndex(v);
        SceneManager.SetActiveScene(scene);
        a?.Invoke();
        IsSceneLoading = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public IEnumerator DisableCanvasRendering(GameObject obj)
    {
        var helper = new CanvasRenderingHelper() { Parent = obj.transform.parent, SiblingIndex = obj.transform.GetSiblingIndex() };

        obj.transform.parent = Dummy;
        
        if (!_disabledRenderers.ContainsKey(obj))
            _disabledRenderers.Add(obj, helper);
        
        yield break;
    }

    public IEnumerator EnableCanvasRendering(GameObject obj)
    {
        if (!_disabledRenderers.ContainsKey(obj)) yield break;
        obj.transform.parent = _disabledRenderers[obj].Parent;
        obj.transform.SetSiblingIndex(_disabledRenderers[obj].SiblingIndex);
        _disabledRenderers.Remove(obj);
        yield break;
    }

    //internal void HideManagerButton()
    //{
    //    HireManagerButton.SetActive(false);
    //}
    public void ProcFinish()
    {
        //EndgameScreen.SetActive(true);
    }

    //public void Finish_Ok()
    //{
    //    BalanceManager.Reset();
    //    Services.PlayerService.SetSecurities(0);
    //    Services.GenerationService.Generators.SetPermanentProfit(1);
    //    Services.GenerationService.Generators.SetPermanentTime(1);
    //    Services.GenerationService.Generators.SetGlobalProfit(1);
    //    Services.GenerationService.Generators.SetGlobalTime(1);
    //    GlobalRefs.PlayerData.GameFinished++;
    //    Services.PlayerService.SetLifetimeEarnings(0.0);
    //    GlobalRefs.PlayerData.Save();
    //    GlobalRefs.IAP.AddCoins(1000);
        
    //    FacebookEventUtils.LogGameCompleteEvent();
    //}

    //public void OpenSupport()
    //{
    //    Application.OpenURL("http://heatherglade.com/#contact_us");
    //}

        
    public void EnableDisableCanvasRendering(CanvasTypeHide type)
    {
        switch (type)
        {
            case CanvasTypeHide.AllEnable:
                StartCoroutine(EnableCanvasRendering(MainScreen));
                StartCoroutine(EnableCanvasRendering(MainMenuObjects));
                break;
            case CanvasTypeHide.GeneratorsEnable: 
                StartCoroutine(EnableCanvasRendering(MainScreen));
                StartCoroutine(EnableCanvasRendering(MainMenuObjects));
                break;
            case CanvasTypeHide.AllDisable:
                StartCoroutine(DisableCanvasRendering(MainScreen));
                StartCoroutine(DisableCanvasRendering(MainMenuObjects));
                break;
            case CanvasTypeHide.GeneratorsDisable: 
                StartCoroutine(DisableCanvasRendering(MainScreen));
                StartCoroutine(DisableCanvasRendering(MainMenuObjects));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private Stack<Action> _stack = new Stack<Action>();
    public void AddBackAction(Action a)
    {
        _stack.Push(a);
    }

    public void RemoveLastBackAction(Action a)
    {
        if (_stack.Any() && _stack.Peek() == a)
        {
            _stack.Pop();
        }
    }
}



public class CanvasRenderingHelper
{
    public int SiblingIndex;
    public Transform Parent;
}

public enum CanvasTypeHide
{
    AllEnable,
    MenuEnable,
    GeneratorsEnable,
    AllDisable,
    MenuDisable,
    GeneratorsDisable,
}