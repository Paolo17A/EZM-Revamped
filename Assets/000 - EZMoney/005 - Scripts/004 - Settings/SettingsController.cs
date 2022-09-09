using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [field: SerializeField] private SettingsCore SettingsCore { get; set; }
    [field: SerializeField] private SettingsData SettingsData { get; set; }

    private void OnEnable()
    {
        SettingsCore.onMusicVolumeValueChange += MusicVolumeValueChange;
        SettingsCore.onEffectsVolumeValueChange += EffectsVolumeValueChange;
    }

    private void OnDisable()
    {
        SettingsCore.onMusicVolumeValueChange -= MusicVolumeValueChange;
        SettingsCore.onEffectsVolumeValueChange -= EffectsVolumeValueChange;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            SettingsCore.MusicVolumeValue = PlayerPrefs.GetFloat("MusicVolume");
        else
            SettingsCore.MusicVolumeValue = 0.5f;

        if (PlayerPrefs.HasKey("EffectsVolume"))
            SettingsCore.EffectsVolumeValue = PlayerPrefs.GetFloat("EffectsVolume");
        else
            SettingsCore.EffectsVolumeValue = 0.5f;

        if (!GameManager.Instance.BGMAudioManager.IsPlaying)
            GameManager.Instance.BGMAudioManager.SwitchToLoadingMusic();
    }

    private void MusicVolumeValueChange(object sender, EventArgs e)
    {
        SettingsData.MusicVolume = SettingsCore.MusicVolumeValue;
        SettingsCore.MusicVolumeSlider.value = SettingsData.MusicVolume;
       GameManager.Instance.BGMAudioManager.AudioSource.volume = SettingsData.MusicVolume;

        PlayerPrefs.SetFloat("MusicVolume", SettingsData.MusicVolume);
    }
    private void EffectsVolumeValueChange(object sender, EventArgs e)
    {
        SettingsData.EffectsVolume = SettingsCore.EffectsVolumeValue;
        SettingsCore.EffectsVolumeSlider.value = SettingsData.EffectsVolume;
        GameManager.Instance.SFXAudioManager.AudioSource.volume = SettingsData.EffectsVolume;

        PlayerPrefs.SetFloat("EffectsVolume", SettingsData.EffectsVolume);
    }
}
