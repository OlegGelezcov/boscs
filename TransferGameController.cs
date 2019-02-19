using UnityEngine.SceneManagement;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Linq;
    
    public class TransferGameController : GameBehaviour {

        public ManagerKickbackScreenInfo[] screens;
        public Image background;
        public Image rollSprite;
        public Image managerIconImage;
        public Text slotText;
        public Text RewardText;  
        public GameObject animationObject;
        public GameObject winViewObject;
        public GameObject titleObject;
        public GameObject particles;
        public Button continueButton;
        

        private TransferCashInfo cashInfo = null;

        public void Setup(TransferCashInfo transferInfo) {
            cashInfo = transferInfo;
            slotText.Deactivate();
            animationObject.Activate();
            winViewObject.Deactivate();
            
            ManagerKickbackScreenInfo screenInfo = null;
            KickbackManagerSpritePositionInfo managerSpriteInfo = null;
            ManagerSlotUtil.SelectSpritePositionInfo(screens, Services.PlanetService.CurrentPlanet.Id, UnityEngine.Random.Range(0, 10),
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
            
            continueButton.SetListener(() => SceneManager.UnloadSceneAsync(7));
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
            slotText.text = $"{cashInfo.TaxPercentInt}%";
            yield return new WaitForSeconds(1);
            slotText.Activate();
            animationObject.Deactivate();       
            GetComponent<AudioSource>().Stop();       
            yield return new WaitForSeconds(0.8f);
            RewardText.text = Services.Currency.CreatePriceString(cashInfo.RemainValue);
            Services.PlayerService.FinishTransferCashOfficially(cashInfo);
            particles.Activate();
            Services.SoundService.PlayOneShot(SoundName.buyGenerator);
            yield return new WaitForSeconds(1.2f);
            winViewObject.SetActive(true);
            particles.Deactivate();

        }
        
    }
    


    public static class ManagerSlotUtil {

        public static void SelectSpritePositionInfo(ManagerKickbackScreenInfo[] screens,
            int planetId, int managerId, out ManagerKickbackScreenInfo screenInfo, 
            out KickbackManagerSpritePositionInfo managerSpriteInfo) {
            screenInfo = screens.ToList().FirstOrDefault(s => s.planetId == planetId);
            if (screenInfo != null) {
                managerSpriteInfo =
                    screenInfo.managers.FirstOrDefault(m => m.managerId == managerId);
            }
            else {
                managerSpriteInfo = null;
            }
        }
        
    }
}