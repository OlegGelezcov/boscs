using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bos.Data;
using Bos.UI;
using UnityEngine;
using UnityEngine.UI;


namespace Bos
{
	public class ShopBoostView : BoostViewBase 
	{
		public Image Icon;
		public Text CountText;
		public int CoinUpgradeId;
		public bool IsTransportBoost;

		protected override void FillInner(GeneratorInfo generatorInfo)
		{
			if (IsTransportBoost)
			{
				CoinUpgradeId = 870 + generatorInfo.GeneratorId;
				var generatorIconData =  ResourceService
					.GeneratorLocalData
					.GetLocalData(generatorInfo.GeneratorId)
					.GetIconData(Planets.CurrentPlanet.Id);
				var generatorSprite = ResourceService.GetSpriteByKey(generatorIconData.icon_id);
				Icon.sprite = generatorSprite;
			}
				

			var shopItem = GetShopItem();
			
			if (shopItem == null)
			{
				UnityEngine.Debug.LogError("Id not exist : " + CoinUpgradeId);
			}
			
			var boostInfo = GetBoostInfo(generatorInfo);
			if (boostInfo != null)
			{
				var count = boostInfo.Count;
				if (count > 1)
				{
					CountText.Activate();
					CountText.text = count.ToString();
				}
				else
				{
					CountText.Deactivate();
				}
			}
			else
			{
				CountText.Deactivate();
			}
		}

		protected override void Invoke(GeneratorInfo generatorInfo)
		{
			var shopItem = GetShopItem();
		
			Services.ViewService.Show(ViewType.UpgradesView, new ViewData
			{
				UserData = new UpgradeViewData
				{
					TabName = UpgradeTabName.CoinsUpgrades,
					CoinUpgradeId = shopItem?.Id ?? 0,
				}
			});
		}
		

		protected override bool CheckState(GeneratorInfo generatorInfo)
		{
			var boostsInfo = GetBoostInfo(generatorInfo);
			return boostsInfo.Count > 0;
		}

		private BosCoinUpgradeData GetShopItem()
		{
			return Services.ResourceService.CoinUpgrades.UpgradeList.FirstOrDefault(val => val.Id == CoinUpgradeId);
		}

		private List<BoostInfo> GetBoostInfo(GeneratorInfo generatorInfo)
		{
			var shopItem = GetShopItem();
			var boostId = ResolveBoostId(shopItem);
			if (shopItem.UpgradeType == UpgradeType.Profit)
			{
				if (IsTransportBoost)
				{
					return generatorInfo.ProfitBoosts.GetBoosts(boostId);
				}
				return  Services.GenerationService.Generators.ProfitBoosts.GetBoosts(boostId);
			}

			if (shopItem.UpgradeType == UpgradeType.Time)
			{
				if (IsTransportBoost)
				{
					return generatorInfo.TimeBoosts.GetBoosts(boostId);
				}
				return  Services.GenerationService.Generators.TimeBoosts.GetBoosts(boostId);
			}
			return null;
		}

		private string ResolveBoostId(BosCoinUpgradeData data)
		{
			switch (data.UpgradeType)
			{
				case UpgradeType.Profit:
				{
					if (data.GeneratorId < 0)
					{
						return data.IsPermanent ? BoostIds.GetPersistCoinUpgradeId(data.Id, false) : BoostIds.GetTempCoinUpgradeId(data.Id, false);
					}
					return data.IsPermanent ? BoostIds.GetPersistLocalCoinUpId(data.Id, false) : BoostIds.GetTempLocalCoinUpId(data.Id, false);
				}
				case UpgradeType.Time:
				{
					if (data.GeneratorId < 0)
					{
						return data.IsPermanent ? BoostIds.GetPersistCoinUpId(data.Id, false) : BoostIds.GetTempCoinUpId(data.Id, false);
					}
					return data.IsPermanent ? BoostIds.GetPersistCoinUpId(data.Id, false) : BoostIds.GetTempCoinUpId(data.Id, false);
				}
				default:
					return "";
			}
		}
	}
}

