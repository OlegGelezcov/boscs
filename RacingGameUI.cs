using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RacingGameUI : MonoBehaviour
{
    public RacingGameController Controller;
    public ToggleGroup ToggleGroup;

    public void Quit()
    {
        SceneManager.UnloadSceneAsync(1);
    }

    public void StartGame()
    {
        var toggle = ToggleGroup.ActiveToggles().FirstOrDefault();
        if (toggle == null)
            return;

        var selid = int.Parse(toggle.name);

        Controller.StartGame();
        gameObject.SetActive(false);
    }
}
