using UniRx;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ManagementViewSwipeItemPatcher : MonoBehaviour {

    public int frameCount = 3;
    public CanvasGroup canvasGroup;

    private void Start() {
        Observable.EveryUpdate().DelayFrame(frameCount).Take(1).Subscribe(tick => {
            canvasGroup.alpha = 1;
            Debug.Log($"make canvas non transparent on {gameObject.name}");
        }).AddTo(gameObject);
    }

    
}
