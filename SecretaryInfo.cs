namespace Bos {
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum SecretaryState { MoveToLoad, Loading, MoveToUnload, Unloading, Completed  }

    [System.Serializable]
    public class SecretaryInfoSave {
        public int generatorId;
        public int count;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public SecretaryState state;
        public float timer;
        //public AddInfoSave adSave;
    }


    public class SecretaryInfo : GameElement {
        private const int kMaxCountOfAds = 4;
        public int GeneratorId { get; private set; }
        public int Count { get; private set; }
        public SecretaryState State { get; private set; }
        private float timer = 0f;
        //private readonly AdInfo adInfo = new AdInfo(kMaxCountOfAds);

        public float Interval {
            get {
                return 1f / (Services.SecretaryService.GetSecretarySpeed(GeneratorId));
            }
        }



        private float MoveInterval
            => Interval * 0.2f;

        private float LoadInterval
            => Interval * 0.3f;

        private float UnloadInterval
            => Interval * 0.3f;

        public float Timer
            => timer;

        public SecretaryInfo(int generatorId)
            : this(generatorId, 0) { }

        public SecretaryInfo(int generatorId, int count) {
            GeneratorId = generatorId;
            Count = count;
            this.State = SecretaryState.MoveToLoad;
        }

        public SecretaryInfo(SecretaryInfoSave save) {
            GeneratorId = save.generatorId;
            Count = save.count;
            State = save.state;
            timer = save.timer;
            //if(save.adSave != null ) {
            //    adInfo.Load(save.adSave);
            //}
        }

        public void AddSecretary(int count) {
            Count += count;
        }

        public SecretaryInfoSave GetSave()
            => new SecretaryInfoSave {
                generatorId = GeneratorId,
                count = Count,
                state = State,
                timer = timer,
                //adSave = adInfo.GetSave()
            };

        private void AddOnSleep() {
            if(timer >= Interval ) {
                int cnt = (int)(timer / Interval);
                timer -= cnt * Interval;
                GameEvents.OnSecretaryWorkCircleCompleted(this, cnt);
            }
        }

        public void Update(float dt, ISpeedModifier speedMofifier ) {
            //adInfo.Update(dt);
            float deltaTime = dt * speedMofifier.GetSpeedMult(GeneratorId); //* adInfo.CurrentMultiplier;
            timer += deltaTime;
            //UnityEngine.Debug.Log($"interval => {Interval}");
            AddOnSleep();
            switch (State) {
                case SecretaryState.MoveToLoad: {
                        if (timer >= MoveInterval) {
                            ChangeState(SecretaryState.Loading);
                        }
                    }
                    break;
                case SecretaryState.Loading: {
                        if (timer >= LoadInterval) {
                            ChangeState(SecretaryState.MoveToUnload);
                        }
                    }
                    break;
                case SecretaryState.MoveToUnload: {
                        if (timer >= MoveInterval) {
                            ChangeState(SecretaryState.Unloading);
                        }
                    }
                    break;
                case SecretaryState.Unloading: {
                        if (timer >= UnloadInterval) {
                            ChangeState(SecretaryState.Completed);
                        }
                    }
                    break;
                case SecretaryState.Completed: {
                        if (timer >= 1) {
                            ChangeState(SecretaryState.MoveToLoad);
                        }
                    }
                    break;
            }
        }

        private void ChangeState(SecretaryState newState) {
            SecretaryState oldState = State;
            if (State != newState) {
                if (newState == SecretaryState.MoveToLoad) {
                    timer = 0;
                    State = SecretaryState.MoveToLoad;
                    GameEvents.OnSecretaryStateChanged(oldState, newState, this);

                } else if (newState == SecretaryState.Loading) {
                    timer -= MoveInterval;
                    State = SecretaryState.Loading;
                    GameEvents.OnSecretaryStateChanged(oldState, newState, this);
                    if (timer >= LoadInterval) {
                        ChangeState(SecretaryState.MoveToUnload);
                    }
                } else if (newState == SecretaryState.MoveToUnload) {
                    timer -= LoadInterval;
                    State = SecretaryState.MoveToUnload;
                    GameEvents.OnSecretaryStateChanged(oldState, newState, this);
                    if (timer >= MoveInterval) {
                        ChangeState(SecretaryState.Unloading);
                    }
                } else if (newState == SecretaryState.Unloading) {
                    timer -= MoveInterval;
                    State = SecretaryState.Unloading;
                    GameEvents.OnSecretaryStateChanged(oldState, newState, this);
                    if (timer > UnloadInterval) {
                        ChangeState(SecretaryState.Completed);
                    }
                } else if (newState == SecretaryState.Completed) {
                    State = SecretaryState.Completed;
                    timer = 0;
                    GameEvents.OnSecretaryStateChanged(oldState, newState, this);
                    GameEvents.OnSecretaryWorkCircleCompleted(this, 1);

                }
            }
        }

        public float NormalizedTimer {
            get {
                switch (State) {
                    case SecretaryState.MoveToUnload:
                    case SecretaryState.MoveToLoad: {
                            return Mathf.Clamp01(timer / MoveInterval);
                        }
                    case SecretaryState.Loading: {
                            return Mathf.Clamp01(timer / LoadInterval);
                        }
                    case SecretaryState.Unloading: {
                            return Mathf.Clamp01(timer / UnloadInterval);
                        }
                }
                return 0;
            }
        }

        //public bool IsAdValid
        //    => adInfo.IsValidForApply;

        //public void ApplyAd()
        //    => adInfo.Apply();

        /*
        public void UpdateSercretary(float deltaTime, IBosServiceCollection services) {
            int reportCount = services.SecretaryService.GetReportCount(GeneratorId);
            if (reportCount > 0 && Count > 0) {
                float speed = services.SecretaryService.GetSecretarySpeed(GeneratorId) * services.TimeChangeService.TimeMult;
                currentReportProcessCounter += speed * Count;
                ReportInfo report = services.SecretaryService.GetReportInfo(GeneratorId);
                while (currentReportProcessCounter >= 1.0f && report.ReportCount > 0) {
                    report.RemoveReports(1);
                    currentReportProcessCounter -= 1.0f;
                }
            } else {
                currentReportProcessCounter = 0;
            }
        }

        public SecretaryInfoSave GetSave()
            => new SecretaryInfoSave {
                generatorId = GeneratorId,
                count = Count,
                currentReportProcessCounter = currentReportProcessCounter
            };
            */

    }



}