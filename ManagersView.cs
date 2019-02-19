namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ManagersView : TypedViewWithCloseButton {

        public override ViewType Type => ViewType.ManagersView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 5;

        public override void Setup(ViewData data) {
            base.Setup(data);
            if(data != null && data.UserData != null ) {
                int managerId = (int)data.UserData;
                if(managerId != 0 ) {
                    var managerScreen = GetComponentInChildren<ManagerScreen>();
                    managerScreen?.MoveToManager(managerId);
                }
            }
            closeButton.SetListener(() => {
                Services.ViewService.Remove(ViewType.ManagersView);
                Services.SoundService.PlayOneShot(SoundName.click);
            });
        }
    }

}