using Bos;
using GhostCore.MVVM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeViewModel : ViewModelBase
{
    private string _title;
    private string _displayPrice;
    private double _price;
    private UpgradeType _upType;
    private string _upgradeDisplayString;
    private int _upgradeValue;
    private Sprite _sprite;

    public int GeneratorIdToUpgrade { get; set; }
    public int Id { get; set; }

    public string Title
    {
        get { return _title; }
        set { _title = value; OnPropertyChanged(nameof(Title)); }
    }

    public double Price
    {
        get { return _price; }
        set
        {
            _price = value;
            if (UseInvestors)
                DisplayPrice = Currencies.Investors.CreatePriceString(_price, false, " ").ToUpper();
            else
                DisplayPrice = Currencies.DefaultCurrency.CreatePriceString(_price, false, " ").ToUpper();

            OnPropertyChanged(nameof(Price));
        }
    }
    public UpgradeType UpgradeType
    {
        get { return _upType; }
        set
        {
            _upType = value;
            UpgradeDisplayString = UpgradeStringHelper.CreateUpgradeString(_upgradeValue, _upType);
            OnPropertyChanged(nameof(UpgradeType));
        }
    }
    public int UpgradeValue
    {
        get { return _upgradeValue; }
        set
        {
            _upgradeValue = value;
            UpgradeDisplayString = UpgradeStringHelper.CreateUpgradeString(_upgradeValue, _upType);
            OnPropertyChanged(nameof(UpgradeValue));
        }
    }

    public string UpgradeDisplayString
    {
        get { return _upgradeDisplayString; }
        set { _upgradeDisplayString = value; OnPropertyChanged(nameof(UpgradeDisplayString)); }
    }
    public string DisplayPrice
    {
        get { return _displayPrice; }
        set { _displayPrice = value; OnPropertyChanged(nameof(DisplayPrice)); }
    }
    public Sprite Sprite
    {
        get { return _sprite; }
        set { _sprite = value; OnPropertyChanged(nameof(Sprite)); }
    }

    public string Description
    {
        get
        {
            switch (UpgradeType)
            {
                case UpgradeType.Profit:
                    return $"Increase your profit by x{_upgradeValue}";
                case UpgradeType.Time:
                    return $"Increase your speed by x{_upgradeValue}";
                case UpgradeType.InvestorEffectiveness:
                    return $"Increase investor effectiveness by +{_upgradeValue}%";
                case UpgradeType.FreeGenerators:
                    return $"Adds another {_upgradeValue} units";
                case UpgradeType.FutureBalance:
                    break;
                case UpgradeType.AdvertisementBoost:
                    break;
                case UpgradeType.Reward:
                    break;
                default:
                    break;
            }

            return null;
        }
    }

    public bool UseInvestors { get; set; }

    public bool CanBuy
    {
        get
        {
            //var bman = (Parent as UpgradeScreen).BalanceManager;
            var resource = GameServices.Instance.PlayerService.CompanyCash.Value; //bman.Balance.Value;
            var cost = _price;

            if (UseInvestors)
            {
                resource = GameServices.Instance.PlayerService.Securities.Value; //bman.PlayerData.Investors;
            }

            if (GeneratorIdToUpgrade > -1)
                return resource >= cost && GameServices.Instance.TransportService.HasUnits(GeneratorIdToUpgrade);
            else
                return resource >= cost;
        }
    }

    public void Buy()
    {
        //(Parent as UpgradeScreen).BuyUpgrade(this);
    }
}

public static class UpgradeStringHelper
{
    public static string CreateUpgradeString(int value, UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Profit:
                return $"Profit x{value}";
            case UpgradeType.Time:
                return $"Speed x{value}";
            case UpgradeType.FutureBalance:
                return $"Future +{value}";
            case UpgradeType.AdvertisementBoost:
                return $"Ad +{value}";
            case UpgradeType.InvestorEffectiveness:
                return $"Bonus {value}%";
            case UpgradeType.FreeGenerators:
                return $"+{value} units";
            default:
                return null;
        }
    }
}
