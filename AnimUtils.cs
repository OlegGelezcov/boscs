namespace Bos {
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.UI;

    public static class AnimUtils {

	    public static BezierData GetBizerQuadData(
		    IPositionObject target, 
		    Vector2 start, Vector2 end, float interval, 
		    System.Action<GameObject> onComplete = null, 
		    float linearFactor = 0.25f, float orthoFactor = 0.25f ) {
		    Vector2 midPoint = BosMath.ControlPoint(start, end, linearFactor, orthoFactor, 1);
		    Vector3[] points = new Vector3[] {
			    start,
			    midPoint,
			    end
		    };
		    return new BezierData() {
			    Target = target,
			    Interval = interval,
			    OnComplete = onComplete,
			    Points = points,
			    Type = BezierType.Quadratic
		    };
	    }

	    public static BezierData GetBezierQubicData(IPositionObject target,
		    Vector2 start, Vector2 end, float interval,
		    System.Action<GameObject> onComplete = null,
		    float linearFactor = 0.25f, float orthoFactor = 0.25f) {
		    Vector2 mid1 = BosMath.ControlPoint(start, end, linearFactor, orthoFactor, 1);
		    Vector2 mid2 = BosMath.ControlPoint(start, end, 1 - linearFactor, 1 - orthoFactor, -1);
		    Vector3[] points = new Vector3[] {
			    start,
			    mid1,
			    mid2,
			    end
		    };
		    return new BezierData() {
			    Target = target,
			    Interval = interval,
			    OnComplete = onComplete,
			    Points = points,
			    Type = BezierType.Qubic
		    };
	    }
	    
	    
        public static Vector2AnimationData GetScaleAnimData(float start, float end, 
            float duration,  
            EaseType easeType, RectTransform target, 
            System.Action onEnd = null) {
            Vector2AnimationData data = new Vector2AnimationData {
                StartValue = new Vector2(start, start),
                EndValue = new Vector2(end, end),
                Duration = duration,
                Target = target.gameObject,
                EaseType = easeType,
                OnStart = (s, o) => target.localScale = new Vector3(s.x, s.y, 1),
                OnUpdate = (s, t, o) => target.localScale = new Vector3(s.x, s.y, 1),
                OnEnd = (s, o) => {
                    target.localScale = new Vector3(s.x, s.y, 1);
                    onEnd?.Invoke();
                }
            };
            return data;
        }

        public static void ScaleMe(this GameObject obj, float start, float end, float duration, EaseType easeType,
            Action onEnd = null) {
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            Vector2Animator animator = obj.GetOrAdd<Vector2Animator>();
            animator.StartAnimation(GetScaleAnimData(start, end, duration, easeType, rectTransform, onEnd));
        }

        public static ColorAnimationData GetColorAnimData(Color start, Color end, float duration, EaseType easeType, 
            RectTransform target, BosAnimationMode mode = BosAnimationMode.Single, System.Action onEnd = null ) {
            Graphic graphic = target.GetComponent<Graphic>();

            ColorAnimationData data = new ColorAnimationData {
                StartValue = start,
                EndValue = end,
                Duration = duration,
                Target = target.gameObject,
                EaseType = easeType,
                OnStart = (c, o) => graphic.color = c,
                OnUpdate = (c, t, o) => graphic.color = c,
                OnEnd = (c, o) => {
                    graphic.color = c;
                    onEnd?.Invoke();
                 },
                 AnimationMode = mode
            };
            return data;
        }

		public static System.Func<float, float, float, float> GetEaseFunc(EaseType type) {
			return easeFunctionMap.ContainsKey(type) ? easeFunctionMap[type] :
				(start, end, timer) => start;			
		}
		private static readonly Dictionary<EaseType, System.Func<float, float, float, float>> easeFunctionMap = 
		new Dictionary<EaseType, System.Func<float, float, float, float>>{
			[EaseType.Linear] = Linear,
			[EaseType.EaseInQuad] = EaseInQuad,
			[EaseType.EaseOutQuad] = EaseOutQuad,
			[EaseType.EaseInOutQuad] = EaseInOutQuad,
			[EaseType.EaseInCubic] = EaseInCubic,
			[EaseType.EaseOutCubic] = EaseOutCubic,
			[EaseType.EaseInOutCubic] = EaseInOutCubic,
			[EaseType.EaseInQuartic] = EaseInQuartic,
			[EaseType.EaseOutQuartic] = EaseOutQuartic,
			[EaseType.EaseInOutQuartic] = EaseInOutQuartic,
			[EaseType.EaseInQuintic] = EaseInQuintic,
			[EaseType.EaseOutQuintic] = EaseOutQuintic,
			[EaseType.EaseInOutQuintic] = EaseInOutQuintic,
			[EaseType.EaseInSin] = EaseInSin,
			[EaseType.EaseOutSin] = EaseOutSin,
			[EaseType.EaseInOutSin] = EaseInOutSin
		};
		
		public static float Linear(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			return change * timer01 + startValue;
		}

		public static float EaseInQuad(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			return change * timer01 * timer01 + startValue;
		}

		public static float EaseOutQuad(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			return -change * timer01 * (timer01 - 2.0f) + startValue;
		}

		public static float EaseInOutQuad(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			timer01 *= 2.0f;
			if(timer01 < 1f ) {
				return change * 0.5f * timer01 * timer01 + startValue;
			}
			timer01--;
			return -change * 0.5f * (timer01 * (timer01 - 2f) - 1) + startValue;
		}

		public static float EaseInCubic(float startValue, float endValue, float timer01){
			float change = endValue - startValue;
			return change * timer01 * timer01 * timer01 + startValue;
		}

		public static float EaseOutCubic(float startValue, float endValue, float timer01){
			float change = endValue - startValue;
			timer01--;
			return change * (timer01 * timer01 * timer01 + 1f) + startValue;
		}

		public static float EaseInOutCubic(float startValue, float endValue, float timer01){
			float change = endValue - startValue;
			timer01 *= 2f;
			if( timer01 < 1f ) {
				return change * 0.5f * timer01 * timer01 * timer01 + startValue;
			}
			timer01 -= 2f;
			return change * 0.5f * (timer01 * timer01 * timer01 + 2f) + startValue;
		}

		public static float EaseInQuartic(float startValue, float endValue, float timer01 ) {
			float change = endValue - startValue;
			return change * timer01 * timer01 * timer01 * timer01 + startValue;
		}

		public static float EaseOutQuartic(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			timer01--;
			return -change * (timer01 * timer01 * timer01 * timer01 - 1.0f) + startValue;
		}

		public static float EaseInOutQuartic(float startValue, float endValue, float timer01 ) {
			float change = endValue - startValue;
			timer01 *= 2f;
			if(timer01 < 1f ) {
				return change * 0.5f * timer01 * timer01 * timer01 * timer01 + startValue;
			}
			timer01 -= 2f;
			return -change * 0.5f * (timer01 * timer01 * timer01 * timer01 - 2f) + startValue;
		}

		public static float EaseInQuintic(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			return change * timer01 * timer01 * timer01 * timer01 * timer01 + startValue;
		}

		public static float EaseOutQuintic(float startValue, float endValue, float timer01 ) {
			float change = endValue - startValue;
			timer01--;
			return change * (timer01 * timer01 * timer01 * timer01 * timer01 + 1f) + startValue;
		}

		public static float EaseInOutQuintic(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			timer01 *= 2f;
			if(timer01 < 1f) {
				return change * 0.5f *timer01 * timer01 * timer01 * timer01 * timer01 + startValue;
			}
			timer01 -= 2f;
			return change * 0.5f * (timer01 * timer01 * timer01 * timer01 * timer01 + 2f) + startValue;
		}

		public static float EaseInSin(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			return -change * Mathf.Cos(timer01 * Mathf.PI * 0.5f) + change + startValue;
		}

		public static float EaseOutSin(float startValue, float endValue, float timer01 ) {
			float change = endValue - startValue;
			return change * Mathf.Sin(timer01 * Mathf.PI * 0.5f) + startValue;
		}

		public static float EaseInOutSin(float startValue, float endValue, float timer01) {
			float change = endValue - startValue;
			return -change * 0.5f * (Mathf.Cos(timer01 * Mathf.PI) - 1f) + startValue;
		}
	}

	public enum EaseType {
		Linear,
		EaseInQuad ,
		EaseOutQuad,
		EaseInOutQuad,
		EaseInCubic,

		EaseOutCubic,

		EaseInOutCubic,

		EaseInQuartic,
		EaseOutQuartic,
		EaseInOutQuartic,

		EaseInQuintic,

		EaseOutQuintic,
		EaseInOutQuintic,

		EaseInSin,

		EaseOutSin,

		EaseInOutSin
	}

}