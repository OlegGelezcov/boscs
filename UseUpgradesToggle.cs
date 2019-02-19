namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using System;
    using Bos.Data;

    public class UseUpgradesToggle : GameBehaviour {
        public GameObject alert;
        public UpgradeTab upgradeTab;

        public override void Start() {
            base.Start();
            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(num => UpdateAlert()).AddTo(gameObject);
            UpdateAlert();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.UpgradeAdded += OnUpgradeAdded;
        }

        public override void OnDisable() {
            GameEvents.UpgradeAdded -= OnUpgradeAdded;
            base.OnDisable();
        }

        private void OnUpgradeAdded(UpgradeData data ) {
            UpdateAlert();
        }

        private void UpdateAlert() 
            => alert.ToggleActivity(HasUpgrades);

        private bool HasUpgrades {
            get {
                switch(upgradeTab) {
                    case UpgradeTab.Cash:
                        return Services.UpgradeService.HasAvailableCashUpgrades;
                    case UpgradeTab.Securities:
                        return Services.UpgradeService.HasAvailableSecuritiesUpgrades;
                    case UpgradeTab.Coins:
                        return Services.StoreService.HasAvailableCoinUpgrades && 
                            (Services.TimeService.UnixTimeInt - Services.UpgradeService.UpgradeCoinsScreenOpenedLastTime > 600);
                    default:
                        return false;
                }
            }
        }
    }

    public enum UpgradeTab {
        Cash,
        Securities,
        Coins
    }
}