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
                PlayerData.IronCount++;
                GameplayCore.AutoIron++;
                GameplayCore.AutoIronTMP.text = GameplayCore.AutoIron.ToString("n0");
            }
            else if (randomReward == 1)
            {
                PlayerData.CopperCount++;
                GameplayCore.AutoCopper++;
                GameplayCore.AutoCopperTMP.text = GameplayCore.AutoCopper.ToString("n0");
            }
            else if (randomReward == 2)
            {
                PlayerData.TinCount++;
                GameplayCore.AutoTin++;
                GameplayCore.AutoIronTMP.text = GameplayCore.AutoTin.ToString("n0");
            }
            GameplayCore.ProcessInventoryPanel();
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
                                dispensedOre = "IronOre";
                                break;
                            case 1:
                                dispensedOre = "CopperOre";
                                break;
                            case 2:
                                dispensedOre = "TinOre";
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
                            switch (randomReward)
                            {
                                case 0:
                                    GameplayCore.AutoIron++;
                                    GameplayCore.AutoIronTMP.text = GameplayCore.AutoIron.ToString("n0");
                                    break;
                                case 1:
                                    GameplayCore.AutoCopper++;
                                    GameplayCore.AutoCopperTMP.text = GameplayCore.AutoCopper.ToString("n0");
                                    break;
                                case 2:
                                    GameplayCore.AutoTin++;
                                    GameplayCore.AutoTinTMP.text = GameplayCore.AutoTin.ToString("n0");
                                    break;
                            }
                            GameplayCore.CalculateAutoEZCoinValue();
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
        if(GameManager.Instance.DebugMode)
        {
            ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina--;
            if (ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina == 0)
                ThisCharacterSlot.UndeployThisCharacter();
            InitializeAutomatedCharacter();
        }
        else
        {
            ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina--;
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
