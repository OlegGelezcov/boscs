namespace Bos.Debug {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class AddCoinsButton : GameBehaviour {

        public override void OnEnable() {
            GetComponent<Button>()?.SetListener(() => {
                //FindObjectOfType<BalanceManager>().IAPManager.Coins.Value += 1000;
                Services.PlayerService.AddCoins(1000);
            });
        }
    }

}