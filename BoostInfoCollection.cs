using System;
using System.Linq;
using System.Text;

namespace Bos {
    using System.Collections.Generic;


    public class BoostCollection {
        private Dictionary<string, BoostInfo> Boosts { get; } = new Dictionary<string, BoostInfo>();
        public double Value { get; private set; } = 1.0;

        private  readonly List<string> expiredIds = new List<string>();
        
        public void Load(BoostCollectionSave save) {
            save.Guard();
            Boosts.Clear();
            foreach (var kvp in save.boosts) {
                BoostInfo boost = new BoostInfo(kvp.Value);
                Add(boost);
            }
        }
        
        public void Add(BoostInfo boost) {
            Boosts[boost.Id] = boost;
            Recompute();
        }

        public bool Remove(string id) {
            if (Boosts.ContainsKey(id)) {
                Boosts.Remove(id);
                Recompute();
                return true;
            }

            return false;
        }

        public int Remove(BoostSourceType sourceType) {
            List<BoostInfo> typedBoosts = FilterBoosts(sourceType);
            foreach (var boost in typedBoosts) {
                Boosts.Remove(boost.Id);
            }
            return typedBoosts.Count;
        }

        private List<BoostInfo> FilterBoosts(BoostSourceType sourceType)
            => Boosts.Values.Where(boost => boost.SourceType == sourceType).ToList();

        private void Recompute() {
            Value = Boosts.Values.ToList().Aggregate(1.0, (v, b) => v * b.Value);
            if (Value.IsZero()) {
                Value = 1.0;
            }
        }

        public void ClearTemp(params string[] keepBoosts) {
            
            List<string> tempIds = new List<string>();
            foreach (var kvp in Boosts) {
                if (kvp.Value.IsPersist == false) {
                    if (!keepBoosts.Contains(kvp.Key)) {
                        if(kvp.Key == "enhance") {
                            UnityEngine.Debug.Log($"Will be removed enhance boost");
                        }
                        tempIds.Add(kvp.Key);
                    }
                }
            }

            foreach (string id in tempIds) {
                Remove(id);
            }
        }

        public void ClearExpired(int currentTime) {
            expiredIds.Clear();
            foreach (var kvp in Boosts) {
                if (kvp.Value.EndTime > 0 && kvp.Value.EndTime < currentTime) {
                    expiredIds.Add(kvp.Key);
                }
            }

            foreach (var id in expiredIds) {
                UnityEngine.Debug.Log($"delete expired boost: {id}");
                Remove(id);
            }
        }

        public void ClearTimed() {
            List<string> timedIds = new List<string>();
            foreach (var kvp in Boosts) {
                if (kvp.Value.EndTime > 0) {
                    timedIds.Add(kvp.Key);
                }
            }

            foreach (string id in timedIds) {
                Remove(id);
            }
        }
        
        public void ClearAll() {
            Boosts.Clear();
        }
        
        

        public BoostCollectionSave GetSave() {
            Dictionary<string, BoostInfoSave> boostSaves = new Dictionary<string, BoostInfoSave>();
            foreach (var kvp in Boosts) {
                boostSaves.Add(kvp.Key, kvp.Value.GetSave());
            }

            return new BoostCollectionSave() {
                boosts = boostSaves
            };
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            Boosts.Values.ToList().ForEach(b => {
                stringBuilder.AppendLine("\t" + b.ToString());
            });
            return stringBuilder.ToString();
        }

        public double GetBoostValue(string boostId)
            => Boosts.ContainsKey(boostId) ? Boosts[boostId].Value : 1.0;

        public List<BoostInfo> GetBoosts(string boostId)
        {
            var boosts = Boosts.Where(val => val.Key.Contains(boostId)).Select(val => val.Value).ToList();
            return boosts;
        }
    }

    [Serializable]
    public class BoostCollectionSave {
        
        public Dictionary<string, BoostInfoSave> boosts;

        public void Guard() {
            if (boosts == null) {
                boosts = new Dictionary<string, BoostInfoSave>();
            }
        }
    }
}