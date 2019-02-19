namespace Bos {
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum AuditorState {
        None,
        MoveToLoad,
        Loading,
        MoveToUnload,
        Unloading,
        Completed
    }

    [System.Serializable]
    public class AuditorSave {
        public string id;
        public int generatorId;
        public int count;
        public float interval;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public AuditorState state;
        public float timer;
        public float totalTimer;
        public float workInterval;
        public float workTimer;

    }

    public class Auditor : GameElement {

        private float timer = 0f;
        private float totalTimer = 0f;

        /// <summary>
        /// Interval of repair reports
        /// </summary>
        private float WorkInterval { get; set; }

        /// <summary>
        /// Timer for repair reports
        /// </summary>
        private float WorkTimer { get; set; }

        /// <summary>
        /// Unique ID of auditor
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Target generator ID for audit
        /// </summary>
        public int GeneratorId { get; private set; }

        //public int Count { get; private set; }

        /// <summary>
        /// Interval of animation
        /// </summary>
        public float Interval { get; private set; }

        /// <summary>
        /// Current animation state
        /// </summary>
        public AuditorState State { get; private set; } = AuditorState.None;


        /// <summary>
        /// Move animation interval
        /// </summary>
        private float MoveInterval
            => Interval * 0.2f;

        /// <summary>
        /// Load animation interval
        /// </summary>
        private float LoadInterval
            => Interval * 0.3f;

        /// <summary>
        /// Unload animation interval
        /// </summary>
        private float UnloadInterval
            => Interval * 0.3f;

        /// <summary>
        /// Animation timer
        /// </summary>
        public float Timer
            => timer;

        /// <summary>
        /// Total animation progress
        /// </summary>
        public float TotalProgress
            => Mathf.Clamp01(totalTimer / Interval);

        /// <summary>
        /// Normalized animation timer for each state normalized in range (0, 1)
        /// </summary>
        public float NormalizedTimer {
            get {
                switch (State) {
                    case AuditorState.MoveToUnload:
                    case AuditorState.MoveToLoad: {
                            return Mathf.Clamp01(timer / MoveInterval);
                        }
                    case AuditorState.Loading: {
                            return Mathf.Clamp01(timer / LoadInterval);
                        }
                    case AuditorState.Unloading: {
                            return Mathf.Clamp01(timer / UnloadInterval);
                        }
                }
                return 0;
            }
        }

        public int RemainCount => Mathf.FloorToInt(WorkInterval / 2);

        public Auditor(int generatorId, int count, float speed) {
            Id = System.Guid.NewGuid().ToString();
            GeneratorId = generatorId;
            //Count = count;
            /*
            Interval = count / speed;
            if (Interval < 10) {
                Interval = 10;
            }*/
            Interval = 10f;
            WorkInterval = count * 2;
            WorkTimer = 0f;
            State = AuditorState.None;
            totalTimer = 0f;
        }

        public Auditor(AuditorSave save ) {
            this.Id = save.id;
            this.GeneratorId = save.generatorId;
            //this.Count = save.count;
            this.Interval = 10.0f; //save.interval;
            this.State = save.state;
            this.timer = save.timer;
            this.totalTimer = save.totalTimer;
            this.WorkInterval = save.workInterval;
            this.WorkTimer = save.workTimer;
            //if(save.adSave != null ) {
            //    adInfo.Load(save.adSave);
            //}
        }

        public AuditorSave GetSave()
            => new AuditorSave {
                id = Id,
                //count = Count,
                generatorId = GeneratorId,
                interval = Interval,
                state = State,
                timer = timer,
                totalTimer = totalTimer,
                workInterval = WorkInterval,
                workTimer = WorkTimer
            };

        public void StartAuditor()
            => ChangeState(AuditorState.MoveToLoad);




        private bool UpdateWorkTimer(float deltaTime ) {
            WorkTimer += deltaTime;
            if(WorkTimer >= 2.0f ) {
                WorkTimer = Mathf.Clamp(WorkTimer, 0f, WorkInterval + 1f);
                int countToRepair = Mathf.FloorToInt(WorkTimer / 2.0f);
                if(countToRepair > 0 ) {
                    WorkInterval -= countToRepair * 2.0f;
                    WorkTimer -= countToRepair * 2.0f;
                    GameEvents.AuditorReportHandleObservable.OnNext(new GameEvents.AuditorRepairInfo {
                        Auditor = this,
                        RepairCount = countToRepair
                    });
                    if(WorkInterval <= 0f ) {
                        ChangeState(AuditorState.Completed);
                        return true;
                    }
                }
            }
            return false;
        }
        public void Update(float dt, ISpeedModifier speedModifier) {

            float deltaTime = dt * speedModifier.GetSpeedMult(GeneratorId); 
            totalTimer += deltaTime;

            if(UpdateWorkTimer(deltaTime)) {
                return;
            }

            switch(State) {
                case AuditorState.MoveToLoad: {
                        timer += deltaTime;
                        if (timer >= MoveInterval) {
                            ChangeState(AuditorState.Loading);
                        }
                    }
                    break;
                case AuditorState.Loading: {
                        timer += deltaTime;
                        if (timer >= LoadInterval) {
                            ChangeState(AuditorState.MoveToUnload);
                        }
                    }
                    break;
                case AuditorState.MoveToUnload: {
                        timer += deltaTime;
                        if (timer >= MoveInterval) {
                            ChangeState(AuditorState.Unloading);
                        }
                    }
                    break;
                case AuditorState.Unloading: {
                        timer += deltaTime;
                        if (timer >= UnloadInterval) {
                            ChangeState(AuditorState.MoveToLoad);
                        }
                    }
                    break;
                case AuditorState.Completed: {

                    }
                    break;
            }
        }

        private void ChangeState(AuditorState newState) {
            AuditorState oldState = State;
            if (State != newState) {
                if (newState == AuditorState.MoveToLoad) {
                    timer = 0f;
                    State = AuditorState.MoveToLoad;
                    GameEvents.OnAuditorStateChanged(oldState, State, this);
                } else if (newState == AuditorState.Loading) {
                    timer -= MoveInterval;
                    State = AuditorState.Loading;
                    GameEvents.OnAuditorStateChanged(oldState, State, this);
                    if (timer >= LoadInterval) {
                        ChangeState(AuditorState.MoveToUnload);
                    }
                } else if (newState == AuditorState.MoveToUnload) {
                    timer -= LoadInterval;
                    State = AuditorState.MoveToUnload;
                    GameEvents.OnAuditorStateChanged(oldState, State, this);
                    if (timer >= MoveInterval) {
                        ChangeState(AuditorState.Unloading);
                    }
                } else if (newState == AuditorState.Unloading) {
                    timer -= MoveInterval;
                    State = AuditorState.Unloading;
                    GameEvents.OnAuditorStateChanged(oldState, State, this);
                    if (timer > UnloadInterval) {
                        ChangeState(AuditorState.Completed);
                    }
                } else if (newState == AuditorState.Completed) {
                    State = AuditorState.Completed;
                    GameEvents.OnAuditorStateChanged(oldState, State, this);
                }

            }
        }

        public bool IsCompleted
            => (State == AuditorState.Completed);


    }


}