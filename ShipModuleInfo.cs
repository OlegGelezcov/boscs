namespace Bos {
    using Bos.Data;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ShipModuleInfo : GameElement {

        public int Id { get; private set; }

        public ShipModuleState State { get; private set; }
        public int OpenTime { get; private set; }

        private ShipModuleData data = null;

        public ShipModuleData Data {
            get {
                if (data == null) {
                    data = Services.ResourceService.ShipModuleRepository.GetModule(Id);
                }
                return data;
            }
        }

        public void SetState(ShipModuleState newState) {
            var oldState = State;
            State = newState;

            if (oldState != newState) {
                if(newState == ShipModuleState.Opened) {
                    OpenTime = Services.TimeService.UnixTimeInt;
                }
                GameEvents.OnShipModuleStateChanged(oldState, State, this);
            }
        }

        public ShipModuleInfo(int id) {
            Id = id;
            State = ShipModuleState.Locked;
            OpenTime = 0;
        }

        public ShipModuleInfo(ShipModuleSave save) {
            Id = save.id;
            State = save.state;
            OpenTime = save.openTime;
        }

        public ShipModuleSave GetSave()
            => new ShipModuleSave { id = Id, state = State, openTime = OpenTime };

        public double Price
            => Data.Currency.Value;

        public CurrencyType CurrencyType
            => Data.Currency.Type;
    }

    [Serializable]
    public class ShipModuleSave {

        public int id;
        public int openTime;

        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public ShipModuleState state;
    }


    public enum ShipModuleState {
        Locked,
        Available,
        Opened
    }

}