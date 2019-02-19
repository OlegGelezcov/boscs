namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class HistoryView : TypedViewWithCloseButton {
        public override ViewType Type => ViewType.HistoryView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override int ViewDepth => 15;

        public override bool IsModal => true;

        public GameObject viewPrefab;
        public Transform layout;
        public GameObject fallbackView;


        private BosItemList<HistoryEntry, HistoryEntryView> itemList = new BosItemList<HistoryEntry, HistoryEntryView>();

        

        public override void Setup(ViewData data) {
            base.Setup(data);
            var historyList = data.UserData as List<HistoryEntry>;

            if(historyList == null || historyList.Count == 0 ) {
                fallbackView.Activate();
            } else {
                fallbackView.Deactivate();
            }

            itemList.Setup(viewPrefab, layout, (itemdata, view) => {
                view.Setup(itemdata);
            }, (data1, data2) => {
                return data1.PlanetId == data2.PlanetId && data1.Interval == data2.Interval;
            });
            itemList.Fill(data.UserData as List<HistoryEntry>);
            closeButton.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.Remove(ViewType.HistoryView, BosUISettings.Instance.ViewCloseDelay);
                closeButton.interactable = false;
            });
        }
    }



}


