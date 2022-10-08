using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using System;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;

public class LobbyCore : MonoBehaviour
{
    #region STATE MACHINE
    //================================================================================
    [SerializeField][ReadOnly] private LobbyStates lobbyState;
    public enum LobbyStates
    {
        NONE,
        CORE,
        SETTINGS,
        LEADERBOARD,
        QUEST,
        GIFT,
        MAIL
    }

    private event EventHandler lobbyStateChange;
    public event EventHandler onLobbySelectStateChange
    {
        add
        {
            if (lobbyStateChange == null || !lobbyStateChange.GetInvocationList().Contains(value))
                lobbyStateChange += value;
        }
        remove { lobbyStateChange -= value; }
    }

    public LobbyStates CurrentLobbyState
    {
        get => lobbyState;
        set
        {
            lobbyState = value;
            lobbyStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    //================================================================================
    #endregion

    #region VARIABLES
    //=========================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private IslandCore IslandCore;

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("CORE PANELS")]
    [SerializeField] private RectTransform CurrencyRT;
    [SerializeField] private CanvasGroup CurrencyCG;
    [SerializeField] private RectTransform MenuRT;
    [SerializeField] private CanvasGroup MenuCG;
    [SerializeField] private RectTransform GameRT;
    [SerializeField] private CanvasGroup GameCG;
    [SerializeField] private RectTransform ProfileRT;
    [SerializeField] private CanvasGroup ProfileCG;
    [SerializeField] private RectTransform ShopRT;
    [SerializeField] private CanvasGroup ShopCG;

    [Header("SETTINGS PANEL")]
    [SerializeField] private RectTransform SettingsRT;
    [SerializeField] private CanvasGroup SettingsCG;

    [Header("LEADERBOARD PANEL")]
    [SerializeField] private RectTransform LeaderboardRT;
    [SerializeField] private CanvasGroup LeaderboardCG;

    [Header("QUEST PANEL")]
    [SerializeField] private RectTransform QuestRT;
    [SerializeField] private CanvasGroup QuestCG;

    [Header("GIFT PANEL")]
    [SerializeField] private RectTransform GiftRT;
    [SerializeField] private CanvasGroup GiftCG;

    [Header("MAIL PANEL")]
    [SerializeField] private RectTransform MailRT;
    [SerializeField] private CanvasGroup MailCG;

    [Header("VIRTUAL CURRENCIES")]
    [SerializeField] private TextMeshProUGUI EZCoinTMP;
    [field: SerializeField] public TextMeshProUGUI EZGemTMP { get; set; }

    [Header("GAME SELECT")]
    [SerializeField] private Button GameSelectBtn;
    [SerializeField] private Image GameSelectImage;
    [SerializeField] private List<Sprite> GameSprites;
    [SerializeField] private Button PreviousGameBtn;
    [SerializeField] private Button NextGameBtn;

    [Header("PROFILE AREA")]
    [SerializeField] private Image ProfileImage;
    [SerializeField] private TextMeshProUGUI SubscriptionLevelTMP;
    [SerializeField] private Image SubscriptionGemImage;
    [SerializeField] private List<Sprite> SubscriptionGemSprites;
    [SerializeField] private TextMeshProUGUI TimeElapsedTMP;

    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] public GetUserDataRequest getUserData;
    [ReadOnly] public GetUserInventoryRequest getUserInventory;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private int GameIndex;
    private int failedCallbackCounter;
    //=========================================================================
    #endregion

    #region INITIALIZATION
    public IEnumerator InitializeLobby()
    {
        if (GameManager.Instance.DebugMode)
        {
            UpdateEZCoinDisplay();
            EZGemTMP.text = PlayerData.EZGem.ToString("n0");
            ProfileImage.sprite = GameManager.Instance.GetProperCharacter(PlayerData.DisplayPicture).displaySprite;
            SubscriptionLevelTMP.text = PlayerData.SubscriptionLevel;
            switch (PlayerData.SubscriptionLevel)
            {
                case "PEARL":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[0];
                    break;
                case "TOPAZ":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[1];
                    break;
                case "SAPPHIRE":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[2];
                    break;
                case "EMERALD":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[3];
                    break;
                case "RUBY":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[4];
                    break;
                case "DIAMOND":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[5];
                    break;
            }

            if (GameManager.Instance.SceneController.CurrentScene == "LobbyScene")
            {
                GameIndex = 0;
                PreviousGameBtn.interactable = false;
            }

            else if (GameManager.Instance.SceneController.CurrentScene == "IslandScene")
            {
                // TODO: look for individual zone tickets when not in debug mode
                IslandCore.UnlockIslandZones("MineA");
            }

        }
        else
            GetUserDataPlayFab();
        yield return null;
    }

    private void GetUserDataPlayFab()
    {
        PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if(resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        if (resultCallback.Data.ContainsKey("SubscriptionLevel"))
                        {
                            PlayerData.SubscriptionLevel = resultCallback.Data["SubscriptionLevel"].Value;
                            SubscriptionLevelTMP.text = PlayerData.SubscriptionLevel;
                            switch (PlayerData.SubscriptionLevel)
                            {
                                case "PEARL":
                                    SubscriptionGemImage.sprite = SubscriptionGemSprites[0];
                                    break;
                                case "TOPAZ":
                                    SubscriptionGemImage.sprite = SubscriptionGemSprites[1];
                                    break;
                                case "SAPPHIRE":
                                    SubscriptionGemImage.sprite = SubscriptionGemSprites[2];
                                    break;
                                case "EMERALD":
                                    SubscriptionGemImage.sprite = SubscriptionGemSprites[3];
                                    break;
                                case "RUBY":
                                    SubscriptionGemImage.sprite = SubscriptionGemSprites[4];
                                    break;
                                case "DIAMOND":
                                    SubscriptionGemImage.sprite = SubscriptionGemSprites[5];
                                    break;
                            }
                        }

                        if (resultCallback.Data.ContainsKey("DisplayImage"))
                        {
                            PlayerData.DisplayPicture = resultCallback.Data["DisplayImage"].Value;
                            ProfileImage.sprite = GameManager.Instance.GetProperCharacter(PlayerData.DisplayPicture).displaySprite;
                        }

                        if (resultCallback.Data.ContainsKey("CharactersRefreshed"))
                        {
                            if (resultCallback.Data["CharactersRefreshed"].Value == "0")
                            {
                                Debug.Log("WIll reset character stamina");
                                PlayerData.ElapsedGameplayTime = new TimeSpan(0, 0, 0);
                                PlayerPrefs.SetInt("ElapsedMinutes", 0);
                                PlayerPrefs.SetInt("ElapsedSeconds", 0);
                                GameManager.Instance.SceneController.AddActionLoadinList(ResetCharacterStamina());
                            }

                            TimeElapsedTMP.text = string.Format("{0:D2}:{1:D2}:{2:D2}", PlayerData.ElapsedGameplayTime.Hours, PlayerData.ElapsedGameplayTime.Minutes, PlayerData.ElapsedGameplayTime.Seconds);
                        }

                        if(resultCallback.Data.ContainsKey("AutoPilot"))
                        {
                            PlayerData.AutoMiningTimeLeft = GameManager.Instance.DeserializeIntValue(resultCallback.Data["AutoPilot"].Value, "Mining");
                            PlayerData.AutoFarmingTimeLeft = GameManager.Instance.DeserializeIntValue(resultCallback.Data["AutoPilot"].Value, "Farming");
                            PlayerData.AutoFishingTimeLeft = GameManager.Instance.DeserializeIntValue(resultCallback.Data["AutoPilot"].Value, "Fishing");
                            PlayerData.AutoWoodcuttingTimeLeft = GameManager.Instance.DeserializeIntValue(resultCallback.Data["AutoPilot"].Value, "Woodcutting");
                        }

                        if(resultCallback.Data.ContainsKey("Quests"))
                        {
                            PlayerData.DailyLogin = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "DailyCheckIn");
                            PlayerData.SocMedShared = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "SocMedShared");
                            PlayerData.AdsWatched = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "AdsWatched");
                            PlayerData.MinsPlayed = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "MinsPlayed");
                            PlayerData.CoinsGained = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "EZCoinsGained");
                            PlayerData.DailyClaimed = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "DailyQuestClaimed");

                            if(PlayerPrefs.HasKey("ElapsedSeconds"))
                                PlayerData.ElapsedGameplayTime = new TimeSpan(0, PlayerData.MinsPlayed, PlayerPrefs.GetInt("ElapsedSeconds"));
                            else
                                PlayerData.ElapsedGameplayTime = new TimeSpan(0, PlayerData.MinsPlayed, 0);
                        }

                        if (GameManager.Instance.SceneController.CurrentScene == "LobbyScene")
                        {
                            GameIndex = 0;
                            PreviousGameBtn.interactable = false;
                        }
                    }
                    else
                    {
                        GameManager.Instance.DisplaySpecialErrorPanel("You have logged into another device");
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        GetUserDataPlayFab,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
    }

    public IEnumerator GetUserVirtualCurrency()
    {
        GetVirtualCurrencyPlayfab();
        yield return null;
    }

    public void GetVirtualCurrencyPlayfab()
    {
        PlayFabClientAPI.GetUserInventory(getUserInventory,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.EZCoin = resultCallback.VirtualCurrency["EC"];
                PlayerData.EZGem = resultCallback.VirtualCurrency["EG"];
                EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
                EZGemTMP.text = PlayerData.EZGem.ToString("n0");

                if(GameManager.Instance.SceneController.CurrentScene == "IslandScene")
                {
                    foreach(ItemInstance item in resultCallback.Inventory)
                    {
                        if (item.ItemId == "MineA")
                        {
                            PlayerData.CanAccessMineA = true;
                            IslandCore.MineA.UnlockZone();
                        }
                        if (item.ItemId == "MineB")
                        {
                            PlayerData.CanAccessMineB = true;
                            IslandCore.MineB.UnlockZone();
                        }
                        if(item.ItemId == "FarmB")
                        {
                            PlayerData.CanAccessFarmB = true;
                            IslandCore.FarmB.UnlockZone();
                        }    
                    }

                }
                HideLoadingPanel();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetVirtualCurrencyPlayfab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    public IEnumerator ResetCharacterStamina()
    {
        ResetCharacterStaminaPlayFab();
        yield return null;
    }

    private void ResetCharacterStaminaPlayFab()
    {
        DisplayLoadingPanel();
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "IncreaseAllCharacterStamina",
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            HideLoadingPanel();
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                    ResetCharacterStaminaPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
        }); ;
    }

    public void NextGame()
    {
        GameIndex++;
        GameSelectImage.sprite = GameSprites[GameIndex];
        GameSelectBtn.interactable = false;
        PreviousGameBtn.interactable = true;
        if (GameIndex == 3)
            NextGameBtn.interactable = false;
    }

    public void PreviousGame()
    {
        GameIndex--;
        GameSelectImage.sprite = GameSprites[GameIndex];

        NextGameBtn.interactable = true;
        if (GameIndex == 0)
        {
            GameSelectBtn.interactable = true;
            PreviousGameBtn.interactable = false;
        }
        else
            GameSelectBtn.interactable = false;
    }
    #endregion

    #region PANELS
    public void ShowCorePanels()
    {
        GameManager.Instance.AnimationsLT.FadePanel(CurrencyRT, null, CurrencyCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(MenuRT, null, MenuCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, null, ProfileCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ShopRT, null, ShopCG, 0, 1, () => { });
        if(GameManager.Instance.SceneController.CurrentScene == "LobbyScene")
            GameManager.Instance.AnimationsLT.FadePanel(GameRT, null, GameCG, 0, 1, () => { });

    }

    public void HideCorePanels()
    {
        GameManager.Instance.AnimationsLT.FadePanel(CurrencyRT, null, CurrencyCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(MenuRT, null, MenuCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, null, ProfileCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ShopRT, null, ShopCG, 1, 0, () => { });
        if (GameManager.Instance.SceneController.CurrentScene == "LobbyScene")
            GameManager.Instance.AnimationsLT.FadePanel(GameRT, null, GameCG, 1, 0, () => { });

    }

    public void ShowSettingsPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(SettingsRT, null, SettingsCG, 0, 1, () => { });
    }

    public void CloseSettingsPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(SettingsRT, SettingsRT, SettingsCG, 1, 0, () => { CurrentLobbyState = LobbyStates.CORE; });
    }

    public void ShowLeaderboardPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(LeaderboardRT, null, LeaderboardCG, 0, 1, () => { });
    }

    public void CloseLeaderboardPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(LeaderboardRT, LeaderboardRT, LeaderboardCG, 1, 0, () => { CurrentLobbyState = LobbyStates.CORE; });
    }

    public void ShowQuestPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(QuestRT, null, QuestCG, 0, 1, () => { });
    }

    public void CloseQuestPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(QuestRT, QuestRT, QuestCG, 1, 0, () => { CurrentLobbyState = LobbyStates.CORE; });
    }

    public void ShowGiftPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(GiftRT, null, GiftCG, 0, 1, () => { });
    }

    public void CloseGiftPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(GiftRT, GiftRT, GiftCG, 1, 0, () => { CurrentLobbyState = LobbyStates.CORE; });
    }

    public void ShowMailPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(MailRT, null, MailCG, 0, 1, () => { });
    }

    public void CloseMailPanel()
    {
        if (!GameManager.Instance.CanUseButtons)
            return;
        GameManager.Instance.AnimationsLT.FadePanel(MailRT, MailRT, MailCG, 1, 0, () => { CurrentLobbyState = LobbyStates.CORE; });
    }
    #endregion

    #region UTILITY
    public void OpenProfileScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "ProfileScene";
    }

    public void OpenShopScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "ShopScene";
    }

    public void OpenLobbyScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
    }

    public void OpenIslandScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "IslandScene";
    }

    public void UpdateEZCoinDisplay()
    {
        EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
    }

    public void UpdateEZGemDisplay()
    {
        EZGemTMP.text = PlayerData.EZGem.ToString("n0");
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
