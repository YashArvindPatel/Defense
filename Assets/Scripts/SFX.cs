using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class SFX : MonoBehaviour
{
    public static SFX instance;

    private void Awake()
    {
        instance = this;
    }

    public AudioSource musicSource;
    public AudioSource audioSource;
    [Header("Open/Close Clips")]
    public AudioClip[] openClips;
    public AudioClip[] closeClips;
    public bool[] openSound;
    [Header("Single Clips")]
    public AudioClip[] singleClips;

    public SliderManager musicManager;
    public SliderManager sfxManager;

    /// <summary>
    /// Open/Close Clips: Settings Btn, Shop Btns, Vibration Btn,
    /// </summary>

    private void Start()
    {
        musicSource.volume = Mathf.Clamp01(PlayerSettings.music / 100);
        audioSource.volume = Mathf.Clamp01(PlayerSettings.sfx / 100);

        IncreaseMusicOverTime(1f);
    }

    public void ReduceMusicOverTime(float time, float value = 0f)
    {
        if (musicSource != null && musicManager.mainSlider.value > value * 100f)
            LeanTween.value(musicSource.volume, value, time).setOnUpdate(x => { if (musicSource != null) musicSource.volume = x; });
    }

    public void IncreaseMusicOverTime(float time, float value = 0f)
    {
        LeanTween.value(value, PlayerSettings.music / 100, time).setOnUpdate(x => { if (musicSource != null) musicSource.volume = x; });
    }

    //For Open Close Sound Buttons
    public void PlaySoundClip(int index)
    {
        if (!openSound[index])
            audioSource.PlayOneShot(openClips[index]);
        else
            audioSource.PlayOneShot(closeClips[index]);

        openSound[index] = !openSound[index];
    }

    //For Single Sound Buttons
    public void PlaySingleSoundClip(int index)
    {
        audioSource.PlayOneShot(singleClips[index]);
    }

    public void ChangeSFXVolume()
    {
        audioSource.volume = Mathf.Clamp01(sfxManager.mainSlider.value / 100);
    }

    public void ChangeMusicVolume()
    {
        musicSource.volume = Mathf.Clamp01(musicManager.mainSlider.value / 100);
    }
}
