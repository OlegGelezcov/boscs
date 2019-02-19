using Bos;
using UnityEngine;
using UnityEngine.UI;

public class RewardView : GameBehaviour
{
    public Image Image;
    public Text Name;

    private Reward _reward;
    private bool _claimed = false;

    public virtual void Activate(Reward reward)
    {
        gameObject.SetActive(true);
        Image.sprite = reward.Icon;
        Name.text = reward.Name;
        _reward = reward;
        Claim();
    }

    private void Claim()
    {
        if (_claimed)
            return;

        var q = GameObject.Find("LocalEventManager");
        var bman = q.GetComponent<LocalEventManager>();

        var _allManRef = new AllManagers()
        {
            GameManager = bman.GameManager,
            UIManager = bman.UIManager
        };

        _reward.Apply(_allManRef);
        _claimed = true;
    }

    public void Reset()
    {
        _claimed = false;
    }
}
