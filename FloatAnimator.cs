namespace Bos {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System;
	using System.Linq;

	public class FloatAnimator : MonoBehaviour {
		private float timer;

		public bool IsBusy => !IsStarted;

		public List<BosFloatAnimationInfo> Datas {get;} = new List<BosFloatAnimationInfo>();

		private readonly List<System.Func<float, float, float, float>> easeFuncs =
    		new List<System.Func<float, float, float, float>>();

		private readonly List<List<AnimationEventInfo<float>>> events =
		 new List<List<AnimationEventInfo<float>>>();

		private readonly List<bool> isStartedStatus = new List<bool>();
        private bool isWaitingForStart = false;


        public bool IsStarted => isStartedStatus.Any(s => s);

		public void Stop() {
			foreach(var data in Datas) {
				data.Data.OnEnd?.Invoke(data.Data.EndValue, data.Data.Target);
			}
			Datas.Clear();
			for(int i = 0; i < isStartedStatus.Count; i++ ) {
				isStartedStatus[i] = false;
			}
			isStartedStatus.Clear();
		}

		public void StartAnimation(FloatAnimationData data) {
			StartAnimation(new List<FloatAnimationData> { data } );
		}

		public void StartAnimation(List<FloatAnimationData> datas ) {
            if (gameObject.activeSelf && gameObject.activeInHierarchy) {
                StartCoroutine(StartAnimationImpl(datas));
            }
		}

        private void MakeNotStarted() {
            for (int i = 0; i < isStartedStatus.Count; i++) {
                isStartedStatus[i] = false;
            }
        }

        private void HandleWaitingForStart() {
            if (isWaitingForStart) {
                MakeNotStarted();
                isWaitingForStart = false;
            }
        }

        private IEnumerator StartAnimationImpl(List<FloatAnimationData> datas) {
            if (isWaitingForStart) {
                yield break;
            }
            if (IsStarted) {
                isWaitingForStart = true;
            }
			yield return new WaitUntil(() => false == IsStarted);
            isWaitingForStart = false;
            
			if(!IsStarted) {
				Datas.Clear();
				foreach(var data in datas) {
					Datas.Add(new BosFloatAnimationInfo {
						Data = data,
						Direction = BosAnimationDirection.Forward
					});
				}
				timer = 0f;
				easeFuncs.Clear();
				events.Clear();

				for(int i = 0; i < Datas.Count; i++ ) {
					easeFuncs.Add(AnimUtils.GetEaseFunc(Datas[i].Data.EaseType));
					List<AnimationEventInfo<float>> dataEvents = new List<AnimationEventInfo<float>>();
					if(Datas[i].Data.Events != null ) {
						foreach(AnimationEvent<float> evt in Datas[i].Data.Events) {
							dataEvents.Add(new AnimationEventInfo<float> {
								Event = evt,
								IsCompleted = false
							});
						}
					}
					events.Add(dataEvents);
				}
				isStartedStatus.Clear();
				for(int i = 0; i < Datas.Count; i++ ) {
					isStartedStatus.Add(true);
				}
				Datas.ForEach(d => d.Data.OnStart?.Invoke(d.Data.StartValue, d.Data.Target));
			}
		}

		public void Update() {
			if(IsStarted) {
				timer += Time.deltaTime;

				for(int i = 0; i < Datas.Count; i++ ) {
					var data = Datas[i];
					float normTimer = Mathf.Clamp01(timer / data.Data.Duration);

					switch(data.Data.AnimationMode){
						case BosAnimationMode.Single: {
							if(normTimer < 1) {
								float x = easeFuncs[i](data.Data.StartValue, data.Data.EndValue, normTimer);
								data.Data.OnUpdate?.Invoke(x, normTimer, data.Data.Target);
								TryCompleteEvents(i, x, normTimer, data.Data);
							} else {
								TryCompleteEvents(i, data.Data.EndValue, normTimer, data.Data);
								data.Data.OnEnd?.Invoke(data.Data.EndValue, data.Data.Target);
								isStartedStatus[i] = false;
							}
						}
						break;
						case BosAnimationMode.Loop: {
							if(normTimer < 1) {
								float x = easeFuncs[i](data.Data.StartValue, data.Data.EndValue, normTimer);
								data.Data.OnUpdate?.Invoke(x, normTimer, data.Data.Target);
								TryCompleteEvents(i, x, normTimer, data.Data);								
							} else {
								TryCompleteEvents(i, data.Data.EndValue, normTimer, data.Data);
								data.Data.OnEnd?.Invoke(data.Data.EndValue, data.Data.Target);
								timer = 0f;
								data.Data.OnStart?.Invoke(data.Data.StartValue, data.Data.Target);
							}
                                HandleWaitingForStart();
						}
						break;
						case BosAnimationMode.PingPong: {
							if(normTimer < 1 ) {
								if(data.Direction == BosAnimationDirection.Forward) {
									float value = easeFuncs[i](data.Data.StartValue, data.Data.EndValue, normTimer);
									data.Data.OnUpdate?.Invoke(value, normTimer, data.Data.Target);
									TryCompleteEvents(i, value, normTimer, data.Data);
								} else {
									float value = easeFuncs[i](data.Data.EndValue, data.Data.StartValue, normTimer);
									data.Data.OnUpdate?.Invoke(value, normTimer, data.Data.Target);
                                    TryCompleteEvents(i, value, normTimer, data.Data);
								}
							} else {
								TryCompleteEvents(i, data.Data.EndValue, normTimer, data.Data);
                                timer = 0f;
                                if(data.Direction == BosAnimationDirection.Forward ) {
                                    data.Data.OnEnd?.Invoke(data.Data.EndValue, data.Data.Target);
                                    data.Direction = BosAnimationDirection.Backward;
                                } else {
                                    data.Data.OnStart?.Invoke(data.Data.StartValue, data.Data.Target);
                                    data.Direction = BosAnimationDirection.Forward;
                                }
							}
                                HandleWaitingForStart();
						}
						break;
					}
				}
			}
		}

		private void TryCompleteEvents(int index, float value, float normTimer, FloatAnimationData data) {
			List<AnimationEventInfo<float>> dataEvents = events[index];
			foreach(AnimationEventInfo<float> evt in dataEvents ) {
				if((evt.Event.Mode == AnimationEventMode.Single && !evt.IsCompleted) || (evt.Event.Mode == AnimationEventMode.Multiple)) {
					if(evt.Event.IsValid?.Invoke(value, normTimer, data.Target) ?? false) {
						evt.Event.OnEvent?.Invoke(value, normTimer, data.Target);
						evt.IsCompleted = true;
					}
				}				
			}
		}
	}

	public class FloatAnimationData {
		public float StartValue {get; set;}
		public float EndValue { get; set;}
		public float Duration {get; set;}
		public GameObject Target {get;set;}
		public EaseType EaseType{get; set;}

		public Action<float, GameObject> OnStart {get;set;}
		public Action<float, float, GameObject> OnUpdate {get; set;}
		public Action<float, GameObject> OnEnd {get; set;}
		public List<AnimationEvent<float>> Events {get; set;}

		public BosAnimationMode AnimationMode {get; set;} = BosAnimationMode.Single;
	}

	public class BosFloatAnimationInfo {
		public FloatAnimationData Data {get; set;}
		public BosAnimationDirection Direction {get; set;}
	}
}

