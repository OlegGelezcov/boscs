using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bos;
using UnityEngine;

public class Vector3Animator : MonoBehaviour {

	//private bool isStarted = false;
	private float timer;

	public bool IsBusy
		=> !IsStarted;

    public List<BosVector3AnimationInfo> Datas { get; private set; } = new List<BosVector3AnimationInfo>();

    private readonly List<System.Func<float, float, float, float>> easeFuncs =
    new List<System.Func<float, float, float, float>>();

    private readonly List<List<AnimationEventInfo<Vector3>>> events = new List<List<AnimationEventInfo<Vector3>>>();

    private readonly List<bool> isStartedStatus = new List<bool>();
    private bool isWaitingForStart = false;


    public bool IsStarted
        => isStartedStatus.Any(s => s);

    //private BosAnimationDirection direction = BosAnimationDirection.Forward;

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

	public void StartAnimation(Vector3AnimationData data) {
        StartAnimation(new List<Vector3AnimationData> { data });
	}

    public void StartAnimation(List<Vector3AnimationData> datas ) {
        StartCoroutine(StartAnimationImpl(datas));
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


    private IEnumerator StartAnimationImpl(List<Vector3AnimationData> datas) {
        if(isWaitingForStart) {
            yield break;
        }
        if (IsStarted) {
            isWaitingForStart = true;
        }
        yield return new WaitUntil(() => false == IsStarted);
        isWaitingForStart = false;
        if(!IsStarted) {
            //this.Datas = datas;
            Datas.Clear();
            foreach(var data in datas) {
                Datas.Add(new BosVector3AnimationInfo { Data = data, Direction = BosAnimationDirection.Forward });
            }
            timer = 0f;
            easeFuncs.Clear();
            events.Clear();
            for(int i = 0; i < Datas.Count; i++ ) {
                easeFuncs.Add(AnimUtils.GetEaseFunc(Datas[i].Data.EaseType));
                List<AnimationEventInfo<Vector3>> dataEvents = new List<AnimationEventInfo<Vector3>>();
                if(Datas[i].Data.Events != null ) {
                    foreach(AnimationEvent<Vector3> evt in Datas[i].Data.Events) {
                        dataEvents.Add(new AnimationEventInfo<Vector3> {
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

                switch(data.Data.AnimationMode) {
                    case BosAnimationMode.Single: {
                            if (normTimer < 1) {
                                float x = easeFuncs[i](data.Data.StartValue.x, data.Data.EndValue.x, normTimer);
                                float y = easeFuncs[i](data.Data.StartValue.y, data.Data.EndValue.y, normTimer);
                                float z = easeFuncs[i](data.Data.StartValue.z, data.Data.EndValue.z, normTimer);
                                Vector3 value = new Vector3(x, y, z);
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
                            if (normTimer < 1) {
                                float x = easeFuncs[i](data.Data.StartValue.x, data.Data.EndValue.x, normTimer);
                                float y = easeFuncs[i](data.Data.StartValue.y, data.Data.EndValue.y, normTimer);
                                float z = easeFuncs[i](data.Data.StartValue.z, data.Data.EndValue.z, normTimer);
                                Vector3 value = new Vector3(x, y, z);
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
                            if (normTimer < 1) {
                                if (data.Direction == BosAnimationDirection.Forward) {
                                    float x = easeFuncs[i](data.Data.StartValue.x, data.Data.EndValue.x, normTimer);
                                    float y = easeFuncs[i](data.Data.StartValue.y, data.Data.EndValue.y, normTimer);
                                    float z = easeFuncs[i](data.Data.StartValue.z, data.Data.EndValue.z, normTimer);
                                    Vector3 value = new Vector3(x, y, z);
                                    data.Data.OnUpdate?.Invoke(value, normTimer, data.Data.Target);
                                    TryCompleteEvents(i, value, normTimer, data.Data);
                                } else {
                                    float x = easeFuncs[i](data.Data.EndValue.x, data.Data.StartValue.x, normTimer);
                                    float y = easeFuncs[i](data.Data.EndValue.y, data.Data.StartValue.y, normTimer);
                                    float z = easeFuncs[i](data.Data.EndValue.z, data.Data.StartValue.z, normTimer);
                                    Vector3 value = new Vector3(x, y, z);
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

    private void TryCompleteEvents(int index, Vector3 value, float normTimer, Vector3AnimationData data ) {
        List<AnimationEventInfo<Vector3>> dataEvents = events[index];
        foreach(AnimationEventInfo<Vector3> evt in dataEvents ) {
            if((evt.Event.Mode == AnimationEventMode.Single && !evt.IsCompleted) || (evt.Event.Mode == AnimationEventMode.Multiple)) {
                if(evt.Event.IsValid?.Invoke(value, normTimer, data.Target) ?? false) {
                    evt.Event.OnEvent?.Invoke(value, normTimer, data.Target);
                    evt.IsCompleted = true;
                }
            }
        }
    }
}

public class Vector3AnimationData {
	public Vector3 StartValue{get; set;}
	public Vector3 EndValue {get; set;}
	public float Duration {get; set;}
    public GameObject Target { get; set; }

	public EaseType EaseType { get; set;}

	public System.Action<Vector3, GameObject> OnStart{ get; set;}
	public System.Action<Vector3, float, GameObject> OnUpdate {get; set;}

	public System.Action<Vector3, GameObject> OnEnd { get; set;}

    public List<AnimationEvent<Vector3>> Events { get; set; }

    public BosAnimationMode AnimationMode { get; set; } = BosAnimationMode.Single;
}

public class BosVector3AnimationInfo {
    public Vector3AnimationData Data { get; set; }
    public BosAnimationDirection Direction { get; set; }
}

public enum BosAnimationMode {
    Single,
    PingPong,
    Loop
}

public enum BosAnimationDirection {
    Forward,
    Backward
}
