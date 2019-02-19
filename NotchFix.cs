using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotchFix : MonoBehaviour
{
	public bool UseSafeArea = false;
	
	private RectTransform _panel;
	private Rect _lastRect = new Rect (0, 0, 0, 0);

	private string _deviceModel;

	private void Start()
	{
		_deviceModel = SystemInfo.deviceModel;
		_panel = GetComponent<RectTransform>();
		UnityEngine.Debug.Log("device model: " + _deviceModel);

		if (UseSafeArea)
		{
			var safeArea = GetSafeArea();
			ApplySafeArea(safeArea);
		}
		else
		{
			Refresh();
		}
	}

	private void Refresh()
	{
		var offset = PlatformUtils.GetNotchOffset();
		if (offset != 0)
		{
			Apply(offset);
		}
	}

	private void TestApply()
	{
		var topOffsetMult = (float)100 / Screen.height;
		_panel.anchoredPosition = Vector3.zero;
		_panel.offsetMax = new Vector2(_panel.offsetMax.x, _panel.offsetMax.y - _panel.rect.height * topOffsetMult);
	}

	private void Apply(float pixelOffsets)
	{
		var topOffsetMult = pixelOffsets / Screen.height;
		_panel.anchoredPosition = Vector3.zero;
		_panel.offsetMax = new Vector2(_panel.offsetMax.x, _panel.offsetMax.y - _panel.rect.height * topOffsetMult);
	}
	
	private Rect GetSafeArea()
	{
		return Screen.safeArea;
	}

	private void ApplySafeArea (Rect r)
	{
		_lastRect = r;
		Vector2 anchorMin = r.position;
		Vector2 anchorMax = r.position + r.size;
		anchorMin.x /= Screen.width;
		anchorMin.y /= Screen.height;
		anchorMax.x /= Screen.width;
		anchorMax.y /= Screen.height;
		_panel.anchorMin = anchorMin;
		_panel.anchorMax = anchorMax;
	}
}
