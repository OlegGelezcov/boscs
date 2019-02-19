namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ManagementSwipeController : GameBehaviour {

        private readonly Vector2 HORIZONTAL_VECTOR = Vector2.right * 1080;

        public RectTransform parent;
        public GameObject prefab;

        private RectTransform currentRect;
        private bool isCurrentTransitionStarted = false;
        private RectTransform rightRect;
        private bool isLeftTransitionStarted = false;
        private RectTransform leftRect;
        private bool isRightTransitionStarted = false;
        private int managerId;
        private IManagementView container;
        private bool isDragStarted = false;
        private bool isActiveController = false;

        public static event System.Action<bool> TransitionStartedChanged;

        private static void OnTransitionStartedChanged(bool value ) {
            TransitionStartedChanged?.Invoke(value);
        }

        public bool IsTransitionStarted
            => isCurrentTransitionStarted || isLeftTransitionStarted || isRightTransitionStarted;
     

        public void ActivateController(int managerId, IManagementView container) {
            if (!isActiveController) {
                this.managerId = managerId;
                this.container = container;
                this.isDragStarted = false;
                this.isCurrentTransitionStarted = false;
                this.isLeftTransitionStarted = false;
                this.isRightTransitionStarted = false;
                this.currentRect = CreateViewInstance(Vector2.zero, managerId).GetComponent<RectTransform>();
                this.isActiveController = true;
                OnTransitionStartedChanged(IsTransitionStarted);
            }
        }

        public int DeactivateController() {
            if (isActiveController) {

                this.isActiveController = false;
                this.isDragStarted = false;
                this.isCurrentTransitionStarted = false;
                this.isLeftTransitionStarted = false;
                this.isRightTransitionStarted = false;
                if (currentRect && currentRect.gameObject) {
                    Destroy(currentRect.gameObject);
                    currentRect = null;
                }
                if (rightRect && rightRect.gameObject) {
                    Destroy(rightRect.gameObject);
                    rightRect = null;
                }
                if (leftRect && leftRect.gameObject) {
                    Destroy(leftRect.gameObject);
                    leftRect = null;
                }
                OnTransitionStartedChanged(IsTransitionStarted);
                return managerId;
            } else {
                return -100;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            SwipeDetector.OnSwipe += OnSwipe;
        }

        public override void OnDisable() {
            SwipeDetector.OnSwipe -= OnSwipe;
            base.OnDisable();
        }

        private void SetPositions(Vector2 curPos ) {
            if(currentRect != null && currentRect.gameObject ) {
                currentRect.anchoredPosition = curPos;
            } 
            if(rightRect != null && rightRect.gameObject) {
                rightRect.anchoredPosition = curPos + HORIZONTAL_VECTOR;
            }
            if(leftRect != null && leftRect.gameObject) {
                leftRect.anchoredPosition = curPos - HORIZONTAL_VECTOR;
            }
        }

        public void MakeTransitionProgrammatically(ManagementView.SwipeResultAction action) {
            if(action == ManagementView.SwipeResultAction.MoveToLeft ) {
                SwipeData swipeData = new SwipeData() {
                    Direction = SwipeDirection.Left,
                    EndPosition = new Vector2(-10, 0),
                    StartPosition = new Vector2(0, 0),
                    IsEnd = false
                };
                isDragStarted = false;
                DragCurrentTransform(swipeData);
                Transition(action);
            } else if(action == ManagementView.SwipeResultAction.MoveToRight ) {
                SwipeData swipeData = new SwipeData() {
                    Direction = SwipeDirection.Right,
                    EndPosition = new Vector2(0, 0),
                    StartPosition = new Vector2(10, 0),
                    IsEnd = false
                };
                isDragStarted = false;
                DragCurrentTransform(swipeData);
                Transition(action);
            }
        }

        private void OnSwipe(SwipeData data) {
            if (isActiveController) {
                if (!data.IsEnd) {
                    if(!IsTransitionStarted) {
                        if(!isDragStarted) {
                            isDragStarted = true;
                        } else {
                            DragCurrentTransform(data);
                        }
                    }
                } else {
                    if (isDragStarted) {
                        isDragStarted = false;
                        var transitionAction = ComputeTransitionAction();
                        Transition(transitionAction);
                    }
                }
            }
        }

        private Vector2 GetTargetPointForCurrentRect(ManagementView.SwipeResultAction transitionAction ) {
            switch(transitionAction) {
                case ManagementView.SwipeResultAction.MoveToLeft: {
                        return -HORIZONTAL_VECTOR;
                    }
                case ManagementView.SwipeResultAction.MoveToRight: {
                        return HORIZONTAL_VECTOR;
                    }
                default: {
                        return Vector2.zero;
                    }
            }
        }

        private void Transition(ManagementView.SwipeResultAction transitionAction ) {
            if(currentRect == null ) {
                return;
            }

            float distance = Vector2.Distance(currentRect.anchoredPosition, GetTargetPointForCurrentRect(transitionAction));
            float interval = distance / container.TransitionSpeed;
            switch (transitionAction) {
                case ManagementView.SwipeResultAction.ReturnToCenter: {

                        isCurrentTransitionStarted = true;
                        OnTransitionStartedChanged(IsTransitionStarted);
                        Vector2AnimationData currentPosData = new Vector2AnimationData {
                            StartValue = currentRect.anchoredPosition,
                            EndValue = Vector2.zero,
                            Duration = interval,
                            EaseType = EaseType.EaseInOutQuad,
                            Target = currentRect.gameObject,
                            OnStart = (p, o) => SetPositions(p),
                            OnUpdate = (p, t, o) => SetPositions(p),
                            OnEnd = (p, o) => {
                                isCurrentTransitionStarted = false;
                                OnTransitionStartedChanged(IsTransitionStarted);
                                SetPositions(p);
                            }
                        };
                        currentRect.gameObject.GetOrAdd<Vector2Animator>().StartAnimation(currentPosData);
                    }
                    break;
                case ManagementView.SwipeResultAction.MoveToLeft: {
                        isCurrentTransitionStarted = true;
                        isRightTransitionStarted = true;
                        OnTransitionStartedChanged(IsTransitionStarted);
                        Vector2AnimationData currentPosData = new Vector2AnimationData {
                            StartValue = currentRect.anchoredPosition,
                            EndValue = -HORIZONTAL_VECTOR,
                            Duration = interval,
                            EaseType = EaseType.EaseInOutQuad,
                            Target = currentRect.gameObject,
                            OnStart = (p, o) => SetPositions(p),
                            OnUpdate = (p, t, o) => SetPositions(p),
                            OnEnd = (p, o) => {
                                isCurrentTransitionStarted = false;
                                SetPositions(p);
                                isRightTransitionStarted = false;
                                MakeRightAsCurrent();
                                container.OnTransitionCompletedIn(managerId);
                                OnTransitionStartedChanged(IsTransitionStarted);
                            }
                        };
                        currentRect.gameObject.GetOrAdd<Vector2Animator>().StartAnimation(currentPosData);

                    }
                    break;
                case ManagementView.SwipeResultAction.MoveToRight: {
                        isCurrentTransitionStarted = true;
                        isLeftTransitionStarted = true;
                        OnTransitionStartedChanged(IsTransitionStarted);
                        Vector2AnimationData currentPosData = new Vector2AnimationData {
                            StartValue = currentRect.anchoredPosition,
                            EndValue = HORIZONTAL_VECTOR,
                            Duration = interval,
                            EaseType = EaseType.EaseInOutQuad,
                            Target = currentRect.gameObject,
                            OnStart = (p, o) => SetPositions(p),
                            OnUpdate = (p, t, o) => SetPositions(p),
                            OnEnd = (p, o) => {
                                isCurrentTransitionStarted = false;
                                SetPositions(p);
                                isLeftTransitionStarted = false;
                                MakeLeftAsCurrent();
                                container.OnTransitionCompletedIn(managerId);
                                OnTransitionStartedChanged(IsTransitionStarted);
                            }
                        };
                        currentRect.gameObject.GetOrAdd<Vector2Animator>().StartAnimation(currentPosData);
                    }
                    break;
            }
        }

        private GameObject CreateViewInstance(Vector2 position, int managerId) {
            GameObject obj = Instantiate<GameObject>(prefab);
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchoredPosition = position;
            rectTransform.SetAsFirstSibling();
            container.InitializeView(obj, managerId);
            return obj;
        }

        private void DragCurrentTransform(SwipeData data) {
            switch(data.Direction) {
                case SwipeDirection.Left: {
                        if (currentRect != null) {
                            currentRect.anchoredPosition -= new Vector2(Mathf.Abs(data.HorizontalLength) * container.DragMult, 0);
                            if (rightRect == null && container.HasRightManager(managerId)) {
                                rightRect = CreateViewInstance(currentRect.anchoredPosition + HORIZONTAL_VECTOR, container.RightManagerId(managerId)).GetComponent<RectTransform>();
                            }
                            if (rightRect != null && rightRect.gameObject) {
                                rightRect.anchoredPosition = currentRect.anchoredPosition + HORIZONTAL_VECTOR;
                            }

                        }
                    }
                    break;
                case SwipeDirection.Right: {
                        if (currentRect != null) {
                            currentRect.anchoredPosition += new Vector2(Mathf.Abs(data.HorizontalLength) * container.DragMult, 0);
                            if (leftRect == null && container.HasLeftManager(managerId)) {
                                leftRect = CreateViewInstance(currentRect.anchoredPosition - HORIZONTAL_VECTOR, container.LeftManagerId(managerId)).GetComponent<RectTransform>();
                            }
                            if (leftRect != null && leftRect.gameObject) {
                                leftRect.anchoredPosition = currentRect.anchoredPosition - HORIZONTAL_VECTOR;
                            }
                        }
                    }
                    break;
            }
        }

        private ManagementView.SwipeResultAction ComputeTransitionAction() {

            if(currentRect == null ) {
                return ManagementView.SwipeResultAction.None;
            }

            if(Mathf.Approximately(currentRect.anchoredPosition.x, 0) && Mathf.Approximately(currentRect.anchoredPosition.y, 0)) {
                return ManagementView.SwipeResultAction.None;
            }
            if(currentRect.anchoredPosition.x < -container.TransitionDistance) {
                if (container.HasRightManager(managerId)) {
                    return ManagementView.SwipeResultAction.MoveToLeft;
                } else {
                    return ManagementView.SwipeResultAction.ReturnToCenter;
                }
            } else if(currentRect.anchoredPosition.x > container.TransitionDistance ) {
                if (container.HasLeftManager(managerId)) {
                    return ManagementView.SwipeResultAction.MoveToRight;
                } else {
                    return ManagementView.SwipeResultAction.ReturnToCenter;
                }
            } else {
                return ManagementView.SwipeResultAction.ReturnToCenter;
            }
        }

        private void MakeRightAsCurrent() {
            if(currentRect != null && currentRect.gameObject) {
                Destroy(currentRect.gameObject);
                currentRect = null;
            }
            currentRect = rightRect;
            rightRect = null;
            managerId = container.RightManagerId(managerId);

            if(leftRect != null && leftRect.gameObject) {
                Destroy(leftRect.gameObject);
                leftRect = null;
            }
        }

        private void MakeLeftAsCurrent() {
            if (currentRect != null && currentRect.gameObject) {
                Destroy(currentRect.gameObject);
                currentRect = null;
            }
            currentRect = leftRect;
            leftRect = null;
            managerId = container.LeftManagerId(managerId);

            if(rightRect != null && rightRect.gameObject ) {
                Destroy(rightRect.gameObject);
                rightRect = null;
            }
        }

        
    }

    public interface IManagementView {
        bool HasRightManager(int managerId);
        bool HasLeftManager(int managerId);
        int RightManagerId(int managerId);
        int LeftManagerId(int managerId);

        void InitializeView(GameObject obj, int managerId);
        float DragMult { get; }
        float TransitionSpeed { get; }
        float TransitionDistance { get; }
        void OnTransitionCompletedIn(int managerId);
    }
}