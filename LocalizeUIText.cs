namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LocalizeUIText : GameBehaviour {

        public Mode mode = Mode.Text;
        public string key;
        public bool toUpperCase = false;
        public bool toLowerCase = false;
        public bool isUpperFirstLetter = false;

        private bool isLocalized = false;

        public bool addSuffix = false;
        public string suffix = string.Empty;

        public override void OnEnable() {
            base.OnEnable();
            Localize();
        }

        private void Localize() {
            if(!isLocalized) {
                switch(mode) {
                    case Mode.Text: {
                            Text text = GetComponent<Text>();
                            if (text != null) {
                                text.text = ConstructString();
                            }
                        }
                        break;
                    case Mode.TextMeshPro: {
                            TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
                            if(text != null) {
                                text.text = ConstructString();
                            }
                        }
                        break;
                }
                isLocalized = true;
            }
        }

        private string ConstructString() {
            if(Services == null ) {
                return "[none]";
            }

            string str = Services.ResourceService.Localization.GetString(key);
            str = str.Trim();
            if (addSuffix) {
                str = str + suffix;
            }
            if (toUpperCase) {
                str = str.ToUpper();
            }
            if(toLowerCase) {
                str = str.ToLower();
            }
            if(isUpperFirstLetter) {
                if(str.Length > 0 ) {
                    str = char.ToUpper(str[0]) + str.Substring(1).ToLower();
                }
            }
            return str;
        }

        public enum Mode {
            Text,
            TextMeshPro
        }
    }

}