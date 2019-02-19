using Bos;
using Bos.UI;
using UnityEngine.UI;

namespace Bos
{
	public class EnhanceBoostView : BoostViewBase
	{
		public Image TransportIcon;
		private int _coinUpgradeEnhanceStartId = 880;
		
		protected override void FillInner(GeneratorInfo generatorInfo)
		{
			var generatorIconData =  ResourceService
				.GeneratorLocalData
				.GetLocalData(generatorInfo.GeneratorId)
				.GetIconData(Planets.CurrentPlanet.Id);
			var generatorSprite = ResourceService.GetSpriteByKey(generatorIconData.icon_id);
			TransportIcon.sprite = generatorSprite;
		}

		protected override void Invoke(GeneratorInfo generatorInfo)
		{
			Services.ViewService.Show(ViewType.UpgradesView, new ViewData
			{
				UserData = new UpgradeViewData
				{
					TabName = UpgradeTabName.CoinsUpgrades,
					CoinUpgradeId = _coinUpgradeEnhanceStartId + generatorInfo.GeneratorId,
				}
			});
		}

		protected override bool CheckState(GeneratorInfo generatorInfo)
		{
			return generatorInfo.IsEnhanced;
		}
	}
}

