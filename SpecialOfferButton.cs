namespace Bos.UI {
    using System;
    using UniRx;
    using UnityEngine.UI;

    public class SpecialOfferButton : GameBehaviour {

        public Button button;
        public Text expireText;
        public Image iconImage;

        private IDisposable disposable;

        #region TypedView overrides

        public override void OnEnable() {
            base.OnEnable();
            Setup();
        }

        public override void OnDisable() {
            Dispose();
            base.OnDisable();           
        }

        public void Setup() {
            var planetLocalData = ResourceService.PlanetNameRepository.GetPlanetNameData(Planets.CurrentPlanetId.Id);
            iconImage.overrideSprite = ResourceService.GetSprite(planetLocalData.ui_icon);
            button.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                ViewService.ShowDelayed(ViewType.SpecialOfferView, BosUISettings.Instance.ViewShowDelay);
            });
            UpdateExpire();

            disposable = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                UpdateExpire();
            });
        }

        private void Dispose() {
            if(disposable != null ) {
                disposable.Dispose();
                disposable = null;
            }
        }

        private void UpdateExpire() {
            ISpecialOfferService service = Services.GetService<ISpecialOfferService>();
            TimeSpan ts = TimeSpan.FromSeconds(service.ExpireInterval);
            expireText.text = $"{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";

            if(service.IsExpired  ) {
                button.SetInteractableWithShader(false);
            } else {
                button.SetInteractableWithShader(true);
            }
        }
        #endregion
    }

}