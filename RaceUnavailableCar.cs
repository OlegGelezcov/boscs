
namespace  Bos.UI
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    using UnityEngine.EventSystems;

    public class RaceUnavailableCar : GameBehaviour {

			public override void OnEnable(){
				base.OnEnable();

				GetComponent<EventTrigger>().SetPointerListeners((dp)=>{}, (cp)=>{}, (up)=>{
					Animator animator = GetComponent<Animator>();
					animator?.SetTrigger("scale");
					Services.GetService<ISoundService>().PlayOneShot(SoundName.race);
				});
			}
	}	
}

