using System.Collections;
using System.Collections.Generic;
using Bos;
using Facebook.Unity;
using JetBrains.Annotations;
using TwitterKit.Unity;
using UnityEngine;
using UnityEngine.UI;

public class SocialScreen : GameBehaviour
{
    public GameObject Facebook10Coins;
    public GameObject Twitter10Coins;
    public GameObject FacebookNormal;
    public GameObject TwitterNormal;

    public Text GetCoinFb;
    public Text GetCoinTw;


    public override void Awake()
    {
        GetCoinFb.SetStringForKey("GET.COINS");
        GetCoinTw.SetStringForKey("GET.COINS");
    }

    public override void OnEnable()
    {
        EnableDisableButtons();
    }

    public void Facebook_Click()
    {
        var perms = new List<string>(){"public_profile", "email"};
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    public void Twitter_Click()
    {
        StartLogin();
    }

    private void EnableDisableButtons()
    {
        if (Player.LegacyPlayerData.FreeCoinsFB)
        {
            Facebook10Coins.SetActive(false);
            FacebookNormal.SetActive(true);
        }
        else
        {
            Facebook10Coins.SetActive(true);
            FacebookNormal.SetActive(false);
        }

        if (Player.LegacyPlayerData.FreeCoinsTW)
        {
            Twitter10Coins.SetActive(false);
            TwitterNormal.SetActive(true);
        }
        else
        {
            Twitter10Coins.SetActive(true);
            TwitterNormal.SetActive(false);
        }
    }
    
    private void AuthCallback (ILoginResult result) {
        if (FB.IsLoggedIn) {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions) {
                Debug.Log(perm);
            }
            
            if (!Player.LegacyPlayerData.FreeCoinsFB)
            {
                Player.LegacyPlayerData.FreeCoinsFB = true;
                Player.AddCoins(10, isFree: true);
            }

            Application.OpenURL(UtilsHelper.CheckPackageAppIsPresent("com.facebook.katana")
                ? "fb://page/1489390894492960"
                : "https://www.facebook.com/transportcapitalist/");

            EnableDisableButtons();
            
        } else {
            Debug.Log("User cancelled login");
        }
    }
    
   


    public void StartLogin () {
        var session = Twitter.Session;
        if (session == null) {
            Twitter.LogIn (LoginComplete, LoginFailure);
        } else {
            LoginComplete (session);
        }
    }

    public void LoginComplete (TwitterSession session) {
        
        if (!Player.LegacyPlayerData.FreeCoinsTW)
        {
            Player.LegacyPlayerData.FreeCoinsTW = true;
            Player.AddCoins(10, isFree: true);
        }

        Application.OpenURL("https://twitter.com/TrCapitalist"); 

        EnableDisableButtons();
    }

    public void LoginFailure (ApiError error) {
        UnityEngine.Debug.Log ("code=" + error.code + " msg=" + error.message);
    }
}
