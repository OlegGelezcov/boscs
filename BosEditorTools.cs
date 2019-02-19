using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bos.Data;
using Newtonsoft.Json;
using System.IO;

public class BosEditorTools : MonoBehaviour {

    [MenuItem("Bos/Set Scale => 1 and Z => 0 on Selections")]
    private static void SetScaleAndZToDefault() {
        foreach(GameObject obj in Selection.gameObjects) {
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
            rectTransform.localScale = Vector3.one;
        }
    }

    [MenuItem("Bos/Add to cash upgrades currency type CASH")]
    private static void AddToCashUpgradesCashCurrencyType() {
        //string path = System.IO.Path.Combine(Application.dataPath, "Resources/Data/cash_upgrade.json");

        List<UpgradeJsonData> list = JsonConvert.DeserializeObject<List<UpgradeJsonData>>(Resources.Load<TextAsset>("Data/cash_upgrade").text);
        foreach(var item in list) {
            item.currencyType = CurrencyType.CompanyCash;
        }

        JsonSerializer serializer = new JsonSerializer {
            Formatting = Formatting.Indented
        };

        using(StreamWriter writer = new StreamWriter(@"C:\Users\House\Desktop\cashupg.json")) {
            using (JsonWriter jWriter = new JsonTextWriter(writer)) {
                serializer.Serialize(jWriter, list);
            }
        }

    }

    [MenuItem("Bos/Add to cash upgrades currency type SECURITIES")]
    private static void AddToCashUpgradesSecuritiesCurrencyType() {
        //string path = System.IO.Path.Combine(Application.dataPath, "Resources/Data/cash_upgrade.json");

        List<UpgradeJsonData> list = JsonConvert.DeserializeObject<List<UpgradeJsonData>>(Resources.Load<TextAsset>("Data/investor_upgrade").text);
        foreach (var item in list) {
            item.currencyType = CurrencyType.Securities;
        }

        JsonSerializer serializer = new JsonSerializer {
            Formatting = Formatting.Indented
        };

        using (StreamWriter writer = new StreamWriter(@"C:\Users\House\Desktop\secupg.json")) {
            using (JsonWriter jWriter = new JsonTextWriter(writer)) {
                serializer.Serialize(jWriter, list);
            }
        }

    }
}
