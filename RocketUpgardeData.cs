using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Bos.Data
{
    public interface IRocketUpgradeRepository : IRepository
    {
        RocketUpgrade GetUpgrade(int lvl);
    }


    public class RocketUpgradeRepository : IRocketUpgradeRepository
    {
        public List<RocketUpgrade> Data;
        
        public bool IsLoaded { get; private set; }

        public RocketUpgradeRepository() {
            Data = new List<RocketUpgrade>();
        }

        public void Load(string file ) {
            if (!IsLoaded) {
                Data = JsonConvert.DeserializeObject<List<RocketUpgrade>>(Resources.Load<TextAsset>(file).text);
                IsLoaded = true;
            }
        }

        public RocketUpgrade GetUpgrade(int lvl)
        {
            var upgrade = Data.FirstOrDefault(val => val.level == lvl);
            return upgrade;
        }
    }

    public class RocketUpgrade
    {
        public float cash;
        public int coin;
        public int cost;
        public int level;
    }
}