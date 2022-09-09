using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "SettingsData", menuName = "EZMoneyPH/Data/SettingsData")]
public class SettingsData : ScriptableObject
{
    #region GETTERS AND SETTERS
    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
        }
    }

    public float EffectsVolume
    {
        get => effectsVolume;
        set
        {
            effectsVolume = value;
        }
    }

    public bool MusicInitialized
    {
        get => musicInitialized;
        set => musicInitialized = value;
    }

    public bool EffectsInitialized
    {
        get => effectsInitialized;
        set => effectsInitialized = value;
    }
    #endregion

    //==========================================================================
    [Header("MUSIC")]
    [SerializeField][ReadOnly][Range(0, 1)] private float musicVolume;
    [SerializeField][ReadOnly] private bool musicInitialized;

    [Header("EFFECTS")]
    [SerializeField][ReadOnly][Range(0, 1)] private float effectsVolume;
    [SerializeField][ReadOnly] private bool effectsInitialized;
    //==========================================================================
}
