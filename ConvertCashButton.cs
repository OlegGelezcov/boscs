/*
namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConvertCashButton : GameBehaviour {

        public override void OnEnable() {
            base.OnEnable();
            GetComponent<Button>().SetListener(() => {
                Services.ViewService.ShowDelayed(ViewType.CashConvertView, BosUISettings.Instance.ViewShowDelay);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        }
    }

}*/
