namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class LeaseManagerCountRollerButton : GameBehaviour {

        public Button selectButton;
        public Text buttonText;

        public SelectCountEnum CountValue { get; private set; } = SelectCountEnum.x1;

        public event System.Action<SelectCountEnum> CountChanged;

        Dictionary<SelectCountEnum, string> countToStringMap = new Dictionary<SelectCountEnum, string> {
            [SelectCountEnum.x1] = "x".Colored("#FDEE21") + "1".Colored("white"),
            [SelectCountEnum.x10] = "x".Colored("#FDEE21") + "10".Colored("white"),
            [SelectCountEnum.x100] = "x".Colored("#FDEE21") + "100".Colored("white"),
            [SelectCountEnum.xMax] = "x".Colored("#FDEE21") + "MAX".Colored("white")
        };

        Dictionary<SelectCountEnum, int> enumCountMap = new Dictionary<SelectCountEnum, int> {
            [SelectCountEnum.x1] = 1,
            [SelectCountEnum.x10] = 10,
            [SelectCountEnum.x100] = 100,
            [SelectCountEnum.xMax] = -1
        };

        public int CountValueInt
            => enumCountMap[CountValue];


        public override void OnEnable() {
            base.OnEnable();
            UpdateButtonText();
            selectButton.SetListener(() => {
                switch(CountValue) {
                    case SelectCountEnum.x1: {
                            CountValue = SelectCountEnum.x10;
                        }
                        break;
                    case SelectCountEnum.x10: {
                            CountValue = SelectCountEnum.x100;
                        }
                        break;
                    case SelectCountEnum.x100: {
                            CountValue = SelectCountEnum.xMax;
                        }
                        break;
                    case SelectCountEnum.xMax: {
                            CountValue = SelectCountEnum.x1;
                        }
                        break;
                }
                UpdateButtonText();
                OnCountChanged();
                Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
            });
        }

        private void UpdateButtonText() 
            => buttonText.text = SelectCountToString(CountValue);

        public enum SelectCountEnum {
            x1,
            x10,
            x100,
            xMax
        }

        private string SelectCountToString(SelectCountEnum value) {
            return countToStringMap[value];
        }

        private void OnCountChanged()
            => CountChanged?.Invoke(CountValue);

    }

    
}