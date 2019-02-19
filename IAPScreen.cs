using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IAPScreen : MonoBehaviour
{

	public Button CoinShop, UseCoin;
	public List<ShopItem> shopItems;
	
	public GameObject ShopContent, UseContent;
	private void Awake()
	{
		CoinShop.onClick.RemoveAllListeners();
		UseCoin.onClick.RemoveAllListeners();
		
		CoinShop.onClick.AddListener(() =>
		{
			ShopContent.SetActive(true);
			UseContent.SetActive(false);
		});
		
		UseCoin.onClick.AddListener(() =>
		{
			ShopContent.SetActive(false);
			UseContent.SetActive(true);
		});
	}
}
