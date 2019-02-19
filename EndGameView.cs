using UnityEngine.UI;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class EndGameView : TypedView {

        public Text rewardCountText;
        public Button getRewardButton;
        
        public override CanvasType CanvasType => CanvasType.UI;
        public override bool IsModal => true;
        public override ViewType Type => ViewType.EndGameView;
        public override int ViewDepth => 150;

        public override void Setup(ViewData data) {
            base.Setup(data);

            rewardCountText.text = ((int) data.UserData).ToString();
            getRewardButton.SetListener(() => {
                Services.GameModeService.EndWinGame((int)data.UserData);
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.Remove(ViewType.EndGameView, BosUISettings.Instance.ViewCloseDelay);
            });
        }
    }


}