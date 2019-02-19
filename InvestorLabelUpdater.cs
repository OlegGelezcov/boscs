using Bos;
using UnityEngine.UI;

public class InvestorLabelUpdater : GameBehaviour
{
    private Text _text;

    private string formatStr = null;
    private string FormatString
        => (formatStr != null) ? formatStr : (formatStr = Services.ResourceService.Localization.GetString("BONUS.INVESTORS"));

    public override void Start()
    {
        _text = GetComponent<Text>();
    }

    public override void Update()
    {
        _text.text = string.Format(FormatString, Services.InvestorService.Effectiveness * 100); //string.Format("BONUS.INVESTORS".GetLocale(LocalizationDataType.ui), GlobalRefs.PlayerData.InvestorEffectiveness * 100);
    }
}
