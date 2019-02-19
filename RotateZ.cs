namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class RotateZ : MonoBehaviour {

		public float rotationSpeed = 100;

		private RectTransform rectTransform;

		public void Start(){
			rectTransform = GetComponent<RectTransform>();
		}

		public void Update() {
			rectTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
		}
	}
}

