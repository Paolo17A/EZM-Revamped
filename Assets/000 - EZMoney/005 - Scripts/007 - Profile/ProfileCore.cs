using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using TMPro;

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
        SWAP
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

    [Header("DEBUGGER")]
    [ReadOnly] public List<CharacterInstanceData> ActualOwnedCharacters;
    private GameObject spawnedCharacter;
    private CharacterImageController spawnedCharacterImage;

    //===========================================================
    #endregion

    public IEnumerator InitializeProfileScene()
    {
        if(GameManager.Instance.DebugMode)
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

            foreach(CharacterInstanceData ownedCharacter in ActualOwnedCharacters)
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

        }
        yield return null;
    }

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
        GameManager.Instance.AnimationsLT.FadePanel(CharacterRT, null, CharacterCG, 0, 1, () => { });
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
        GameManager.Instance.SceneController.CurrentScene = "EntryScene";
    }
    #endregion
}
