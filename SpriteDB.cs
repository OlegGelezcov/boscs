using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteDB : MonoBehaviour
{
    public Sprite[] Sprites;

    public static Dictionary<string, Sprite> SpriteRefs = null;

    private void Awake()
    {
        if (SpriteRefs != null)
            return;

        UpdateState();

    }

    private  void UpdateState() {
        if(SpriteRefs == null ) {
            SpriteRefs = new Dictionary<string, Sprite>();
            foreach (var x in Sprites)
            {
                if (x == null)
                    continue;
                SpriteRefs.Add(x.name, x);
            }
        }
    }

    public static Sprite GetSprite(string spriteName) {
        if(SpriteRefs.ContainsKey(spriteName)) {
            return SpriteRefs[spriteName];
        }
        return null;
    }

}
