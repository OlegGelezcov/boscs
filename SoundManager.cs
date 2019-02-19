using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public AudioMixer Mixer;

    public AudioClip[] OneShots;
    public AudioSource SFXAudioSource;
    public AudioSource backgroundMusicSource;

    public bool DisableAudioWhenInEditor;

    private readonly Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();

    private Dictionary<string, AudioClip> Clips {
        get {
            if(cachedClips.Count == 0) {
                foreach(var clip in OneShots) {
                    cachedClips.Add(clip.name, clip);
                }
            }
            return cachedClips;
        }
    }

    private SoundManager _instance;
    public static SoundManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    //public bool IsOn
    //{
    //    get { return Convert.ToBoolean(PlayerPrefs.GetInt("mute", 0)); }
    //    set { PlayerPrefs.SetInt("mute", Convert.ToInt32(value)); }
    //}

    
    
    private void Start()
    {
        if (DisableAudioWhenInEditor && Application.isEditor)
        {
            gameObject.SetActive(false);
        }

        //if (IsOn)
        //{
        //    Mute();
        //}
    }

    /*
    public void Mute()
    {
        AudioListener.volume = 0;
    }

    public void Unmute()
    {
        AudioListener.volume = 1.0f;
    }*/

    public void PlayOneShot(string name)
    {
        /*
        foreach (var x in OneShots)
        {
            if (x.name == name)
            {
                SFXAudioSource.PlayOneShot(x);
                return;
            }
        }*/

        if(Clips.ContainsKey(name)) {
            SFXAudioSource.PlayOneShot(Clips[name]);
        }
    }
}
