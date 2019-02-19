namespace Bos {
    using Ozh.Tools.Functional;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SpriteContainer : GameBehaviour {
        public string containerName;
        public Sprite[] sprites;

        public Sprite GetSprite(string name) {
            return sprites.FirstOrDefault(s => s.name == name);
        }


    }

}