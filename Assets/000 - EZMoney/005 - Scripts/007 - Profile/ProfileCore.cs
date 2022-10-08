using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using static CharacterInstanceData;

public class ProfileCore : MonoBehaviour
{
    #region STATE MACHINE
    //================================================================================
    [SerializeField][ReadOnly] private ProfileStates profileState;
    public enum ProfileStates
    {
        NONE,
        DISPLAY,
        PROFILE,
        CHARACTER,
        AUTO,
        SWAP,
        AUTODATA
    }

    private event EventHandler profileStateChange;
    public event EventHandler onProfileSelectStateChange
    {
        add
        {
            if (profileStateChange == null || !profileStateChange.GetInvocationList().Contains(value))
                profileStateChange += value;
        }
        remove { profileStateChange -= value; }
    }

    public ProfileStates CurrentProfileState
    {
        get => profileState;
        set
        {
            profileState = value;
            profileStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    //================================================================================
    #endregion

    #region VARIABLES
    //===========================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("LOADING PANEL")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("CORE PANELS")]
    [SerializeField] private RectTransform TopRT;
    [SerializeField] private CanvasGroup TopCG;
    [SerializeField] private RectTransform SideButtonsRT;
    [SerializeField] private CanvasGroup SideButtonsCG;
    [SerializeField] private TextMeshProUGUI DisplayNameTMP;
    [SerializeField] private TextMeshProUGUI PlayfabIDTMP;
    [field: SerializeField] public Image DisplayImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI EZCoinsTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI EZGemsTMP { get; set; }

    [Header("DISPLAY PANEL")]
    [SerializeField] private RectTransform DisplayRT;
    [SerializeField] private CanvasGroup DisplayCG;

    [Header("PROFILE PANEL")]
    [SerializeField] private RectTransform ProfileRT;
    [SerializeField] private CanvasGroup ProfileCG;
    [SerializeField] private TextMeshProUGUI MiningStatsTMP;
    [SerializeField] private TextMeshProUGUI FarmingStatsTMP;
    [SerializeField] private TextMeshProUGUI FishingStatsTMP;
    [SerializeField] private TextMeshProUGUI WoodcuttingStatsTMP;

    [Header("CHARACTER PANEL")]
    [SerializeField] private RectTransform CharacterRT;
    [SerializeField] private CanvasGroup CharacterCG;
    [SerializeField] private Transform CharacterContainer;
    [SerializeField] private GameObject CharacterPrefab;

    [Header("AUTO PANEL")]
    [SerializeField] private RectTransform AutoRT;
    [SerializeField] private CanvasGroup AutoCG;
    [SerializeField] private Button AutoMiningBtn;
    [SerializeField] private Button AutoFarmingBtn;
    [SerializeField] private Button AutoFishingBtn;
    [SerializeField] private Button AutoWoodcuttingBtn;

    [Header("SWAP PANEL")]
    [SerializeField] private RectTransform SwapRT;
    [SerializeField] private CanvasGroup SwapCG;

    [Header("AUTOPILOT DATA PANEL")]
    [SerializeField] private RectTransform AutoPilotDataRT;
    [SerializeField] private CanvasGroup AutoPilotDataCG;
    [field: SerializeField][field: ReadOnly] public AutopilotDataCore SelectedAutoPilot;
    [field: SerializeField] public TextMeshProUGUI AutoTimeLeftTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI WorkersDeployedTMP { get; set; }
    [field: SerializeField] public Image SelectedPilotImage { get; set; }


    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] public GetUserDataRequest getUserData;
    [ReadOnly] public GetUserInventoryRequest getUserInventory;
    [ReadOnly] public GetPlayerStatisticsRequest getPlayerStatistics;
    [ReadOnly] public ListUsersCharactersRequest listUsersCharacters;
    [ReadOnly] public GetCharacterDataRequest getCharacterData;

    [Header("DEBUGGER")]
    [ReadOnly] public List<CharacterInstanceData> ActualOwnedCharacters;
    private GameObject spawnedCharacter;
    private CharacterImageController spawnedCharacterImage;
    private int failedCallbackCounter;

    //===========================================================
    #endregion

    #region INITIALIZATION
    public IEnumerator InitializeProfileScene()
    {
        DisplayImage.sprite = GameManager.Instance.GetProperCharacter(PlayerData.DisplayPicture).displaySprite;
        DisplayNameTMP.text = PlayerData.DisplayName;
        PlayfabIDTMP.text = PlayerData.PlayfabID;
        EZCoinsTMP.text = PlayerData.EZCoin.ToString("n0");
        EZGemsTMP.text = PlayerData.EZGem.ToString("n0");
        MiningStatsTMP.text = PlayerData.MiningEZCoin.ToString("n0");
        FarmingStatsTMP.text = PlayerData.FarmingEZCoin.ToString("n0");
        FishingStatsTMP.text = PlayerData.FishingEZCoin.ToString("n0");
        WoodcuttingStatsTMP.text = PlayerData.WoodcuttingEZCoin.ToString("n0");

        if (!PlayerData.OwnsAutoMining)
            AutoMiningBtn.interactable = false;
        if (!PlayerData.OwnsAutoFarming)
            AutoFarmingBtn.interactable = false;
        if (!PlayerData.OwnsAutoFishing)
            AutoFishingBtn.interactable = false;
        if (!PlayerData.OwnsAutoWoodCutting)
            AutoWoodcuttingBtn.interactable = false;

        //CHARACTER INITIALIZATION
        ActualOwnedCharacters.Clear();
        foreach (Transform child in CharacterContainer)
            Destroy(child.gameObject);

        foreach (CharacterInstanceData ownedCharacter in PlayerData.OwnedCharacters)
        {
            if (ownedCharacter.BaseCharacterData != null)
                ActualOwnedCharacters.Add(ownedCharacter);
            else
                break;
        }

        foreach (CharacterInstanceData ownedCharacter in ActualOwnedCharacters)
        {
            spawnedCharacter = Instantiate(CharacterPrefab);
            spawnedCharacter.transform.SetParent(CharacterContainer);
            spawnedCharacter.transform.localScale = new Vector3(1, 1, 1);
            spawnedCharacter.transform.localPosition = new Vector3(0, 0, 0);

            spawnedCharacterImage = spawnedCharacter.GetComponent<CharacterImageController>();
            spawnedCharacterImage.CharacterID = ownedCharacter.CharacterInstanceID;
            spawnedCharacterImage.CharacterData = ownedCharacter.BaseCharacterData;
            spawnedCharacterImage.SetCharacterImageData();
            spawnedCharacterImage.StaminaTMP.text = ownedCharacter.CharacterCurrentStamina.ToString() + "/" + ownedCharacter.BaseCharacterData.stamina.ToString();
        }
        yield return null;
    }

    public IEnumerator GetUserData()
    {
        GetUserDataPlayFab();
        yield return null;
    }

    private void GetUserDataPlayFab()
    {
        DisplayLoadingPanel();
        PlayFabClientAPI.GetUserData(getUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                {
                    PlayfabIDTMP.text = PlayerData.PlayfabID;
                    PlayerData.DisplayPicture = resultCallback.Data["DisplayImage"].Value;
                    DisplayImage.sprite = GameManager.Instance.GetProperCharacter(PlayerData.DisplayPicture).displaySprite;
                    DisplayNameTMP.text = PlayerData.DisplayName;

                    GameManager.Instance.SceneController.AddActionLoadinList(GetUserInventory());
                    GameManager.Instance.SceneController.AddActionLoadinList(GetPlayerStatistics());
                    GameManager.Instance.SceneController.AddActionLoadinList(ListAllCharacters());
                }
                else
                    GameManager.Instance.DisplayDualLoginErrorPanel();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetUserDataPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    public IEnumerator GetUserInventory()
    {
        GetUserInventoryPlayFab();
        yield return null;
    }

    public void GetUserInventoryPlayFab()
    {
        PlayFabClientAPI.GetUserInventory(getUserInventory,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.EZCoin = resultCallback.VirtualCurrency["EC"];
                PlayerData.EZGem = resultCallback.VirtualCurrency["EG"];
                EZCoinsTMP.text = PlayerData.EZCoin.ToString("n0");
                EZGemsTMP.text = PlayerData.EZGem.ToString("n0");
                foreach(ItemInstance item in resultCallback.Inventory)
                {
                    if (item.ItemId == "MiningPilot")
                        PlayerData.OwnsAutoMining = true;
                    if (item.ItemId == "FarmingPilot")
                        PlayerData.OwnsAutoFarming = true;
                    if (item.ItemId == "FishingPilot")
                        PlayerData.OwnsAutoFishing = true;
                    if (item.ItemId == "WoodcuttingPilot")
                        PlayerData.OwnsAutoWoodCutting = true;
                }

                MiningStatsTMP.text = PlayerData.MiningEZCoin.ToString("n0");
                FarmingStatsTMP.text = PlayerData.FarmingEZCoin.ToString("n0");
                FishingStatsTMP.text = PlayerData.FishingEZCoin.ToString("n0");
                WoodcuttingStatsTMP.text = PlayerData.WoodcuttingEZCoin.ToString("n0");

                if (!PlayerData.OwnsAutoMining)
                    AutoMiningBtn.interactable = false;
                if (!PlayerData.OwnsAutoFarming)
                    AutoFarmingBtn.interactable = false;
                if (!PlayerData.OwnsAutoFishing)
                    AutoFishingBtn.interactable = false;
                if (!PlayerData.OwnsAutoWoodCutting)
                    AutoWoodcuttingBtn.interactable = false;

                HideLoadingPanel();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetUserInventoryPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    public IEnumerator GetPlayerStatistics()
    {
        GetPlayerStatisticsPlayFab();
        yield return null;
    }

    private void GetPlayerStatisticsPlayFab()
    {
        DisplayLoadingPanel();
        PlayFabClientAPI.GetPlayerStatistics(getPlayerStatistics,
            resultCallback =>
            {
                foreach(StatisticValue stat in resultCallback.Statistics)
                {
                    if(stat.StatisticName == "TotalEZCoinMining")
                    {
                        PlayerData.MiningEZCoin = stat.Value;
                        MiningStatsTMP.text = PlayerData.MiningEZCoin.ToString("n0");
                    }
                    if (stat.StatisticName == "TotalEZCoinFarming")
                    {
                        PlayerData.FarmingEZCoin = stat.Value;
                        FarmingStatsTMP.text = PlayerData.FarmingEZCoin.ToString("n0");
                    }
                    if (stat.StatisticName == "TotalEZCoinFishing")
                    {
                        PlayerData.FishingEZCoin = stat.Value;
                        FishingStatsTMP.text = PlayerData.FishingEZCoin.ToString("n0");
                    }
                    if (stat.StatisticName == "TotalEZCoinWoodcutting")
                    {
                        PlayerData.WoodcuttingEZCoin = stat.Value;
                        WoodcuttingStatsTMP.text = PlayerData.WoodcuttingEZCoin.ToString("n0");
                    }
                }
                HideLoadingPanel();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetPlayerStatisticsPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private IEnumerator ListAllCharacters()
    {
        ListAllCharactersPlayFab();
        yield return null;
    }

    public void ListAllCharactersPlayFab()
    {
        DisplayLoadingPanel();
        PlayFabClientAPI.GetAllUsersCharacters(listUsersCharacters,
            resultCallback =>
            {
                for(int i = 0; i < resultCallback.Characters.Count; i++)
                {
                    PlayerData.OwnedCharacters[i].CharacterInstanceID = resultCallback.Characters[i].CharacterId;
                }

                GetEachCharacterDataPlayFab();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    ListAllCharactersPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void GetEachCharacterDataPlayFab()
    {
        ActualOwnedCharacters.Clear();
        foreach (Transform child in CharacterContainer)
            Destroy(child.gameObject);

        foreach (CharacterInstanceData ownedCharacter in PlayerData.OwnedCharacters)
        {
            if (ownedCharacter.CharacterInstanceID != "")
                ActualOwnedCharacters.Add(ownedCharacter);
            else
                break;
        }
        for (int i = 0; i < ActualOwnedCharacters.Count; i++)
        {
            spawnedCharacter = Instantiate(CharacterPrefab);
            spawnedCharacter.transform.SetParent(CharacterContainer);
            spawnedCharacter.transform.localScale = new Vector3(1, 1, 1);
            spawnedCharacter.transform.localPosition = new Vector3(0, 0, 0);

            spawnedCharacterImage = spawnedCharacter.GetComponent<CharacterImageController>();
            spawnedCharacterImage.ImageCG.alpha = 0;
            spawnedCharacterImage.CharacterID = ActualOwnedCharacters[i].CharacterInstanceID;
            spawnedCharacterImage.ProfileCore = this;
            spawnedCharacterImage.GetCharacterData(ActualOwnedCharacters[i]);
        }
        HideLoadingPanel();
    }

    #endregion

    #region PANELS
    public void ShowCorePanels()
    {
        GameManager.Instance.AnimationsLT.FadePanel(TopRT, null, TopCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(SideButtonsRT, null, SideButtonsCG, 0, 1, () => { });
        CurrentProfileState = ProfileStates.PROFILE;
    }
    public void ShowDisplayPanel()
    {
        if (DisplayRT.gameObject.activeSelf)
            return;
        HideActivePanel();
        GameManager.Instance.AnimationsLT.FadePanel(DisplayRT, null, DisplayCG, 0, 1, () => { });
    }

    public void ShowProfilePanel()
    {
        if (ProfileRT.gameObject.activeSelf)
            return;
        HideActivePanel();
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, null, ProfileCG, 0, 1, () => { });
    }

    public void ShowCharacterPanel()
    {
        if (CharacterRT.gameObject.activeSelf)
            return;
        HideActivePanel();
        GameManager.Instance.AnimationsLT.FadePanel(CharacterRT, null, CharacterCG, 0, 1, () =>
        {
            /*if (!GameManager.Instance.DebugMode)
                ListAllCharactersPlayFab();*/
        } );
    }

    public void ShowAutoPanel()
    {
        if (AutoRT.gameObject.activeSelf)
            return;
        HideActivePanel();
        GameManager.Instance.AnimationsLT.FadePanel(AutoRT, null, AutoCG, 0, 1, () => { });
    }

    public void ShowSwapPanel()
    {
        if (SwapRT.gameObject.activeSelf)
            return;
        HideActivePanel();
        GameManager.Instance.AnimationsLT.FadePanel(SwapRT, null, SwapCG, 0, 1, () => { });
    }

    public void ShowAutoDataPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(AutoPilotDataRT, null, AutoPilotDataCG, 0, 1, () => { });
    }
    
    public void HideAutoDataPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(AutoPilotDataRT, AutoPilotDataRT, AutoPilotDataCG, 1, 0, () => 
        {
            SelectedAutoPilot = null;
            CurrentProfileState = ProfileStates.AUTO; 
        });

    }

    public void HideActivePanel()
    {
        if (DisplayRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(DisplayRT, DisplayRT, DisplayCG, 1, 0, () => { });
        else if (ProfileRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, ProfileRT, ProfileCG, 1, 0, () => { });
        else if (CharacterRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(CharacterRT, CharacterRT, CharacterCG, 1, 0, () => { });
        else if (AutoRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(AutoRT, AutoRT, AutoCG, 1, 0, () => { });
        else if (SwapRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(SwapRT, SwapRT, SwapCG, 1, 0, () => { });
    }
    #endregion

    #region UTILITY
    public void OpenLobbyScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
    }

    public void OpenPreviousScene()
    {
        GameManager.Instance.SceneController.CurrentScene = GameManager.Instance.SceneController.LastScene;
    }

    public void LogOutButton()
    {
        PlayerPrefs.DeleteAll();
        PlayerData.ResetPlayerData();
        GameManager.Instance.SceneController.CurrentScene = "EntryScene";
    }

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
        HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
