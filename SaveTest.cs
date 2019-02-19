namespace Bos.Debug.Test {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SaveTest : MonoBehaviour {

        private SomeService someService = new SomeService();

        private SaveService saveService;

        void Start() {
            saveService = FindObjectOfType<SaveService>();
            saveService.Setup(new PlayerPrefsStorage());
            saveService.Register(someService);
            saveService.Load(someService);
        }

        void OnGUI() {
            GUILayout.BeginVertical();
            if(GUILayout.Button("Modify") ) {
                someService.ChangeValues();
            }
            if(GUILayout.Button("Save")) {
                saveService.Save(someService);
            }
            GUILayout.EndVertical();
        }
    }

    public class SomeService : ISaveable {

        private int intValue;
        private string strValue;
        private float fValue;
        private bool bValue;
        private List<string> someStrings;

        public void ChangeValues() {
            intValue = 144;
            strValue = "changed";
            fValue = 19f;
            bValue = false;
            someStrings = new List<string> { "r", "t" };
        }

        public class SaveInfo {
            public int IntValue { get; set; }
            public string StrValue { get; set; }
            public float FloatValue { get; set; }
            public bool BoolValue { get; set; }
            public List<string> SomeStrings { get; set; }
        }

        public System.Type SaveType => typeof(SaveInfo);

        public string SaveKey => "some_service";

        public object GetSave() {
            return new SaveInfo {
                IntValue = intValue,
                StrValue = strValue,
                FloatValue = fValue,
                BoolValue = bValue,
                SomeStrings = someStrings.Select(s => s).ToList()
            };
        }

        public void LoadDefaults() {
            intValue = 15;
            strValue = "initial string";
            fValue = 123f;
            bValue = true;
            someStrings = new List<string> { "a", "b", "c" };
            IsLoaded = true;
            Debug.Log("load defaults");

        }

        public void Reset() {
            IsLoaded = false;
            LoadDefaults();
        }

        public void LoadSave(object obj) {
            SaveInfo info = obj as SaveInfo;
            if(info != null ) {
                intValue = info.IntValue;
                strValue = info.StrValue;
                fValue = info.FloatValue;
                bValue = info.BoolValue;
                someStrings = info.SomeStrings;
                IsLoaded = true;
                Debug.Log($"load values int => {intValue}, string => {strValue}, float => {fValue}, bool => {bValue}, list => {someStrings.Count}");
            } else {
                LoadDefaults();
            }
        }

        public void ResetFull()
        {
            //throw new System.NotImplementedException();
        }

        public void ResetByInvestors()
        {
            //throw new System.NotImplementedException();
        }

        public void ResetByPlanets()
        {
            //throw new System.NotImplementedException();
        }

        public void ResetByWinGame() {
            
        }

        public bool IsLoaded { get; private set; }
    }

}