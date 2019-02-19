using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
[RequireComponent(typeof(NStateToggleButton))]
public class MultiplierController : MonoBehaviour {
    public GameManager GameManager;
    public Generator Generator;

    private NStateToggleButton _tog;

    private void Start() {
        _tog = GetComponent<NStateToggleButton>();
        _tog.StateChanged += _tog_StateChanged;
        var state = PlayerPrefs.GetInt($"BuyMultiplier {Generator.Id}", 0);
    }

    private void _tog_StateChanged(object sender, EventArgs e) {
        int res = 0;
        int.TryParse(_tog.CurrentState, out res);
        //Generator.BuyMultiplier.Value = res;
        Generator.SetBuyMultiplier(res);
        PlayerPrefs.SetInt($"BuyMultiplier {Generator.Id}", res);
        PlayerPrefs.Save();
    }
}
*/
