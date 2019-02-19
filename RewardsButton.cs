using Bos;

public class RewardsButton : GameBehaviour
{
    public LootboxOpenView LootboxOpenView; 

    public override void Start()
    {
        base.Start();
    }

    public void Click()
    {
        if (Services.RewardsService.AvailableRewards > 0) {
            LootboxOpenView.Prepare();
        }
    }
}
