using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ButtonLabelInteractableUpdater : MonoBehaviour
{
    private Button _button;

    public Text Label;

    public Color EnabledColor;
    public Color DisabledColor;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    private void FixedUpdate()
    {
        if (Label == null)
            return;

        if (_button.interactable)
        {
            Label.color = EnabledColor;
        }
        else
        {
            Label.color = DisabledColor;
        }
    }
}
