namespace Bos {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using DBG = UnityEngine.Debug;

	public static class DebugUtils {

		public static void MoveOnNextPlanet(){
			GameServices Services = GameServices.Instance;

			if(Services.PlanetService.HasNextPlanet) {
                Services.PlanetService.ForceSetOpened(Services.PlanetService.NextPlanetId);
                Services.SoundService.PlayOneShot(SoundName.click);
            } else {
                DBG.Log("Unable to go at next planet...");
                DBG.Log($"has next planet => {Services.PlanetService.HasNextPlanet}, next planet id => {Services.PlanetService.NextPlanetId}, current planet id => {Services.PlanetService.CurrentPlanet.Id}");
            }
		}
	}

}