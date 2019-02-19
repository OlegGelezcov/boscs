/*
namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class TransportManagerService : SaveableGameBehaviour, ITransportManagerService {


        public Dictionary<int, bool> HiredManagers { get; } = new Dictionary<int, bool>();

        #region ITransportManagerService implementation
        public void Setup(object data = null) {
            
        }

        //public void Hire(int generatorId) {
        //    bool isHiredOld = IsHired(generatorId);
        //    if(false == isHiredOld) {
        //        HiredManagers[generatorId] = true;
        //        GameEvents.OnTransportManagerHired(generatorId);
        //    }
        //}

        //public bool IsHired(int generatorId) {
        //    return HiredManagers.ContainsKey(generatorId) && (true == HiredManagers[generatorId]);
        //}

        //public int HiredCount
        //    => HiredManagers.Where(kvp => kvp.Value).Count();

        #endregion


        #region SaveableGameBehaviour overrides
        public override string SaveKey => "transport_manager_service";

        public override Type SaveType => typeof(TransportManagerServiceSave);

        public override object GetSave() {
            return new TransportManagerServiceSave {
                hiredManagers = HiredManagers
            };
        }



        public override void LoadDefaults() {
            HiredManagers.Clear();
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            TransportManagerServiceSave save = obj as TransportManagerServiceSave;
            if(save != null ) {
                if(save.hiredManagers != null) {
                    HiredManagers.Clear();
                    foreach(var kvp in save.hiredManagers) {
                        HiredManagers[kvp.Key] = kvp.Value;
                    }
                    IsLoaded = true;
                } else {
                    LoadDefaults();
                }
            } else {
                LoadDefaults();
            }
        }


        #endregion
    }

    public interface ITransportManagerService : IGameService {
        bool IsHired(int generatorId);
        void Hire(int generatorId);
        int HiredCount { get; }
    }

    [Serializable]
    public class TransportManagerServiceSave {
        public Dictionary<int, bool> hiredManagers;
    }
}*/
