using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerScreen : MonoBehaviour
{

	public RectTransform managerScrollContent;
	public float width = 183f;

	public void MoveToManager(int index)
	{
		managerScrollContent.anchoredPosition  = new Vector2(managerScrollContent.anchoredPosition.x, 183f * Mathf.Clamp(index, 0, 5));
	}
}
