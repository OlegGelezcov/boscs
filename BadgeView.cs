namespace Bos.UI {
    using UnityEngine;
    using UnityEngine.UI;

    public class BadgeView : GameBehaviour {
        public int badgeId;
        //public Sprite badgeSprite;
        public GameObject frame;
        public Image iconImage;
        public Button button;


        private Badge badge = null;

        private void Setup() {
            badge = Services.BadgeService.GetNonUniqueData(badgeId);
            iconImage.overrideSprite = Services.ResourceService.GetSpriteByKey(badge.SpriteId);
            frame.SetActive(Services.BadgeService.IsBadgeEarned(badge));
            if (Services.BadgeService.IsBadgeEarned(badge)) {
                iconImage.color = new Color(1, 1, 1, 1);
            }
            button.SetListener(() => {
                Services.SoundService.PlayOneShot(SoundName.click);
                Services.ViewService.Show(ViewType.BadgeInfoView, new ViewData() {
                    UserData = badge
                });
            });
        }

        public override void OnEnable() {
            base.OnEnable();
            Setup();
            GameEvents.BadgeAdded += OnBadgeAdded;
        }

        public override void OnDisable() {
            GameEvents.BadgeAdded -= OnBadgeAdded;
            base.OnDisable();
        }

        private void OnBadgeAdded(Badge badge) {
            if(badge.Id == badgeId ) {
                Setup();
            }
        }
    }

}