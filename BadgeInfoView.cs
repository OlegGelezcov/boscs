namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;


    public class BadgeInfoView : TypedViewWithCloseButton
    {
		public Image badgeIconImage;
		public Text badgeNameText;
		public Text objectiveText;
		public Text descriptionText;

		public GameObject rewardLabel;
		public Text rewardText;

        public override ViewType Type => ViewType.BadgeInfoView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

		public override int ViewDepth => 25;

		public override void Setup(ViewData obj) {
			Badge badge = obj.UserData as Badge;
			if(badge == null ) {
				throw new System.ArgumentException(nameof(Badge));
			}

            badgeIconImage.overrideSprite =
                Services.ResourceService.GetSpriteByKey(badge.SpriteId);
			
			badgeNameText.text = badge.Name;
			objectiveText.text = badge.ObjectiveText;
			descriptionText.text = badge.Description;
			rewardText.text = badge.RewardText;

			if(string.IsNullOrWhiteSpace(badge.RewardText)) {
				rewardLabel.Deactivate();
			} else {
				rewardLabel.Activate();
			}

			closeButton.SetListener(() => {
				Services.SoundService.PlayOneShot(SoundName.click);
				Services.ViewService.Remove(ViewType.BadgeInfoView, BosUISettings.Instance.ViewCloseDelay);
				closeButton.interactable = false;
			});
		}
    }
}
