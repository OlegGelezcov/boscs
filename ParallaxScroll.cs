namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ParallaxScroll : MonoBehaviour {

        public bool isParallaxEnabled = false;

        public ScrollRect parentRect;
        public RectTransform parentContent;
        public RectTransform currentContent;

        public float minEdge = -0.5f;
        public float maxEdge = 1.5f;

        public float parallaxCoefficient = 0.8f;
        //public float smoothTime = 0.3f;
        public float differentThreshold = 0.1f;

        //private float speed = 0f;

        public float patchSpeed = 10;


        private float prevScrollPos;
        private ScrollRect childRect;

        void OnEnable() {
            prevScrollPos = parentRect.verticalNormalizedPosition;
            childRect = GetComponent<ScrollRect>();
            childRect.verticalNormalizedPosition = parentRect.verticalNormalizedPosition;
        }

        void Update() {
            if (!Mathf.Approximately(parentContent.sizeDelta.y, currentContent.sizeDelta.y)) {
                currentContent.sizeDelta = new Vector2(currentContent.sizeDelta.x, parentContent.sizeDelta.y);
            }

            if (isParallaxEnabled) {


                float currentPos = parentRect.verticalNormalizedPosition;
                float myPos = childRect.verticalNormalizedPosition;
                float delta = currentPos - prevScrollPos;
                prevScrollPos = currentPos;

                float diff = Mathf.Abs(currentPos - myPos);

                myPos += delta * (diff < differentThreshold ? parallaxCoefficient : 1);


                /*
                float minPos = currentPos - differentThreshold;
                //minPos = Mathf.Max(minPos, 0);

                float maxPos = currentPos + differentThreshold;
                //maxPos = Mathf.Min(maxPos, 1);

                myPos = Mathf.Clamp(myPos, 0 - differentThreshold, 1 + differentThreshold);

                childRect.verticalNormalizedPosition = myPos;

                //childRect.verticalNormalizedPosition = Mathf.SmoothDamp(childRect.verticalNormalizedPosition, myPos, ref speed, smoothTime); //myPos;

                
                */
                if(myPos >= minEdge && myPos <= maxEdge) {
                    childRect.verticalNormalizedPosition = Mathf.Clamp(myPos, minEdge, maxEdge);
                } else {
                    float notPatched = childRect.verticalNormalizedPosition + delta;
                    float patchDiff =  Mathf.Abs(parentRect.verticalNormalizedPosition - notPatched);
                    if(patchDiff > 0.01) {
                        childRect.verticalNormalizedPosition = Mathf.Lerp(notPatched, parentRect.verticalNormalizedPosition, patchSpeed * Time.deltaTime);
                    } else {
                        childRect.verticalNormalizedPosition = notPatched;
                    }
                }

                //print($"Parent scroll Y => {parentRect.verticalNormalizedPosition}, child scroll Y => {childRect.verticalNormalizedPosition}");
                
            } else {
                childRect.verticalNormalizedPosition = parentRect.verticalNormalizedPosition;
            }
        }
    }

}