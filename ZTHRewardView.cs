using UnityEngine.UI;

namespace Bos
{
	using System.Collections;
	using UnityEngine;
	using Bos.UI;
	
	public class ZTHRewardView : MonoBehaviour
	{
		public Text Coin;
		
		private void OnEnable()
		{
			StartCoroutine(DelayedDestroy());
		}

		IEnumerator DelayedDestroy()
		{
			GameServices.Instance.SoundService.PlayOneShot(SoundName.badgeUnlock);
			yield return new WaitForSeconds(1.2f);
			GameServices.Instance.SoundService.PlayOneShot(SoundName.buyCoinUpgrade);
			yield return new WaitForSeconds(0.8f);
			Destroy(gameObject);
		}
	}
}
