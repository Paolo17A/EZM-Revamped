using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;

public class ProfileController : MonoBehaviour
{
    private void OnEnable()
    {
        if(GameManager.Instance.DebugMode)
            GameManager.Instance.SceneController.AddActionLoadinList(ProfileCore.InitializeProfileScene());
        if(!GameManager.Instance.DebugMode)
        {
            GameManager.Instance.SceneController.AddActionLoadinList(ProfileCore.GetUserData());
        }
        ProfileCore.onProfileSelectStateChange += ProfileStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        ProfileCore.onProfileSelectStateChange -= ProfileStateChange;
    }

    [SerializeField] private ProfileCore ProfileCore;

    private void Awake()
    {
        ProfileCore.ActualOwnedCharacters = new List<CharacterInstanceData>();
        ProfileCore.getUserData = new GetUserDataRequest();
        ProfileCore.getUserInventory = new GetUserInventoryRequest();
        ProfileCore.getPlayerStatistics = new GetPlayerStatisticsRequest();
        ProfileCore.listUsersCharacters = new ListUsersCharactersRequest();
        ProfileCore.getCharacterData = new GetCharacterDataRequest();
    }

    private void Start()
    {
        ProfileCore.ShowCorePanels();
    }

    private void ProfileStateChange(object sender, EventArgs e)
    {
        if (ProfileCore.CurrentProfileState == ProfileCore.ProfileStates.PROFILE)
            ProfileCore.ShowProfilePanel();
        else if (ProfileCore.CurrentProfileState == ProfileCore.ProfileStates.DISPLAY)
            ProfileCore.ShowDisplayPanel();
        else if (ProfileCore.CurrentProfileState == ProfileCore.ProfileStates.CHARACTER)
            ProfileCore.ShowCharacterPanel();
        else if (ProfileCore.CurrentProfileState == ProfileCore.ProfileStates.AUTO)
            ProfileCore.ShowAutoPanel();
        else if (ProfileCore.CurrentProfileState == ProfileCore.ProfileStates.SWAP)
            ProfileCore.ShowSwapPanel();
        else if (ProfileCore.CurrentProfileState == ProfileCore.ProfileStates.AUTODATA)
            ProfileCore.ShowAutoDataPanel();
    }

    public void ProfileStateToIndex(int state)
    {
        switch(state)
        {
            case (int)ProfileCore.ProfileStates.DISPLAY:
                ProfileCore.CurrentProfileState = ProfileCore.ProfileStates.DISPLAY;
                break;
            case (int)ProfileCore.ProfileStates.PROFILE:
                ProfileCore.CurrentProfileState = ProfileCore.ProfileStates.PROFILE;
                break;
            case (int)ProfileCore.ProfileStates.CHARACTER:
                ProfileCore.CurrentProfileState = ProfileCore.ProfileStates.CHARACTER;
                break;
            case (int)ProfileCore.ProfileStates.AUTO:
                ProfileCore.CurrentProfileState = ProfileCore.ProfileStates.AUTO;
                break;
            case (int)ProfileCore.ProfileStates.SWAP:
                ProfileCore.CurrentProfileState = ProfileCore.ProfileStates.SWAP;
                break;
            case (int)ProfileCore.ProfileStates.AUTODATA:
                ProfileCore.CurrentProfileState = ProfileCore.ProfileStates.AUTODATA;
                break;
        }
    }
}
