using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using MyBox;

public class EntryCore : MonoBehaviour
{
    #region STATE MACHINE
    //==================================================================
    public enum EntryStates
    {
        NONE,
        PLAY,
        LOGIN,
        LINKS,
        SETTINGS
    }

    private event EventHandler entryStateChange;
    public event EventHandler onEntryStateChange
    {
        add
        {
            if (entryStateChange == null || !entryStateChange.GetInvocationList().Contains(value))
                entryStateChange += value;
        }
        remove { entryStateChange -= value; }
    }

    public EntryStates CurrentEntryState
    {
        get => entryStates;
        set
        {
            entryStates = value;
            entryStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private EntryStates entryStates;
    //==================================================================
    #endregion

    //======================================================================
    [field: SerializeField] public Animator EntryAnimator { get; set; }
    //[field: SerializeField] private LoginCore LoginCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [field: Header("LOADING")]
    [field: SerializeField] private GameObject LoadingPanel { get; set; }
    [field: SerializeField] private TextMeshProUGUI LoadingTMP { get; set; }

    [field: Header("LOGIN")]
    [field: SerializeField] public TMP_InputField UsernameLoginTMP { get; set; }
    [field: SerializeField] public TMP_InputField PasswordLoginTMP { get; set; }
    //======================================================================

    public void LoginButton()
    {
        if(UsernameLoginTMP.text.Length == 0)
        {
            GameManager.Instance.DisplayErrorPanel("Please input your username");
            return;
        }
        else if(PasswordLoginTMP.text.Length == 0)
        {
            GameManager.Instance.DisplayErrorPanel("Please input your password");
            return;
        }

        ProcessLogin();
    }

    private void ProcessLogin()
    {
        if(GameManager.Instance.DebugMode)
        {
            PlayerPrefs.SetString("Username", UsernameLoginTMP.text);
            PlayerPrefs.SetString("Password", PasswordLoginTMP.text);
            PlayerData.DisplayName = UsernameLoginTMP.text;
            PlayerData.SubscriptionLevel = "PEARL";
            //PlayerData.TotalGameTimeSpan = new TimeSpan(0, 0, 0, 0);

            ResetLoginPanel();
            CurrentEntryState = EntryStates.NONE;
            GameManager.Instance.SceneController.CurrentScene = "GameSelectScene";
        }
        else
        {
            //LoginCore.LoginWithPlayfab(UsernameLoginTMP.text, PasswordLoginTMP.text);
        }
    }

    public void DisplayLoadingPanel(string message)
    {
        LoadingPanel.SetActive(true);
        LoadingTMP.text = message;
        GameManager.Instance.PanelActivated = true;
    }

    public void HideLoadingPanel()
    {
        LoadingPanel.SetActive(false);
        LoadingTMP.text = "";
        GameManager.Instance.PanelActivated = false;
    }

    public void LogOutButton()
    {
        PlayerData.ResetPlayerData();
        PlayerPrefs.DeleteAll();
        CurrentEntryState = EntryCore.EntryStates.PLAY;
    }

    #region LINKS
    public void OpenWebsite()
    {
        Application.OpenURL("https://ezmoneyph.com/");
    }
    public void OpenFacebook()
    {
        Application.OpenURL("https://www.facebook.com/EZMoneyPH/");
    }
    public void OpenTwitter()
    {
        Application.OpenURL("https://twitter.com/RealEZMoneyPH");
    }
    public void OpenInstagram()
    {
        Application.OpenURL("https://www.instagram.com/officialezmoneyph/");
    }
    #endregion

    #region UTILITY
    public void ResetLoginPanel()
    {
        UsernameLoginTMP.text = "";
        PasswordLoginTMP.text = "";
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenMiningScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "MiningScene";
    }    
    #endregion
}
