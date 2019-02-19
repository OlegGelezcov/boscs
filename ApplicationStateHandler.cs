using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class ApplicationStateHandler : MonoBehaviour
//{
//    /*
//    private List<Action> _onPauseHandlers;
//    private List<Action> _onResumeHandlers;
//    private List<Action> _onQuitHandlers;

//    private bool isResumeCalled = false;
//    private bool isPauseCalled = false;
    

//    public ApplicationStateHandler()
//    {
//        _onPauseHandlers = new List<Action>();
//        _onQuitHandlers = new List<Action>();
//        _onResumeHandlers = new List<Action>();
//    }

//    private void Awake()
//    {
//        GlobalRefs.StateHandler = this;
//    }

//    public void RegisterPauseHandler(Action handler)
//    {
//        RegisterHandler(ApplicationHandlerType.Pause, handler);
//    }

//    public void RegisterResumeHandler(Action handler)
//    {
//        RegisterHandler(ApplicationHandlerType.Resume, handler);
//    }

//    public void RegisterQuitHandler(Action handler)
//    {
//        RegisterHandler(ApplicationHandlerType.Quit, handler);
//    }

//    public void RegisterHandler(ApplicationHandlerType type, Action handler)
//    {
//        switch (type)
//        {
//            case ApplicationHandlerType.Pause:
//                _onPauseHandlers.Add(handler); return;
//            case ApplicationHandlerType.Resume:
//                _onResumeHandlers.Add(handler); return;
//            case ApplicationHandlerType.Quit:
//                _onQuitHandlers.Add(handler); return;
//            default:
//                break;
//        }
//    }

//    private void OnApplicationPause(bool pause)
//    {
//        if (pause)
//        {
//            if (!isPauseCalled) {
//                _onPauseHandlers.ForEach(handler => handler());
//                isPauseCalled = true;
//                isResumeCalled = false;
//                PlayerPrefs.Save();
//            }
//        }
//        else
//        {
//            if (!isResumeCalled) {
//                _onResumeHandlers.ForEach(handler => handler());
//                isResumeCalled = true;
//                isPauseCalled = false;
//            }
//        }

        
//        //StreamingSerialization.Save("pdata.json", GlobalRefs.PlayerData);
//    }

//    private void OnApplicationFocus(bool focus ) {
//        if(focus) {
//            if(!isResumeCalled) {
//                _onResumeHandlers.ForEach(handler => handler());
//                isResumeCalled = true;
//                isPauseCalled = false;
//                PlayerPrefs.Save();
//            }
//        } else {
//            if(!isPauseCalled) {
//                _onPauseHandlers.ForEach(handler => handler());
//                isPauseCalled = true;
//                isResumeCalled = false;
//            }
//        }
//    }

//    private void OnApplicationQuit()
//    {
//        _onQuitHandlers.ForEach(handler => handler());
//        PlayerPrefs.Save();
//    }*/

//}

//public enum ApplicationHandlerType
//{
//    Pause,
//    Resume,
//    Quit
//}
