namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class SaveableGameBehaviour : GameBehaviour, ISaveable {



        public virtual void Register() {
            Services.SaveService?.Register(this);
        }

        public abstract void ResetFull();

        public abstract void ResetByInvestors();

        public abstract void ResetByPlanets();

        public abstract void ResetByWinGame();

        public abstract string SaveKey { get; }

        public  bool IsLoaded { get; protected set; }

        public abstract Type SaveType { get; }

        public abstract object GetSave();

        public abstract void LoadDefaults();

        public abstract void LoadSave(object obj);
    }

}