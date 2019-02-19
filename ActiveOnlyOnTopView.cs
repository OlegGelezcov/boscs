namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using Bos.UI;

    public class ActiveOnlyOnTopView : GameBehaviour {

        public GameObject[] targetObjects;
        private bool IsInitialized { get; set; }
        public override void Start() {
            base.Start();
            if(!IsInitialized) {
                GameEvents.ViewShowedObservable.Subscribe(_ => {
                    if (gameObject.activeSelf) {
                        StartCoroutine(UpdateObjectsDelayed());
                    } else {
                        UpdateObjects();
                    }
                }).AddTo(gameObject);
                GameEvents.ViewHidedObservable.Subscribe(_ => {
                    if (gameObject.activeSelf) {
                        StartCoroutine(UpdateObjectsDelayed());
                    } else {
                        UpdateObjects();
                    }
                }).AddTo(gameObject);
                IsInitialized = true;
            }
        }

        private IEnumerator UpdateObjectsDelayed() {
            yield return new WaitForSeconds(0.2f);
            UpdateObjects();
        }

        private void UpdateObjects() {
            var parentView = GetComponentInParent<BaseView>();
            if(parentView == null ) { return; }

            UnityEngine.Debug.Log($"Service Max Sibling Index: {ViewService.MaxModalSiblingIndex}, parent sibling Index: {parentView.transform.GetSiblingIndex()}".Attrib(bold: true, color: "y"));
            if(parentView.transform.GetSiblingIndex() == ViewService.MaxModalSiblingIndex) {
                targetObjects.Activate();
            } else {
                targetObjects.Deactivate();
            }
        }
    }

}