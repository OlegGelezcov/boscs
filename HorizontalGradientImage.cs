namespace Bos.UI{

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.UI;

    public class HorizontalGradientImage : GameBehaviour {

		public Color[] colors;

		// Use this for initialization
		public override void Start () {
			Texture2D tex = new Texture2D(colors.Length, 1, TextureFormat.ARGB32, mipmap: false, linear: false);
			tex.filterMode = FilterMode.Bilinear;
			for(int i = 0; i < colors.Length; i++ ) {
				tex.SetPixel(i, 0, colors[i]);
			}
			tex.Apply();
			Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * .5f);
			GetComponent<Image>().sprite = sprite;
			GetComponent<Image>().overrideSprite = sprite;
		}
		

	}
}
