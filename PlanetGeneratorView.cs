namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlanetGeneratorView : GameBehaviour {

        public int generatorId;
        public UnlockedGeneratorView unlockedView;
        public LockedGeneratorView lockedView;
        public ResearchGeneratorView researchView;

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GeneratorStateChanged += OnGeneratorStateChanged;
            
            UpdateState(Services.GenerationService.Generators.GetGeneratorInfo(generatorId));
        }

        public override void OnDisable() {
            GameEvents.GeneratorStateChanged -= OnGeneratorStateChanged;
            base.OnDisable();
        }

        private void OnGeneratorStateChanged(GeneratorState oldState, GeneratorState newState, GeneratorInfo info) {
            UpdateState(info);
        }

        private void UpdateState(GeneratorInfo info ) {
            if (info.GeneratorId == generatorId) {
                switch (info.State) {
                    case GeneratorState.Active: {
                            lockedView.Deactivate();
                            researchView.Deactivate();
                            unlockedView.Activate();
                            unlockedView.Setup(generatorId);
                        }
                        break;
                    case GeneratorState.Unlockable:
                    case GeneratorState.Locked: {
                            researchView.Deactivate();
                            unlockedView.Deactivate();
                            lockedView.Activate();
                            lockedView.Setup(generatorId);
                        }
                        break;
                    case GeneratorState.Researchable: {
                            unlockedView.Deactivate();
                            lockedView.Deactivate();
                            researchView.Activate();
                            researchView.Setup(generatorId);
                        }
                        break;
                    case GeneratorState.Unknown: {
                            //Debug.Log($"unknown state at generator => {generatorId}, deactivate all");
                            unlockedView.Deactivate();
                            lockedView.Deactivate();
                            researchView.Deactivate();
                        }
                        break;
                }
            }
        }
    }

}