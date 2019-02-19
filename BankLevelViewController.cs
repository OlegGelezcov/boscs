namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BankLevelViewController : GameBehaviour {

        public float delay = .2f;
        public BankLevelView[] views;

        public override void Start() {
            base.Start();
            StartCoroutine(AnimateImpl());
        }

        private IEnumerator AnimateImpl() {

            while(true) {
                foreach (var view in views) {
                    var currentView = view;
                    if (currentView.IsUnlocked) {
                        var animator = currentView.GetComponent<Vector3Animator>();
                        if (currentView.IsUnlocked) {
                            var rectTransform = view.GetComponent<RectTransform>();
                            var secondScaleData = rectTransform.ConstructScaleAnimationData(Vector3.one * 1.2f, Vector3.one, 0.15f, BosAnimationMode.Single, EaseType.EaseInOutQuad);
                            var firstScaleData = rectTransform.ConstructScaleAnimationData(Vector3.one, Vector3.one * 1.2f, 0.15f, BosAnimationMode.Single, EaseType.EaseInOutQuad, () => {
                                animator.StartAnimation(secondScaleData);
                            });
                            animator.StartAnimation(firstScaleData);
                        }
                        yield return new WaitForSeconds(delay);
                    } else {
                        break;
                    }
                }

                yield return new WaitForSeconds(8);
            }
        }
    }

}