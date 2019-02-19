using Bos;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RacingGameController : MonoBehaviour
{
    public GameObject[] Cars;
    public float MaxSpeed = 5;
    public float WinChance = 0.75f;

    private float[] _speedFactors;
    private bool _run;
    private int _playerIndex;
    private List<int> _finishOrder;

    public Transform FinishLine;

    public GameObject CountdownScreen;
    public GameObject WinScreen;
    public GameObject LoseScreen;

    public RewardManagerBase RewardManager;


    public List<float> chances = new List<float> {0.75f, 0.65f, 0.55f};

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        var selectedIndex = 0;

        if (chances.Count > LocalData.LastCardChoosenTempCount)
        {
            WinChance = chances[LocalData.LastCardChoosenTempCount];
        }

        CountdownScreen.SetActive(true);
        _speedFactors = new float[4];
        for (int i = 0; i < 4; i++)
        {
            if (i == selectedIndex)
            {
                if (Random.Range(0f, 1f) <= WinChance)
                    _speedFactors[i] = 1;
                else
                    _speedFactors[i] = 0.65f;
            }
            else
            {
                _speedFactors[i] = Random.Range(0.7f, 0.9f);
            }
        }

        _playerIndex = selectedIndex;

        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlayOneShot("race_ready");
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlayOneShot("race");
        yield return new WaitForSeconds(1f);
        CountdownScreen.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        _run = true;
        _finishOrder = new List<int>();
    }


    private void Update()
    {
        if (_run)
        {
            for (int i = 0; i < 4; i++)
            {
                Cars[i].transform.position += new Vector3(0, MaxSpeed * _speedFactors[i] * Time.deltaTime, 0);

                if (Cars[i].transform.position.y >= FinishLine.transform.position.y)
                {
                    if (!_finishOrder.Contains(i))
                        _finishOrder.Add(i);
                }
            }

            if (_finishOrder.Count == 4)
            {
                _run = false;
                if (_finishOrder[0] == _playerIndex)
                {
                    ShowWin();
                }
                else
                {
                    ShowLose();
                }
                enabled = false;
            }
        }
    }

    private void ShowLose()
    {
        LoseScreen.SetActive(true);
        SoundManager.Instance.PlayOneShot("race_fail");
    }

    private void ShowWin()
    {
        RewardManager.CreateReward();
        WinScreen.SetActive(true);
        SoundManager.Instance.PlayOneShot("race_win");
    }
    
    public void Exit()
    {
        FindObjectOfType<GameUI>()?.HideRaceGame();
    }
}

