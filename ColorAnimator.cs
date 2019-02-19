namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class ColorAnimator : MonoBehaviour {
        private float timer;

        public List<BosColorAnimationInfo> Datas { get;  } = new List<BosColorAnimationInfo>();

        private readonly List<System.Func<float, float, float, float>> easeFuncs =
            new List<System.Func<float, float, float, float>>();

        private readonly List<bool> isStartedStatus = new List<bool>();

        public bool IsStarted
            => isStartedStatus.Any(s => s);

        public bool IsPaused { get; set; } = false;

        private readonly List<List<AnimationEventInfo<Color>>> events = new List<List<AnimationEventInfo<Color>>>();
        private bool isWaitingForStart = false;

        public void StartAnimation(ColorAnimationData data)
            => StartAnimation(new List<ColorAnimationData> { data });

        public void StartAnimation(List<ColorAnimationData> datas) 
            => StartCoroutine(StartAnimationImpl(datas));

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

        private IEnumerator StartAnimationImpl(List<ColorAnimationData> datas) {
            if (isWaitingForStart) {
                yield break;
            }
            if (IsStarted) {
                isWaitingForStart = true;
            }
            yield return new WaitUntil(() => !IsStarted);
            isWaitingForStart = false;

            if(!IsStarted) {
                Datas.Clear();
                foreach(var data in datas ) {
                    Datas.Add(new BosColorAnimationInfo {
                        Data = data,
                        Direction = BosAnimationDirection.Forward
                    });
                }
                timer = 0f;
                easeFuncs.Clear();
                events.Clear();
                for(int i = 0; i < Datas.Count; i++ ) {
                    easeFuncs.Add(AnimUtils.GetEaseFunc(Datas[i].Data.EaseType));
                    List<AnimationEventInfo<Color>> dataEvents = new List<AnimationEventInfo<Color>>();
                    if(Datas[i].Data.Events != null ) {
                        foreach(AnimationEvent<Color> evt in Datas[i].Data.Events) {
                            dataEvents.Add(new AnimationEventInfo<Color> {
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
            if (IsStarted && !IsPaused) {
                timer += Time.deltaTime;

                for(int i = 0; i < Datas.Count; i++ ) {
                    var data = Datas[i];

                    float normTimer = Mathf.Clamp01(timer / data.Data.Duration);
                    switch(data.Data.AnimationMode) {
                        case BosAnimationMode.Single: {
                            if(normTimer < 1 ) {
                                float r = easeFuncs[i](data.Data.StartValue.r, data.Data.EndValue.r, normTimer);
                                float g = easeFuncs[i](data.Data.StartValue.g, data.Data.EndValue.g, normTimer);
                                float b = easeFuncs[i](data.Data.StartValue.b, data.Data.EndValue.b, normTimer);
                                float a = easeFuncs[i](data.Data.StartValue.a, data.Data.EndValue.a, normTimer);
                                Color value = new Color(r, g, b, a);
                                data.Data.OnUpdate?.Invoke(value, normTimer, data.Data.Target);
                                TryCompleteEvents(i, value, normTimer, data.Data);
                            } else {
                                TryCompleteEvents(i, data.Data.EndValue, normTimer, data.Data);
                                data.Data.OnEnd?.Invoke(data.Data.EndValue, data.Data.Target);
                                isStartedStatus[i] = false;
                            }
                        }
                        break;
                        case BosAnimationMode.Loop: {
                            if(normTimer < 1 ) {
                                float r = easeFuncs[i](data.Data.StartValue.r, data.Data.EndValue.r, normTimer);
                                float g = easeFuncs[i](data.Data.StartValue.g, data.Data.EndValue.g, normTimer);
                                float b = easeFuncs[i](data.Data.StartValue.b, data.Data.EndValue.b, normTimer);
                                float a = easeFuncs[i](data.Data.StartValue.a, data.Data.EndValue.a, normTimer);
                                Color value = new Color(r, g, b, a);
                                data.Data.OnUpdate?.Invoke(value, normTimer, data.Data.Target);
                                TryCompleteEvents(i, value, normTimer, data.Data);
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
                                    if (data.Direction == BosAnimationDirection.Forward) {
                                        float r = easeFuncs[i](data.Data.StartValue.r, data.Data.EndValue.r, normTimer);
                                        float g = easeFuncs[i](data.Data.StartValue.g, data.Data.EndValue.g, normTimer);
                                        float b = easeFuncs[i](data.Data.StartValue.b, data.Data.EndValue.b, normTimer);
                                        float a = easeFuncs[i](data.Data.StartValue.a, data.Data.EndValue.a, normTimer);
                                        Color value = new Color(r, g, b, a);
                                        data.Data.OnUpdate?.Invoke(value, normTimer, data.Data.Target);
                                        TryCompleteEvents(i, value, normTimer, data.Data);
                                    } else {
                                        float r = easeFuncs[i](data.Data.EndValue.r, data.Data.StartValue.r, normTimer);
                                        float g = easeFuncs[i](data.Data.EndValue.g, data.Data.StartValue.g, normTimer);
                                        float b = easeFuncs[i](data.Data.EndValue.b, data.Data.StartValue.b, normTimer);
                                        float a = easeFuncs[i](data.Data.EndValue.a, data.Data.StartValue.a, normTimer);
                                        Color value = new Color(r, g, b, a);
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

        private void TryCompleteEvents(int index, Color value, float normTimer, ColorAnimationData Data) {
            List<AnimationEventInfo<Color>> dataEvents = events[index];
            foreach (AnimationEventInfo<Color> evt in dataEvents) {
                if ((evt.Event.Mode == AnimationEventMode.Single && !evt.IsCompleted)
                    || (evt.Event.Mode == AnimationEventMode.Multiple)) {
                    if (evt.Event.IsValid?.Invoke(value, normTimer, Data.Target) ?? false) {
                        evt.Event.OnEvent?.Invoke(value, normTimer, Data.Target);
                        evt.IsCompleted = true;
                    }
                }
            }
        }

        public void Stop() {
            foreach (var data in Datas) {
                data.Data.OnEnd?.Invoke(data.Data.EndValue, data.Data.Target);
            }
            Datas.Clear();
            for (int i = 0; i < isStartedStatus.Count; i++) {
                isStartedStatus[i] = false;
            }
            isStartedStatus.Clear();
        }
    }

    public class ColorAnimationData {
        public Color StartValue { get; set; }
        public Color EndValue { get; set; }
        public float Duration { get; set; }

        public GameObject Target { get; set; }

        public EaseType EaseType { get; set; }

        public System.Action<Color, GameObject> OnStart { get; set; }
        public System.Action<Color, float, GameObject> OnUpdate { get; set; }

        public System.Action<Color, GameObject> OnEnd { get; set; }

        public List<AnimationEvent<Color>> Events { get; set; }

        public BosAnimationMode AnimationMode { get; set; } = BosAnimationMode.Single;
    }


    public class BosColorAnimationInfo {
        public ColorAnimationData Data { get; set;}
        public BosAnimationDirection Direction { get; set; }
    }
}