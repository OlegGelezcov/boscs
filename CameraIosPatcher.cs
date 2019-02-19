using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraIosPatcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
		#if UNITY_IOS
		GetComponent<Camera>().fieldOfView = 85;
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
