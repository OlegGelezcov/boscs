namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /*
    public class EnhancedGeneratorCollection : GameElement, IEnhancedGenerators {

        private readonly List<int> enhancedGenerators = new List<int>();

        public List<int> EnhancedGenerators
            => enhancedGenerators;

        public void Clear()
            => enhancedGenerators.Clear();

        public bool IsEnhanced(int generatorId)
            => enhancedGenerators.Contains(generatorId);

        public void Enhance(int generatorId) {
            if(false == IsEnhanced(generatorId)) {
                enhancedGenerators.Add(generatorId);
            }
        }

        public void AddRange(IEnumerable<int> generators) {
            if(generators != null) {
                enhancedGenerators.AddRange(generators);
            }
        }
    }

    public interface IEnhancedGenerators {
        void Clear();
        bool IsEnhanced(int generatorId);
        void Enhance(int generatorId);
        List<int> EnhancedGenerators { get; }
        void AddRange(IEnumerable<int> generators);
    }
    */
}