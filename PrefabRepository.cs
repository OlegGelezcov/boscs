namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Ozh.Tools.Functional;

    public class PrefabRepository<K> : IPrefabRepository<K> {

        private readonly Dictionary<K, string> prefabPathDictionary = new Dictionary<K, string>();
        private readonly ObjectCache<K, GameObject> prefabCache = new ObjectCache<K, GameObject>();


        public PrefabRepository() {
            prefabPathDictionary.Clear();
        }

        public Option<GameObject> GetPrefabF(K key) {
            GameObject prfb = GetPrefab(key);
            return (prfb != null) ? F.Some(prfb) : F.None;
        }

        public GameObject GetPrefab(K key) {
            if (prefabCache.Contains(key)) {
                return prefabCache.GetObject(key);
            } else if (prefabPathDictionary.ContainsKey(key)) {
                GameObject prefab = Resources.Load<GameObject>(prefabPathDictionary[key]);
                prefabCache.Add(key, prefab);
                return prefab;
            }
            UnityEngine.Debug.LogError($"don't exists prefab path for key => {key}");
            return null;
        }

        public bool HasPrefab(K key) => prefabCache.Contains(key);

        public void AddPath(K key, string path) => prefabPathDictionary[key] = path;

        public void AddPrefab(K key, GameObject prefab) {
            if (!HasPrefab(key)) {
                prefabCache.Add(key, prefab);
            }
        }

    }

    public interface IPrefabRepository<K> {
        GameObject GetPrefab(K key);
        Option<GameObject> GetPrefabF(K key);
        bool HasPrefab(K key);
        void AddPath(K key, string path);
    }

    public class ObjectCache<K, U> {
        private readonly Dictionary<K, U> cache = new Dictionary<K, U>();

        public void Add(K key, U obj) {
            cache[key] = obj;
        }

        public bool Contains(K key)
            => cache.ContainsKey(key);

        public virtual U GetObject(K key) {
            if (Contains(key)) {
                return cache[key];
            }
            return default(U);
        }

        public void Clear() {
            cache.Clear();
        }

        public IList<U> Items => new List<U>(cache.Values);
    }

    public class SpriteCache : ObjectCache<string, Sprite> {

        private readonly Dictionary<string, string> pathDict = new Dictionary<string, string>();
        private Sprite transparentSprite;

        public void AddPath(string key, string path) {
            pathDict[key] = path;
        }

        public override Sprite GetObject(string key) {
            Sprite result = base.GetObject(key);
            if(result == null ) {
                if(pathDict.ContainsKey(key)) {
                    result = Resources.Load<Sprite>(pathDict[key]);
                    if(result != null ) {
                        Add(key, result);
                    }
                }
            }
            return result;
        }

        public Sprite Transparent {
            get {
                return (transparentSprite != null) ? transparentSprite :
                    (transparentSprite = GetObject("transparent"));
            }
        }
    }

    public class AudioCache : ObjectCache<SoundName, AudioClip> {
        private readonly Dictionary<SoundName, string> pathDict = new Dictionary<SoundName, string>();

        public void AddPath(SoundName name, string path) {
            pathDict[name] = path;
        }

        public override AudioClip GetObject(SoundName key) {
            AudioClip result =  base.GetObject(key);
            if(result == null ) {
                if(pathDict.ContainsKey(key)) {
                    result = Resources.Load<AudioClip>(pathDict[key]);
                    if(result != null ) {
                        Add(key, result);
                    }
                }
            }
            return result;
        }
    }
}