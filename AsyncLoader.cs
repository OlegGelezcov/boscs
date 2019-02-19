using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncLoader : MonoBehaviour
{

	public List<GameObject> items;
    public float delay = 0.05f;
    public bool isAutoActivate = true;

	private void OnEnable()
	{
        if (isAutoActivate) {
            ActivateItems(null);
        }
	}

    public void ActivateItems(Action callback)
        => StartCoroutine(ActivateCoro(callback));

	public IEnumerator ActivateCoro(Action callback)
	{
		yield return new WaitForEndOfFrame();
		foreach (var item in items)
		{
            if (item != null && item.gameObject != null) {
                item.Activate();
            }

            yield return new WaitForSeconds(delay);
		}

        callback?.Invoke();
	}

	private void OnDisable()
	{
		foreach (var item in items)
       {
            if (item != null && item.gameObject != null) {
                item.Deactivate();
            }
        			
       }
	}
}
