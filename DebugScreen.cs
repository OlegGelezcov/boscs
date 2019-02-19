using Bos;
using UnityEngine.UI;

public class DebugScreen : GameBehaviour
{
    public InputField MoneyText;

    public void AddMoney()
    {
        double res = 0;
        if (double.TryParse(MoneyText.text, out res))
        {
            Player.AddGenerationCompanyCash(res);
        }
    }
}
