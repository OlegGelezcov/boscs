namespace Bos {
    using Newtonsoft.Json;
    using System;
    using UnityEngine;



    public class TempMechanicInfo : GameElement {

        /// <summary>
        /// Temp mechanic info ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Target generator id
        /// </summary>
        public int GeneratorId { get; private set; }

        /// <summary>
        /// Count to repair
        /// </summary>
        //public int Count { get; private set; }

        /// <summary>
        /// Interval to repair
        /// </summary>
        public float Interval { get; private set; }

        /// <summary>
        /// Current mechanic animation state
        /// </summary>
        public TempMechanicState State { get; private set; } = TempMechanicState.None;

        private float totalTimer = 0f;

        private float timer = 0f;

        private float MoveInterval
            => Interval * 0.2f;

        private float LoadInterval
            => Interval * 0.3f;

        private float UnloadInterval
            => Interval * 0.3f;

        public float Timer
            => timer;

        public float TotalProgress
           => Mathf.Clamp01(totalTimer / Interval);

        
        private float WorkInterval { get; set; }
        private float workTimer { get; set; }

        public int RemainCount
            => Mathf.FloorToInt(WorkInterval * 0.5f);

        public float NormalizedTimer {
            get {
                switch(State) {
                    case TempMechanicState.MoveToUnload:
                    case TempMechanicState.MoveToLoad: {
                            return Mathf.Clamp01(timer / MoveInterval);
                        }
                    case TempMechanicState.Loading: {
                            return Mathf.Clamp01(timer / LoadInterval);
                        }
                    case TempMechanicState.Unloading: {
                            return Mathf.Clamp01(timer / UnloadInterval);
                        }
                }
                return 0;
            }
        }

        public TempMechanicInfo(int generatorId, int count, float speed) {
            this.Id = Guid.NewGuid().ToString();
            this.GeneratorId = generatorId;
            WorkInterval = count * 2;
            Interval = 10f;
            State = TempMechanicState.None;
            totalTimer = 0f;
        }

        public void StartMechanic()
            => ChangeState(TempMechanicState.MoveToLoad);


        public TempMechanicInfo(TempMechanicInfoSave save) {
            this.Id = save.id;
            this.GeneratorId = save.generatorId;
            //this.Count = save.count;
            this.Interval = 10.0f;//save.interval;
            this.State = save.state;
            this.timer = save.timer;
            this.totalTimer = save.totalTimer;
            this.WorkInterval = save.workInterval;
            this.workTimer = save.workTimer;
        }

        public TempMechanicInfoSave GetSave()
            => new TempMechanicInfoSave {
                id = Id,
                generatorId = GeneratorId,
                interval = Interval,
                state = State,
                timer = timer,
                workInterval = WorkInterval,
                workTimer = workTimer
            };

        public void Update(float dt, ISpeedModifier speedModifier) {

            float deltaTime = dt * speedModifier.GetSpeedMult(GeneratorId);
            totalTimer += deltaTime;

            workTimer += deltaTime;
            if(workTimer >= 2.0f ) {
                workTimer = Mathf.Clamp(workTimer, 0, WorkInterval + 1);
                int countToRepair = Mathf.FloorToInt(workTimer / 2.0f);
                if(countToRepair > 0 ) {
                    WorkInterval -= countToRepair * 2.0f;
                    workTimer -= countToRepair * 2.0f;
                    GameEvents.TempMechanicRepairedTransportObservable.OnNext(new GameEvents.TempMechanicRepairInfo {
                        Mechanic = this,
                        RepairCount = countToRepair
                    });
                    if(WorkInterval <= 0f ) {
                        ChangeState(TempMechanicState.Completed);
                        return;
                    }
                }
            }

            switch(State) {
                case TempMechanicState.MoveToLoad: {
                        timer += deltaTime;
                        if(timer >= MoveInterval ) {
                            ChangeState(TempMechanicState.Loading);
                        }
                    }
                    break;
                case TempMechanicState.Loading: {
                        timer += deltaTime;
                        if(timer >= LoadInterval ) {
                            ChangeState(TempMechanicState.MoveToUnload);
                        }
                    }
                    break;
                case TempMechanicState.MoveToUnload: {
                        timer += deltaTime;
                        if(timer >= MoveInterval) {
                            ChangeState(TempMechanicState.Unloading);
                        }
                    }
                    break;
                case TempMechanicState.Unloading: {
                        timer += deltaTime;
                        if(timer >= UnloadInterval) {
                            timer = 0;
                            ChangeState(TempMechanicState.MoveToLoad);
                        }
                    }
                    break;
                case TempMechanicState.Completed: {

                    }
                    break;
            }
        }

        private void ChangeState(TempMechanicState newState ) {
            TempMechanicState oldState = State;
            if(State != newState) {
                if(newState == TempMechanicState.MoveToLoad) {
                    timer = 0f;
                    State = TempMechanicState.MoveToLoad;
                    GameEvents.OnTempMechanicInfoStateChanged(oldState, State, this);
                } else if(newState == TempMechanicState.Loading ) {
                    timer -= MoveInterval;
                    State = TempMechanicState.Loading;
                    GameEvents.OnTempMechanicInfoStateChanged(oldState, State, this);
                    if (timer >= LoadInterval) {
                        ChangeState(TempMechanicState.MoveToUnload);
                    }
                } else if(newState == TempMechanicState.MoveToUnload ) {
                    timer -= LoadInterval;
                    State = TempMechanicState.MoveToUnload;
                    GameEvents.OnTempMechanicInfoStateChanged(oldState, State, this);
                    if(timer >= MoveInterval) {
                        ChangeState(TempMechanicState.Unloading);
                    }
                } else if(newState == TempMechanicState.Unloading) {
                    timer -= MoveInterval;
                    State = TempMechanicState.Unloading;
                    GameEvents.OnTempMechanicInfoStateChanged(oldState, State, this);
                    if (timer > UnloadInterval) {
                        //ChangeState(TempMechanicState.Completed);
                        ChangeState(TempMechanicState.MoveToLoad);
                    }               
                } else if(newState == TempMechanicState.Completed ) {
                    State = TempMechanicState.Completed;
                    GameEvents.OnTempMechanicInfoStateChanged(oldState, State, this);
                }
                
            }
        }

        public bool IsCompleted
            => (State == TempMechanicState.Completed);
    }

    public enum TempMechanicState {
        None,
        MoveToLoad,
        Loading,
        MoveToUnload,
        Unloading,
        Completed
    }

    [System.Serializable]
    public class TempMechanicInfoSave {
        public string id;
        public int generatorId;
        public int count;
        public float interval;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public TempMechanicState state;
        public float timer;
        public float totalTimer;

        public float workInterval;
        public float workTimer;
    }

}