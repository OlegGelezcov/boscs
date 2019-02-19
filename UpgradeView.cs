using Bos;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Upgrade))]
public class UpgradeView : ReactiveMonoBehaivor
{
    private Upgrade _upgrade;

    public Color DisabledTextColor;
    public Sprite Sprite;

    [Header("UI Elements")]
    public Image Icon;
    public Text NameView;
    public Text PriceView;
    public Text UpgradeTextView;
    public Button BuyButton;
    public Image Background;

    public bool CanBuy { get; private set; }
    public double RealtimeCost { get; private set; }

    public double OwnedUpgrades
    {
        get
        {
            return CalcOwnedUpgrades(_upgrade);
        }
    }


    public int UpgradeLevel
    {
        get
        {
            return CalcUpgradeLevel(_upgrade);
        }
    }

    public string PlayerPrefsKey
    {
        get
        {
            return string.Format("upgradeView_siblingIndex_{0}_{1}_{2}", _upgrade.GeneratorIdToUpgrade, name, _upgrade.CostType);
        }
    }

    private void Start()
    {
        _upgrade = GetComponent<Upgrade>();

        Icon.sprite = Sprite;

        var g = PlayerPrefs.GetInt(PlayerPrefsKey, -1);
        if (g != -1)
        {
            transform.SetSiblingIndex(g);
        }

        switch (_upgrade.UpgradeType)
        {
            case UpgradeType.Profit:
                UpgradeTextView.text = string.Format("Profit x{0}", _upgrade.ProfitMultiplier);
                break;
            case UpgradeType.Time:
                UpgradeTextView.text = string.Format("Speed x{0}", _upgrade.TimeMultiplier);
                break;
        }

        OnCompanyCashChanged(new CurrencyNumber(), GameServices.Instance.PlayerService.CompanyCash);
    }

    private void OnEnable() {
        GameEvents.CompanyCashChanged += OnCompanyCashChanged;
    }

    private void OnDisable() {
        GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
    }

    private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue) {
        Reload(newValue.Value);
    }

    public double CalcOwnedUpgrades(Upgrade up)
    {
        if (up.UpgradeType == UpgradeType.Profit)
        {
            IGenerationService generationService = GameServices.Instance.GenerationService;

            if (generationService.Generators.Contains(up.GeneratorIdToUpgrade)) {
                return generationService.Generators.GetGeneratorInfo(up.GeneratorIdToUpgrade).ProfitBoosts.Value; //GetProfitValue(up.GeneratorIdToUpgrade);
            }
        }

        if (up.UpgradeType == UpgradeType.Time)
        {
            if(GameServices.Instance.GenerationService.Generators.Contains(up.GeneratorIdToUpgrade)) {
                return GameServices.Instance.GenerationService.Generators.GetGeneratorInfo(up.GeneratorIdToUpgrade).TimeBoosts.Value; //GetTimeValue(up.GeneratorIdToUpgrade);
            }
        }

        return 0;
    }
    public int CalcUpgradeLevel(Upgrade up)
    {
        return GameServices.Instance.UpgradeService.GetUpgradeLevel(up);
    }


    private void Reload(double balance)
    {
        NameView.text = _upgrade.Names.Length > UpgradeLevel ? _upgrade.Names[UpgradeLevel] : _upgrade.OverflowName;
        var cost = _upgrade.CalculateCost(UpgradeLevel);
        var enabledCondition = false;

        ITransportUnitsService transportService = GameServices.Instance.TransportService;

        switch (_upgrade.CostType)
        {
            case CostType.Balance:
                if (_upgrade.GeneratorIdToUpgrade != -1)
                    enabledCondition = balance >= cost && transportService.HasUnits(_upgrade.GeneratorIdToUpgrade);
                else
                    enabledCondition = balance >= cost;

                PriceView.text = GameServices.Instance.Currency.CreatePriceString(cost, separateWithEndl: false, separator: " ", useDecimalFormat: true);
                break;
            case CostType.Investors:
                if (_upgrade.GeneratorIdToUpgrade != -1)
                    enabledCondition = GameServices.Instance.PlayerService.Securities.Value >= cost && transportService.HasUnits(_upgrade.GeneratorIdToUpgrade);
                else
                    enabledCondition = GameServices.Instance.PlayerService.Securities.Value >= cost;
                PriceView.text = string.Format("{0} investor(s)", cost);
                break;
            case CostType.Coins: {
                    var playerService = GameServices.Instance.PlayerService;
                    if (_upgrade.GeneratorIdToUpgrade != -1)
                        enabledCondition = playerService.Coins >= cost && transportService.HasUnits(_upgrade.GeneratorIdToUpgrade);
                    else
                        enabledCondition = playerService.Coins >= cost;
                    PriceView.text = string.Format("{0} coin(s)", cost);
                }
                break;
        }

        RealtimeCost = cost;

        if (enabledCondition)
        {
            BuyButton.interactable = true;
            PriceView.color = Color.white;
            UpgradeTextView.color = Color.white;
            CanBuy = true;
        }
        else
        {
            BuyButton.interactable = false;
            PriceView.color = DisabledTextColor;
            UpgradeTextView.color = DisabledTextColor;
            CanBuy = false;
        }

    }

    public void BuyUpgrade()
    {
        GameServices.Instance.UpgradeService.BuyUpgrade(_upgrade);
        var slideAndCollapse = GetComponent<SlideAndCollapse>();
        slideAndCollapse.Animate(() =>
        {
            transform.SetAsLastSibling();
            var g = transform.GetSiblingIndex();
            PlayerPrefs.SetInt(PlayerPrefsKey, g);
        });
    }

    public void BuyUpgradeWithoutAnimation()
    {
        GameServices.Instance.UpgradeService.BuyUpgrade(_upgrade);
        transform.SetAsLastSibling();
        var g = transform.GetSiblingIndex();
        PlayerPrefs.SetInt(PlayerPrefsKey, g);
    }
}
