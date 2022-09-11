using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileController : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.SceneController.AddActionLoadinList(ProfileCore.InitializeProfileScene());
        ProfileCore.onProfileSelectStateChange += ProfileStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        ProfileCore.onProfileSelectStateChange -= ProfileStateChange;
    }

    [SerializeField] private ProfileCore ProfileCore;

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
        }
    }
}
