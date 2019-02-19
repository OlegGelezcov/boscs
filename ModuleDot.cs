namespace Bos {
    using Bos.UI;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ModuleDot : GameBehaviour {
        public int moduleId;
        public bool IsSelected { get; private set; }

        public override void Start() {
            base.Start();
            GetComponent<Button>().SetListener(() => {
                if(!IsSelected) {
                    GetComponentInParent<BuyModuleView>().Setup(new ViewData { UserData = new ModuleViewModel {
                        ScreenType = ModuleScreenType.Normal,
                         ModuleId = moduleId
                    } });
                }
            });
        }

        public void Select() => IsSelected = true;

        public void Unselect() => IsSelected = false;

    }

}