using System;
using System.Runtime.InteropServices;
using Bos.UI;
using UniRx;
using UnityEngine.UI;

namespace Bos
{
	public class TransportInfoView : TypedViewWithCloseButton
	{
		public BoostViewBase[] Boosts;
		public Image TransportImage;
		public Text Profit;
		public Text TransportName;
		public Text AdTime;
		public Button AdButton;
		private GeneratorInfo _generatorInfo;
		private void Start()
		{
			base.Start();
			closeButton.SetListener(() => {
				Services.ViewService.Remove(Type, BosUISettings.Instance.ViewCloseDelay);
				closeButton.interactable = false;
				Services.GetService<ISoundService>().PlayOneShot(SoundName.click);
			});
		}


		private void OnEnable()
		{
			GameEvents.ViewHided += OnViewChanged;
		}

		private void OnDisable()
		{
			GameEvents.ViewHided -= OnViewChanged;
		}

		public override void Setup(ViewData data)
		{
			base.Setup(data);
			FillAdButton();
			_generatorInfo = data.UserData as GeneratorInfo;
			
			var generatorIconData =  ResourceService
				.GeneratorLocalData
				.GetLocalData(_generatorInfo.GeneratorId)
				.GetIconData(Planets.CurrentPlanet.Id);
			
			var generatorSprite = ResourceService.GetSpriteByKey(generatorIconData.big_icon_id);
			TransportImage.sprite = generatorSprite;
			TransportImage.SetNativeSize();

			TransportName.text = _generatorInfo.Data.Name;
			
			if (_generatorInfo.IsAutomatic)
			{
				Profit.text = $"{_generatorInfo.ProfitResult.ValuePerSecond.ToCurrencyNumber()}/sec";
			}
			else
			{
				System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(_generatorInfo.AccumulateInterval);
				string interval = "";
				
				if (timeSpan.TotalHours > 1)
				{
					interval = string.Format("{0} h", timeSpan.Hours);
				}
				else if (timeSpan.TotalMinutes > 1)
				{
					interval = string.Format("{0} m", timeSpan.Minutes);
				}
				else if(timeSpan.TotalSeconds > 1)
				{
					interval = string.Format("{0} sec", timeSpan.Seconds);
				}
				else
				{
					interval = "sec";
				}

				Profit.text = $"{_generatorInfo.ProfitResult.ValuePerRound.ToCurrencyNumber()} / {interval}";
			}

			UpdateBooster();
		}

		
		private void UpdateBooster()
		{
			foreach (var boost in Boosts)
			{
				boost.Fill(_generatorInfo);
			}
		}

		private void FillAdButton()
		{
			IX2ProfitService service = Services.GetService<IX2ProfitService>();
			
			AdButton.SetListener(() => {
				Sounds.PlayOneShot(SoundName.click);
				ViewService.Show(ViewType.X2ProfitView);
			});

			Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => {
				int interval = service.AvailableAfterInterval;
				if (interval == 0) {
					AdTime.text = "00:00:00";
				}
				else {
					TimeSpan ts = TimeSpan.FromSeconds(interval);
					AdTime.text = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
				}
			}).AddTo(gameObject);
		}

		private void OnViewChanged(ViewType viewType)
		{
			if (_generatorInfo != null)
			{
				UpdateBooster();
			}
		}


		public override CanvasType CanvasType => CanvasType.UI;
		public override bool IsModal => true;
		public override ViewType Type => ViewType.TransportInfoView;
	}
}

