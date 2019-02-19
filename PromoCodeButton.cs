using Bos;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromoCodeButton : MonoBehaviour
{
    public const string PROMO_CODE_URL_BASE = "http://tcpromocodeservice.azurewebsites.net/api/promocode";

    public InputField TextBox;
    public PromoCodeResultScreen ResultScreen;

    public void Click()
    {
        var str = TextBox.text;
        if (string.IsNullOrWhiteSpace(str))
        {
            ResultScreen.Show("Hold on", "It seems you didn't enter a code", showProgress: false, showContinue: true);
            return;
        }

        ResultScreen.Show("Please wait", "Verifying your code...", showProgress: true, showContinue: false);

        StartCoroutine(VerifyCode(str));
    }

    private IEnumerator VerifyCode(string str)
    {
        var deviceId = SystemInfo.deviceUniqueIdentifier;

        using (WWW req = new WWW($"{PROMO_CODE_URL_BASE}/{str}/{deviceId}"))
        {
            yield return req;

            if (string.IsNullOrWhiteSpace(req.text))
            {
                ResultScreen.Show("Error", "Unable to verify code.", showProgress: false, showContinue: true);
            }
            else
            {
                var resp = JsonConvert.DeserializeObject<RedeemResult>(req.text);
                string title = null;
                string message = null;

                switch (resp.Status)
                {
                    case RedeemStatus.OK:
                        title = "All Good";
                        message = $"Succcesfully redeemed code. You were granted {(int)resp.Value} coins.";

                        //GlobalRefs.IAP.AddCoins((int)resp.Value, free: true);
                        GameServices.Instance.PlayerService.AddCoins((int)resp.Value, isFree: true);
                        break;
                    case RedeemStatus.AlreadyUsed:
                        title = "Code used";
                        message = resp.Message;
                        break;
                    case RedeemStatus.Expired:
                        title = "Code Expired";
                        message = resp.Message;
                        break;
                    case RedeemStatus.FailUnknown:
                        title = "Oops";
                        message = resp.Message;
                        break;
                    case RedeemStatus.NotFound:
                        title = "Not found";
                        message = resp.Message;
                        break;
                    default:
                        break;
                }
                ResultScreen.Show(title, message, showProgress: false, showContinue: true);
            }
        }
    }
}