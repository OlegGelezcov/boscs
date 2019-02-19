namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class BankLevelView : GameBehaviour {

        public int level;
        public Text[] levelTexts;
        public GameObject unlockedView;
        public GameObject lockedView;
        public GameObject nextLevelView;

        public override void OnEnable() {
            base.OnEnable();
            UpdateView();
            GameEvents.BankLevelChanged += OnBankLevelChanged;
        }

        public override void OnDisable() {
            GameEvents.BankLevelChanged -= OnBankLevelChanged;
            base.OnDisable();
        }

        private void UpdateView() {
            levelTexts.ToList().ForEach(t => t.text = level.ToString());
            BosUtils.If(() => IsUnlocked, () => {
                unlockedView.Activate();
                BosUtils.MakeList(nextLevelView, lockedView).ForEach(v => v.Deactivate());
            }, () => {
                BosUtils.If(() => level == Services.BankService.NextLevel, () => {
                    nextLevelView.Activate();
                    BosUtils.MakeList(lockedView, unlockedView).ForEach(v => v.Deactivate());
                }, () => {
                    lockedView.Activate();
                    BosUtils.MakeList(unlockedView, nextLevelView).ForEach(v => v.Deactivate());
                });
            }); 
        }

        public bool IsUnlocked {
            get {
                return Services.BankService.CurrentBankLevel >= level;
            }
        }

        #region Game Events
        private void OnBankLevelChanged(int oldLevel, int newLevel) {
            UpdateView();
        }
        #endregion
    }

}