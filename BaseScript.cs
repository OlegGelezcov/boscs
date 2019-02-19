namespace Bos.SplitLiner {
	
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class BaseScript : MonoBehaviour {
		private PlayerMotor motor;

		void Start()
		{
			motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();
		}

		void OnTriggerEnter(Collider other) 
		{
			if (other.tag == "Player")
			{
				motor.OnDeath();
			}
			if(other.tag == "red1")
			{
				Destroy (other.gameObject);
			}
		}
	}
	
}


