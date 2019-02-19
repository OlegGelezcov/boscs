using System;
using System.Collections.Generic;
using Bos.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bos
{
	public class TreasureHuntContentView : GameBehaviour
	{
		public TreasureHuntGameController TreasureHuntGameController;
		public List<TreasureHuntChestView> ChestViews;
        public GameObject timerParent;
		public Text TimeDisplay;
		public Text triesCountText;
		public Text getFreeSpins;
		public Button WatchAd;
		public Button buyTryButton;
		
		private float _pauseTS;
		private bool _canPlay = true;
		private bool _wasDisabled;
		private DateTime _lastDisabled;
		private readonly UpdateTimer updateTimer = new UpdateTimer();
		

		public override void Start()
		{
			WatchAd.SetListener(GetFreeSpins);
			buyTryButton.SetListener(BuyTries);
			UpdateControls();
			updateTimer.Setup(1, (dt) => {
				var service = Services.TreasureHuntService;
				var isNoTries = !service.HasTries;

				if (service.NextTriesUpdateTime > Services.TimeService.UnixTimeInt) {
					var tDisplay =
						System.TimeSpan.FromSeconds(service.NextTriesUpdateTime - Services.TimeService.UnixTimeInt);
					var answer = $"{tDisplay.Minutes:D2}:{tDisplay.Seconds:D2}";
					TimeDisplay.text = answer;
				} else {
					if (isNoTries) {
						Services.TreasureHuntService.SetTries(Services.TreasureHuntService.MaxTries);
						TimeDisplay.text = "00:00";
					}
				}
			}, true);

			foreach (var view in ChestViews)
			{
				view.ChestButton.onClick.AddListener(() => { Play(view); });
			}
		}
		
		public void Play(TreasureHuntChestView chest)
		{
			if (!_canPlay || !Services.TreasureHuntService.HasTries)
				return;
			
			_canPlay = false;
			chest.ChestButton.interactable = false;
			EventSystemController.OffEventSystemForSeconds(2f);
			
			TreasureHuntGameController.OpenChest(chest, () =>
			{
				_canPlay = true;
				Services.TreasureHuntService.RemoveTries(1);
				chest.ChestButton.interactable = false;
				if (!TreasureHuntGameController.HasAnyReward())
				{
					Services.TreasureHuntService.RemoveTries(Services.TreasureHuntService.TriesCount);
				}
                GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.ChestOpened));
			});      
		}
		
		public override void OnEnable() {
			base.OnEnable();
			GameEvents.TreasureHuntTriesChanged += OnTriesChanged;
			GameEvents.TreasureHuntMaxTriesChanged += OnMaxTriesChanged;
			GameEvents.TreasureHuntReload += OnTreasureHuntReload;
			//GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.BreakLinesOpened));
			UpdateControls();
		}

		public override void OnDisable() {
			GameEvents.TreasureHuntTriesChanged -= OnTriesChanged;
			GameEvents.TreasureHuntMaxTriesChanged -= OnMaxTriesChanged;
			GameEvents.TreasureHuntReload -= OnTreasureHuntReload;
			base.OnDisable();
		}

		private void OnTriesChanged(int oldCount, int newCount ) {
			UpdateControls();
		}

		private void OnMaxTriesChanged(int oldCount, int newCount ) {
			UpdateControls();
		}
		
		private void OnTreasureHuntReload() {
			ReloadChest();
			TreasureHuntGameController.Setup();
		}


		private void OnApplicationFocus(bool hasFocus)
		{
			if(hasFocus)
				UpdateControls();
		}


		public void ReloadChest()
		{
			foreach (var chest in ChestViews)
			{
				chest.SetAnim(TreasureHuntAnimType.Lock);
				chest.ChestButton.interactable = true;
			}
		}


		public void BuyTries() {
			var productData = ResourceService.Products.GetProduct(33);
			if (productData != null) {
				Services.Inap.PurchaseProduct(productData);
			} else {
				UnityEngine.Debug.LogError($"not found product id {33}");
			}
		}
    
		public void IncreaseTriesPaid()
		{
			Services.TreasureHuntService.AddMaxTries(1);
		}
		
		
		private void UpdateControls() {
			var service = Services.TreasureHuntService;
			var isNoTries = !service.HasTries;
			if (isNoTries)
			{
				foreach (var chest in ChestViews)
				{
					chest.SetAnim(TreasureHuntAnimType.Gray);
					chest.ChestButton.interactable = false;
				}
			}

			if (service.TriesCount == service.MaxTries)
			{
				ReloadChest();
				TimeDisplay.text = "00:00";
			}

			WatchAd.gameObject.SetActive(isNoTries);
			triesCountText.text = service.TriesCount > 0 ? service.TriesCount.ToString() : service.MaxTries.ToString();
			getFreeSpins.text = string.Format(Services.ResourceService.Localization.GetString("btn_get_fmt_tries"), service.MaxTries);

            timerParent.SetActive(isNoTries);
		}
		
		public void GetFreeSpins()
		{
			Services.AdService.WatchAd("TreasureHunt", () =>
			{
				Services.TreasureHuntService.SetTries(Services.TreasureHuntService.MaxTries);
				Services.TreasureHuntService.ResetNextTriesTime();
				TimeDisplay.text = "00:00";
				FacebookEventUtils.LogADEvent("TreasureHunt");
				GameEvents.OnTreasureHuntReload();
			});
		}
		
		public override void Update() {
			updateTimer.Update();
		}
	}

}

