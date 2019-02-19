namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    //public class ResearchedGeneratorCollection : IResearchedGenerators {

    //    public Dictionary<int, bool> ResearchedGenerators { get; }
    //        = new Dictionary<int, bool> {
    //            [0] = true,
    //            [1] = true
    //        };

    //    public void LoadSave(object saveObj) {
    //        ResearchedGeneratorsSave save = saveObj as ResearchedGeneratorsSave;
    //        ResearchedGenerators.Clear();
    //        if (save != null && save.generators != null ) {
                
    //            foreach(var kvp in save.generators) {
    //                ResearchedGenerators.Add(kvp.Key, kvp.Value);
    //            }
    //        } 
    //        if(ResearchedGenerators.Count == 0 ) {
    //            LoadDefaults();
    //        }
    //    }

    //    public object GetSave() {
    //        return new ResearchedGeneratorsSave {
    //            generators = ResearchedGenerators
    //        };
    //    }

    //    public void LoadDefaults() {
    //        ResearchedGenerators.Clear();
    //        ResearchedGenerators[0] = true;
    //        ResearchedGenerators[1] = true;
    //    }

    //    public void Reset() {
    //        LoadDefaults();
    //    }

    //    public bool IsResearched(int generatorId ) {
    //        return ResearchedGenerators.ContainsKey(generatorId) && ResearchedGenerators[generatorId] == true;
    //    }

    //    public void Research(int generatorId ) {
    //        if(false == IsResearched(generatorId)) {
    //            ResearchedGenerators[generatorId] = true;
    //            GameEvents.OnGeneratorResearched(generatorId);
    //        }
    //    }
    //}

    //[Serializable]
    //public class ResearchedGeneratorsSave {

    //    public Dictionary<int, bool> generators;

    //}

    //public interface IResearchedGenerators : ISaveableBase {


    //    bool IsResearched(int generatorId);
    //    void Research(int generatorId);

    //}
}