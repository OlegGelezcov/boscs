using Bos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManagerGameController : GameBehaviour {

    public ManagerKickbackScreenInfo[] screens;
    public Image background;
    public Image rollSprite;
    public Image managerIconImage;
    public Text slotText;
    public Text RewardText;  
    public GameObject Anim;
    public GameObject win;
    public GameObject titleObject;
    public GameObject particles;

    //public List<double> KickBacksPecent = new List<double>(){5, 10 , 15 , 25};
    //public List<int> KickBacksWegith = new List<int>(){55 , 25 , 15, 5};

    
    
    public override void Start()
    {
        slotText.Deactivate();
        Anim.Activate();
        win.Deactivate();

        var rollbackInfo = Services.ManagerService.CurrentRollbackInfo;
        /*
        ManagerKickbackScreenInfo screenInfo = null;
        foreach(var si in screens) {
            if(si.planetId == rollbackInfo.PlanetId) {
                screenInfo = si;
                break;
            }
        }
        KickbackManagerSpritePositionInfo managerSpriteInfo = null;
        if(screenInfo != null ) {
            foreach(var msi in screenInfo.managers) {
                if(msi.managerId == rollbackInfo.ManagerId) {
                    managerSpriteInfo = msi;
                    break;
                }
            }
        }*/

        ManagerKickbackScreenInfo screenInfo = null;
        KickbackManagerSpritePositionInfo managerSpriteInfo = null;
        ManagerSlotUtil.SelectSpritePositionInfo(screens, rollbackInfo.PlanetId, rollbackInfo.ManagerId,
            out screenInfo, out managerSpriteInfo);


        if(screenInfo != null ) {
            background.overrideSprite = screenInfo.backSprite;
            rollSprite.overrideSprite = screenInfo.rollSprite;
        }
        if(managerSpriteInfo != null ) {
            managerIconImage.overrideSprite = managerSpriteInfo.managerSprite;
            managerIconImage.GetComponent<RectTransform>().anchoredPosition = managerSpriteInfo.managerPosition;
        }

        StartCoroutine(InternalPull());
        GetComponent<AudioSource>().Play();
        ScaleTitle();
    }

    private void ScaleTitle() {
        var data1 = AnimUtils.GetScaleAnimData(1, 1.1f, 0.4f, EaseType.EaseInOutSin, titleObject.GetComponent<RectTransform>(), () => {
            var data2 = AnimUtils.GetScaleAnimData(1.1f, 1, 0.4f, EaseType.EaseInOutSin, titleObject.GetComponent<RectTransform>());
            titleObject.GetOrAdd<Vector2Animator>().StartAnimation(data2);
        });
        titleObject.GetOrAdd<Vector2Animator>().StartAnimation(data1);
    }


    private IEnumerator InternalPull()
    {
  
        yield return new WaitForSeconds(1f);
        var rollbackInfo = Services.ManagerService.CurrentRollbackInfo;
        
        slotText.text = $"{rollbackInfo.RollbackPercentInt}%";
        yield return new WaitForSeconds(1);
        slotText.Activate();
        Anim.Deactivate();       
        GetComponent<AudioSource>().Stop();       
        yield return new WaitForSeconds(0.8f);
        var payed = GameServices.Instance.GetService<IManagerService>().FinishRollback();
        RewardText.text = Services.Currency.CreatePriceString(payed, false, " ");
        particles.Activate();
        Services.SoundService.PlayOneShot(SoundName.buyGenerator);
        yield return new WaitForSeconds(1.2f);
        win.SetActive(true);
        particles.Deactivate();

    }


    public void Continue()
    {
        SceneManager.UnloadSceneAsync(4);
    }
    
}

[System.Serializable]
public class ManagerKickbackScreenInfo {
    public int planetId;
    public Sprite backSprite;
    public Sprite rollSprite;
    public KickbackManagerSpritePositionInfo[] managers;
}

[System.Serializable]
public class KickbackManagerSpritePositionInfo {
    public int managerId;
    public Sprite managerSprite;
    public Vector2 managerPosition;
}


