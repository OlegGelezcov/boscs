using Bos;
using Bos.UI;
using Ozh.Tools.Functional;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Manager))]
public class ManagerView : GameBehaviour
{
    private Manager _man;
    private Animator _buttonAnimator;

    public Color DisabledTextColor;
    //public Sprite Sprite;
    //public BalanceManager BalanceManager;

    [Header("UI Elements")]
    public Image Icon;
    public Text NameView;
    public Text PriceView;
    public Text DescriptionView;
    public Button BuyButton;
    public GameObject Checkmark;

    private Text _hireText;
    
    private ReactiveProperty<bool> _bought = new ReactiveProperty<bool>();
    
    public override void Start()
    {
        
        _man = GetComponent<Manager>();

        //guard from setting teleport manager before mars
        if(_man.GeneratorIdToManage.IsTeleport() && !Planets.IsMarsOpened ) {
            gameObject.SetActive(false);
            return;
        }

        _buttonAnimator = BuyButton.GetComponent<Animator>();

        var generator = Services.ResourceService.Generators.GetGeneratorData(_man.GeneratorIdToManage); //GameData.instance.GetGenerator(_man.GeneratorIdToManage);
        DescriptionView.text = generator.Name.GetLocalizedString().ToUpper() + " " + "Manager".GetLocalizedString().ToUpper();

        if (Services.ManagerService.IsHired(_man.GeneratorIdToManage))
        {
            BuyButton.gameObject.SetActive(false);
            Checkmark.SetActive(true);
            _bought.Value = true;
        }

        Services.ViewService.Utils.ApplyManagerIcon(Icon, Services.GenerationService.GetGetenerator(_man.GeneratorIdToManage), true);
        NameView.SetManagerName(_man.GeneratorIdToManage);

        PriceView.text = BosUtils.GetStandardCurrencyString(_man.Cost);
        _hireText = BuyButton.GetComponentInChildren<Text>();
        _bought.Subscribe(val => { UpdateState(Services.PlayerService.CompanyCash.Value); });
        OnCompanyCashChanged(new CurrencyNumber(), Services.PlayerService.CompanyCash);
    }

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.CompanyCashChanged += OnCompanyCashChanged;
    }

    public override void OnDisable() {
        base.OnDisable();
        GameEvents.CompanyCashChanged -= OnCompanyCashChanged;
    }

    private void OnCompanyCashChanged(CurrencyNumber oldValue, CurrencyNumber newValue)
        => UpdateState(newValue.Value);

    private void UpdateState(double balance)
    {
        _bought.Value = Services.ManagerService.IsHired(_man.GeneratorIdToManage);

        if (balance >= _man.Cost
            && Services.TransportService.HasUnits(_man.GeneratorIdToManage)
            && !_bought.Value)
        {
            BuyButton.interactable = true;
            PriceView.color = Color.white;
            if (!_bought.Value)
                Checkmark.SetActive(true);

            _hireText.color = new Color32(255, 238, 33, 255);
          
        }
        else
        {
            if (_bought.Value)
            {
                BuyButton.gameObject.SetActive(false);
                Checkmark.SetActive(true);
            }
            else
            {
                BuyButton.interactable = false;
                PriceView.color = DisabledTextColor;
                Checkmark.SetActive(false);
            }     
            _hireText.color = new Color32(165, 161, 103, 255);
            PriceView.color = Color.white;
        }
    }

    public void HireManager()
    {
        Services.ManagerService.HireManager(_man.GeneratorIdToManage);
        _buttonAnimator.SetTrigger("exit");
        Checkmark.SetActive(true);
        _bought.Value = true;
    }
}
