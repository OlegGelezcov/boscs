namespace Bos.UI {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class PlanetHeaderMenuIconView : GameBehaviour {

		public int planetId;

		private IPlanetHeaderLine parent;

		private RectTransform rectTransform;

		private RectTransform RectTransform
			=> (rectTransform != null) ? rectTransform :
				(rectTransform = GetComponent<RectTransform>());

		public void Setup(IPlanetHeaderLine parent) {
			this.parent = parent;
		}

		public void SetIndex(int index) {
			CurrentIndex = index;
		}

		public void MoveTo(PlanetImagePositionSize positionSize, float duration) {
			Vector2 currentPosition = RectTransform.anchoredPosition;
			Vector2 currentSize = RectTransform.sizeDelta;
			
			Vector2AnimationData positionData = new Vector2AnimationData{
				 StartValue = currentPosition,
				 EndValue = positionSize.position,
				 Duration = duration,
				 EaseType = EaseType.EaseInOutCubic,
				 OnStart = (pos, obj) => { 
					 RectTransform.anchoredPosition = pos;
					 if(parent.IsActiveIndex(CurrentIndex)) {
						 RectTransform.gameObject.Activate();
					 }
				 },
				 OnUpdate = (pos, t, obj) => RectTransform.anchoredPosition = pos,
				 OnEnd = (pos, obj) => {
					 RectTransform.anchoredPosition = pos;
					 SetIndex(positionSize.index);
					 if(!parent.IsActiveIndex(positionSize.index)) {
						RectTransform.gameObject.Deactivate();
					 }
				 }
			};

			Vector2AnimationData sizeData = new Vector2AnimationData{
				StartValue = currentSize,
				EndValue = positionSize.size,
				Duration = duration,
				EaseType = EaseType.EaseInOutCubic,
				OnStart = (size, obj) => {
					RectTransform.sizeDelta = size;
				},
				OnUpdate = (size, t, obj) => {
					RectTransform.sizeDelta = size;
				},
				OnEnd = (size, obj) => {
					RectTransform.sizeDelta = size;
				}
			};
			StartCoroutine(MoveToImpl(new List<Vector2AnimationData>{ positionData, sizeData}));
		}

		private IEnumerator MoveToImpl(List<Vector2AnimationData> datas) {
			Vector2Animator vec2Animator = RectTransform.gameObject.GetOrAdd<Vector2Animator>();
			yield return new WaitUntil(() => vec2Animator.IsStarted == false);
			vec2Animator.StartAnimation(datas);
		}

		public void SetPositionSize(PlanetImagePositionSize positionSize) {
			RectTransform.anchoredPosition = positionSize.position;
			RectTransform.sizeDelta = positionSize.size;
			SetIndex(positionSize.index);
		}

		public int CurrentIndex { get; private set;}
	
		public int PlanetId
			=> planetId;

		
	}
}
