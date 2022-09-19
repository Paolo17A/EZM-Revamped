using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDisplayController : MonoBehaviour
{
    //====================================================================================
    [SerializeField] private ProfileCore ProfileCore;
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private CharacterData CharacterData;

    [Header("PLAYFAB VARIABLES")]
    private GetUserDataRequest getUserData;
    private UpdateUserDataRequest updateUserData;
    private int failedCallbackCounter;
    //====================================================================================
    private void Awake()
    {
        getUserData = new GetUserDataRequest();
        updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
    }

    public void ChangeDisplayPicture()
    {
        if (GameManager.Instance.DebugMode)
        {
            ProfileCore.DisplayImage.sprite = CharacterData.displaySprite;
            PlayerData.DisplayPicture = CharacterData.animalID;
        }
        else
        {
            ProfileCore.DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        updateUserData.Data.Clear();
                        updateUserData.Data.Add("DisplayImage", CharacterData.animalID);
                        PlayFabClientAPI.UpdateUserData(updateUserData,
                            resultCallback =>
                            {
                                failedCallbackCounter = 0;
                                ProfileCore.HideLoadingPanel();
                                ProfileCore.DisplayImage.sprite = CharacterData.displaySprite;
                                PlayerData.DisplayPicture = CharacterData.animalID;
                            },
                            errorCallback =>
                            {
                                ErrorCallback(errorCallback.Error,
                                    ChangeDisplayPicture,
                                    () => ProcessError(errorCallback.ErrorMessage));
                            });
                    }
                    else
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        ChangeDisplayPicture,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

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
        ProfileCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        ProfileCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
