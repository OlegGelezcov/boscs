namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlanetInfo {
        private IBosServiceCollection services;
        private IPlanetService planetService;

        public PlanetServerData Data { get; private set; }


        public int Id { get; private set; }

        public PlanetState State { get; private set; }
        public float UnlockTimer { get; private set; }
        public int StartTime { get; private set; }
        public int EndTime { get; private set; }

        private PlanetNameData planetLocalData = null;

        public PlanetNameData LocalData
            => planetLocalData ?? (planetLocalData = services.ResourceService.PlanetNameRepository.GetPlanetNameData(Id));

        public PlanetInfo(int id, IBosServiceCollection services) {
            this.Id = id;
            this.services = services;
            this.planetService = services.GetService<IPlanetService>();
            UpdateData();
        }


        public void SetStartTime(int time)
            => StartTime = time;

        public void SetEndTime(int time)
            => EndTime = time;

        public float UnlockTimerMult {
            get {
                return services.TimeChangeService.TimeMult;
            }
        }

        public double OpeningProgress {
            get {
                if (State == PlanetState.Opening) {
                    return 1.0f - BosMath.Clamp01(UnlockTimer/ Data.OpeningTime);
                }
                return 0.0f;
            }
        }

        public double OpeningRemaningTime {
            get {
                return UnlockTimer;
            }
        }



        public void RemoveFromUnlockTimer(float interval) {
            if (State == PlanetState.Opening) {
                UnlockTimer -= interval * UnlockTimerMult; 
            }
        }

        public void ApplySpeedMult() {
            UnlockTimer -= 30 * 60;
        }

        public void Update(float interval) {
            if(State == PlanetState.Opening) {
                RemoveFromUnlockTimer(interval);
                TryFinishOpening();
            }
        }

        public void Open() {
            if (State == PlanetState.Closed) {
                UnlockTimer = (float)Data.OpeningTime;
                SetState(PlanetState.Opening);
            }
        }

        private bool TryFinishOpening() {
            if (State == PlanetState.Opening) {
                if(UnlockTimer <= 0f ) {
                    SetState(PlanetState.ReadyToOpen);
                    return true;
                }
            }
            return false;
        }

        public void SetState(PlanetState newState) {
            PlanetState oldState = State;
            State = newState;
            if (oldState != State) {
                GameEvents.OnPlanetStateChanged(oldState, newState, this);             
            }
        }

        public void Load(PlanetSave save) {
            State = save.state;
            UnlockTimer = save.unlockTimer;
            StartTime = save.startTime;
            EndTime = save.endTime;
        }

        public void UpdateData()
            => this.Data = services.GetService<IResourceService>().Planets.GetPlanet(Id);

        public PlanetSave GetSave() {
            return new PlanetSave {
                id = Id,
                state = State,
                unlockTimer = UnlockTimer,
                startTime = StartTime,
                endTime = EndTime,
            };
        }

        public override string ToString() {
            return $"planet id: {Id}, state: {State}, unlock timer: {UnlockTimer}, data not null: {Data != null}";
        }
    }

    [Serializable]
    public class PlanetSave {
        public int id;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public PlanetState state;

        public float unlockTimer;
        public int startTime;
        public int endTime;
    }

}