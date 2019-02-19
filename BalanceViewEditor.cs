using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(BalanceManager))]
//public class BalanceManagerEditor : Editor
//{
//    private int _selectedCurrency;
//    private string[] _currencies = new string[] { "$", "RON" };

//    public override void OnInspectorGUI()
//    {
//        var man = ((BalanceManager)serializedObject.targetObject);

//        for (int i = 0; i < _currencies.Length; i++)
//        {
//            if (_currencies[i] == man.Currency.CurrencySign)
//                _selectedCurrency = i;
//        }


//        GUILayout.Label("Balance Value");
//        var balance = GUILayout.TextField(man.Balance.ToString());

//        _selectedCurrency = EditorGUILayout.Popup("Currency Type", _selectedCurrency, _currencies);

//        EditorGUILayout.Separator();

//        if (GUILayout.Button("Set"))
//        {
//            man.Balance = new BigInt(balance);
//            man.Currency = Currencies.GetFromString(_currencies[_selectedCurrency]);
//        }

//    }
//}
