using Bos;
using UnityEngine.UI;

public class RewardCount : GameBehaviour
{
    private Text _text;

    public override void Start()
    {
        _text = GetComponent<Text>();
    }

    public override void Update() {
        base.Update();
        _text.text = $"{Services.RewardsService.AvailableRewards}x";
    }
}
