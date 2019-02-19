namespace Bos.SplitLiner {
	
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Bos;
	

public class SplitLinerGameManager : GameBehaviour {


	public PlayerMotor motor;
	public SpawnManager sm;
	public SplitLinerAudioManager am;

	private int tempScore = 1;
	private int highScore = 0;

	public AudioSource powerUp;

	public float lookTimer = 0f;
	public float timerDuration = 2f;
	public float startingAmount = 1f;
	public float currentAmount;

	private const string kSaveKey = "split_liner_score";

    public static event System.Action YellowBlockHitted;
    public static event System.Action GreenBlockHitted;
        public static event System.Action<bool> GameEnd;

    private static void OnGameEndEvent(bool success) {
            GameEnd?.Invoke(success);
        }

    public static void OnYellowBlockHitted() {
            YellowBlockHitted?.Invoke();
        }
	
	public static void OnGreenBlockHitted() {
		GreenBlockHitted?.Invoke();
	}
	
	public override void Start () 
	{
		motor =   GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerMotor> ();
		sm = GameObject.FindGameObjectWithTag ("SM").GetComponent<SpawnManager> ();
		am = FindObjectOfType<SplitLinerAudioManager>(); 

		highScore = PlayerPrefs.GetInt (kSaveKey);
		tempScore = highScore;

		if (highScore == 0)
		{
			tempScore = 1;
		}

		currentAmount = startingAmount;

	}
	
	public override void Update () 
	{

		if (tempScore == 0)
		{
			Time.timeScale = 0.5f;
			StartCoroutine (endSlomo (0.5f));
			highScore++;
			tempScore = highScore;
			powerUp.Play ();
		}


		if (tempScore < 0)
		{
			tempScore = 0;
		}

		am.tempScore = tempScore;

		if(motor.isLookedAt)
		{
			lookTimer += Time.deltaTime;
			currentAmount -= 0.5f * Time.deltaTime;
		}

		if (currentAmount <= 0)
		{
			OnGameEnd (false);
		}

	}

	public void DecrementTemp()
	{
		tempScore--;
	}

	public void EndTheGame(bool success)
	{
		StartCoroutine (ExecuteAfterTime (1f, success));
	}

	public void OnGameEnd(bool success)
	{
		if (PlayerPrefs.GetInt (kSaveKey) < highScore)
		{
			PlayerPrefs.SetInt (kSaveKey, highScore);
		}

		//SceneManager.LoadScene ("SplitLiner");
		am.allowChance = true;
            OnGameEndEvent(success);
	}

	IEnumerator ExecuteAfterTime (float time, bool success)
	{
		yield return new WaitForSeconds (time);
	
		OnGameEnd (success);
	}

	IEnumerator endSlomo(float time)
	{
		yield return new WaitForSeconds (time);

		Time.timeScale = 1f;
	}


}

}
