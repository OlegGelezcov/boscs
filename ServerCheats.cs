using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Bos;
using Bos.Data;
using Bos.Debug;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_IOS
   using UnityEngine.iOS; 
#endif

using UnityEngine.Networking;

public class ServerCheats : GameBehaviour, IServerBalanceService {

	//public static ServerCheats sharedInstance = null;

    private string _secretKey = "BA9A16CF942BC68EB56BA3C2D28D1";
    private string _timeData;

    private string _currentDate;
    private string _login = "prcode";
    private string _password = "CrjhjKtnj";
    private string _auth = "AUTHORIZATION";

    public string balanceUrl = "http://bos.heatherglade.com/get_balance";
    public string managerPrice = "http://bos.heatherglade.com/get_managers_prices";

    //public bool IsBalanceLoaded { get; private set; } = false;
    //public bool IsManagerLoaded { get; private set; } = false;
    
    //private void Awake() 
    //{
        
    //    if (sharedInstance == null) {
    //        sharedInstance = this;
    //    } else if (sharedInstance != this) {
    //        Destroy (gameObject);  
    //    }
        
    //}

    public void GetBalanceFromServer() {
        StartCoroutine(UpdateTime());
    }

    //public bool IsLoaded
    //    => IsBalanceLoaded && IsManagerLoaded;


    private IEnumerator UpdateTime()
    {       
        var authorization = Authenticate(_login, _password);
        var www = UnityWebRequest.Get(balanceUrl);
        www.SetRequestHeader(_auth, authorization);
        yield return www.Send();
        Debug.Log(balanceUrl.Colored(ConsoleTextColor.magenta).Bold());
        if (www.isDone)
        {
            if (www.isHttpError || www.isNetworkError) {
                Debug.Log($"error => {www.error}");
                yield break;
            }
            else {
                var token = www.downloadHandler.text;
                string jsonPayload = string.Empty;
                try
                {
                    jsonPayload = JWT.JsonWebToken.Decode(token, _secretKey);
                    UpdateBalance(jsonPayload);
               
                }
                catch (Exception e)
                {
                    Debug.Log(jsonPayload.BoldItalic().Colored(ConsoleTextColor.navy));
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }              
            }

        }
        
        www = UnityWebRequest.Get(managerPrice);
        www.SetRequestHeader(_auth, authorization);
        yield return www.Send();

        if (www.isDone)
        {
            var token = www.downloadHandler.text;
            try
            {
                var jsonPayload = JWT.JsonWebToken.Decode(token, _secretKey);
                UpdateManagers(jsonPayload);
               
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
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

    private void UpdateBalance(string json)
    {
        //Debug.Log(json);
        Services.GetService<IConsoleService>().AddOutput(json, ConsoleTextColor.yellow, true);
        var obj = JObject.Parse(json);

        int[] ids = Services.ResourceService.Generators.NormalIds;

        foreach(int id in ids ) {
            try {
                if (obj["response"]["data"][id] != null) {
                    var m = obj["response"]["data"][id];
                    GeneratorJsonData jsonData = new GeneratorJsonData {
                        id = id,
                        baseCost = m[1].ToString().ToDouble(),
                        incrementFactor = m[2].ToString().ToDouble(),
                        baseGeneration = m[3].ToString().ToDouble(),
                        timeToGenerate = m[4].ToString().ToFloat(),
                        //coinPrice = m[5].ToString().ToInt(),
                        enhancePrice = m[6].ToString().ToInt(),
                        profitIncrementFactor = m[7].ToString().ToDouble()
                        
                    };
                    Services.ResourceService.Generators.ReplaceValues(jsonData);
                }
            }catch(System.Exception exception) {
                Debug.LogError($"error of parsing generator => {id}");
            }
        }

        /*
        foreach (var generator in GameData.instance.generators)
        {
            if (obj["response"]["data"][generator.Id] != null)
            {
                var m = obj["response"]["data"][generator.Id];
                generator.BaseCost = m[1].ToString().ToDouble();
                generator.IncrementFactor = m[2].ToString().ToDouble();
                generator.BaseGeneration = m[3].ToString().ToDouble();
                generator.TimeToGenerate = m[4].ToString().ToFloat();
                generator.CoinPrice = m[5].ToString().ToInt();
                generator.EnhancePrice = m[6].ToString().ToInt();
                generator.ProfitIncrementFactor = m[7].ToString().ToDouble();

            }
        }*/
        GameEvents.OnGeneratorBalanceLoadedFromNet();
        Services.ResourceService.SetGeneratorsLoaded();
    }
    
    private void UpdateManagers(string json)
    {
        //Debug.Log(json);
        Services.GetService<IConsoleService>().AddOutput(json, ConsoleTextColor.yellow, true);
        var obj = JObject.Parse(json);

        var managers = obj["response"]["data"];
        //if (GameData.instance.managers == null) GameData.instance.managers = new List<ManagerPrototype>();
        //GameData.instance.managers.Clear();
        var index = 0;
        foreach (var manager in managers)
        {
            //var prot = GameData.instance.managers[index];
            //prot.Id = index;
            double baseCost = manager[0].ToString().ToDouble();
            double coef = manager[1].ToString().ToDouble();
            Services.ResourceService.Managers.UpdateValues(index, baseCost, coef);
            index++;
        }

        Services.ResourceService.SetManagersLoaded();
    }

    public void Setup(object data = null) {
        
    }

    public void UpdateResume(bool pause)
        => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");
}

public interface IServerBalanceService : Bos.IGameService {
    void GetBalanceFromServer();
    //bool IsLoaded { get; }
}