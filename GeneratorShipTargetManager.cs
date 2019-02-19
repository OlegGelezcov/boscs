namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class GeneratorShipTargetManager : GameBehaviour {

        private static readonly System.Random random = new System.Random();

        public List<GeneratorShipTarget> targets = new List<GeneratorShipTarget>();
        public int maxActiveTargets;
        public float cooldown = 10;

        private readonly UpdateTimer activateTimer = new UpdateTimer();


        public override void OnEnable() {
            base.OnEnable();
            targets.ForEach(t => t.DeactivateTarget());
            activateTimer.Setup(1, dt => ActivateTargets());
        }

        public override void Update() {
            base.Update();
            activateTimer.Update();
        }

        private int ActiveCount
            => targets.Count(t => t.IsActive);

        public GeneratorShipTarget GetTargetForFollower(RectTransform follower) {
            float maxDistance = float.MaxValue;
            GeneratorShipTarget target = null;
            foreach(var t in targets ) {
                if (t.IsActive) {
                    var trs = t.GetComponent<RectTransform>();
                    float distance = Vector2.Distance(trs.anchoredPosition, follower.anchoredPosition);
                    if (distance < maxDistance) {
                        maxDistance = distance;
                        target = t;
                    }
                }
            }
            return target;
        }

        public void OnTargetReached(GeneratorShipTarget target) {
            target.DeactivateTarget();
        }

        private IEnumerable<GeneratorShipTarget> AllowedTargets {
            get {

                foreach(var target in targets ) {
                    if(!target.IsActive && target.IsInactiveLongerThen(cooldown)) {
                        yield return target;
                    }
                }
            }
        }

        private void ActivateTargets() {
            int needActivate = maxActiveTargets - ActiveCount;
            if(needActivate > 0 ) {
                List<GeneratorShipTarget> allowed = AllowedTargets.ToList();
                var shuffled = allowed.OrderBy(t => random.Next()).ToList();

                for(int i = 0; i < Mathf.Min(shuffled.Count, needActivate); i++ ) {
                    shuffled[i].ActivateTarget();
                }
            }
        }

    }

}