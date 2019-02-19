using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveMonoBehaivor : MonoBehaviour {

	public ConnectionCollector Collerctor = new ConnectionCollector();

	private void OnDestroy()
	{
		Collerctor.DisconnectAll();
	}
}
