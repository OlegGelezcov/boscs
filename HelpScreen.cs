using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpScreen : MonoBehaviour
{
    public float[] ScrollPositions;
    public ScrollRect Scroll;

    public void JumpTo(int i)
    {
        var q = ScrollPositions[i];
        Hashtable ht = iTween.Hash(
               "from", Scroll.verticalScrollbar.value,
               "to", q,
               "time", .5f,
               "onupdate", "UpdateProgressFill",
               "easetype", iTween.EaseType.easeInOutCubic);

        iTween.ValueTo(gameObject, ht);
    }

    private void UpdateProgressFill(float newValue)
    {
        Scroll.verticalScrollbar.value = newValue;
    }
}
