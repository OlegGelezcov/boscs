namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StoreItemsList : GameBehaviour {

        public AsyncLoader asyncLoader;
        public ScrollRect scrollRect;
        public FloatAnimator animator;
        public float totalInterval = 1.5f;

        private readonly Dictionary<StoreItemSection, float> sectionPoints = new Dictionary<StoreItemSection, float> {
            [StoreItemSection.CompanyCash] = 1,
            [StoreItemSection.Coins] = 0.742f,
            [StoreItemSection.Securities] = 0.245f,
            [StoreItemSection.PlayerCash] = 0
        };

        private float GetDistance(StoreItemSection section ) {
            return sectionPoints[section] - scrollRect.verticalNormalizedPosition;
        }

        private float GetAnimationInterval(float distance) {
            return Mathf.Abs(distance) / (1f / totalInterval);
        }

        public void Setup(StoreItemSection section) {
            Services.RunCoroutine(SetupImpl(section));
        }

        private IEnumerator SetupImpl(StoreItemSection section) {
            yield return new WaitUntil(() => asyncLoader.gameObject.activeSelf && asyncLoader.gameObject.activeInHierarchy);
            asyncLoader.ActivateItems(() => {
                print("activated completed");
                var scrollData = new FloatAnimationData {
                    StartValue = scrollRect.verticalNormalizedPosition,
                    EndValue = sectionPoints[section],
                    Duration = GetAnimationInterval(GetDistance(section)),
                    AnimationMode = BosAnimationMode.Single,
                    EaseType = EaseType.EaseInOutQuad,
                    Target = scrollRect.gameObject,
                    OnStart = (v, o) => scrollRect.verticalNormalizedPosition = v,
                    OnUpdate = (v, t, o) => scrollRect.verticalNormalizedPosition = v,
                    OnEnd = (v, o) => scrollRect.verticalNormalizedPosition = v
                };
                animator.StartAnimation(scrollData);
            });
        }

        public override void Update() {
            base.Update();
            //Debug.Log($"scroll rect vertical normalized position => {scrollRect.verticalNormalizedPosition}");
        }
    }

    public enum StoreItemSection {
        CompanyCash,
        Coins,
        Securities,
        PlayerCash
    }
}