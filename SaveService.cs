using System.Linq;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;
    using System;
    using UDebug = UnityEngine.Debug;
    using Bos.Debug;

    public class SaveService : GameBehaviour, ISaveService {

        private readonly Dictionary<Type, ISaveable> registeredSaveables = new Dictionary<Type, ISaveable>();
        private IStorage storage;

        public void Setup(object data = null) {
            if(data == null || !(data is IStorage)) {
                throw new UnityException($"invalid setup service data...");
            }
            storage = data as IStorage;
        }

        public void UpdateResume(bool pause)
            => UDebug.Log($"{nameof(SaveService)}.{nameof(UpdateResume)}() => {pause}");


        public void Register(ISaveable saveable) {
            registeredSaveables[saveable.GetType()] = saveable;
            if(!saveable.IsLoaded) {
                Load(saveable);
            }
        }

        public bool IsRegistered(ISaveable saveable) {
            return registeredSaveables.ContainsKey(saveable.GetType());
        }

        public void Load(ISaveable saveable) {
            if(IsRegistered(saveable)) {
                string saveString = storage?.GetString(saveable.SaveKey, string.Empty);
                if(string.IsNullOrEmpty(saveString)) {
                    saveable.LoadDefaults();
                } else {
                    try {
                        object save = JsonConvert.DeserializeObject(saveString, saveable.SaveType);
                        saveable.LoadSave(save);
                    } catch(System.Exception exception) {
                        UDebug.LogError(exception.Message);
                        saveable.LoadDefaults();
                    }
                }
            } else {
                throw new UnityException($"Load: saveable of type => {saveable.GetType().Name} is not registered on ISaveService");
            }
        }

        public void Save(ISaveable saveable, bool flush = true) {
            if(IsRegistered(saveable)) {
                if (saveable.IsLoaded) {
                    try {
                        string saveString = JsonConvert.SerializeObject(saveable.GetSave());
                        storage?.SetString(saveable.SaveKey, saveString, flush);
                        //UDebug.Log($"SAVED: {saveable.SaveKey} => {saveString}".Colored(ConsoleTextColor.magenta));
                    } catch (System.Exception exception) {
                        Services.GetService<IConsoleService>().AddOutput($"exception when save {saveable.SaveType.Name}", ConsoleTextColor.red, true);
                        Services.GetService<IConsoleService>().AddOutput(exception.Message, ConsoleTextColor.red, true);
                        Services.GetService<IConsoleService>().AddOutput(exception.StackTrace, ConsoleTextColor.red, true);
                    }
                    
                }
            } else {
                throw new UnityException($"Save: saveable of type => {saveable.GetType().Name} is not registered on ISaveService");
            }
        }

        public void SaveAll() {
            foreach(var pair in registeredSaveables ) {
                if(pair.Value != null ) {
                    Save(pair.Value, flush: false);
                    //UDebug.Log($"Saved in  => {pair.Key.Name.Colored(ConsoleTextColor.aqua)}");
                }
            }
            storage?.Flush();
        }

        public void ResetByInvestors() {
            foreach(var pair in registeredSaveables ) {
                if(pair.Value != null ) {
                    if (pair.Value.IsLoaded) {
                        pair.Value.ResetByInvestors();
                    }
                }
            }
            SaveAll();
        }

        public void ResetByPlanets() {
            foreach(var pair in registeredSaveables) {
                if(pair.Value != null ) {
                    if (pair.Value.IsLoaded) {
                        pair.Value.ResetByPlanets();
                    }
                }
            }
            SaveAll();
        }

        public void ResetByWinGame() {
            int resettedCount = 0;
            foreach (var pair in registeredSaveables) {
                if (pair.Value != null) {
                    if (pair.Value.IsLoaded) {
                        pair.Value.ResetByWinGame();
                        UDebug.Log($"service => {pair.Value.SaveKey} resetted by win game");
                        resettedCount++;
                    } else {
                        pair.Value.LoadDefaults();
                        UDebug.Log($"service => {pair.Value.SaveKey} NOT resetted by win game");
                    }
                }
            }
            UDebug.Log(
                $"resetted services count => {resettedCount}, total services count => {registeredSaveables.Count}");
            SaveAll();
        }

        public void ResetAll(params string[] excludeKeys) {
            UDebug.Log($"SaveService Reset All exlude => {excludeKeys.AsString()}".Colored(ConsoleTextColor.yellow));
            foreach(var pair in registeredSaveables) {
                if(pair.Value != null ) {
                    string excludedKey = Array.Find(excludeKeys, key => key == pair.Value.SaveKey);

                    if (string.IsNullOrEmpty(excludedKey)) {
                        pair.Value.ResetFull();
                    } else {
                        UDebug.Log($"Key => {pair.Value.SaveKey} was excluded from resetting".Colored(ConsoleTextColor.yellow));
                    }
                }
            }
            SaveAll();
        }
    }

    public interface ISaveService : IGameService {
        void Register(ISaveable saveable);
        bool IsRegistered(ISaveable saveable);
        void Load(ISaveable saveable);
        void Save(ISaveable saveable, bool flush  = true);
        void SaveAll();
        void ResetAll(params string[] excludeKeys);

        void ResetByInvestors();
        void ResetByPlanets();
        void ResetByWinGame();
    }

    public interface ISaveable : ISaveableBase {
        //object GetSave();
        //void LoadSave(object obj);
        //void LoadDefaults();
        string SaveKey { get; }
        bool IsLoaded { get; }
        Type SaveType { get; }
        //void Reset();
    }

    public interface ISaveableBase {
        object GetSave();
        void LoadDefaults();
        void ResetFull();
        void ResetByInvestors();      
        void ResetByPlanets();
        void ResetByWinGame();
        void LoadSave(object obj);
    }

    public interface IStorage {
        string GetString(string key, string defaultValue = "");
        void SetString(string key, string value, bool flush = true);
        void Flush();
    }

    public class PlayerPrefsStorage : IStorage {
        public string GetString(string key, string defaultValue = "") {
            return PlayerPrefs.GetString(key, defaultValue);
           
        }

        public void SetString(string key, string value, bool flush = true) {
            PlayerPrefs.SetString(key, value);
            if(flush) {
                Flush();
                
            }
        }

        public void Flush() {
            PlayerPrefs.Save();
            //UnityEngine.Debug.Log("Data FLUSHED On Disk".Colored(ConsoleTextColor.yellow));
        }
    }
}