using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AchievementPopup : MonoBehaviour
{
    private Animator _animator;
    private bool _isVisible;

    public float MaybeShowChance = 0.2f;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Show()
    {
        _animator.SetTrigger("proc");
        _isVisible = true;
    }

    public void MaybeShow()
    {
        if ((1 - MaybeShowChance) < Random.Range(0, 1f))
        {
            Show();
        }
    }

    public void Hide()
    {
        _animator.SetTrigger("proc");
        _isVisible = false;
    }

    public void Tap()
    {
        if (!_isVisible)
            Show();
        else
            Hide();
    }
}
