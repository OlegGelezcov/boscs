using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bos;
using Ozh.Tools.Functional;
using UnityEngine;
using UnityEngine.UI;

public class ItemView : GameBehaviour
{
	public int productId;
	public Text price, desc;
	public Button buy;

	public bool usCash;
	
	public override void OnEnable()
	{
        Services.Inap.GetProductByResourceId(productId).Match(
            () => {
                return F.None;
            }, product => {
                price.text = product.metadata.localizedPriceString;
                return F.Some(product);
            });

		
		
		buy.onClick.RemoveAllListeners();
		buy.onClick.AddListener(() => Services.Inap.PurchaseProduct(ResourceService.Products.GetProduct(productId)));
		
		if (desc != null && usCash)
		{
            var p = ResourceService.Products.GetProduct(productId); //IAPManager.instance.GetStoreListingById(productId);
			if (p != null)
			{
				var split = Services.Currency.CreatePriceStringSeparated(Services.PlayerService.MaxCompanyCash * p.BonusValue);
				if (split.Length > 1)
					desc.text = $"{split[0]}\n{split[1]}";
				else
					desc.text = $"{split[0]}";
			}
		}
	}
}
