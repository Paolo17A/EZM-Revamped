using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MyBox;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;

public class CharacterPrefabCore : MonoBehaviour
{
    #region STATE MACHINE
    //================================================================================
    [SerializeField][ReadOnly] private CharacterStates characterState;
    public enum CharacterStates
    {
        NONE,
        IDLE,
        WALKING,
        WORKING
    }

    private event EventHandler characterStateChange;
    public event EventHandler onCharacterStateChange
    {
        add
        {
            if (characterStateChange == null || !characterStateChange.GetInvocationList().Contains(value))
                characterStateChange += value;
        }
        remove { characterStateChange -= value; }
    }

    public CharacterStates CurrentCharacterState
    {
        get => characterState;
        set
        {
            characterState = value;
            characterStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
    //=====================================================================================
    [ReadOnly] public GameplayCore GameplayCore;

    [field: Header("CHARACTER DATA")]
    [field: SerializeField][field: ReadOnly] public CharacterSlotController ThisCharacterSlot { get; set; }
    [SerializeField] public CharacterData ThisCharacterData;
    [SerializeField] public Animator CharacterAnimator;
    [SerializeField] public Rigidbody CharacterRB;
    [SerializeField] public NavMeshAgent CharacterNavMesh;

    [field: Header("ORE DATA")]
    [field: SerializeField][field: ReadOnly] public bool willMineOre { get; set; }
    [field: SerializeField][field: ReadOnly] public OreController AssignedOre { get; set; }


    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] public UpdateCharacterDataRequest updateCharacterData;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] public bool isSelected;
    [SerializeField][ReadOnly] public Vector3 destinationVector;
    private int failedCallbackCounter;
    [ReadOnly] public bool apiCallOngoing;
    //=====================================================================================

    public void MineOre()
    {
        AssignedOre.OreHealth -= ThisCharacterData.strength;
        if(AssignedOre.OreHealth <= 0)
        {
            AssignedOre.SetCooldown(); 
            ReduceCharacterStamina();
        }
    }

    private void ReduceCharacterStamina()
    {
        ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina--;
        CurrentCharacterState = CharacterStates.IDLE;

        if (!ThisCharacterSlot.ForAutoPilot)
        {
            AssignedOre.OccupyingCharacter = null;
            AssignedOre = null;
            willMineOre = false;
        }

        if (GameManager.Instance.DebugMode)
        {
            if (ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina == 0)
            {
                ThisCharacterSlot.UndeployThisCharacter();
            }  
        }
        else
        {
            apiCallOngoing = true;
            updateCharacterData.CharacterId = ThisCharacterSlot.ThisCharacterInstance.CharacterInstanceID;
            updateCharacterData.Data.Clear();
            updateCharacterData.Data.Add("CurrentStamina", ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina.ToString());
            PlayFabClientAPI.UpdateCharacterData(updateCharacterData,
                resultCallback =>
                {
                    apiCallOngoing = false;
                    if (ThisCharacterSlot.ThisCharacterInstance.CharacterCurrentStamina == 0)
                    {
                        ThisCharacterSlot.UndeployThisCharacter();
                    }
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
