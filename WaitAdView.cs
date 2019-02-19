namespace Bos.UI {
    using AppodealAds.Unity.Api;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class WaitAdView : TypedView {

        public Button closeButton;
        private readonly UpdateTimer updateTimer = new UpdateTimer();
        private WaitAdData Data { get;  set; }
        private bool isAdLoaded = false;

        #region TypedView overrides
        public override ViewType Type => ViewType.WaitAdView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 101;

        public override void Setup(ViewData data) {
            base.Setup(data);
            Data = data.UserData as WaitAdData;

            updateTimer.Setup(.3f, dt => {
                if (!isAdLoaded) {
                    if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO)) {
                        isAdLoaded = true;
                        ViewService.Remove(ViewType.WaitAdView);
                        if (Data != null) {
                            Services.AdService.WatchAd(Data.ContentType, Data.Action);
                        }             
                    }
                }
            });
            closeButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                ViewService.Remove(ViewType.WaitAdView);
            });
        }
        #endregion

        public override void Update() {
            base.Update();
            updateTimer.Update();
        }
    }

    public class WaitAdData {
        public string ContentType { get; set; }
        public Action Action { get; set; }
    }

}