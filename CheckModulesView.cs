namespace  Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class CheckModulesView : GameBehaviour {

		public override void OnEnable(){
			base.OnEnable();
			StartCoroutine(ScaleImpl());
		}

		private IEnumerator ScaleImpl() {
			GetComponent<RectTransform>().SetUniformScale(0.01f);
			yield return new WaitForSeconds(0.15f);
			Scale();
		}
		private void Scale() {
			var scaleData = AnimUtils.GetScaleAnimData(0.01f, 1f, 0.2f, EaseType.EaseInOutQuartic, GetComponent<RectTransform>());
			gameObject.GetOrAdd<Vector2Animator>().StartAnimation(scaleData);
		}
	}
}


