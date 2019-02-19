using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DependPopup : MonoBehaviour {
    public static DependPopup Instance;
    private void Awake() {
        Instance = this;
    }

    public Image Icon;
    public Text Title, Desc;

    /* 
    public void Fill(SimpleGeneratorView unlockable, int dependId) {
        var depend = GeneratorsScreen.Instance.generatorvievs.FirstOrDefault(val => val.Generator.Id == dependId);
        Icon.sprite = depend.BaseSprite;
        Title.text = string.Format("DEPEND.TITLE".GetLocalizedString(), depend.Generator.name.ToUpper());
        Desc.text = string.Format("DEPEND.DESC".GetLocalizedString(),
            unlockable.Generator.name.ToLower(), depend.Generator.Name.ToLower());
    }*/
    
}
