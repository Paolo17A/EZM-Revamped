using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutopilotDataCore : MonoBehaviour
{
    //======================================================================
    [SerializeField] private ProfileCore ProfileCore;
    [SerializeField] private ProfileController ProfileController;
    [SerializeField] private PlayerData PlayerData;

    [Header("PILOT DATA")]
    [SerializeField] private Sprite PilotSprite;
    [SerializeField] private CharacterInstanceData.Roles RoleNeeded;
    private TimeSpan RemainingTimeSpan;
    private int DeployedWorkers;
    //======================================================================
    public void DeclareAsSelected()
    {
        DeployedWorkers = 0;
        ProfileCore.SelectedAutoPilot = this;
        ProfileCore.SelectedPilotImage.sprite = PilotSprite;
        switch(RoleNeeded)
        {
            case CharacterInstanceData.Roles.MINER:
                RemainingTimeSpan = TimeSpan.FromSeconds((double)PlayerData.AutoMiningTimeLeft);
                break;
            case CharacterInstanceData.Roles.FARMER:
                RemainingTimeSpan = TimeSpan.FromSeconds((double)PlayerData.AutoFarmingTimeLeft);
                break;
            case CharacterInstanceData.Roles.FISHER:
                RemainingTimeSpan = TimeSpan.FromSeconds((double)PlayerData.AutoFishingTimeLeft);
                break;
            case CharacterInstanceData.Roles.WOODCUTTER:
                RemainingTimeSpan = TimeSpan.FromSeconds((double)PlayerData.AutoWoodcuttingTimeLeft);
                break;
        }
        ProfileCore.AutoTimeLeftTMP.text = string.Format("{0:00}:{1:00}:{2:00}", RemainingTimeSpan.Hours, RemainingTimeSpan.Minutes, RemainingTimeSpan.Seconds);
        ProfileCore.WorkersDeployedTMP.text = "Workers Deployed: " + CountDeployedWorkers();

        ProfileController.ProfileStateToIndex(6);
    }

    private int CountDeployedWorkers()
    {
        foreach(CharacterInstanceData character in ProfileCore.ActualOwnedCharacters)
        {
            if (character.CharacterCurrentRole == RoleNeeded && character.OnAutoPilot)
                DeployedWorkers++;
        }
        return DeployedWorkers;
    }

}
