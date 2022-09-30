using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class QuestCore : MonoBehaviour
{
    //=======================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private LobbyCore LobbyCore;

    [Header("UI ELEMENTS")]
    [SerializeField] private Slider QuestProgressSlider;
    [SerializeField] private Button DailyLoginBtn;
    [SerializeField] private Button DailyClaimBtn;
    [SerializeField] private TextMeshProUGUI MinsPlayedTMP;
    [SerializeField] private TextMeshProUGUI CoinsGainedTMP;

    [Header("PLAYFAB VARIABLES")]
    private GetUserDataRequest getUserData;
    private UpdateUserDataRequest updateUserData;

    [Header("DEBUGGER")]
    private int failedCallbackCounter;
    //=======================================================================

    private void Awake()
    {
        getUserData = new GetUserDataRequest();
        updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
    }

    #region INITIALIZATION
    public void InitializeQuestData()
    {
        if (GameManager.Instance.DebugMode)
        {
            QuestProgressSlider.value = 0;

            if (PlayerData.DailyLogin == 0)
                DailyLoginBtn.interactable = true;
            else
            {
                DailyLoginBtn.interactable = false;
                QuestProgressSlider.value += 0.2f;
            }

            if (PlayerData.SocMedShared > 0)
                QuestProgressSlider.value += 0.2f;

            if (PlayerData.AdsWatched > 0)
                QuestProgressSlider.value += 0.2f;

            MinsPlayedTMP.text = (int)PlayerData.ElapsedGameplayTime.TotalMinutes + "/30";
            PlayerData.MinsPlayed = (int)PlayerData.ElapsedGameplayTime.TotalMinutes;
            if (PlayerData.MinsPlayed >= 30)
                QuestProgressSlider.value += 0.2f;

            CoinsGainedTMP.text = PlayerData.CoinsGained + "/100";
            if (PlayerData.CoinsGained >= 100)
                QuestProgressSlider.value += 0.2f;

            ProcessDailyClaimButton();
        }
        else
        {
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        if (resultCallback.Data.ContainsKey("Quests"))
                        {
                            PlayerData.DailyLogin = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "DailyCheckIn");
                            PlayerData.SocMedShared = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "SocMedShared");
                            PlayerData.AdsWatched = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "AdsWatched");
                            PlayerData.CoinsGained = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "EZCoinsGained");
                            PlayerData.DailyClaimed = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "DailyQuestClaimed");

                            QuestProgressSlider.value = 0;

                            if (PlayerData.DailyLogin == 0)
                                DailyLoginBtn.interactable = true;
                            else
                            {
                                DailyLoginBtn.interactable = false;
                                QuestProgressSlider.value += 0.2f;
                            }

                            if (PlayerData.SocMedShared > 0)
                                QuestProgressSlider.value += 0.2f;

                            if (PlayerData.AdsWatched > 0)
                                QuestProgressSlider.value += 0.2f;

                            MinsPlayedTMP.text = (int)PlayerData.ElapsedGameplayTime.TotalMinutes + "/30";
                            PlayerData.MinsPlayed = (int)PlayerData.ElapsedGameplayTime.TotalMinutes;
                            if (PlayerData.MinsPlayed >= 30)
                                QuestProgressSlider.value += 0.2f;

                            CoinsGainedTMP.text = PlayerData.CoinsGained + "/100";
                            if (PlayerData.CoinsGained >= 100)
                                QuestProgressSlider.value += 0.2f;

                            ProcessDailyClaimButton();
                        }
                    }
                    else
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeQuestData,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }
    #endregion

    #region DAILY LOGIN
    public void DailyLogIn()
    {
        if (PlayerData.DailyLogin == 0)
        {
            if (GameManager.Instance.DebugMode)
            {
                PlayerData.DailyLogin++;
                QuestProgressSlider.value += 0.2f;
                ProcessDailyClaimButton();
                DailyLoginBtn.interactable = false;
                Debug.Log("You have logged in for the day");
            }
            else
            {
                LobbyCore.DisplayLoadingPanel();
                PlayFabClientAPI.GetUserData(getUserData,
                    resultCallback =>
                    {
                        if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                        {
                            PlayerData.DailyLogin++;
                            updateUserData.Data.Clear();
                            updateUserData.Data.Add("Quests", PlayerData.SerializeCurrentQuestData());

                            PlayFabClientAPI.UpdateUserData(updateUserData,
                                resultCallback =>
                                {
                                    failedCallbackCounter = 0;
                                    LobbyCore.HideLoadingPanel();
                                    QuestProgressSlider.value += 0.2f;
                                    ProcessDailyClaimButton();
                                    DailyLoginBtn.interactable = false;
                                },
                                errorCallback =>
                                {
                                    PlayerData.DailyLogin--;
                                    ErrorCallback(errorCallback.Error,
                                        DailyLogIn,
                                        () => ProcessError(errorCallback.ErrorMessage));
                                });
                        }
                        else
                            GameManager.Instance.DisplayDualLoginErrorPanel();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            DailyLogIn,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });

            }
        }
        else
            GameManager.Instance.DisplayErrorPanel("You have already claimed daily log-in");

    }
    #endregion

    #region SOCMED
    public void ShareToSocMed()
    {
        new NativeShare().SetText("Start playing EZMoneyPH!").SetUrl("https://marketplace.optibit.tech/home/customer/dashboard")
            .SetCallback((result, shareTarget) => ProcessShareResult(result))
            .Share();
    }

    private void ProcessShareResult(NativeShare.ShareResult result)
    {
        if (result == NativeShare.ShareResult.Shared)
            UpdateSocMedShared();
    }

    private void UpdateSocMedShared()
    {
        PlayerData.SocMedShared++;
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.SocMedShared == 1)
            {
                QuestProgressSlider.value += 0.2f;
                ProcessDailyClaimButton();
            }
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        updateUserData.Data.Clear();
                        updateUserData.Data.Add("Quests", PlayerData.SerializeCurrentQuestData());
                        PlayFabClientAPI.UpdateUserData(updateUserData,
                            resultCallback =>
                            {
                                failedCallbackCounter = 0;
                                LobbyCore.HideLoadingPanel();
                                if (PlayerData.SocMedShared == 1)
                                {
                                    QuestProgressSlider.value += 0.2f;
                                    ProcessDailyClaimButton();
                                }
                            },
                            errorCallback =>
                            {
                                PlayerData.SocMedShared--;
                                ErrorCallback(errorCallback.Error,
                                    UpdateSocMedShared,
                                    () => ProcessError(errorCallback.ErrorMessage));
                            });
                    }
                    else
                        GameManager.Instance.DisplayDualLoginErrorPanel();                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        UpdateSocMedShared,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }
    #endregion

    #region CLAIM REWARD
    public void ClaimDailyQuest()
    {
        if (QuestProgressSlider.value == 1)
        {
            if (PlayerData.DailyClaimed == 0)
            {
                PlayerData.DailyClaimed++;
                if (GameManager.Instance.DebugMode)
                {
                    PlayerData.EZGem++;
                    LobbyCore.EZGemTMP.text = PlayerData.EZGem.ToString("n0");
                    DailyClaimBtn.interactable = false;
                }
                else
                {
                    LobbyCore.DisplayLoadingPanel();
                    PlayFabClientAPI.GetUserData(getUserData,
                        resultCallback =>
                        {
                            if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                            {
                                updateUserData.Data.Clear();
                                updateUserData.Data.Add("Quests", PlayerData.SerializeCurrentQuestData());
                                PlayFabClientAPI.UpdateUserData(updateUserData,
                                    resultCallback =>
                                    {
                                        failedCallbackCounter = 0;
                                        ProcessDailyClaimButton();
                                        ClaimQuestRewardCloudscript();
                                    },
                                    errorCallback =>
                                    {
                                        PlayerData.SocMedShared--;
                                        ErrorCallback(errorCallback.Error,
                                            UpdateSocMedShared,
                                            () => ProcessError(errorCallback.ErrorMessage));
                                    });
                            }
                            else
                                GameManager.Instance.DisplayDualLoginErrorPanel();
                        },
                        errorCallback =>
                        {
                            ErrorCallback(errorCallback.Error,
                                UpdateSocMedShared,
                                () => ProcessError(errorCallback.ErrorMessage));
                        });
                }
            }
            else
                GameManager.Instance.DisplayErrorPanel("You have already claimed today's reward");
        }
        else
            GameManager.Instance.DisplayErrorPanel("You have not yet completed all the quests");
    }

    private void ClaimQuestRewardCloudscript()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "ClaimQuestReward",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            PlayerData.EZGem++;
            LobbyCore.UpdateEZGemDisplay();
            LobbyCore.HideLoadingPanel();
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                ClaimQuestRewardCloudscript,
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region UTILITY
    private void ProcessDailyClaimButton()
    {
        if (QuestProgressSlider.value == 1)
            DailyClaimBtn.interactable = true;
        else
            DailyClaimBtn.interactable = false;
    }
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
