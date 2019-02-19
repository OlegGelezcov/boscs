using Bos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfitMultipliersView : GameBehaviour
{

    private Text _profitMulView;

    public override void Start()
    {
        _profitMulView = GetComponent<Text>();
    }

    public override void Update() {
        base.Update();
        _profitMulView.text = string.Format("x{0}", Services.GenerationService.Generators.ProfitBoosts.Value);
    }

}
