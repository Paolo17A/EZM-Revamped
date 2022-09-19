using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class SwapCore : MonoBehaviour
{
    //========================================================================================
    [SerializeField] private ProfileCore ProfileCore;
    [SerializeField] private PlayerData PlayerData;

    [Header("INPUT FIELDS")]
    [SerializeField] private VerticalLayoutGroup VirtualCurrencyContainer;
    [SerializeField] private TMP_InputField EZCoinTMP;
    [SerializeField] private TMP_InputField EZGemTMP;
    [SerializeField] private Button SwapBtn;

    [Header("PLAYFAB VARIABLES")]
    private GetUserDataRequest getUserData;
    private GetUserInventoryRequest getUserInventory;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private bool willSwapEZGem;
    private int failedCallbackCounter;
    //========================================================================================
    private void Awake()
    {
        getUserData = new GetUserDataRequest();
        getUserInventory = new GetUserInventoryRequest();
    }

    public void InterchangeInput()
    {
        EZGemTMP.text = "";
        EZCoinTMP.text = "";
        willSwapEZGem = !willSwapEZGem;

        if (willSwapEZGem)
        {
            EZGemTMP.interactable = true;
            EZCoinTMP.interactable = false;
            VirtualCurrencyContainer.reverseArrangement = true;
        }
        else
        {
            EZGemTMP.interactable = false;
            EZCoinTMP.interactable = true;
            VirtualCurrencyContainer.reverseArrangement = false;
        }
    }

    public void DisplayTakehomeEZCoin()
    {
        if (EZGemTMP.text == "")
        {
            EZCoinTMP.text = "";
            SwapBtn.interactable = false;
        }
        else
        {
            if (int.Parse(EZGemTMP.text) > PlayerData.EZGem)
            {
                EZGemTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must not exceed " + PlayerData.EZGem);
            }
            else if (int.Parse(EZGemTMP.text) < 1)
            {
                EZGemTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must be at least 1 EZGem");
            }
            else
            {
                EZCoinTMP.text = (int.Parse(EZGemTMP.text) * 95).ToString();
                SwapBtn.interactable = true;
            }
        }
    }

    public void DisplayTakehomeEZGem()
    {
        if (EZCoinTMP.text == "")
        {
            EZGemTMP.text = "";
            SwapBtn.interactable = false;
        }
        else
        {
            if (int.Parse(EZCoinTMP.text) > PlayerData.EZCoin)
            {
                EZCoinTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must not exceed " + PlayerData.EZCoin);
            }
            else if (int.Parse(EZCoinTMP.text) < 105)
            {
                EZCoinTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must be at least 105 EZCoin");
            }
            else
            {
                EZGemTMP.text = (int.Parse(EZCoinTMP.text) / 105).ToString();
                SwapBtn.interactable = true;
            }
        }
    }

    public void InputMaxValue()
    {
        if (willSwapEZGem)
        {
            EZGemTMP.text = PlayerData.EZGem.ToString();
            DisplayTakehomeEZCoin();
        }
        else
        {
            EZCoinTMP.text = PlayerData.EZCoin.ToString();
            DisplayTakehomeEZGem();
        }
    }

    public void SwapCurrencies()
    {
        if (!willSwapEZGem && int.Parse(EZCoinTMP.text) % 105 != 0)
            Debug.Log("You will have an excess of " + (int.Parse(EZCoinTMP.text) % 105) + "EZGems");
        else
            Debug.Log("You will have no excess EZCoins");


        if (GameManager.Instance.DebugMode)
        {
            if (willSwapEZGem)
            {
                PlayerData.EZGem -= int.Parse(EZGemTMP.text);
                PlayerData.EZCoin += int.Parse(EZCoinTMP.text);
            }
            else
            {
                PlayerData.EZGem += int.Parse(EZGemTMP.text);
                PlayerData.EZCoin -= int.Parse(EZCoinTMP.text) - (int.Parse(EZCoinTMP.text) % 105);
            }

            ProfileCore.EZCoinsTMP.text = PlayerData.EZCoin.ToString();
            ProfileCore.EZGemsTMP.text = PlayerData.EZGem.ToString();
            EZCoinTMP.text = "";
            EZGemTMP.text = "";
            SwapBtn.interactable = false;
        }
        else
        {
            ProfileCore.DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        string functionName = "";
                        if (willSwapEZGem)
                            functionName = "SwapGemForCoin";
                        else
                            functionName = "SwapCoinForGem";
                        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                        {
                            FunctionName = functionName,
                            FunctionParameter = new { coin = int.Parse(EZCoinTMP.text), gem = int.Parse(EZGemTMP.text) },
                            GeneratePlayStreamEvent = true
                        },
                        resultCallback =>
                        {
                            failedCallbackCounter = 0;
                            EZCoinTMP.text = "";
                            EZGemTMP.text = "";
                            SwapBtn.interactable = false;
                            ProfileCore.GetUserInventoryPlayFab();

                        },
                        errorCallback =>
                        {
                            ErrorCallback(errorCallback.Error,
                                SwapCurrencies,
                                () => ProcessError(errorCallback.ErrorMessage));
                        });
                    }
                    else
                    {
                        ProfileCore.HideLoadingPanel();
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        SwapCurrencies,
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
