using Bos;
using Bos.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using Bos.Data;

public class EnhancementWindow : GameBehaviour {
    //private SimpleGeneratorView _view;
    private UnlockedGeneratorView newView;

    public Image NormalIcon;
    public Image UpgradedIcon;
    public Text NormalName;
    public Text UpgradeName;
    public Image ManagerIcon;
    public Image UpgradeManagerIcon;



    public void Show(UnlockedGeneratorView view) {
        newView = view;
        NormalIcon.overrideSprite = view.baseSprite;
        UpgradedIcon.overrideSprite = view.enhancedIconSprite;
        ManagerIcon.overrideSprite = view.managerIcon.sprite;
        UpgradeManagerIcon.overrideSprite = view.managerIcon.sprite;
        NormalName.text = UpgradeName.text = view.GeneratorName;
        gameObject.SetActive(true);
    }

    public void Click() {
        //newView?.OnEnhanceConfirm();
    }

    public void Close() {
        gameObject.SetActive(false);
        if(newView != null ) {
            Analytics.CustomEvent($"ENHANCE_WINDOW_{newView.GeneratorId}_CLOSE");
        }
    }
}
