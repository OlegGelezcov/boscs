namespace Bos.UI
{
    using Bos.Data;
    using UniRx;
    using UnityEngine.UI;

    /// <summary>
    /// Used on module images in PlanetView
    /// </summary>
    public class PlanetShipModuleImage : GameBehaviour
    {
        public int moduleId;

        #region Old prototype when I changed material on Image
        /*
        public Image image;
        private bool IsInitialized { get; set; }
        private PlanetNameData localPlanetData;

        private PlanetNameData LocalPlanetData
            => localPlanetData ?? (localPlanetData = ResourceService.PlanetNameRepository.GetPlanetNameData(GetComponentInParent<PlanetView>().planetId));

        public override void OnEnable()
        {
            base.OnEnable();
            Setup();
        }

        public void Setup()
        {
            if (!IsInitialized)
            {
                var planetView = GetComponentInParent<PlanetView>();
                string matId = $"pl{moduleId}-{planetView.planetId}";

                image.material = ResourceService.Materials.GetMaterial(matId, image.material);
                GameEvents.ModuleStateChangedObservable.Subscribe(args => {
                    //when this module state changed we update material properties
                    if (args.Module.Id == moduleId)
                    {
                        Setup();
                    }
                }).AddTo(gameObject);
                IsInitialized = true;
            }
            UpdateView();

        }

        /// <summary>
        /// Set material to visible or semitransparent state
        /// Used material animation
        /// </summary>
        private void UpdateView()
        {
            var modules = Services.Modules;
            if (modules.IsOpened(moduleId))
            {
                image.AnimateModuleToVisibleState();
            }
            else
            {
                if (moduleId == LocalPlanetData.module_id)
                {
                    image.PingPongModuleVisibility(duration: 0.5f);
                }
                else
                {
                    image.SetModuleToInvisible();
                }
            }
        }
        */
        #endregion
    }

}