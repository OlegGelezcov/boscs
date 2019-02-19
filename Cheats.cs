using Bos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cheats : GameBehaviour
{
    public static bool NO_PREF_SAVE = false;

    public AllManagers Managers;
    public LocalEventManager EvtManager;

    public InputField tbMoney;

    public void Add10Mil()
    {
        Player.AddGenerationCompanyCash(10000000);
    }

    public void AddCustom()
    {
        var text = tbMoney.text;
        if (string.IsNullOrEmpty(text))
            return;

        double val = 0;

        double.TryParse(text, out val);
        Player.AddGenerationCompanyCash(val);
    }

    public void Add5kCoins()
    {
        //Managers.IAPManager.AddCoins(5000, free: true);
    }

    public void ExitWithCleanPrefs()
    {
        NO_PREF_SAVE = true;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Application.Quit();
    }

    public void NextEvent100Perc()
    {
        EvtManager.TriggerEvent();
    }

    public void CloseCheats()
    {
        gameObject.SetActive(false);
    }
}
