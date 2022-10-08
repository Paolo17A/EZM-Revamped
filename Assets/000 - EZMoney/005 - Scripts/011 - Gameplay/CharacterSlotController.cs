using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class CharacterSlotController : MonoBehaviour
{
    //==================================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("SLOT DATA")]
    [SerializeField] private GameplayCore GameplayCore;
    [SerializeField] private int SlotIndex;
    [SerializeField] public Button UndeployBtn;
    [SerializeField] public Slider StaminaSlider;

    [Header("AUTOMATION DATA")]
    [SerializeField] public bool ForAutoPilot;
    [SerializeField] private CharacterAutomationController CharacterAutomationController;

    [Header("CHARACTER DATA")]
    [SerializeField] private Image CharacterImage;
    [SerializeField][ReadOnly] public CharacterInstanceData ThisCharacterInstance;
    [SerializeField][ReadOnly] private GameObject CharacterGO;

    [Header("PLAYFAB VARIABLES")]
    [SerializeField][ReadOnly] private UpdateCharacterDataRequest updateCharacterData;

    private int failedCallbackCounter;
    private bool isUndeploying;
    //==================================================================
    private void Awake()
    {
        updateCharacterData = new UpdateCharacterDataRequest();
        updateCharacterData.Data = new Dictionary<string, string>();
    }

    public void ProcessCharacterSlot()
    {
        if (GameplayCore.UndeployedCharacters.Count == 0)
            return;
        if(!ForAutoPilot)
        {
            if (ThisCharacterInstance == null)
            {
                GameplayCore.SelectedCharacterSlot = this;
                GameplayCore.CurrentGameplayState = GameplayCore.GameplayStates.CHARACTER;
            }
            else if (ThisCharacterInstance.CharacterCurrentState == CharacterInstanceData.States.DEPLOYED)
            {
                ThisCharacterInstance.CharacterCurrentState = CharacterInstanceData.States.IDLE;
                CharacterGO = Instantiate(ThisCharacterInstance.BaseCharacterData.AnimatedCharacterPrefab);
                //CharacterGO.transform.position = new Vector3(0, 5, -27);

                if(GameplayCore.CurrentZone == GameplayCore.Zones.DEFAULT)
                    CharacterGO.transform.position = new Vector3(0, 5, -27);
                else if (GameplayCore.CurrentZone == GameplayCore.Zones.NORTH)
                    CharacterGO.transform.position = new Vector3(0, 5, 6);
                else if (GameplayCore.CurrentZone == GameplayCore.Zones.SOUTH)
                    CharacterGO.transform.position = new Vector3(0, 5, -60);
                else if (GameplayCore.CurrentZone == GameplayCore.Zones.EAST)
                    CharacterGO.transform.position = new Vector3(35, 5, -30);
                else if (GameplayCore.CurrentZone == GameplayCore.Zones.WEST)
                    CharacterGO.transform.position = new Vector3(-30, 5, -30);
                CharacterGO.GetComponent<CharacterPrefabCore>().ThisCharacterSlot = this;
                CharacterGO.GetComponent<CharacterPrefabCore>().GameplayCore = GameplayCore;
            }
        }
        else
        {
            if (GameplayCore.NeededRole == CharacterInstanceData.Roles.MINER && PlayerData.AutoMiningTimeLeft <= 0)
                return;

            if(ThisCharacterInstance == null)
            {
                ThisCharacterInstance = GameplayCore.UndeployedCharacters[0];
                int strongestCharacterIndex = 0;
                for(int i = 0; i < GameplayCore.UndeployedCharacters.Count; i++)
                {
                    if (GameplayCore.UndeployedCharacters[i].BaseCharacterData.strength >= ThisCharacterInstance.BaseCharacterData.strength && GameplayCore.UndeployedCharacters[i].CharacterCurrentStamina > 0)
                    {
                        strongestCharacterIndex = i;
                        ThisCharacterInstance = GameplayCore.UndeployedCharacters[strongestCharacterIndex];
                    }
                }
                if(ThisCharacterInstance.CharacterCurrentStamina == 0)
                {
                    Debug.Log(ThisCharacterInstance.CharacterInstanceID + " has no energy");
                    ThisCharacterInstance = null;
                    return;
                }

                GameplayCore.UndeployedCharacters[strongestCharacterIndex].OnAutoPilot = true;
                GameplayCore.AutomatedCharacters.Add(GameplayCore.UndeployedCharacters[strongestCharacterIndex]);
                GameplayCore.UndeployedCharacters.RemoveAt(strongestCharacterIndex);
                GameplayCore.StartAutomationBtn.interactable = true;
                InitializeCharacterSlot();
                if(!GameManager.Instance.DebugMode)
                    SetAutopilotPlayFab();
            }
        }
    }

    public void InitializeCharacterSlot()
    {
        UndeployBtn.gameObject.SetActive(true);
        StaminaSlider.gameObject.SetActive(true);
        StaminaSlider.value = (float)ThisCharacterInstance.CharacterCurrentStamina / ThisCharacterInstance.BaseCharacterData.stamina;
        CharacterImage.sprite = ThisCharacterInstance.BaseCharacterData.deployedSprite;
        ThisCharacterInstance.CharacterCurrentState = CharacterInstanceData.States.DEPLOYED;
    }

    private void SetAutopilotPlayFab()
    {
        updateCharacterData.CharacterId = ThisCharacterInstance.CharacterInstanceID;
        updateCharacterData.Data.Clear();
        updateCharacterData.Data.Add("OnAutopilot", "1");

        PlayFabClientAPI.UpdateCharacterData(updateCharacterData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (ForAutoPilot && GameplayCore.AutomationActivated)
                    CharacterAutomationController.InitializeAutomatedCharacter();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    SetAutopilotPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    public void UndeployThisCharacter()
    {
        if(GameManager.Instance.DebugMode)
        {
            Destroy(CharacterGO);
            CharacterGO = null;
            ThisCharacterInstance.CharacterCurrentState = CharacterInstanceData.States.INVENTORY;
            if (ForAutoPilot)
            {
                GameplayCore.AutomatedCharacters.Remove(ThisCharacterInstance);
                ThisCharacterInstance.OnAutoPilot = false;
                if (GameplayCore.AutomatedCharacters.Count > 0)
                    GameplayCore.StartAutomationBtn.interactable = true;
                else
                {
                    GameplayCore.StartAutomationBtn.interactable = false;
                    GameplayCore.AutomationActivated = false;
                }
            }
            GameplayCore.UndeployedCharacters.Add(ThisCharacterInstance);

            CharacterImage.sprite = GameplayCore.EmptySlotSprite;
            UndeployBtn.gameObject.SetActive(false);
            StaminaSlider.gameObject.SetActive(false);
            ThisCharacterInstance = null;
        }
        else
        {
            if(!isUndeploying)
            {
                isUndeploying = true;
                updateCharacterData.CharacterId = ThisCharacterInstance.CharacterInstanceID;
                updateCharacterData.Data.Clear();
                updateCharacterData.Data.Add("OnAutopilot", "0");

                PlayFabClientAPI.UpdateCharacterData(updateCharacterData,
                    resultCallback =>
                    {
                        isUndeploying = false;
                        failedCallbackCounter = 0;
                        Destroy(CharacterGO);
                        CharacterGO = null;
                        ThisCharacterInstance.CharacterCurrentState = CharacterInstanceData.States.INVENTORY;
                        if (ForAutoPilot)
                        {
                            GameplayCore.AutomatedCharacters.Remove(ThisCharacterInstance);
                            ThisCharacterInstance.OnAutoPilot = false;
                            if (GameplayCore.AutomatedCharacters.Count > 0)
                                GameplayCore.StartAutomationBtn.interactable = true;
                            else
                            {
                                GameplayCore.StartAutomationBtn.interactable = false;
                                GameplayCore.AutomationActivated = false;
                            }
                        }
                        GameplayCore.UndeployedCharacters.Add(ThisCharacterInstance);

                        CharacterImage.sprite = GameplayCore.EmptySlotSprite;
                        UndeployBtn.gameObject.SetActive(false);
                        StaminaSlider.gameObject.SetActive(false);
                        ThisCharacterInstance = null;
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            SetAutopilotPlayFab,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
            
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
