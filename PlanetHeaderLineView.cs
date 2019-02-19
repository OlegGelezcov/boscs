namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
    using System.Linq;
    using Bos.Debug;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class PlanetHeaderLineView : GameBehaviour, IPlanetHeaderLine {

		public PlanetImagePositionSize[] planetPositionSizes;
		
		public EventTrigger trigger;

		//public PlanetImagePositionSize invalidPositionSize;

		public PlanetHeaderMenuIconView[] planetViews;

		public PlanetImagePositionSize GetInvalidPositionSize(int index ) {
			return new PlanetImagePositionSize {
				position = new Vector2(10000, 10000),
				size = new Vector2(100, 100),
				index = index
			};
		}

		public string GetInfoString() {
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			foreach(PlanetHeaderMenuIconView view in planetViews) {
				builder.AppendLine($"planet => {view.planetId} index => {view.CurrentIndex}");
			}
			return builder.ToString();
		}

		public override void OnEnable() {
			GameEvents.PlanetStateChanged += OnPlanetStateChanged;
			Setup();
		}

		public override void OnDisable(){
			GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
		}

		private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planet) {
			Setup();
		}

		private void Setup(){
			foreach(var planetView in planetViews) {
				
				int index = GetIndexForPlanet(planetView.PlanetId);
				var positionSize = GetPlanetImagePositionSize(planetView.PlanetId, index);

				planetView.Setup(this);
				planetView.SetIndex(index);

				if(IsActiveIndex(index)) {
					if(positionSize != null ) {
						planetView.Activate();
						planetView.SetPositionSize(positionSize);
					} else {
						Debug.LogError($"position size for planet => {planetView.PlanetId} is NULL for index => {index}");
						planetView.SetPositionSize(GetInvalidPositionSize(index));
						planetView.Deactivate();
					}
				} else {
					if(positionSize != null ) {
						planetView.SetPositionSize(positionSize);
						planetView.Deactivate();
					} else {
						planetView.SetPositionSize(GetInvalidPositionSize(index));
						planetView.Deactivate();
					}
				}
			}

			trigger.SetPointerListeners((dp) => {
				ScaleIn();
			}, (cp) => {
				Services.GetService<IViewService>().ShowDelayed(ViewType.PlanetsView, 0.15f);
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
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

		public override void Update(){
			if(Input.GetKeyUp(KeyCode.RightArrow)) {
				MoveRight();
			}
			if(Input.GetKeyUp(KeyCode.LeftArrow)) {
				MoveLeft();
			}
			if(Input.GetKeyUp(KeyCode.Space)) {
				print(GetInfoString().Colored(ConsoleTextColor.orange).Bold());
			}
		}

		public void MoveRight() {
			if(IsMoveRightAllowed) {
				for(int i = 0; i < planetViews.Length; i++ ) {
					int rightIndex = planetViews[i].CurrentIndex - 1;
					var positionSize = GetPlanetImagePositionSize(planetViews[i].PlanetId, rightIndex);
					if(positionSize != null ) {
						if(IsActiveIndex(planetViews[i].CurrentIndex)) {
							planetViews[i].Activate();
							planetViews[i].MoveTo(positionSize, 0.2f);
						} else if(IsActiveIndex(rightIndex)){
							planetViews[i].SetPositionSize(positionSize);
							planetViews[i].Activate();
						} else {
							planetViews[i].SetPositionSize(positionSize);
							planetViews[i].Deactivate();
						}
					} else {
						planetViews[i].Deactivate();
						planetViews[i].SetPositionSize(GetInvalidPositionSize(rightIndex));
					}
				}
			}
		}
		
		public void MoveLeft() {
			if(planetViews[0].CurrentIndex < MaxActiveIndex ) {
				for(int i = 0; i < planetViews.Length; i++ ) {
					int leftIndex = planetViews[i].CurrentIndex + 1;
					var positionSize = GetPlanetImagePositionSize(planetViews[i].PlanetId, leftIndex);
					if(positionSize != null ) {
						if(IsActiveIndex(planetViews[i].CurrentIndex) && IsActiveIndex(leftIndex)) {
							planetViews[i].Activate();
							planetViews[i].MoveTo(positionSize, 0.2f);
						} else  if(IsActiveIndex(leftIndex) && !IsActiveIndex(planetViews[i].CurrentIndex)){
							planetViews[i].SetPositionSize(positionSize);
							planetViews[i].Activate();
						} else {
							planetViews[i].SetPositionSize(positionSize);
							planetViews[i].Deactivate();
						}
					} else {
						planetViews[i].Deactivate();
						planetViews[i].SetPositionSize(GetInvalidPositionSize(leftIndex));
					}
				}
			}
		}
		public int GetIndexForPlanet(int planetId) {
			int currentPlanetId = Services.GetService<IPlanetService>().CurrentPlanet.Id;
			int index = 1;

			if(planetId == currentPlanetId) {
				return index;
			} else if(planetId > currentPlanetId) {
				index += (planetId - currentPlanetId);
				return index;
			} else if(planetId < currentPlanetId) {
				index -= (currentPlanetId - planetId);
			}
			return index;
		}

		public bool IsActiveIndex(int index ) {
			return MinActiveIndex <= index && index <= MaxActiveIndex;
		}

		public int MinActiveIndex
			=> 0;
		
		public int MaxActiveIndex
			=> 4;

		public bool IsMoveRightAllowed
			=> planetViews[planetViews.Length - 1].CurrentIndex > 1;


		public PlanetImagePositionSize GetPlanetImagePositionSize(int planetId, int index){

			//patch: sun image has other dimensions than other planet images...
			if(planetId != -1 && index == 0 ) {
				PlanetImagePositionSize targPositionSize = planetPositionSizes.FirstOrDefault(ps => ps.index == 0);
				return new PlanetImagePositionSize {
					position = new Vector2(200, 0),
					size = targPositionSize.size,
					index = 0
				};
			} else {
				//normal search
				for(int i = 0; i < planetPositionSizes.Length; i++ ) {
					if(planetPositionSizes[i].index == index ) {
						return planetPositionSizes[i];
					}
				}
			}
			return null;
		}
	}

	[System.Serializable]
	public class PlanetImagePositionSize {
		public Vector2 position;
		public Vector2 size;

		public int index;
	}

	public interface IPlanetHeaderLine {

		PlanetImagePositionSize GetPlanetImagePositionSize(int planetId, int index);
		int MinActiveIndex { get;}
		int MaxActiveIndex { get;}
		bool IsActiveIndex(int index);

		bool IsMoveRightAllowed { get; }

		

		int GetIndexForPlanet(int planetId);
	}
}