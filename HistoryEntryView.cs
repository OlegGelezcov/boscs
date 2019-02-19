namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using UnityEngine.EventSystems;

    public class HistoryEntryView : GameBehaviour, IListItemView<HistoryEntry> {

        public Image backImage;
        public TextMeshProUGUI nameText;
        public Image planetIcon;
        public Text countText;
        public Button trigger;

        private RectTransform backImageRectTransform = null;

        private RectTransform BackImageRectTransform
            => (backImageRectTransform != null) ? backImageRectTransform :
            (backImageRectTransform = backImage.GetComponent<RectTransform>());

        public HistoryEntry Data { get; private set; }

        public void Setup(HistoryEntry entry) {
            this.Data = entry;
            var planetNameData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(entry.PlanetId);
            backImage.overrideSprite = Services.ResourceService.GetSprite(planetNameData.history_back);
            nameText.text = Services.ResourceService.Localization.GetString(planetNameData.name);
            planetIcon.overrideSprite = Services.ResourceService.GetSprite(planetNameData.ui_icon);
            countText.text = BosUtils.GetCurrencyString(entry.Securities.ToCurrencyNumber());
            trigger.SetListener(() => {
                Vector3AnimationData data1 = new Vector3AnimationData {
                    AnimationMode = BosAnimationMode.Single,
                    Duration = 0.3f,
                    EaseType = EaseType.EaseInOutQuad,
                    StartValue = Vector3.one,
                    EndValue = new Vector3(1, 1.15f, 1),
                    Target = backImage.gameObject,
                    OnStart = (s, go) => BackImageRectTransform.localScale = s,
                    OnUpdate = (s, t, go) => BackImageRectTransform.localScale = s,
                    OnEnd = (s, go) => {
                        BackImageRectTransform.localScale = s;
                        Vector3AnimationData data2 = new Vector3AnimationData {
                            AnimationMode = BosAnimationMode.Single,
                            Duration = 0.3f,
                            EaseType = EaseType.EaseInOutQuad,
                            Target = backImage.gameObject,
                            StartValue = new Vector3(1, 1.15f, 1),
                            EndValue = Vector3.one,
                            OnStart = (s2, go2) => BackImageRectTransform.localScale = s2,
                            OnUpdate = (s2, t2, go2) => BackImageRectTransform.localScale = s2,
                            OnEnd = (s2, go2) => BackImageRectTransform.localScale = s2
                        };
                        backImage.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(data2);
                    }
                };
                backImage.gameObject.GetOrAdd<Vector3Animator>().StartAnimation(data1);
            });
        }
    }

}