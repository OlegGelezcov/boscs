namespace Bos.SplitLiner {
    
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhenCollision : MonoBehaviour {

	private PlayerMotor motor;
	private AudioSource aud;
	private SplitLinerGameManager gm;
	private SpawnManager sm;

	public GameObject red1;
	public GameObject red2;
	public GameObject red3;
	

	private GameObject go;
	private Vector3 tempPos;

	private bool isHandled = false;

	void Awake() {
		motor = FindObjectOfType<PlayerMotor>(); 
		aud = FindObjectOfType<SpawnManager>().GetComponent<AudioSource>(); 
		gm = FindObjectOfType<SplitLinerGameManager>(); 
	}


	/*
	void Update()
	{
		red1 = GameObject.FindGameObjectWithTag ("red1");
		red2 = GameObject.FindGameObjectWithTag ("red2");
		red3 = GameObject.FindGameObjectWithTag ("red3");
	}*/
	

	void OnTriggerEnter(Collider other) {

		if (isHandled) {
			return;
		}
		
		var splitLineCollider = GetComponent<SplitLinerCollider>();
		Debug.Log($"trigger enter with collider type => {splitLineCollider.colliderType}");
		
		switch (splitLineCollider.colliderType) {
			case SplitLinerColliderType.yellow: {
				gm.DecrementTemp();
				gameObject.SetActive(false);
				aud.Play ();
                        SplitLinerGameManager.OnYellowBlockHitted();
			}
				break;
			case SplitLinerColliderType.green: {
				gm.DecrementTemp();
				gameObject.SetActive(false);
				aud.Play ();
                        SplitLinerGameManager.OnGreenBlockHitted();
			}
				break;
			case SplitLinerColliderType.red1:
			case SplitLinerColliderType.red2:
			case SplitLinerColliderType.red3: {
				motor.OnDeath();
			}
				break;
			case SplitLinerColliderType.blue1: {
				motor.needBounce = true;
				tempPos = transform.position;
				StartCoroutine(SwapOnOtherCollider(0.2f, red1));
			}
				break;
			case SplitLinerColliderType.blue2: {
				motor.needBounce = true;
				tempPos = transform.position;
				StartCoroutine(SwapOnOtherCollider(0.2f, red2));
			}
				break;
			case SplitLinerColliderType.blue3: {
				motor.needBounce = true;
				tempPos = transform.position;
				StartCoroutine(SwapOnOtherCollider(0.2f, red3));
			}
				break;
		}

		isHandled = true;

	}

	private IEnumerator SwapOnOtherCollider(float delay, GameObject otherColliderObject) {
		yield return new WaitForSeconds(delay);
		go = Instantiate<GameObject>(otherColliderObject);
		go.transform.position = tempPos;
		gameObject.SetActive(false);
		go.transform.SetParent(SpawnManager.transform, true);
		SpawnManager.ReplaceTile(gameObject, go);
	}

	private SpawnManager spawnManager = null;

	private SpawnManager SpawnManager
		=> (spawnManager != null) ? spawnManager : (spawnManager = FindObjectOfType<SpawnManager>());
	

}

}