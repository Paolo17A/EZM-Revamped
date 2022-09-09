using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyBox;

public class SettingsCore : MonoBehaviour
{
    #region GETTERS AND SETTERS
    private event EventHandler musicVolumeValueChange;
    public event EventHandler onMusicVolumeValueChange
    {
        add
        {
            if(musicVolumeValueChange == null || !musicVolumeValueChange.GetInvocationList().Contains(value))
                musicVolumeValueChange += value;
        }
        remove => musicVolumeValueChange -= value;
    }
    public float MusicVolumeValue
    {
        get => musicVolumeValue;
        set
        {
            musicVolumeValue = value;
            musicVolumeValueChange?.Invoke(this, EventArgs.Empty);
        }
    }
    private event EventHandler effectsVolumeValueChange;
    public event EventHandler onEffectsVolumeValueChange
    {
        add
        {
            if (effectsVolumeValueChange == null || !effectsVolumeValueChange.GetInvocationList().Contains(value))
                effectsVolumeValueChange += value;
        }
        remove => effectsVolumeValueChange -= value;
    }
    public float EffectsVolumeValue
    {
        get => effectsVolumeValue;
        set
        {
            effectsVolumeValue = value;
            effectsVolumeValueChange?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
    //=============================================================================
    [Header("MUSIC")]
    [SerializeField][ReadOnly] private float musicVolumeValue;
    [field: SerializeField] public Slider MusicVolumeSlider { get; set; }

    [Header("EFFECTS")]
    [SerializeField][ReadOnly] private float effectsVolumeValue;
    [field: SerializeField] public Slider EffectsVolumeSlider { get; set; }
    //=============================================================================

    public void ChangeMusicVolume()
    {
        MusicVolumeValue = MusicVolumeSlider.value;
    }

    public void ChangeEffectsVolume()
    {
        EffectsVolumeValue = EffectsVolumeSlider.value;
    }
}
