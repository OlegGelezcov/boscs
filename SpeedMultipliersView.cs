using Bos;
using UnityEngine;
using UnityEngine.UI;
public class SpeedMultipliersView : GameBehaviour
{
    private Text _view;

    public override void Start()
    {
        _view = GetComponent<Text>();
    }

    private void FixedUpdate()
    {
        _view.text = string.Format("x{0}", Services.GenerationService.Generators.TimeBoosts.Value);
    }
}
