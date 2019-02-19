using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParticleEffect : MonoBehaviour
{
    public ParticleSystem Animation;
    public float ClickDelay = 0.5f;

    private float _nextAnimTime;

    public void TriggerAnimation()
    {
        if (Time.timeSinceLevelLoad >= _nextAnimTime)
        {
            Animation.Play();
            _nextAnimTime = Time.timeSinceLevelLoad + ClickDelay;
        }
    }

}
