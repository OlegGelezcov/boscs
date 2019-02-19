using UnityEngine.UI;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Bos.Data;

    public class ShopCoinUpgradeView : GameBehaviour, IListItemView<BosCoinUpgradeData> {

        public Text nameText;
        public Text descriptionText;
        public Text priceText;
        public Image iconImage;
        public Button buyButton;

        public Image BgSimple;
        public Image BgSpecial;

        public BosCoinUpgradeData Data { get; private set; }
        public BosItemList<BosCoinUpgradeData, ShopCoinUpgradeView> Parent { get; private set; }

        public void Setup(BosCoinUpgradeData data, BosItemList<BosCoinUpgradeData, ShopCoinUpgradeView> parent) {
            this.Data = data;
            this.Parent = parent;

            if (data.GeneratorId < 0) {
                nameText.text = data.Name.GetLocalizedString();
            } else {
                var generatorLocalData = ResourceService.GeneratorLocalData.GetLocalData(data.GeneratorId);
                string generatorName = LocalizationObj.GetString(generatorLocalData.GetName(Planets.CurrentPlanetId.Id).name);
                string sourceFmt = LocalizationObj.GetString(data.Name);
                string result = sourceFmt.Replace("{0}", generatorName);
                nameText.text = result;
            }
            descriptionText.text = data.Description.GetLocalizedString();
            priceText.text = data.Price.ToString();

            BgSimple.gameObject.SetActive(data.UpgradeType != UpgradeType.Enhance);
            BgSpecial.gameObject.SetActive(data.UpgradeType == UpgradeType.Enhance);
            
            if (data.Icon.IsValid) {
                iconImage.overrideSprite = data.Icon.GetSprite();
            } else {
                var generatorLocalData = Services.ResourceService.GeneratorLocalData.GetLocalData(data.GeneratorId);
                var iconData = generatorLocalData.GetIconData(Services.PlanetService.CurrentPlanet.Id);
                if(iconData.icon_id.IsValid()) {
                    iconImage.overrideSprite = Services.ResourceService.GetSpriteByKey(iconData.icon_id);
                } else {
                    iconImage.overrideSprite = Services.ResourceService.Sprites.FallbackSprite;
                }
            }

            buyButton.SetListener(() => {
                var result = Services.GetService<IStoreService>().Purchase(data);
                switch (result) {
                    case TransactionState.DontEnoughCurrency: {
                            //NotEnoughCoinsScreen.Instance.Show(data.Price);
                        Services.ViewService.Show(ViewType.CoinRequiredView, new ViewData {
                            UserData = data.Price
                        });
                    }
                        break;
                    case TransactionState.AlreadyPurchased: {
                            parent.Remove(this);
                        }
                        break;
                    case TransactionState.Success: {
                            if(data.IsOneTime) {
                                buyButton.interactable = false;
                            }
                        }
                        break;
                }

                Debug.Log($"Purchase state => {result}");
            });


        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ShopItemPurchased += OnCoinUpgradePurchased;
        }

        public override void OnDisable() {
            GameEvents.ShopItemPurchased -= OnCoinUpgradePurchased;
            base.OnDisable();
        }



        private void OnCoinUpgradePurchased(IShopItem data) {
            if(Data != null && (Data.Id == data.ItemId)) {
                GameObject prefab = GameServices.Instance.ResourceService.Prefabs.GetPrefab("coins");
                GameObject instance = Instantiate<GameObject>(prefab);
                instance.transform.SetParent(buyButton.transform, false);
                instance.transform.localPosition = new Vector3(0, 0, -1);
                instance.transform.localScale = Vector3.one;
                instance.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                Destroy(instance, 3);
                instance.transform.SetParent(GetParentViewTransform(), true);
                instance.GetComponent<ParticleSystem>().Play();
                Services.SoundService.PlayOneShot(SoundName.buyGenerator);
                print("coins created...");

                if(Data.IsOneTime) {
                    Parent?.Remove(this);
                }
            }
        }

        private RectTransform GetParentViewTransform() {
            BaseView parentView = GetComponentInParent<BaseView>();
            if(parentView != null ) {
                return parentView.GetComponent<RectTransform>();
            } else {
                return GetComponent<RectTransform>();
            }
        }
    }


}