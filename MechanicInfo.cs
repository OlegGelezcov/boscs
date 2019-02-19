namespace Bos {
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class MechanicInfoSave {
        public int id;
        public int count;
        //public float interval;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public MechanicState state;
        public float timer;
        //public AddInfoSave adSave;
    }

    public class MechanicInfo : GameElement {
        private const int kMaxCountOfAds = 4;

        public int Id { get; private set; }
        public int Count { get; private set; }
        public float Interval {
            get {
                float val = 1f / Services.MechanicService.ServiceSpeed;
                if(val < 10 ) {
                    val = 10;
                }
                return val;
            }
        }
        public MechanicState State { get; private set; }
       //private readonly AdInfo adInfo = new AdInfo(kMaxCountOfAds);
        private float timer = 0f;


        private float MoveInterval
            => Interval * 0.2f;

        private float LoadInterval
            => Interval * 0.3f;

        private float UnloadInterval
            => Interval * 0.3f;

        public float Timer
            => timer;

        public void AddMechanic(int cnt) {
            int oldCount = Count;
            Count += cnt;
            if(oldCount != Count ) {
                GameEvents.OnMechanicAdded(this);
            }
        }

        public MechanicInfo(int id, int count) {
            this.Id = id;
            this.Count = count;
            //this.Interval = 1f / speed;
            //if (Interval < 10) {
            //    Interval = 10;
            //}
            this.State = MechanicState.MoveToLoad;
        }

        public MechanicInfo(MechanicInfoSave save ) {
            this.Id = save.id;
            this.Count = save.count;
            //this.Interval = save.interval;
            this.State = save.state;
            this.timer = save.timer;
            //if(save.adSave != null ) {
            //    adInfo.Load(save.adSave);
            //}
        }

        public MechanicInfoSave GetSave()
            => new MechanicInfoSave {
                id = Id,
                count = Count,
                //interval = Interval,
                state = State,
                timer = timer,
                //adSave = adInfo.GetSave()
            };

        private void AddOnSleep() {
            if (timer >= Interval) {
                int cnt = (int)(timer / Interval);
                timer -= cnt * Interval;
                GameEvents.OnMechanicWorkCircleCompleted(this, cnt);
            }
        }

        public void Update(float dt, ISpeedModifier speedModifier) {
            //adInfo.Update(dt);
            float deltaTime = dt * speedModifier.GetSpeedMult(Id); //* adInfo.CurrentMultiplier;
            timer += deltaTime;

            AddOnSleep();

            switch (State) {
                case MechanicState.MoveToLoad: {                     
                        if(timer >= MoveInterval) {
                            ChangeState(MechanicState.Loading);
                        }
                    }
                    break;
                case MechanicState.Loading: {
                        if (timer >= LoadInterval) {
                            ChangeState(MechanicState.MoveToUnload);
                        }
                    }
                    break;
                case MechanicState.MoveToUnload: {
                        if (timer >= MoveInterval) {
                            ChangeState(MechanicState.Unloading);
                        }
                    }
                    break;
                case MechanicState.Unloading: {
                        if (timer >= UnloadInterval) {
                            ChangeState(MechanicState.Completed);
                        }
                    }
                    break;
                case MechanicState.Completed: {
                        if (timer >= 1) {
                            ChangeState(MechanicState.MoveToLoad);
                        }
                    }
                    break;
            }
        }

        private void ChangeState(MechanicState newState) {
            MechanicState oldState = State;
            if(State != newState ) {
                if(newState == MechanicState.MoveToLoad ) {
                    timer = 0;
                    State = MechanicState.MoveToLoad;
                    GameEvents.OnMechanicStateChanged(oldState, newState, this);
                    
                } else if (newState == MechanicState.Loading) {
                    timer -= MoveInterval;
                    State = MechanicState.Loading;
                    GameEvents.OnMechanicStateChanged(oldState, newState, this);
                    if(timer >= LoadInterval ) {
                        ChangeState(MechanicState.MoveToUnload);
                    }
                } else if(newState == MechanicState.MoveToUnload ) {
                    timer -= LoadInterval;
                    State = MechanicState.MoveToUnload;
                    GameEvents.OnMechanicStateChanged(oldState, newState, this);
                    if (timer >= MoveInterval) {
                        ChangeState(MechanicState.Unloading);
                    }
                } else if(newState == MechanicState.Unloading ) {
                    timer -= MoveInterval;
                    State = MechanicState.Unloading;
                    GameEvents.OnMechanicStateChanged(oldState, newState, this);
                    if(timer > UnloadInterval ) {
                        ChangeState(MechanicState.Completed);
                    }
                } else if(newState == MechanicState.Completed ) {
                    State = MechanicState.Completed;
                    timer = 0;
                    GameEvents.OnMechanicStateChanged(oldState, newState, this);
                    GameEvents.OnMechanicWorkCircleCompleted(this, 1);
                    
                }
            } 
        }

        public float NormalizedTimer {
            get {
                switch (State) {
                    case MechanicState.MoveToUnload:
                    case MechanicState.MoveToLoad: {
                            return Mathf.Clamp01(timer / MoveInterval);
                        }
                    case MechanicState.Loading: {
                            return Mathf.Clamp01(timer / LoadInterval);
                        }
                    case MechanicState.Unloading: {
                            return Mathf.Clamp01(timer / UnloadInterval);
                        }
                }
                return 0;
            }
        }

        /*
        public bool IsAdValid
            => adInfo.IsValidForApply;

        public void ApplyAd()
            => adInfo.Apply();*/
    }

    public enum MechanicState { MoveToLoad, Loading, MoveToUnload, Unloading, Completed }
}