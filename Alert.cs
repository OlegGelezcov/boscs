using Bos;
using Bos.Data;
using System;
using System.Linq;
using UnityEngine;

/*
[Serializable]
public class Alert
{
	public GameObject sceneObject;
	public GameObject menuObject;
	public AlertType type;
	private bool _show;

	public void TryShow(GameManager gm, bool isMenu)
	{
		SetActive(CheckShow(gm), isMenu);
	}

	public void SetActive(bool active, bool isMenu)
	{
		if (sceneObject != null)
			sceneObject.SetActive(active && !isMenu);
		if (menuObject != null)
			menuObject.SetActive(active && isMenu);
	}

	protected virtual bool CheckShow(GameManager gm)
	{
		return false;
	}
}*/


/*
[Serializable]
public class AlerManager : Alert
{
	protected override bool CheckShow(GameManager gm)
	{
		var pdata = gm.Managers.BalanceManager.PlayerData;
        var curBal = GameServices.Instance.PlayerService.CompanyCash.Value; //gm.Managers.BalanceManager.Balance.Value;
        IManagerRepository managerRepository = GameServices.Instance.ResourceService.Managers;

        foreach (var manager in managerRepository.ManagerCollections)
		{
			if (manager.BaseCost <= curBal && 
                (false == GameServices.Instance.ManagerService.IsHired(manager.Id)) && 
                GameServices.Instance.TransportService.HasUnits(manager.Id))
				return true;
		} 
		return false;
	}
}*/


/*
[Serializable]
public class AlerUpgrades : Alert
{
	protected override bool CheckShow(GameManager gm)
	{
		return gm.UpgradeScreen.CanShowUpgradeAlert();
	}
}*/

/*
[Serializable]
public class AlerRewards : Alert
{
	protected override bool CheckShow(GameManager gm)
	{
		var pdata = gm.Managers.BalanceManager.PlayerData;
		return pdata.AvailableRewards > 0;
	}
}*/

    /*
[Serializable]
public class AlerInvestor : Alert
{
	public static bool InvestorAlreadyShown;
	private double _lastInvestorsCount;
	
	protected override bool CheckShow(GameManager gm)
	{
        var targetInvestors = GameServices.Instance.InvestorService.CalculateCashFromInvestors(); //gm.Managers.BalanceManager.CalcBonusPercentage();
		if (InvestorAlreadyShown)
		{
			if ((int) (targetInvestors / _lastInvestorsCount) < 2) return false;
			InvestorAlreadyShown = false;
			_lastInvestorsCount = targetInvestors;
			return true;
		}
		_lastInvestorsCount = targetInvestors;
		return targetInvestors >= 1;
	}
}*/

    /*
[Serializable]
public class AlerShop : Alert
{
	public static bool OpenCoinScreen;
	private int _oldCoin = 0;
	protected override bool CheckShow(GameManager gm)
	{
        //var coins = gm.Managers.IAPManager.Coins.Value;
        int coins = GameServices.Instance.PlayerService.Currency.Coins;

		if (OpenCoinScreen)
		{
			var available = gm.Managers.IAPManager.IapScreen.shopItems.Where(val => val.Price <= coins);
			if (available.Count() != 0)
				_oldCoin = gm.Managers.IAPManager.IapScreen.shopItems.Where(val => val.Price <= coins).Max(val => val.Price);
			
			return false;
		}

		var item = gm.Managers.IAPManager.IapScreen.shopItems.FirstOrDefault(val => val.Price <= coins);
		if (item == null)
		{
			_oldCoin = 0;
			return false;
		}
		var newItem = gm.Managers.IAPManager.IapScreen.shopItems.FirstOrDefault(val => val.Price <= coins && val.Price > _oldCoin);
		return newItem != null;
	}
}*/

/*
[Serializable]
public class AlerRace : Alert
{
	private bool _showBy20min = false;
	private bool _needShowByReset = true;
	private TimeSpan _timer = TimeSpan.Zero;
	public static bool OpenRaceScreen;
	
	protected override bool CheckShow(GameManager gm)
	{
		if (OpenRaceScreen)
		{
			_needShowByReset = false;
			_showBy20min = false;
			_timer = TimeSpan.Zero;
			return false;
		}

		if (LocalData.TriesRefundDateRace <= DateTime.Now && _needShowByReset)
		{
			return true;
		}

		if (LocalData.TriesRefundDateRace > DateTime.Now && !_needShowByReset)
		{
			_needShowByReset = true;
		}


		if (_timer.TotalMinutes < 20)
		{
			_timer = _timer.Add(TimeSpan.FromSeconds(1));
		}
		else
		{
			return true;
		}

		return false;
	}
}*/


/*
[Serializable]
public class AlerSlotMachine : Alert
{
	private bool _showBy20min = false;
	private bool _needShowByReset = true;
	private TimeSpan _timer = TimeSpan.Zero;
	public static bool OpenCasinoScreen;
	
	protected override bool CheckShow(GameManager gm)
	{
		if (OpenCasinoScreen)
		{
			_needShowByReset = false;
			_showBy20min = false;
			_timer = TimeSpan.Zero;
			return false;
		}

		
		if (LocalData.TriesRefundDateCasino <= DateTime.Now && _needShowByReset)
		{
			return true;
		}

		if (LocalData.TriesRefundDateCasino > DateTime.Now && !_needShowByReset)
		{
			_needShowByReset = true;
		}


		if (_timer.TotalMinutes < 20)
		{
			_timer = _timer.Add(TimeSpan.FromSeconds(1));
		}
		else
		{
			return true;
		}

		return false;
	}
}*/



public enum AlertType
{
	Manager,
	Upgrades,
	Rewards,
	Investor,
	Shop,
	Race,
	SlotMachine
}