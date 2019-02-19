namespace Bos.Data {
    
    using System.Collections;
    using System.Collections.Generic;
    using Bos;
    using UnityEngine;

    public class MaterialCache : ObjectCache<string, Material> {

        public Material GetMaterial(string key, Material baseMaterial) {
            if (Contains(key)) {
                return GetObject(key);
            }
            else {
                Material material = new Material(baseMaterial);
                material.hideFlags = HideFlags.HideAndDontSave;
                Add(key, material);
                return material;
            }
        }
    }

}