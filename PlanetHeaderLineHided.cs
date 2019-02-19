
namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;

	public class PlanetHeaderLineHided : GameBehaviour {

		public EventTrigger trigger;

		public override void OnEnable(){
			base.OnEnable();
			trigger.SetPointerListeners((dp) => {
				ScaleIn();
			}, (cp) => {
				Services.GetService<IViewService>().ShowDelayed(ViewType.PlanetsView, 0.15f);
			}, (up) => {
				ScaleOut();
			});
		}

		private void ScaleIn(){
			Vector2AnimationData data = new Vector2AnimationData{
				StartValue = Vector2.one,
				EndValue = 0.95f * Vector2.one, 
				Target = gameObject,
				EaseType = EaseType.EaseInOutCubic,
				Duration = 0.15f,
				OnStart = (s, o) => { o.GetComponent<RectTransform>().localScale = new Vector3(s.x, s.y, 1);},
				OnUpdate = (s, t, o) => { o.GetComponent<RectTransform>().localScale = new Vector3(s.x, s.y, 1); },
				OnEnd = (s, o) => { o.GetComponent<RectTransform>().localScale = new Vector3(s.x, s.y, 1 );}
			};
			GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData>{ data });
		}

		private void ScaleOut(){
			Vector2AnimationData data = new Vector2AnimationData{
				StartValue = 0.95f * Vector2.one,
				EndValue = Vector2.one, 
				Target = gameObject,
				EaseType = EaseType.EaseInOutCubic,
				Duration = 0.15f,
				OnStart = (s, o) => { o.GetComponent<RectTransform>().localScale = new Vector3(s.x, s.y, 1);},
				OnUpdate = (s, t, o) => { o.GetComponent<RectTransform>().localScale = new Vector3(s.x, s.y, 1); },
				OnEnd = (s, o) => { o.GetComponent<RectTransform>().localScale = new Vector3(s.x, s.y, 1 );}
			};
			GetComponent<Vector2Animator>().StartAnimation(new List<Vector2AnimationData>{ data });
		}
	}	
}

