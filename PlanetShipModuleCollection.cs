using System.Linq;

namespace Bos.UI
{
    public class PlanetShipModuleCollection : GameBehaviour
    {
        public int planetId;
        public PlanetShipModuleImage[] modules;

        public override void Start()
        {
            base.Start();
            
            //UpdateModules();
        }

        public override void OnEnable() {
            base.OnEnable();
            Setup(planetId);
        }

        /// <summary>
        /// Activate only modules required for this planet
        /// </summary>
        /// <param name="planetId">Target planet id</param>
        public void Setup(int planetId ) {
            var planetLocalData = Planets.GetPlanet(planetId).LocalData;
            if(planetLocalData.IsModuleRequired) {
                modules.ToList().ForEach(m => {
                    if (m.moduleId <= planetLocalData.module_id) {
                        m.Activate();
                    } else {
                        m.Deactivate();
                    }
                });
            } else {
                modules.DeactivateEnumerable();
            }
        }

        #region old implementation based on change material properties of parts
        private void UpdateModules()
        {
            var planetLocalData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(planetId);

            if(!planetLocalData.IsModuleRequired )
            {
                ToggleModules(false);
            } else
            {
                if(Services.Modules.IsOpened(planetLocalData.module_id))
                {
                    var planetData = Planets.GetPlanet(planetId);
                    if(planetData.State == PlanetState.Opening || planetData.State == PlanetState.Opened ) {
                        ToggleModules(false);
                    } else {
                        ToggleModules(true);
                    }
                } else
                {
                    var planetData = Planets.GetPlanet(planetId);
                    if (planetData.State == PlanetState.Opening || planetData.State == PlanetState.Opened) {
                        ToggleModules(false);
                    } else {
                        ToggleModules(true);
                    }
                }
            }
        }

        private void ToggleModules(bool isActive )
        {
            modules.ToggleActivity(isActive);
        }
        #endregion
    }

}