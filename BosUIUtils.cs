using System.Linq;
using Bos.Data;

namespace Bos.UI {
    using Ozh.Tools.Functional;
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class BosUIUtils : GameElement {

		private GameManager gameManeger = null;

		public GameManager GameManager
			=> (gameManeger != null) ?
				gameManeger :
				(gameManeger = GameObject.FindObjectOfType<GameManager>());
		
		public Image UpdatePlayerSmallIcon(Image targetImage) {
			int currentPlanetId = Services.PlanetService.CurrentPlanet.Id;
			Gender currentGender = Services.PlayerService.Gender;
			var playerIconData = Services.ResourceService.PlayerIcons.GetSmall(currentPlanetId, currentGender);
			targetImage.overrideSprite = Services.ResourceService.GetSprite(playerIconData);
			return targetImage;
		}

		public void UpdateUpgradeAlert(GameObject alertObject) {
			if (Services.UpgradeService.HasAvailableUpgrades) {
				alertObject.Activate();
			}
			else {
				alertObject.Deactivate();
			}
		}

		public void UpdateManagerAlert(GameObject alertObject) {
			if(Services.ManagerService.HasAvailableManager) {
				alertObject.Activate();
			} else {
				alertObject.Deactivate();
			}
		}

		public void UpdateRewardAlert(GameObject alertObject) {
			if (Services.RewardsService.AvailableRewards > 0) {
				alertObject.Activate();
			}
			else {
				alertObject.Deactivate();
			}
		}

		public void UpdateShopAlert(GameObject alertObject) {
			if(GameManager == null ) {
				alertObject.Deactivate();
			} else {
                int count = Services.ResourceService.CoinUpgrades.UpgradeList.Count(data => {
                    bool notOneTimeAllowed = (false == data.IsOneTime) && (Services.PlayerService.Coins >= data.Price);
                    if (notOneTimeAllowed) {
                        return true;
                    }
                    bool isOneTimeAllowedNotPurchased = data.IsOneTime && (!Services.StoreService.IsCoinUpgradePurchased(data)) && (Services.PlayerService.Coins >= data.Price);
                    if (isOneTimeAllowedNotPurchased) {
                        return true;
                    }
                    return false;
                });
                if(count > 0 ) {
                    alertObject.Activate();
                } else {
                    alertObject.Deactivate();
                }
			}
		}

        public bool IsInvestorSellStateOk(List<InvestorSellState> states)
            => states.Count == 1 && states[0] == InvestorSellState.Ok;

        public void UpdateInvestorAlert(GameObject alertObject) {

            var states = Services.InvestorService.SellState;

            if (IsInvestorSellStateOk(states) && IsInvestorAlertNotBlockedWithSecurities()) {
                alertObject.Activate();
            } else if (Services.InvestorService.IsBlockedBy(states, InvestorSellState.NoTries) && 
                (Services.TimeService.UnixTimeInt > Services.InvestorService.TimeOfAlertBlockedByStatus + 20 * 60)) {
                alertObject.Activate();
            } else {
                alertObject.Deactivate();
            }
		}

        private bool IsInvestorAlertNotBlockedWithSecurities() {
            var investorService = Services.InvestorService;
            if(!investorService.AlertInfo.IsAlertShowedInSession) {
                return true;
            }
            if(investorService.AlertInfo.IsAlertShowedInSession && 
                (Services.PlayerService.Securities.Value >= investorService.AlertInfo.CountOfSecuritiesToShowNextAlert)) {
                return true;
            }
            return false;
        }

		public void ApplyManagerIcon(Image image, GeneratorInfo generator, bool isUseAutomatic) {
			if (generator.Data.Type == GeneratorType.Normal) {
				var spriteData = Services.ResourceService.ManagerLocalDataRepository
					.GetIconData(generator.GeneratorId, Services.PlanetService.CurrentPlanet.Id, isUseAutomatic);
                //Debug.Log($"try set manager icon for generator {generator.GeneratorId} and planet {Services.PlanetService.CurrentPlanet.Id}");
				image.overrideSprite = Services.ResourceService.GetSprite(spriteData);
			}
		}

        public static void ApplyLoadingSprite(LoadingScreenSprite[] allSprites, int currentPlanet, Image targetImage) {
            for(int i = 0; i < allSprites.Length; i++ ) {
                if(allSprites[i].planetId == currentPlanet) {
                    targetImage.overrideSprite = allSprites[i].loadingSprite;
                    break;
                }
            }
        }

        public Vector2 WorldPosToCanvasPos(Camera camera, Canvas canvas, Vector3 worldPosition) {
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Vector2 scaledPos = new Vector2((pos.x - Screen.width * 0.5f) / scaler.scaleFactor,
                (pos.y - Screen.height * 0.5f) / scaler.scaleFactor);
            return scaledPos;
        }

        public void CreateEndGameCoins(System.Action endAction) {
            Services.RunCoroutine(CreateEndGameCoinsImpl(endAction));
        }

		public TutorialFinger CreateTutorialFinger(TutorialFingerData data) {
			GameObject prefab = Services.ResourceService.Prefabs.GetPrefab("finger");
			GameObject instance = GameObject.Instantiate(prefab);
			instance.GetComponent<RectTransform>().SetParent(Services.ViewService.GetCanvasTransform(CanvasType.UI), false);
			var fingerView = instance.GetComponent<TutorialFinger>();
			fingerView.Setup(data);
			return fingerView;
		}


        private IEnumerator CreateEndGameCoinsImpl(System.Action endAction) {
            GameObject lastSiblingPrefab = Services.ResourceService.Prefabs.GetPrefab("lastsibling");
            GameObject lastSiblingInstance = GameObject.Instantiate(lastSiblingPrefab);
            lastSiblingInstance.GetComponent<RectTransform>().SetParent(Services.ViewService.GetCanvasTransform(CanvasType.UI), false);

            GameObject winCoinPrefab = Services.ResourceService.Prefabs.GetPrefab("wincoin");
            for(int i = 0; i < 20; i++) {
                GameObject instance = GameObject.Instantiate(winCoinPrefab, Services.ViewService.GetCanvasTransform(CanvasType.UI), false);
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                //rectTransform.SetParent(Services.ViewService.GetCanvasTransform(CanvasType.UI));
                EndGameCoin coin = instance.GetComponent<EndGameCoin>();
                coin.Setup();
                Services.SoundService.PlayOneShot(SoundName.buyCoinUpgrade);
                yield return new WaitForSeconds(.2f);
                if(i == 19 ) {
                    yield return new WaitForSeconds(coin.interval - .2f);
                }

            }
            yield return new WaitForSeconds(.5f);
            GameObject.Destroy(lastSiblingInstance);
            lastSiblingInstance = null;

            endAction?.Invoke();
            
        }

        public Option<string> ApplyGeneratorName(Text text, GeneratorInfo generator) {
            var generatorLocalData = Services.ResourceService.GeneratorLocalData.GetLocalData(generator.GeneratorId);
            int currentPlanetId = Services.PlanetService.CurrentPlanetId.Id;
            var planetData = Services.ResourceService.PlanetNameRepository.GetPlanetNameData(Services.PlanetService.CurrentPlanetId.Id);
            if(generatorLocalData == null ) {
                Debug.LogError($"GeneratorLocalData is null => {generator.GeneratorId}");
                return F.None; ;
            }
            if(planetData == null) {
                Debug.LogError($"PlanetNameData is null => {generator.PlanetId}");
                return F.None;
            }
            var optionName = generatorLocalData.ConstructGeneratorName(currentPlanetId,
                (strId) => Services.ResourceService.Localization.GetString(strId));
            return optionName.Match(() => {
                if (text != null) {
                    text.text = string.Empty;
                }
                return F.None;
            }, (str) => {
                if (text != null) {
                    text.text = str;
                }
                return F.Some(str);
            });
        }

    }

    public static class BosUIExtensions {

        public static GameObject MessageBoxAnimateOut(this RectTransform background, Action endAction = null ) {
            CanvasGroup canvasGroup = background.gameObject.GetOrAdd<CanvasGroup>();
            FloatAnimator floatAnimator = background.gameObject.GetOrAdd<FloatAnimator>();
            Vector3Animator scaleAnimator = background.gameObject.GetOrAdd<Vector3Animator>();
            floatAnimator.StartAnimation(new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = .15f,
                EaseType = EaseType.EaseInOutQuad,
                Target = background.gameObject,
                OnStart = canvasGroup.UpdateAlphaFunctor(),
                OnUpdate = canvasGroup.UpdateAlphaTimedFunctor(),
                OnEnd = canvasGroup.UpdateAlphaFunctor()
            });

            scaleAnimator.StartAnimation(
                background.ConstructScaleAnimationData(
                    startValue: new Vector3(1.0f, 1.0f, 1f),
                    endValue: Vector3.one * .5f,
                    duration: .15f,
                    mode: BosAnimationMode.Single,
                    easeType: EaseType.EaseInOutQuad,
                    endAction: () => endAction?.Invoke())
                );
            return background.gameObject;
        }

        public static GameObject MessageBoxAnimateIn(this RectTransform background, Action endAction = null) {
            CanvasGroup canvasGroup = background.gameObject.GetOrAdd<CanvasGroup>();
            FloatAnimator floatAnimator = background.gameObject.GetOrAdd<FloatAnimator>();
            Vector3Animator scaleAnimator = background.gameObject.GetOrAdd<Vector3Animator>();

            floatAnimator.StartAnimation(new FloatAnimationData {
                StartValue = 0.5f,
                EndValue = 1,
                Duration = .15f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = background.gameObject,
                OnStart = canvasGroup.UpdateAlphaFunctor(),
                OnUpdate = canvasGroup.UpdateAlphaTimedFunctor(),
                OnEnd = canvasGroup.UpdateAlphaFunctor(() => endAction?.Invoke())
            });

            /*
            scaleAnimator.StartAnimation(
                background.ConstructScaleAnimationData(
                    startValue: new Vector3(0.5f, 0.5f, 1f),
                    endValue: Vector3.one,
                    duration: .15f,
                    mode: BosAnimationMode.Single,
                    easeType: EaseType.EaseInOutQuad,
                    endAction: () => endAction?.Invoke())
                );*/

            return background.gameObject;
        }


        public static void SetManagerName(this Text text, ManagerInfo manager)
            => text.SetManagerName(manager.Id);

        public static void SetManagerName(this Text text, int managerId ) {
            text.SetManagerName(managerId, GameServices.Instance.PlanetService.CurrentPlanetId.Id);
        } 

        public static void SetManagerName(this Text text, int managerId, int planetId ) {
            Option<ObjectName> managerNameObject = GameServices.Instance.ResourceService.ManagerLocalDataRepository
                .GetManagerLocalData(managerId).GetName(planetId);
            managerNameObject.Match(() => {
                text.text = string.Empty;
                return F.None;
            }, (nameObj) => {
                text.text = GameServices.Instance.ResourceService.Localization.GetString(nameObj.name);
                return F.Some(nameObj);
            });
        }
    }

}

