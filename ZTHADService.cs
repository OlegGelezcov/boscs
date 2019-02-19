using System;
using Bos.UI;

namespace Bos
{
	using System.Collections;
	using System.Collections.Generic;
    using UniRx;
    using UnityEngine;

	public class ZTHADService : SaveableGameBehaviour, IZTHADService
	{
		public bool IsShow { get; private set; }
		public bool IsComplete { get;  private set; }


		public bool IsGetSmallReward { get; private set; }
		public bool IsGetBigReward { get;  private set; }
		
		
		public DateTime WatchedDateTime;
		public const int SmallReward = 5;
		public const int BigReward = 10;

		private const int Min5 = 300;
		private const int Min40 = 2400;

		private ZTHRewardView RewardView;

        private bool isInitialized = false;

        

		public void Setup(object data = null)
		{
            if (!isInitialized) {
                

                Subject<bool> viewShowedSubject = new Subject<bool>();
                IObservable<bool> stopObservable = viewShowedSubject.AsObservable();

                Subject<bool> smallSubject = new Subject<bool>();
                IObservable<bool> stopSmallObservable = smallSubject.AsObservable();

                Subject<bool> bigSubject = new Subject<bool>();
                IObservable<bool> stopBigObservable = bigSubject.AsObservable();

                IObservable<long> baseObservable = Observable.Interval(TimeSpan.FromSeconds(20))
                    .Where(_ => IsServvicesLoaded())
                    .Where(_ => IsMustBe())
                    .TakeWhile(_ => !IsComplete);

                baseObservable.Do(_ => UnityEngine.Debug.Log("check view modal")).Where(_ => !IsShow)
                .TakeUntil(stopObservable)
                .Subscribe(_ => {
                    ViewService.ShowDelayed(UI.ViewType.ZTHAdView, BosUISettings.Instance.ViewShowDelay, new ViewData {
                        ViewDepth = ViewService.NextViewDepth
                    });
                    IsShow = true;
                    WatchedDateTime = DateTime.Now;
                    viewShowedSubject.OnNext(IsShow);
                }).AddTo(gameObject);

                baseObservable.Do(_ => UnityEngine.Debug.Log("check view small"))
                    .Where(val => {
                        return !IsGetSmallReward &&
                        (DateTime.Now - WatchedDateTime).TotalSeconds > Min5 &&
                        CheckInstall();
                    })
                    .TakeUntil(stopSmallObservable)
                    .Subscribe(val => {
                        CreateRewardView(SmallReward);
                        Services.PlayerService.AddCoins(SmallReward);
                        IsGetSmallReward = true;
                        smallSubject.OnNext(IsGetSmallReward);
                    }).AddTo(gameObject);

                baseObservable.Do(_ => UnityEngine.Debug.Log("check view big"))
                    .Where(_ => {
                        return !IsGetBigReward &&
                                IsGetSmallReward &&
                                (DateTime.Now - WatchedDateTime).TotalSeconds > Min40;
                    }).TakeUntil(stopBigObservable)
                    .Subscribe(_ => {
                        CreateRewardView(BigReward);
                        Services.PlayerService.AddCoins(BigReward);
                        IsGetBigReward = true;
                        IsComplete = true;
                        bigSubject.OnNext(IsGetBigReward);
                    }).AddTo(gameObject);

                isInitialized = true;
            }
		}

        public void UpdateResume(bool pause)
            => UnityEngine.Debug.Log($"{GetType().Name}.{nameof(UpdateResume)}() => {pause}");

        private bool IsMustBe() {
            return IsSubmarinOpened ||
                IsPurchasedProduct ||
                isWatchAd10Times;
        }

        private bool IsServvicesLoaded() {
            return  Services.TransportService.IsLoaded &&
                    Services.PlayerService.IsLoaded &&
                    Services.AdService.IsLoaded &&
                    IsLoaded;
        }


		private string zthBundleId = "com.heatherglade.zero2hero";
		private bool CheckInstall()
		{
#if UNITY_EDITOR
			return true;
#else
            var isInstall = UtilsHelper.CheckPackageAppIsPresent(zthBundleId);
			UnityEngine.Debug.Log($"app {zthBundleId} is {isInstall}");
			return isInstall;
#endif
		}



		private void CreateRewardView(int coin)
		{
			if (RewardView == null || RewardView.gameObject == null)
			{
				var prefab = Instantiate(Resources.Load("Prefabs/UI/ZTHRewardView")) as GameObject;
				RewardView = prefab.GetComponent<ZTHRewardView>();
				prefab.transform.SetParent(ViewService.GetCanvasTransform(CanvasType.UI));
				prefab.transform.SetAsLastSibling();
				prefab.transform.localPosition = Vector3.zero;
				prefab.transform.localScale = Vector3.one;
				RewardView.Coin.text = coin.ToString();
			}
		}
		
		private bool IsSubmarinOpened
			=> Services.TransportService.HasUnits(TransportUnitsService.kSubmarinId);

		private bool IsPurchasedProduct
			=> Services.PlayerService.HasPurchasedProducts();

		private bool isWatchAd10Times
			=> Services.AdService.TotalAdsWatched >= 10;
		
		
		public void OpenZthPage()
		{
#if UNITY_ANDROID
			Application.OpenURL($"market://details?id={zthBundleId}");
#elif UNITY_IPHONE
			Application.OpenURL($"itms-apps://itunes.apple.com/app/{zthBundleId}");
#endif
		}
		
#region Saveable pattern implementation
		public override string SaveKey => "zth_service";
		public override Type SaveType => typeof(ZTHADServiceSave);

		public override void ResetFull() {
			LoadDefaults();
		}

		public override void ResetByInvestors() { IsLoaded = true; }
		public override void ResetByPlanets() { IsLoaded = true; }
		public override void ResetByWinGame() { IsLoaded = true; }

		public override object GetSave() {
			var save = new ZTHADServiceSave {
				isShow = IsShow,
				isComplete = IsComplete,
				watchedDateTime = WatchedDateTime,
				isGetSmallReward = IsGetSmallReward,
				isGetBigReward = IsGetBigReward
			};
            //UnityEngine.Debug.Log($"save ZTH: {save.ToString()}");
            return save;
		}

		public override void LoadDefaults() {
			IsShow = false;
			IsComplete = false;
			IsGetSmallReward = false;
			IsGetBigReward = false;
			WatchedDateTime = DateTime.Now;
            IsLoaded = true;
        }

		public override void LoadSave(object obj) {
			ZTHADServiceSave save = obj as ZTHADServiceSave;
			if(save != null ) {
                //UnityEngine.Debug.Log($"loading ZTH: {save.ToString()}");
				IsShow = save.isShow;
				IsComplete = save.isComplete;
				WatchedDateTime = save.watchedDateTime;
				IsGetSmallReward = save.isGetSmallReward;
				IsGetBigReward = save.isGetBigReward;
				IsLoaded = true;
			} else {
                UnityEngine.Debug.Log($"load ZTH error, setup default");
				LoadDefaults();
			}
		}
#endregion
	}

	public interface IZTHADService : IGameService
	{
		bool IsShow { get; }
		bool IsComplete { get; }

		void OpenZthPage();
	}

	public class ZTHADServiceSave
	{
		public bool isShow;
		public bool isComplete;
		public bool isGetSmallReward;
		public bool isGetBigReward;
		public DateTime watchedDateTime;

        public override string ToString() {
            return $"isShow:{isShow}, isComple: {isComplete}, isGetSmallReward: {isGetSmallReward}, isGetBigReward: {isGetBigReward}, watched at: {watchedDateTime}";
        }
    }
}
