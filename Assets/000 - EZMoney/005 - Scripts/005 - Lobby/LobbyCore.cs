using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using System;
using System.Linq;

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

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;
    [SerializeField] private TextMeshProUGUI LoadingTMP;

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
    [SerializeField] private TextMeshProUGUI EZGemTMP;

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

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private int GameIndex;
    //=========================================================================
    #endregion

    #region INITIALIZATION
    public IEnumerator InitializeLobby()
    {
        if(GameManager.Instance.DebugMode)
        {
            EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
            EZGemTMP.text = PlayerData.EZGem.ToString("n0");
            SubscriptionLevelTMP.text = PlayerData.SubscriptionLevel;
            switch(PlayerData.SubscriptionLevel)
            {
                case "PEARL":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[0];
                    break;
                case "TOPAZ":
                    SubscriptionGemImage.sprite = SubscriptionGemSprites[1];
                    break;
                case "SAPHIRE":
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

            GameIndex = 0;
            PreviousGameBtn.interactable = false;
        }
        else
        {

        }
        yield return null;
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
        GameManager.Instance.AnimationsLT.FadePanel(GameRT, null, GameCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, null, ProfileCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ShopRT, null, ShopCG, 0, 1, () => { });
    }

    public void HideCorePanels()
    {
        GameManager.Instance.AnimationsLT.FadePanel(CurrencyRT, null, CurrencyCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(MenuRT, null, MenuCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(GameRT, null, GameCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, null, ProfileCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(ShopRT, null, ShopCG, 1, 0, () => { });
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
}
