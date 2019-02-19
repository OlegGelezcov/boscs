namespace Bos.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;


    public class PlanetModulePriceSection : GameBehaviour
    {
        
        public Image currencyIcon;
        public Text priceText;
        public Text moduleNameText;

        private bool IsInitialized { get; set; }
        private PlanetView planetView;

        public override void Start()
        {
            base.Start();
            CachePlanetView();

            if (!IsInitialized)
            {
                planetView.ButtonAnimatedSubject.Subscribe(isAnimated => {
                    if (isAnimated)
                    {
                        ClearModuleName();
                    } else
                    {
                        UpdateModuleName();
                    }
                }).AddTo(gameObject);

                GameEvents.ModuleStateChangedObservable.Subscribe(args => {
                    if(ModuleId == args.Module.Id )
                    {
                        UpdateView();
                    }

                }).AddTo(gameObject);
                IsInitialized = true;
            }

            UpdateView();
        }

        public void UpdateView()
        {
            CachePlanetView();
            var planetLocalData = ResourceService.PlanetNameRepository.GetPlanetNameData(planetView.planetId);
            if(planetLocalData.IsModuleRequired && !Services.Modules.IsOpened(planetLocalData.module_id))
            {
                var moduleData = Services.Modules.GetModule(planetLocalData.module_id).Data;
                currencyIcon.overrideSprite = ResourceService.GetCurrencySprite(moduleData.Currency);
                priceText.text = BosUtils.GetCurrencyString(moduleData.Currency, "#FFFFFF", "FFE565");
                UpdateModuleName();

            } else
            {
                DeactivateAll();
            }
        }



        private void UpdateModuleName()
        {
            var planetLocalData = ResourceService.PlanetNameRepository.GetPlanetNameData(planetView.planetId);
            if(planetLocalData.IsModuleRequired)
            {
                moduleNameText.text = LocalizationObj.GetString(ResourceService.ModuleNameRepository.GetModuleNameId(planetLocalData.module_id));
            } else
            {
                ClearModuleName();
            }
        }

        private void ClearModuleName() => moduleNameText.text = string.Empty;

        private void DeactivateAll()
            => new MonoBehaviour[] { currencyIcon, priceText, moduleNameText }.ToggleActivity(false);

        private void CachePlanetView()
        {
            if(planetView == null )
            {
                planetView = GetComponentInParent<PlanetView>();
            }
        }

        private int ModuleId
        {
            get
            {
                CachePlanetView();
                var planetData = ResourceService.PlanetNameRepository.GetPlanetNameData(planetView.planetId);
                return planetData.module_id;
            }
        }

    }

}