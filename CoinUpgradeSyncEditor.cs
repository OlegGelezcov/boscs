using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CoinUpgradeSync))]
public class CoinUpgradeSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var q = (CoinUpgradeSync)target;

        if (GUILayout.Button("Sync from Shop"))
        {
            var ut = q.UpgradesContainer.transform;
            var st = q.ShopContainer.transform;
            for (int i = 0; i < ut.childCount; i++)
            {
                var target = ut.GetChild(i).gameObject;
                var src = st.GetChild(i).gameObject;
                
                var targetSI = target.GetComponent<ShopItem>();
                var srcSI = src.GetComponent<ShopItem>();

                targetSI.ItemId = srcSI.ItemId;
                targetSI.TargetId = srcSI.TargetId;
                targetSI.Name = srcSI.Name;
                targetSI.Description = srcSI.Description;
                targetSI.Price = srcSI.Price;
                targetSI.Icon = srcSI.Icon;
                targetSI.OneTimePurchase = srcSI.OneTimePurchase;
                targetSI.UpgradeType = srcSI.UpgradeType;
                targetSI.ProfitMultiplier = srcSI.ProfitMultiplier;
                targetSI.TimeMultiplier = srcSI.TimeMultiplier;
                targetSI.DaysOfFutureBalance = srcSI.DaysOfFutureBalance;

                Undo.RecordObject(targetSI, "Auto Set Value");
                EditorUtility.SetDirty(targetSI);
            }
        }
        
    }
}
