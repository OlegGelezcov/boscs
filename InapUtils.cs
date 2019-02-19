using System;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;
    using UDBG = UnityEngine.Debug;

    public static class InapUtils  {
        public static bool IsDeviceJailBrokened() {
#if UNITY_EDITOR || UNITY_ANDROID
            return false;
#elif UNITY_IOS
            var paths = new[]
            {
            "/Applications/Cydia.app",
            "/private/var/lib/cydia",
            "/private/var/tmp/cydia.log",
            "/System/Library/LaunchDaemons/com.saurik.Cydia.Startup.plist",
            "/usr/libexec/sftp-server",
            "/usr/bin/sshd",
            "/usr/sbin/sshd",
            "/Applications/FakeCarrier.app",
            "/Applications/SBSettings.app",
            "/Applications/WinterBoard.app",
            "/system/app/Superuser.apk",
            "/sbin/su",
            "/system/bin/su",
            "/system/xbin/su",
            "/data/local/xbin/su",
            "/data/local/bin/su",
            "/system/sd/xbin/su",
            "/system/bin/failsafe/su",
            "/data/local/su",
            "/su/bin/su"
            };
            return paths.Any(File.Exists);
#else
            return false;
#endif
        }

        public static bool IsProductReceiptValid(Product product) {
            if(product == null ) {
                return false;
            }

#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            try {
                var result = validator.Validate(product.receipt);
                UDBG.Log("Receipt is valid. Contents:");
                foreach (IPurchaseReceipt productReceipt in result) {
                    UDBG.Log(productReceipt.productID);
                    UDBG.Log(productReceipt.purchaseDate);
                    UDBG.Log(productReceipt.transactionID);
                }
            } catch (IAPSecurityException exception) {
                UDBG.Log("Invalid receipt not unlcokded content".Attrib(bold: true, italic: true, color: "r"));
                return false;
            }
#endif
            return true;
        }
        
        
        private static string _login = "prcode";
        private static string _password = "CrjhjKtnj";
        private static string _auth = "AUTHORIZATION";
        private static string _secretKey = "BA9A16CF942BC68EB56BA3C2D28D1";
        
        public static IEnumerator VerifyIosReceipt(byte[] appleConfigAppReceipt)
        {
            var authorization = Authenticate(_login, _password);
            var url = "bos.heatherglade.com/purchase?defs=" + GetEncodeQuery();
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("session_data=ios"),
                new MultipartFormFileSection("receipt", appleConfigAppReceipt, "receipt", "application/pkix-cert")
            };
            var www = UnityWebRequest.Post(url, formData, Encoding.UTF8.GetBytes("(.*)$)"));
            www.SetRequestHeader(_auth, authorization);
            www.SetRequestHeader("Content-Type", "application/pkix-cert");

            yield return www.Send();
            while (!www.isDone)
                yield return null;

            bool valid = true;
            if (www.isDone)
            {
                var token = www.downloadHandler.text;
                try
                {
                    var jsonPayload = JWT.JsonWebToken.Decode(token, _secretKey);
                    var obj = JObject.Parse(jsonPayload);
                    if (Equals(obj["response"]["code"].ToString(), "success"))
                    {
                        UnityEngine.Debug.Log("receipt is valid");
                    }
                    else
                    {
                        UnityEngine.Debug.Log("receipt is not valid");
                        valid = false;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.Message);
                    valid = false;
                }
            }            
            yield return valid;
        }
        
        private static string Authenticate(string username, string password)
        {
            var auth = username + ":" + password;
            auth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
            auth = "Basic " + auth;
            return auth;
        }
        
        private static string GetEncodeQuery()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var deviceOS = SystemInfo.operatingSystem;
#if UNITY_ANDROID
            deviceOS = "android";
            deviceId = SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IPHONE
        deviceOS = "ios";
        deviceId = UnityEngine.iOS.Device.vendorIdentifier;
#endif

#if UNITY_EDITOR
            deviceId = SystemInfo.deviceUniqueIdentifier;
            deviceOS = "Editor";
#endif
            var query = "{ \"device_id\": " + "\"" + deviceId + "\"" + ", \"device_os\" : " + "\"" + deviceOS + "\"" + "}";
            var bytes = Encoding.ASCII.GetBytes(query);
            var encodedText = Convert.ToBase64String(bytes).Trim('=');
            return encodedText;
        }
    }
}