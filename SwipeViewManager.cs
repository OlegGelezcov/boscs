namespace Bos.UI {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class SwipeViewManager<TData, TView>  where TView : MonoBehaviour {

		public float SwipeDistance {get; set; } = 100;
		public float SwipeSpeed { get; set; } = 2000;
		public float SwipeMult { get; set; } = 0.5f;

		public float ScreenSize { get; set; } = 1080;

		public float StopThreshold {get; set; } = 70;



		public System.Func<TData, bool> HasRight { get; set; }
		public System.Func<TData, bool> HasLeft { get; set; }

		public System.Func<TData, TData> RightData {get; set;}

		public System.Func<TData, TData> LeftData {get; set;}

		public System.Action<TData, TView> SetupView { get; set; }

		public System.Action TransitionAction {get; set;}

		public System.Action<TView> ActiveViewChanged { get; set; }


		private readonly List<RectTransform> swipedTransforms 
		 = new List<RectTransform>();

		public RectTransform ActiveRectTransform { get; private set; } = null;

		public TData CurrentData { get; private set;}
		public TView CurrentView { get; private set;}

		private ManagementView.SwipeResultAction CurrentAction { get; set; } 
			= ManagementView.SwipeResultAction.None;
		
		private readonly SwipeObjectCache cache 
		 	= new SwipeObjectCache();

		private bool IsSwiped {get; set; } = false;


		private Transform layout;
		private TView prefabView;

		private void Resetup(TData currentData, TView currentView ) {
			Setup(layout, prefabView, currentData, currentView);
		}



		public void Setup(Transform layout, TView prefabView,  TData currentData, TView currentView) {
			this.layout = layout;
			this.prefabView = prefabView;

			cache.Setup(layout, prefabView.gameObject);
			CurrentData = currentData;
			CurrentView = currentView;
			ActiveRectTransform = currentView.GetComponent<RectTransform>();

			SetupView?.Invoke(CurrentData, CurrentView);

		}


		public void HandleSwipe(SwipeData data ) {
			
			if(!data.IsEnd) {

				if(!IsSwiped) {
					ConstructSwipeObjects();
					IsSwiped = true;
				} else {
					swipedTransforms.ForEach(trs => {
						switch(data.Direction) {
							case SwipeDirection.Left: {
								trs.anchoredPosition -= new Vector2(data.HorizontalLength, 0) * SwipeMult;
							}
							break;
							case SwipeDirection.Right: {
								trs.anchoredPosition += new Vector2(data.HorizontalLength, 0) * SwipeMult;
							}
							break;
						}
					});

				}
			} else {
				IsSwiped = false;
				if(ActiveRectTransform == null ) {
					return;
				}

				float x = ActiveRectTransform.anchoredPosition.x;
				if( x < -SwipeDistance ) {
					if(HasRight(CurrentData)) {
						CurrentAction = ManagementView.SwipeResultAction.MoveToLeft;
					} else {
						CurrentAction = ManagementView.SwipeResultAction.ReturnToCenter;
					}
				} else if(x > SwipeDistance) {
					if(HasLeft(CurrentData)) {
						CurrentAction = ManagementView.SwipeResultAction.MoveToRight;
					} else {
						CurrentAction = ManagementView.SwipeResultAction.ReturnToCenter;
					}
				} else {
					CurrentAction = ManagementView.SwipeResultAction.ReturnToCenter;
				}
			}
		}

        public void MoveRight() {
            if(CurrentAction == ManagementView.SwipeResultAction.None && !IsSwiped) {
                if(HasLeft(CurrentData)) {
                    ConstructSwipeObjects();
                    CurrentAction = ManagementView.SwipeResultAction.MoveToRight;
                    Debug.Log("swipe manager -> move right...");
                }
            }
        }

        public bool IsAllowMoveRight
            => (CurrentAction == ManagementView.SwipeResultAction.None) && (!IsSwiped) && HasLeft(CurrentData);

        public bool IsAllowMoveLeft
            => (CurrentAction == ManagementView.SwipeResultAction.None) && (!IsSwiped) && HasRight(CurrentData);


        public void MoveLeft() {
            if(CurrentAction == ManagementView.SwipeResultAction.None && !IsSwiped) {
                if(HasRight(CurrentData)) {
                    ConstructSwipeObjects();
                    CurrentAction = ManagementView.SwipeResultAction.MoveToLeft;
                    Debug.Log("swipe manager -> move left");
                }
            }
        }



		public void Update() {
			if(!IsSwiped ) {
				Vector2 targetPoint = new Vector2(0, -1);
				switch(CurrentAction) {
					case ManagementView.SwipeResultAction.ReturnToCenter: {
						targetPoint = Vector2.zero;
					}
					break;
					case ManagementView.SwipeResultAction.MoveToLeft: {
						targetPoint = new Vector2(-ScreenSize, 0);
					}
					break;
					case ManagementView.SwipeResultAction.MoveToRight: {
						targetPoint = new Vector2(ScreenSize, 0);
					}
					break;
				}

				if(CurrentAction != ManagementView.SwipeResultAction.None ) {
					Vector2 direction = targetPoint - ActiveRectTransform.anchoredPosition;
					direction.Normalize();
					swipedTransforms.ForEach(trs => trs.anchoredPosition += direction * SwipeSpeed * Time.deltaTime);
					if(Vector2.Distance(ActiveRectTransform.anchoredPosition, targetPoint) < StopThreshold ) {
						ActiveRectTransform.anchoredPosition = targetPoint;
						if(HasLeft(CurrentData) && swipedTransforms.Count > 0) {
							swipedTransforms[0].anchoredPosition = targetPoint - new Vector2(ScreenSize, 0);
						}
						if(HasRight(CurrentData)) {
							if(swipedTransforms.Count > 0 ) {
								swipedTransforms[swipedTransforms.Count - 1].anchoredPosition = targetPoint + new Vector2(ScreenSize, 0);
							}
						}
						Finalize(CurrentAction);
						CurrentAction = ManagementView.SwipeResultAction.None;
					}
				}
			}
		}

        public bool IsMoving
            => CurrentAction != ManagementView.SwipeResultAction.None;


		private void Finalize(ManagementView.SwipeResultAction action ) {
			switch(action) {
				case ManagementView.SwipeResultAction.ReturnToCenter: {
					if(HasLeft(CurrentData)) {
						if(swipedTransforms.Count > 0 ) {
							cache.PushObject(swipedTransforms[0].gameObject);
						}
					}
					if(HasRight(CurrentData) ) {
						if(swipedTransforms.Count > 0 ) {
							cache.PushObject(swipedTransforms[swipedTransforms.Count - 1].gameObject);
						}
					}
                        swipedTransforms.Clear();
				}
				break;
				case ManagementView.SwipeResultAction.MoveToLeft: {
					if(HasLeft(CurrentData)) {
						cache.PushObject(swipedTransforms[0].gameObject);
						cache.PushObject(swipedTransforms[1].gameObject);
						var rightData = RightData(CurrentData);
						var rightView = ChangeCurrentRect(swipedTransforms[2]);						
						Resetup(rightData, rightView);
						ActiveViewChanged?.Invoke(CurrentView);
						swipedTransforms.Clear();
					} else {
						cache.PushObject(swipedTransforms[0].gameObject);
						var rightData =RightData(CurrentData);
						var rightRect = ChangeCurrentRect(swipedTransforms[1]);
						Resetup(rightData, rightRect);
						swipedTransforms.Clear();
					}
				}
				break;
				case ManagementView.SwipeResultAction.MoveToRight: {
					if(HasRight(CurrentData)) {
						cache.PushObject(swipedTransforms[2].gameObject);
						cache.PushObject(swipedTransforms[1].gameObject);
						var leftData = LeftData(CurrentData);
						var leftRect = ChangeCurrentRect(swipedTransforms[0]);

						Resetup(leftData, leftRect);
						swipedTransforms.Clear();
					} else {
						cache.PushObject(swipedTransforms[1].gameObject);
						var leftData = LeftData(CurrentData);
						var leftRect = ChangeCurrentRect(swipedTransforms[0]);
						Resetup(leftData, leftRect);
						swipedTransforms.Clear();
					}
				}
				break;
			}
		}

		private TView ChangeCurrentRect(RectTransform rectTransform ) {
			CurrentView = rectTransform.GetComponent<TView>();
			
			return CurrentView;
		}

		private void ConstructSwipeObjects() {
			ActiveRectTransform = CurrentView.GetComponent<RectTransform>();
			swipedTransforms.ForEach(trs => {
				if(trs != ActiveRectTransform ) {
					cache.PushObject(trs.gameObject);
				}
			});

			swipedTransforms.Clear();
			swipedTransforms.Add(ActiveRectTransform);

			if(HasLeft(CurrentData)) {
				GameObject obj = cache.PopObject();
				RectTransform leftTrs = obj.GetComponent<RectTransform>();
				SetupView(LeftData(CurrentData), leftTrs.GetComponent<TView>());
				leftTrs.anchoredPosition = new Vector2(ActiveRectTransform.anchoredPosition.x - ScreenSize,
					ActiveRectTransform.anchoredPosition.y);
				swipedTransforms.Insert(0, leftTrs);
			}

			if(HasRight(CurrentData)) {
				GameObject obj = cache.PopObject();
				RectTransform rightTrs = obj.GetComponent<RectTransform>();
				SetupView(RightData(CurrentData), rightTrs.GetComponent<TView>());
				rightTrs.anchoredPosition = new Vector2(ActiveRectTransform.anchoredPosition.x + ScreenSize, 
				ActiveRectTransform.anchoredPosition.y);
				swipedTransforms.Add(rightTrs);
			}

			TransitionAction?.Invoke();
		}

        public void ClearCache() {
            cache.Clear();
        }
	}

	public class SwipeObjectCache {
		private GameObject prefab;
		private Transform parent;

		private int maxSize;


		private readonly Stack<GameObject> cache =
			new Stack<GameObject>();

		public void Setup(Transform parent, GameObject prefab, int maxSize = 3) {
			this.prefab = prefab;
			this.parent = parent;
			this.maxSize = maxSize;
		}

		public GameObject PopObject() {
			if(cache.Count == 0 ) {
				GameObject instance = GameObject.Instantiate(prefab);
				instance.transform.SetParent(parent, false);
				return instance;
			} else {
				GameObject instance = cache.Pop();
				instance.Activate();
				return instance;
			}
		}

		public void PushObject(GameObject instance ) {
				instance.Deactivate();
				cache.Push(instance);
				instance.GetComponent<RectTransform>()
					.anchoredPosition = new Vector2(40000, 0);
		}

        public void Clear() {
            while(cache.Count > 0 ) {
                GameObject obj = cache.Pop();
                if(obj != null && obj ) {
                    GameObject.Destroy(obj);
                }
            }
        }
	}
}
