using System;
using UniRx;

namespace Bos.UI {
	using UnityEngine;
	
	public class ModuleAlert : GameBehaviour {

		public GameObject[] alerts;
		
		public override void Start() {
			base.Start();
			IShipModuleService modules = Services.Modules;
			Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
				if (modules.IsAllowByAnyModule()) {
					alerts.Activate();
				}
				else {
					alerts.Deactivate();
				}
			}).AddTo(gameObject);
		}
		
		
	}

}