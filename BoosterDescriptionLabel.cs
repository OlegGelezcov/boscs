using Bos;
using UnityEngine;
using UnityEngine.UI;

public class BoosterDescriptionLabel : GameBehaviour
{
    public Text Text; 
    private float frameTime = 0;


    public override void Update() {
        base.Update();
        frameTime -= Time.deltaTime;
        if(frameTime <= 0f ) {
            frameTime += 0.53f;
            Text.text = Services.PlayerService.IsHasMicromanager ?
                "BOOST.UNLOCK".GetLocalizedString() :
                "BOOST.LOCK".GetLocalizedString();
        }

    }
}
