
namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class BosUISettings  {

		private BosUISettings(){}

		public float ViewCloseDelay {get;} = 0.1f;

		public float ViewShowDelay { get; } = 0.1f;


		private static BosUISettings instance = null;

		public static BosUISettings Instance
			=> (instance != null ) ? instance : (instance = new BosUISettings());
	}	
}

