using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using PlayFab;
using PlayFab.ClientModels;
using System;
using MyBox;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] string _iOsAdUnitId = "Interstitial_iOS";
    string _adUnitId;

    private float bgVolumeValue;
    private float fxVolumeValue;

    [Header("SCENE VARIABLES")]
    [ReadOnly] public bool willSwitchScene;
    [ReadOnly] public string sceneToLoad;
    [ReadOnly] public bool adCurrentlyShowing;

    [Header("PLAYFAB VARIABLES")]
    private UpdateUserDataRequest updateUserData;
    private int failedCallbackCounter;

    void Awake()
    {
        updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        // Get the Ad Unit ID for the current platform:
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;
    }

    // Load content to the Ad Unit:
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // Show the loaded content in the Ad Unit:
    public void ShowAd()
    {
        // Note that if the ad content wasn't previously loaded, this method will fail
        Debug.Log("Showing Ad: " + _adUnitId);
        adCurrentlyShowing = true;
        bgVolumeValue = GameManager.Instance.BGMAudioManager.AudioSource.volume;
        fxVolumeValue = GameManager.Instance.SFXAudioManager.AudioSource.volume;
        GameManager.Instance.BGMAudioManager.AudioSource.volume = 0;
        GameManager.Instance.SFXAudioManager.AudioSource.volume = 0;
        Advertisement.Show(_adUnitId, this);
    }

    // Implement Load Listener and Show Listener interface methods: 
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // Optionally execute code if the Ad Unit successfully loads content.
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
        //GameManager.Instance.DisplayErrorPanel($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");

        LoadAd();
        GameManager.Instance.BGMAudioManager.AudioSource.volume = bgVolumeValue;
        GameManager.Instance.SFXAudioManager.AudioSource.volume = fxVolumeValue;
        adCurrentlyShowing = false;
        if (willSwitchScene)
            GameManager.Instance.SceneController.CurrentScene = sceneToLoad;

        sceneToLoad = "";
        willSwitchScene = false;
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        //GameManager.Instance.DisplayErrorPanel($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");

        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.

        LoadAd();
        GameManager.Instance.BGMAudioManager.AudioSource.volume = bgVolumeValue;
        GameManager.Instance.SFXAudioManager.AudioSource.volume = fxVolumeValue;
        adCurrentlyShowing = false;
        if (willSwitchScene)
            GameManager.Instance.SceneController.CurrentScene = sceneToLoad;

        sceneToLoad = "";
        willSwitchScene = false;
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) 
    {
        PlayerData.AdsWatched++;
        IncreaseAdsWatched();
        LoadAd();
        GameManager.Instance.BGMAudioManager.AudioSource.volume = bgVolumeValue;
        GameManager.Instance.SFXAudioManager.AudioSource.volume = fxVolumeValue;
        adCurrentlyShowing = false;
        if (willSwitchScene)
            GameManager.Instance.SceneController.CurrentScene = sceneToLoad;

        sceneToLoad = "";
        willSwitchScene = false;
    }

    #region PLAYFAB
    private void IncreaseAdsWatched()
    {
        if (GameManager.Instance.DebugMode)
            return;

        updateUserData.Data.Clear();
        updateUserData.Data.Add("Quests", PlayerData.SerializeCurrentQuestData());
        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    IncreaseAdsWatched,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    #endregion

    #region UTILITY
    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                ProcessError("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
