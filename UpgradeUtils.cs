using Bos.Data;

namespace Bos.UI {
 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 
  public static class UpgradeUtils  {

      public static bool IsNotTeleportWhenEarthOrMoon(IGeneratorUpgradeData data) {
          if (data.GeneratorId == Bos.GenerationService.TELEPORT_ID) {
              if (false == GameServices.Instance.PlanetService.IsMarsOpened) {
                  return false;
              }
          }

          return true;
      }
  }
}