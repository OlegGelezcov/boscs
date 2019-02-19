namespace Bos.UI {
    using UnityEngine.UI;

    public class DependGeneratorView : TypedView {

        public Image iconImage;
        public Text titleText;
        public Text descriptionText;
        public Button closeButton;


        public override ViewType Type => ViewType.DependGeneratorView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override void Setup(ViewData data) {
            base.Setup(data);
            GeneratorInfo generator = data.UserData as GeneratorInfo;
            if(generator == null || !generator.IsDependent) { return; }
            object currentNameStr = LocalizationObj.GetString(generator.LocalData.GetName(Planets.CurrentPlanetId.Id).name);
            var requiredGenerator = Services.GenerationService.GetGetenerator(generator.LocalData.required_id);
            object requiredNameStr = LocalizationObj.GetString(requiredGenerator.LocalData.GetName(Planets.CurrentPlanetId.Id).name);
            titleText.text = LocalizationObj.GetString("DEPEND.TITLE").Format(requiredNameStr);
            descriptionText.text = LocalizationObj.GetString("DEPEND.DESC").Format(currentNameStr, requiredNameStr);
            iconImage.overrideSprite = ResourceService.GetSpriteByKey(requiredGenerator.LocalData.GetIconData(Planets.CurrentPlanetId.Id).icon_id);

            closeButton.SetListener(() => {
                Sounds.PlayOneShot(SoundName.click);
                ViewService.Remove(ViewType.DependGeneratorView);
            });
        }
    }

}