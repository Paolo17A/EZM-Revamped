using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using MyBox;
using System;

public class CharacterImageController : MonoBehaviour
{
    //================================================================
    [SerializeField] public ProfileCore ProfileCore;
    [field: Header("CHARACTER DATA")]
    [field: SerializeField] public string CharacterID { get; set; }
    [field: SerializeField] public CharacterData CharacterData { get; set; }

    [field: Header("ANIMAL DATA")]
    [field: SerializeField] public CanvasGroup ImageCG { get; set; }
    [SerializeField] private Image AnimalImage;
    [SerializeField] private TextMeshProUGUI StrengthTMP;
    [SerializeField] private TextMeshProUGUI SpeedTMP;
    [SerializeField] public TextMeshProUGUI StaminaTMP;
    [SerializeField] private TMP_Dropdown RoleDropdown;

    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] private GetCharacterDataRequest getCharacterData;
    private int failedCallbackCounter;
    //================================================================
    private void Awake()
    {
        getCharacterData = new GetCharacterDataRequest();
    }

    public void SetCharacterImageData()
    {
        AnimalImage.sprite = CharacterData.characterPanelSprite;
        StrengthTMP.text = CharacterData.strength.ToString();
        SpeedTMP.text = CharacterData.speed.ToString();
    }

    public void GetCharacterData(CharacterInstanceData characterInstanceData)
    {
        getCharacterData.CharacterId = CharacterID;
        PlayFabClientAPI.GetCharacterData(getCharacterData,
            resultCallback =>
            {
                CharacterData = GameManager.Instance.GetProperCharacter(resultCallback.Data["AnimalID"].Value);
                characterInstanceData.BaseCharacterData = CharacterData;
                characterInstanceData.CharacterCurrentStamina = int.Parse(resultCallback.Data["CurrentStamina"].Value);
                switch (resultCallback.Data["Role"].Value)
                {
                    case "MINER":
                        characterInstanceData.CharacterCurrentRole = CharacterInstanceData.Roles.MINER;
                        break;
                    case "FARMER":
                        characterInstanceData.CharacterCurrentRole = CharacterInstanceData.Roles.FARMER;
                        break;
                    case "FISHER":
                        characterInstanceData.CharacterCurrentRole = CharacterInstanceData.Roles.FISHER;
                        break;
                    case "WOODCUTTER":
                        characterInstanceData.CharacterCurrentRole = CharacterInstanceData.Roles.WOODCUTTER;
                        break;
                }
                characterInstanceData.CharacterCurrentState = CharacterInstanceData.States.INVENTORY;
                if (resultCallback.Data["OnAutopilot"].Value == "1")
                    characterInstanceData.OnAutoPilot = true;
                else
                    characterInstanceData.OnAutoPilot = false;
                SetCharacterImageData();
                StaminaTMP.text = characterInstanceData.CharacterCurrentStamina .ToString() + "/" + CharacterData.stamina;
                ImageCG.alpha = 1;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => GetCharacterData(characterInstanceData),
                    () => ProcessError(errorCallback.ErrorMessage)); 
            });
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
