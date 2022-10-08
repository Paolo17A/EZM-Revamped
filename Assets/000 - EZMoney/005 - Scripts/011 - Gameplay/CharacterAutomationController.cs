using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using PlayFab;
using PlayFab.ClientModels;
using static OreController;
using System;

public class CharacterAutomationController : MonoBehaviour
{
    //===================================================================================
    [SerializeField] private GameplayCore GameplayCore;
    [SerializeField] private CharacterSlotController ThisCharacterSlot;
    [SerializeField] private PlayerData PlayerData;

    [Header("AUTOMATION")]
    [SerializeField][ReadOnly] private float workingTime;
    [SerializeField][ReadOnly] private bool isWorking;

    [Header("PLAYFAB VARIABLES")]
    [SerializeField][ReadOnly] private UpdateUserDataRequest updateUserData;
    [SerializeField][ReadOnly] private GetUserDataRequest getUserData;
    [SerializeField][ReadOnly] private UpdateCharacterDataRequest updateCharacterData;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private int currentStamina;
    [SerializeField][ReadOnly] private int randomReward;
    private int failedCallbackCounter;

    //===================================================================================
    private void Awake()
    {
        updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        getUserData = new GetUserDataRequest();
        updateCharacterData = new UpdateCharacterDataRequest();
        updateCharacterData.Data = new Dictionary<string, string>();
    }

    private void Update()
    {
        if(ThisCharacterSlot.ThisCharacterInstance != null && GameplayCore.AutomationActivated && isWorking)
        {
            workingTime -= Time.deltaTime;
            if(workingTime <= 0)
            {
                isWorking = false;
                GameplayCore.UpdateAutoTimeLeft();
                DispenseOre();
                ReduceCharacterStamina();
            }
        }
    }

    public void InitializeAutomatedCharacter()
    {
        if (ThisCharacterSlot.ThisCharacterInstance != null)
        {
            currentStamina = ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina;
            workingTime = 110 - 10 * ThisCharacterSlot.ThisCharacterInstance.BaseCharacterData.speed;
            isWorking = true;
        }
    }

    private void DispenseOre()
    {
        randomReward = UnityEngine.Random.Range(0, 3);
        if(GameManager.Instance.DebugMode)
        {
            if (randomReward == 0)
            {
                PlayerData.AutoIronCount++;
                GameplayCore.AutoIronTMP.text = PlayerData.AutoIronCount.ToString("n0");
            }
            else if (randomReward == 1)
            {
                PlayerData.AutoCopperCount++;
                GameplayCore.AutoCopperTMP.text = PlayerData.AutoCopperCount.ToString("n0");
            }
            else if (randomReward == 2)
            {
                PlayerData.AutoTinCount++;
                GameplayCore.AutoIronTMP.text = PlayerData.AutoTinCount.ToString("n0");
            }
            GameplayCore.ProcessAutoInventoryPanel();
            GameplayCore.CalculateAutoEZCoinValue();
        }
        else
        {
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        string dispensedOre = "";
                        switch (randomReward)
                        {
                            case 0:
                                dispensedOre = "AutoIronOre";
                                break;
                            case 1:
                                dispensedOre = "AutoCopperOre";
                                break;
                            case 2:
                                dispensedOre = "AutoTinOre";
                                break;
                        }
                        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                        {
                            FunctionName = "GrantOreToUser",
                            FunctionParameter = new { oreID = dispensedOre },
                            GeneratePlayStreamEvent = true
                        },
                        resultCallback =>
                        {
                            failedCallbackCounter = 0;
                            GameplayCore.GetUserInventoryPlayFab();
                        },
                        errorCallback =>
                        {
                            ErrorCallback(errorCallback.Error,
                                DispenseOre,
                                () => ProcessError(errorCallback.ErrorMessage));
                        });
                    }
                    else
                        GameManager.Instance.DisplaySpecialErrorPanel("You have logged into another device");
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        DispenseOre,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void ReduceCharacterStamina()
    {
        ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina--;
        ThisCharacterSlot.StaminaSlider.value = (float)ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina / ThisCharacterSlot.ThisCharacterInstance.BaseCharacterData.stamina;
        if (GameManager.Instance.DebugMode)
        {
            if (ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina == 0)
                ThisCharacterSlot.UndeployThisCharacter();
            InitializeAutomatedCharacter();
        }
        else
        {
            updateCharacterData.CharacterId = ThisCharacterSlot.ThisCharacterInstance.CharacterInstanceID;
            updateCharacterData.Data.Clear();
            updateCharacterData.Data.Add("CurrentStamina", ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina.ToString());
            PlayFabClientAPI.UpdateCharacterData(updateCharacterData,
                resultCallback =>
                {
                    if (ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina == 0)
                        ThisCharacterSlot.UndeployThisCharacter();
                    InitializeAutomatedCharacter();
                },
                errorCallback =>
                {
                    ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina++;
                    ErrorCallback(errorCallback.Error,
                        ReduceCharacterStamina,
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
        GameplayCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        GameplayCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
