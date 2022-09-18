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
    [field: SerializeField] private LoginCore LoginCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("PLAY PANEL")]
    [SerializeField] private RectTransform PlayRT;
    [SerializeField] private CanvasGroup PlayCG;

    [field: Header("LOGIN PANEL")]
    [field: SerializeField] public TMP_InputField UsernameLoginTMP { get; set; }
    [field: SerializeField] public TMP_InputField PasswordLoginTMP { get; set; }
    [SerializeField] private RectTransform LoginRT;
    [SerializeField] private CanvasGroup LoginCG;

    [Header("LINKS PANEL")]
    [SerializeField] private RectTransform LinksRT;
    [SerializeField] private CanvasGroup LinksCG;

    [field: Header("SETTINGS PANEL")]
    [SerializeField] private RectTransform SettingsRT;
    [SerializeField] private CanvasGroup SettingsCG;
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
            /*PlayerPrefs.SetString("Username", UsernameLoginTMP.text);
            PlayerPrefs.SetString("Password", PasswordLoginTMP.text);
            PlayerData.DisplayName = UsernameLoginTMP.text;
            PlayerData.SubscriptionLevel = "PEARL";
            //PlayerData.TotalGameTimeSpan = new TimeSpan(0, 0, 0, 0);

            ResetLoginPanel();
            CurrentEntryState = EntryStates.NONE;*/
            PlayerData.DisplayName = UsernameLoginTMP.text;
            GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
        }
        else
        {
            LoginCore.LoginWithPlayFab(UsernameLoginTMP.text, PasswordLoginTMP.text);
        }
    }

    #region PANELS
    public void ShowPlayPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(PlayRT, null, PlayCG, 0, 1, () => { });
    }

    public void HidePlayPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(PlayRT, PlayRT, PlayCG, 1, 0, () => { }); ;
    }

    public void ShowLoginPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HidePlayPanel();
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, null, LoginCG, 0, 1, () => { });
    }

    public void CloseLoginPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        ResetLoginPanel();
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, LoginRT, LoginCG, 1, 0, () => { CurrentEntryState = EntryStates.PLAY; });
    }

    public void ShowLinksPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HidePlayPanel();
        GameManager.Instance.AnimationsLT.FadePanel(LinksRT, null, LinksCG, 0, 1, () => { });
    }

    public void CloseLinksPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(LinksRT, LinksRT, LinksCG, 1, 0, () => { CurrentEntryState = EntryStates.PLAY; });
    }

    public void ShowSettingsPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HidePlayPanel();
        GameManager.Instance.AnimationsLT.FadePanel(SettingsRT, null, SettingsCG, 0, 1, () => { });
    }

    public void CloseSettingsPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(SettingsRT, SettingsRT, SettingsCG, 1, 0, () => { CurrentEntryState = EntryStates.PLAY; });
    }
    #endregion

    public void DisplayLoadingPanel()
    {
        LoadingPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void HideLoadingPanel()
    {
        LoadingPanel.SetActive(false);
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
