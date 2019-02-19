namespace Bos {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UDebug = UnityEngine.Debug;

    public class SleepService : GameBehaviour, ISleepService {

        public int SleepInterval { get; private set; } = 0;

        public bool IsRunning { get; private set; } = false;

        public void Setup(object data = null) { }

        private bool isOnPauseCalled = false;
        private bool isOnResumeCalled = false;

        public override void Start(){
            OnResume();
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{nameof(SleepService)}.{nameof(UpdateResume)} => {pause}");


        private void OnApplicationPause(bool isPause) {
            //UDebug.Log($"on applicayion pause pause: {isPause}");
            if(isPause) {
                OnPause();
            } else {
                OnResume();
            }
        }

        private void OnApplicationFocus(bool isFocus ) {
            //UDebug.Log($"on application focus: {isFocus}");
            if(isFocus) {
                OnResume();
            } else {
                OnPause();
            }
        }

        private void OnPause() {
            //UDebug.Log("sleepsrv: onpause called");
            if (!isOnPauseCalled) {
                if (IsRunning) {
                    WriteTime();
                    IsRunning = false;
                    //UDebug.Log($"set is running to false");
                }
                isOnPauseCalled = true;
                isOnResumeCalled = false;
                GameEvents.OnPause();             
                Services?.SaveService?.SaveAll();
                //UDebug.Log($"OnPause EVENT()".Colored(ConsoleTextColor.lightblue));
            } 
        }

        private void OnResume() {
            //UDebug.Log($"sleepsrv: resume callled");
            //Services?.GetService<IConsoleService>()?.AddOnScreenText("OnResume()");
            if (!isOnResumeCalled) {
                if (!IsRunning) {
                    //UpdateSleepInterval();
                    //IsRunning = true;
                    StartCoroutine(UpdateSleepIntervalWhenTimeServiceReceivedImpl());
                    //UDebug.Log($"sleepserv: UpdateSleepInterval coroutine callled");
                }
                isOnResumeCalled = true;
                isOnPauseCalled = false;
                
                //UDebug.Log($"OnResume EVENT()".Colored(ConsoleTextColor.lightblue));
            }
        }

        private void OnApplicationQuit() {
            if(IsRunning) {
                WriteTime();
                IsRunning = false;
            }
            GameEvents.OnQuit();
            //UDebug.Log($"OnQuit EVENT()".Colored(ConsoleTextColor.lightblue));
        }

        private IEnumerator UpdateSleepIntervalWhenTimeServiceReceivedImpl() {
            yield return new WaitUntil(() => Services.TimeService.IsValid);

            //UDebug.Log($"sleepserv.updatesleepinterval: wait for timer updated");
            bool isTimeUpdated = false;
            Services.TimeService.SendRequestWithAction(() => { isTimeUpdated = true; });
            yield return new WaitUntil(() => isTimeUpdated);
            //UDebug.Log($"sleepsrv: after timer updated");

            UpdateSleepInterval();
            IsRunning = true;
            //UDebug.Log($"Sleep Service Started with interval => {SleepInterval}".Colored(ConsoleTextColor.green).BoldItalic());
            GameEvents.OnResume();
            //Services?.GetService<IConsoleService>()?.AddOnScreenText($"OnResume() event with inter => {SleepInterval}");
        }

        private void UpdateSleepInterval() {

            SleepInterval = BosUtils.UnixTimestampInt - ReadTime();
            //UDebug.Log($"UPDATE SLEEP INTERVAL: {SleepInterval}, Unix Time: {BosUtils.UnixTimestampInt}, SAVED TIME: {ReadTime()}".Colored(ConsoleTextColor.navy).BoldItalic());
            if (SleepInterval < 0) {
                SleepInterval = 0;
            }
        }

        private void WriteTime() {
            //UDebug.Log($"WRITE TIME: {BosUtils.UnixTimestampInt}".Colored(ConsoleTextColor.navy).BoldItalic());
            PlayerPrefs.SetInt("pause_time", BosUtils.UnixTimestampInt);
            PlayerPrefs.Save();
        }

        private int ReadTime() {
            return PlayerPrefs.GetInt("pause_time", BosUtils.UnixTimestampInt);
        }
    }

    public interface ISleepService : IGameService {
        int SleepInterval { get; }
        bool IsRunning { get; }
    }
}