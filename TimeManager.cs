namespace Bos {
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Bos;
using Bos.Debug;
using Bos.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.Networking;
using UDebug = UnityEngine.Debug;

public class TimeManager : SaveableGameBehaviour, ITimeService  {
    
        public bool isUseLocalTime = false;
        public float pingDelta = 10;

        private bool isTimerUpdated = false;

        private const string kFormate = "yyyy-MM-dd HH:mm:ss";
        private const string kUrl = "bos.heatherglade.com/get_device_key?defs=";
        private const string kSecretKey = "BA9A16CF942BC68EB56BA3C2D28D1";

        private const string kLogin = "prcode";
        private const string kPassword = "CrjhjKtnj";
        private const string kAuth = "AUTHORIZATION";

        private double deltaServer = 0;
        private double unixTime = 0;
        private TimerFail timerFail = null;

        #region ITimeService

        public void Setup(object obj) {
            timerFail = new TimerFail(10, () => !ViewService.Exists(UI.ViewType.ReconnectView), () => {
                if (!ViewService.Exists(UI.ViewType.ReconnectView)) {
                    ViewService.Show(UI.ViewType.ReconnectView);
                }
            }, () => {
                if(ViewService.Exists(UI.ViewType.ReconnectView)) {
                    var view = ViewService.FindView<ReconnectView>(ViewType.ReconnectView);
                    view?.ScheduleRemove();
                }
            });
        }

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        public DateTime Now
            => BosUtils.startDate.AddSeconds(UnixTime);


        public double UnixTime
            => unixTime + deltaServer;

        public int UnixTimeInt
            => (int)UnixTime;

        public bool IsValid { get; private set;} = false;

        #endregion

       

        public void StartService() {
            if(!isUseLocalTime) {
                StartCoroutine(UpdateTime());
            }
        }


        public override void Update()
        {
            if(isUseLocalTime) {
                isTimerUpdated = true;
                IsValid = true;
            } else {
                deltaServer += Time.deltaTime;
                timerFail?.Update(Time.deltaTime);
            }
        }
        
        private string GetEncodeQuery()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var deviceOS = SystemInfo.operatingSystem;
    #if UNITY_ANDROID
            deviceOS = "android";
            deviceId = SystemInfo.deviceUniqueIdentifier;
    #elif UNITY_IPHONE
            deviceOS = "ios";
            deviceId = Device.vendorIdentifier;
    #endif

    #if UNITY_EDITOR
            deviceId = SystemInfo.deviceUniqueIdentifier;
            deviceOS = "Editor";
    #endif
            var query = "{ \"device_id\": " +"\"" +deviceId + "\"" + ", \"device_os\" : " + "\""+ deviceOS +"\"" + "}";
            var bytes = Encoding.ASCII.GetBytes(query);
            var encodedText = Convert.ToBase64String(bytes).Trim('=');
            //Debug.LogError(encodedText);
            return encodedText;
        }

        private string authorization = string.Empty;
        private string url = string.Empty;

        private IEnumerator UpdateTime()
        {       
            authorization = Authenticate(kLogin, kPassword);
            url = "bos.heatherglade.com/get_device_key?defs=" + GetEncodeQuery();
            while (true)
            {
                yield return SendRequest(url, authorization);
                yield return new WaitForSeconds(pingDelta);
            }
        }

        public void SendRequestWithAction(System.Action action ) {
            StartCoroutine(SendRequestWithActionImpl(action));
        }

        private IEnumerator SendRequestWithActionImpl(System.Action action ) {
            yield return new WaitUntil(() => authorization.IsValid() && url.IsValid());
            yield return SendRequest(url, authorization);
            action?.Invoke();
        }

        private IEnumerator SendRequest(string url, string authorization)
        {
            var www = UnityWebRequest.Get(url);
            www.SetRequestHeader(kAuth, authorization);
            var op = www.SendWebRequest();
            yield return op;

            if(op.isDone) {
                var token = op.webRequest.downloadHandler.text;
                try {
                    var jsonPayload = JWT.JsonWebToken.Decode(token, kSecretKey);
                    //print($"TIME JSON => {jsonPayload}".Colored(ConsoleTextColor.green));
                    var date  = GetDateTimeFromJson(jsonPayload);
                    unixTime = BosUtils.GetUnixTimeFor(date);
                    deltaServer = 0;
                    IsValid = true;
                    isTimerUpdated = true;
                    timerFail.SetSuccess();
                } catch (Exception e) {
                    UnityEngine.Debug.Log(e.Message);
                    timerFail.SetFail();
                }
            }
        }

        private string Authenticate(string username, string password)
        {
            var auth = username + ":" + password;
            auth = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
            auth = "Basic " + auth;
            return auth;
        }

        private static DateTime GetDateTimeFromJson(string json)
        {
            var obj = JObject.Parse(json);
            var dateTimeString = obj["response"]["data"]["sts"].ToString();
            return DateTime.ParseExact(dateTimeString, kFormate, CultureInfo.InvariantCulture);
        }

        public override string SaveKey
            => "time_service";


        public override Type SaveType => typeof(TimeServiceSave);
        public override object GetSave()
        {
            //UDebug.Log($"TIME SERVICE SAVED".Colored(ConsoleTextColor.navy).BoldItalic());
            return new TimeServiceSave {
                 savedUnixTime = unixTime
            };
        }

        public override void ResetFull() {
            LoadDefaults();
        }

        public override void ResetByInvestors() {
            
        }

        public override void ResetByPlanets() {
            
        }

        public override void LoadDefaults()
        {
            unixTime = 0;
            IsLoaded = true;
        }
    
        public override void ResetByWinGame() {
            LoadDefaults();
        }

        public override void LoadSave(object obj)
        {
            TimeServiceSave save = obj as TimeServiceSave;

            if(save != null) {
                IsLoaded = true;
            } else {
                LoadDefaults();
            }
        }

        public IEnumerator WaitTimerUpdate() {
            yield return new WaitUntil(() => IsValid);
        }
    }

    public interface ITimeService : IGameService {

        bool IsValid {get;}

        double UnixTime{get;}

        int UnixTimeInt {get;}

        DateTime Now { get; }

        void StartService();

        IEnumerator WaitTimerUpdate();

        void SendRequestWithAction(System.Action action);
    }

    public class TimeServiceSave {
        public double savedUnixTime;
    }

    public class TimerFail {
        private float timer = 0f;
        public bool IsFail { get; private set; }

        private readonly float timeout;
        private Action failAction;
        private Func<bool> failActionCondition;
        private Action successAction;


        public TimerFail(float timeout, Func<bool> failActionPredicate, Action failAction, Action successAction) {
            this.IsFail = false;
            this.timeout = timeout;
            this.failAction = failAction;
            this.failActionCondition = failActionPredicate;
            this.successAction = successAction;
        }

        public void SetFail() {
            if (!IsFail) {
                IsFail = true;
                timer = 0f;
            }
        }

        public void SetSuccess() {
            bool oldFail = IsFail;
            IsFail = false;
            timer = 0f;
            if(oldFail ) {
                successAction?.Invoke();
            }
        }

        public void Update(float deltaTime) {
            if(IsFail) {
                timer += deltaTime;
                if(timer >= timeout ) {
                    if(failActionCondition?.Invoke() ?? false) {
                        failAction?.Invoke();
                        timer = 0f;
                    }
                }
            }
        }
    }
}