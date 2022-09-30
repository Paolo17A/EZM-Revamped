using MyBox;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using System;

public class OreController : MonoBehaviour
{
    #region VARIABLES
    public enum Ore { NONE, COPPER, TIN, IRON }
    //============================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private GameplayCore GameplayCore;
    [field: SerializeField][field: ReadOnly] public CharacterPrefabCore OccupyingCharacter { get; set; }
    [SerializeField][ReadOnly] private MeshRenderer OreMeshRenderer;
    [SerializeField] public int OreHealth;
    [SerializeField] private Ore DispensedOre;
    [SerializeField][ReadOnly] public string OreName;

    [field: Header("COOLDOWN")]
    [field: SerializeField][field: ReadOnly] public bool OnCooldown { get; set; }
    [SerializeField] private int CooldownTimerLeft;
    [SerializeField][ReadOnly] private float CurrentCountdownNumber;

    [Header("PLAYFAB VARIABLES")]
    private GetUserDataRequest getUserData;

    private int failedCallbackCounter;
    //============================================================================
    #endregion

    private void Awake()
    {
        getUserData = new GetUserDataRequest();
    }

    private void Start()
    {
        OreName = transform.name;
        OreMeshRenderer = GetComponent<MeshRenderer>();
    }

    public void Update()
    {
        if (OnCooldown)
        {
            if (CooldownTimerLeft > 0f)
            {
                CurrentCountdownNumber -= Time.deltaTime;
                CooldownTimerLeft = (int)CurrentCountdownNumber;
            }
            else
            {
                CooldownTimerLeft = 10;
                CurrentCountdownNumber = CooldownTimerLeft;
                OnCooldown = false;
                OreMeshRenderer.enabled = true;
                if (OccupyingCharacter != null && OccupyingCharacter.ThisCharacterSlot.ForAutoPilot)
                {
                    OccupyingCharacter.CurrentCharacterState = CharacterPrefabCore.CharacterStates.WORKING;
                }
            }
        }

    }

    public void SetCooldown()
    {
        CooldownTimerLeft = 10;
        CurrentCountdownNumber = CooldownTimerLeft;
        OnCooldown = true;
        OreMeshRenderer.enabled = false;
        DispenseOreToPlayer();
    }

    private void DispenseOreToPlayer()
    {
        if (GameManager.Instance.DebugMode)
        {
            switch (DispensedOre)
            {
                case Ore.COPPER:
                    PlayerData.CopperCount++;
                    break;
                case Ore.TIN:
                    PlayerData.TinCount++;
                    break;
                case Ore.IRON:
                    PlayerData.IronCount++;
                    break;
            }
            GameplayCore.ProcessInventoryPanel();
            GameplayCore.CalculateEZCoinValue();
        }
        else
        {
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        string dispensedOre = "";
                        switch (DispensedOre)
                        {
                            case Ore.COPPER:
                                dispensedOre = "CopperOre";
                                break;
                            case Ore.TIN:
                                dispensedOre = "TinOre";
                                break;
                            case Ore.IRON:
                                dispensedOre = "IronOre";
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
                                DispenseOreToPlayer,
                                () => ProcessError(errorCallback.ErrorMessage));
                        });
                    }
                    else
                        GameManager.Instance.DisplaySpecialErrorPanel("You have logged into another device");
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        DispenseOreToPlayer,
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
