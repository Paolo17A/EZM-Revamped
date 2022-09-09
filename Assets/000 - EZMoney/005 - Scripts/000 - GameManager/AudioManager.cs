using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //================================================================================
    [field: SerializeField] public AudioSource AudioSource { get; set; }

    [Header("BACKGROUND MUSIC")]
    [SerializeField] private AudioClip loadingBGM;
    [SerializeField] private AudioClip gameplayBGM;

    [field: Header("DEBUGGER")]
    [field: SerializeField] public bool IsPlaying { get; set; }
    //================================================================================

    public void PlayAudio()
    {
        AudioSource.Play();
    }

    public void SwitchToGameplayMusic()
    {
        AudioSource.Stop();
        AudioSource.clip = gameplayBGM;
    }

    public void SwitchToLoadingMusic()
    {
        AudioSource.Stop();
        AudioSource.clip = loadingBGM;
        PlayAudio();
        AudioSource.mute = false;
        IsPlaying = true;
    }

}
