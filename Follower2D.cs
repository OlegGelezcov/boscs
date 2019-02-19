namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Follower2D : GameBehaviour {

        public float epsilon = 50;
        public RectTransform shipTransform;
        public RectTransform startPoint;

        private float rotationSpeed = 10;
        public float maxMoveSpeed = 380;
        public float acceleration = 150;
        private float currentSpeed = 0f;
        private const float kWaitDuration = 10;
        //private const float kExploreDuration = 5;
        public Vector2 exploreDurationRange = new Vector2(5, 5);

        private bool isMoved = false;
        private RectTransform target = null;
        private GeneratorShipTarget targetBehaviour = null;
        private RectTransform myTransform;
        private GeneratorShipTargetManager manager;
        private Vector2 lastDirection = Vector2.zero;

        private bool isExploring = false;
        private float waitTimer = 0f;
        private float exploreTimer = 0f;
        private bool isReturning = false;
        private bool isPreparing = false;
        private float ExploreDuration { get; set; }

        public override void Start() {
            base.Start();
            ExploreDuration = UnityEngine.Random.Range(exploreDurationRange.x, exploreDurationRange.y);
            myTransform = GetComponent<RectTransform>();
            manager = myTransform.parent.GetComponent<GeneratorShipTargetManager>();
            RequestTarget();
        }

        private void RequestTarget() {
            targetBehaviour = manager.GetTargetForFollower(myTransform);
            if(targetBehaviour != null ) {
                target = targetBehaviour.GetComponent<RectTransform>();
            } else {
                target = null;
            }
        }

        private void ResetTarget() {
            targetBehaviour = null;
            target = null;
        }


        private float UpdateRotation(RectTransform target ) {
            Vector2 directionFull = (target.anchoredPosition - myTransform.anchoredPosition);
            lastDirection = directionFull.normalized;
            float magnitude = directionFull.magnitude;

            float rotZ = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, rotZ - 90);
            myTransform.localRotation = Quaternion.Slerp(myTransform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            return magnitude;
        }
        

        public override void Update() {
            base.Update();
            if (isExploring) {
                if (target != null) {

                    float magnitude = UpdateRotation(target);

                    if (magnitude < epsilon) {
                        isMoved = false;
                        manager.OnTargetReached(targetBehaviour);
                        ResetTarget();
                    } else {
                        isMoved = true;
                    }

                    if (isMoved) {
                        currentSpeed += acceleration * Time.deltaTime;
                        if (currentSpeed > maxMoveSpeed) {
                            currentSpeed = maxMoveSpeed;
                        }

                        myTransform.anchoredPosition += currentSpeed * lastDirection * Time.deltaTime;
                    } else {
                        Deccelerate();
                    }
                } else {
                    Deccelerate();
                    RequestTarget();
                }

                exploreTimer += Time.deltaTime;
                if(exploreTimer > ExploreDuration ) {
                    isExploring = false;
                    isReturning = true;
                }
            } else if(isReturning ) {
                float magnitude = UpdateRotation(startPoint);
                myTransform.anchoredPosition += currentSpeed * lastDirection * Time.deltaTime;
                if (magnitude < epsilon) {
                    if (!isPreparing) {
                        myTransform.anchoredPosition = startPoint.anchoredPosition;
                        myTransform.localRotation = Quaternion.identity;
                        isReturning = false;
                        isPreparing = true;
                        PrepareWaiting();
                    }
                }
            } else {
                if (!isPreparing) {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= kWaitDuration) {
                        if (!isPreparing) {
                            PrepareExploring();
                            isPreparing = true;
                        }
                    }
                }
            }
        }

        private void PrepareWaiting() {
            FloatAnimationData animData = new FloatAnimationData {
                StartValue = shipTransform.localEulerAngles.z,
                EndValue = 0,
                Duration = 0.5f,
                Target = shipTransform.gameObject,
                EaseType = EaseType.EaseInOutQuad,
                OnStart = shipTransform.UpdateZRotation(),
                OnUpdate = shipTransform.UpdateZRotationTimed(),
                OnEnd = shipTransform.UpdateZRotation(() => {
                    isPreparing = false;
                    waitTimer = 0;                   
                })
            };
            shipTransform.gameObject.GetOrAdd<FloatAnimator>().StartAnimation(animData);
        }

        private void PrepareExploring() {
            FloatAnimationData animData = new FloatAnimationData {
                StartValue = 0,
                EndValue = 52,
                Duration = 0.5f,
                EaseType = EaseType.EaseInOutQuad,
                Target = shipTransform.gameObject,
                OnStart = (v, o) => shipTransform.localRotation = Quaternion.Euler(0, 0, v),
                OnUpdate = (v, t, o) => shipTransform.localRotation = Quaternion.Euler(0, 0, v),
                OnEnd = (v, o) => {
                    shipTransform.localRotation = Quaternion.Euler(0, 0, v);
                    isExploring = true;
                    isReturning = true;
                    isPreparing = false;
                    waitTimer = 0;
                    exploreTimer = 0;
                }
            };
            shipTransform.gameObject.GetOrAdd<FloatAnimator>().StartAnimation(animData);
        }

        private void Deccelerate() {
            if (currentSpeed > 0) {
                currentSpeed -= acceleration * Time.deltaTime;
                if (currentSpeed < 0) {
                    currentSpeed = 0;
                }
                if (currentSpeed > 0) {
                    myTransform.anchoredPosition += currentSpeed * lastDirection * Time.deltaTime;
                }
            }
        }
    }

}