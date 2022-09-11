using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.SceneController.AddActionLoadinList(LobbyCore.InitializeLobby());
        GameManager.Instance.SceneController.ActionPass = true;
        LobbyCore.onLobbySelectStateChange += LobbyStateChange;
    }

    private void OnDisable()
    {
        LobbyCore.onLobbySelectStateChange -= LobbyStateChange;
    }

    private void Start()
    {
        LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CORE;
    }

    private void LobbyStateChange(object sender, EventArgs e)
    {
        if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.CORE)
            LobbyCore.ShowCorePanels();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.SETTINGS)
            LobbyCore.ShowSettingsPanel();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.LEADERBOARD)
            LobbyCore.ShowLeaderboardPanel();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.QUEST)
        {
            QuestCore.InitializeQuestData();
            LobbyCore.ShowQuestPanel();
        }
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.GIFT)
            LobbyCore.ShowGiftPanel();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.MAIL)
            LobbyCore.ShowMailPanel();
    }

    [SerializeField] private LobbyCore LobbyCore;
    [SerializeField] private QuestCore QuestCore;

    public void LobbyStateToIndex(int state)
    {
        switch (state)
        {
            case (int)LobbyCore.LobbyStates.CORE:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CORE;
                break;
            case (int)LobbyCore.LobbyStates.SETTINGS:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.SETTINGS;
                break;
            case (int)LobbyCore.LobbyStates.LEADERBOARD:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.LEADERBOARD;
                break;
            case (int)LobbyCore.LobbyStates.QUEST:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.QUEST;
                break;
            case (int)LobbyCore.LobbyStates.GIFT:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.GIFT;
                break;
            case (int)LobbyCore.LobbyStates.MAIL:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.MAIL;
                break;
        }
    }
}
