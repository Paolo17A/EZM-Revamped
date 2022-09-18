using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class LeaderboardCore : MonoBehaviour
{
    //=========================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private LobbyCore LobbyCore;
    [SerializeField] private List<PlacementController> Placements;

    private int failedCallbackCounter;
    private GetUserDataRequest getUserData;
    private GetLeaderboardRequest getLeaderboard;
    //=========================================================

    private void Awake()
    {
        getUserData = new GetUserDataRequest();
        getLeaderboard = new GetLeaderboardRequest();
    }

    public void InitializeLeaderboard()
    {
        foreach (PlacementController placement in Placements)
            placement.PlacementCG.alpha = 0;
        if (GameManager.Instance.DebugMode)
        { 
            Placements[0].gameObject.SetActive(true);
            Placements[0].NameTMP.text = PlayerData.DisplayName;
            Placements[0].GemTMP.text = PlayerData.LifetimeEZGem.ToString("n0");
        }
        else
        {
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        getLeaderboard.StartPosition = 0;
                        getLeaderboard.StatisticName = "TotalEZCoinOverall";
                        getLeaderboard.MaxResultsCount = 5;
                        PlayFabClientAPI.GetLeaderboard(getLeaderboard,
                            resultCallback =>
                            {
                                failedCallbackCounter = 0;
                                for (int i = 0; i < resultCallback.Leaderboard.Count; i++)
                                {
                                    Placements[i].PlacementCG.alpha = 1;
                                    Placements[i].NameTMP.text = resultCallback.Leaderboard[i].DisplayName;
                                    Placements[i].GemTMP.text = resultCallback.Leaderboard[i].StatValue.ToString("n0");
                                }
                            },
                            errorCallback =>
                            {
                                ErrorCallback(errorCallback.Error,
                                    InitializeLeaderboard,
                                    () => ProcessError(errorCallback.ErrorMessage));
                            });
                    }
                    else
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeLeaderboard,
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
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
