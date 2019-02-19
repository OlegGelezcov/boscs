using System.Linq;

namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TutorialWaveController : GameBehaviour {

        public TutorialFingerWave[] waves;

        private TutorialFingerWave GetAvailableWave() {
            return waves.FirstOrDefault(wave => wave.IsAvailable);
        }

        public void StartWave() {
            StartCoroutine(StartWaveImpl());
        }

        private IEnumerator StartWaveImpl() {
            yield return new WaitUntil(() => GetAvailableWave() != null);
            GetAvailableWave()?.StartWave();
        }
    }


}