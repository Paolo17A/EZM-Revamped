using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryController : MonoBehaviour
{
    //========================================================================================
    private void OnEnable()
    {
        EntryCore.onEntryStateChange += EntryStateChange;

        GameManager.Instance.SceneController.ActionPass = true;
        EntryCore.CurrentEntryState = EntryCore.EntryStates.PLAY;
    }

    private void OnDisable()
    {
        EntryCore.onEntryStateChange -= EntryStateChange;
    }

    private void EntryStateChange(object sender, EventArgs e)
    {
        if (EntryCore.CurrentEntryState == EntryCore.EntryStates.PLAY)
            EntryCore.ShowPlayPanel();
        else if (EntryCore.CurrentEntryState == EntryCore.EntryStates.LOGIN)
        {
            EntryCore.ShowLoginPanel();
            if(!GameManager.Instance.DebugMode && PlayerPrefs.HasKey("Username") && PlayerPrefs.HasKey("Password"))
            {
                EntryCore.UsernameLoginTMP.text = PlayerPrefs.GetString("Username");
                EntryCore.PasswordLoginTMP.text = PlayerPrefs.GetString("Password");
                LoginCore.LoginWithPlayFab(PlayerPrefs.GetString("Username"), PlayerPrefs.GetString("Password"));
            }

        }
        else if (EntryCore.CurrentEntryState == EntryCore.EntryStates.LINKS)
            EntryCore.ShowLinksPanel();
        else if (EntryCore.CurrentEntryState == EntryCore.EntryStates.SETTINGS)
            EntryCore.ShowSettingsPanel();
    }
    //========================================================================================

    [field: SerializeField] private EntryCore EntryCore { get; set; }
    [field: SerializeField] private LoginCore LoginCore { get; set; }

    public void EntryStateToIndex(int state)
    {
        switch(state)
        {
            case (int)EntryCore.EntryStates.PLAY:
                EntryCore.CurrentEntryState = EntryCore.EntryStates.PLAY;
                break;
            case (int)EntryCore.EntryStates.LOGIN:
                EntryCore.CurrentEntryState = EntryCore.EntryStates.LOGIN;
                break;
            case (int)EntryCore.EntryStates.LINKS:
                EntryCore.CurrentEntryState = EntryCore.EntryStates.LINKS;
                break;
            case (int)EntryCore.EntryStates.SETTINGS:
                EntryCore.CurrentEntryState = EntryCore.EntryStates.SETTINGS;
                break;
        }
    }
}
