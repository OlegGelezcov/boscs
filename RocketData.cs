namespace Bos.Data {
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IRocketData : IRepository {
        float GetProbForIndex(int index);
    }

    public class RocketData  : IRocketData {

        private List<float> CoinProbs { get; set; }

        public bool IsLoaded { get; private set; }

        public RocketData() {
            CoinProbs = new List<float>();
        }

        public void Load(string file ) {
            if (!IsLoaded) {
                CoinProbs = JsonConvert.DeserializeObject<List<float>>(Resources.Load<TextAsset>(file).text);
                IsLoaded = true;
            }
        }

        public float GetProbForIndex(int index) {
            return RocketDataUtils.GetProbForIndex(CoinProbs, index);
        }
    }

    public class MoqRocketData : IRocketData {

        private List<float> CoinProbs { get; set; }

        public bool IsLoaded { get; private set; }

        public MoqRocketData() {
            CoinProbs = new List<float> {
                0.05f, 0.02f, 0.005f, 0.001f, 0.0001f
            };
            IsLoaded = true;
        }

        public float GetProbForIndex(int index) {
            return RocketDataUtils.GetProbForIndex(CoinProbs, index);
        }

        public void Load(string file) {

        }
    }

    public static class RocketDataUtils {

        public static float GetProbForIndex(List<float> targetList, int index) {
            if(index < targetList.Count ) {
                return targetList[index];
            }
            return targetList[targetList.Count - 1];
        }
    }

}