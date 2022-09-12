using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System;
using System.Linq;
using TMPro;

public class ShopCore : MonoBehaviour
{
    #region STATE MACHINE
    //====================================================================================================
    [SerializeField][ReadOnly] private ShopStates shopState;
    public enum ShopStates
    {
        NONE,
        CHARACTERS,
        AUTOPILOT
    }
    private event EventHandler shopStateChange;
    public event EventHandler onShopStateChange
    {
        add
        {
            if (shopStateChange == null || shopStateChange.GetInvocationList().Contains(value))
                shopStateChange += value;
        }
        remove { shopStateChange -= value; }
    }

    public ShopStates CurrentShopState
    {
        get => shopState;
        set
        {
            shopState = value;
            shopStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    //==========================================================================================================
    #endregion

    #region VARIABLES
    //=========================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("CORE PANEL")]
    [SerializeField] private RectTransform CoreRT;
    [SerializeField] private CanvasGroup CoreCG;

    [Header("SHOP PANEL")]
    [SerializeField] private RectTransform ShopRT;
    [SerializeField] private CanvasGroup ShopCG;

    [Header("CHARACTERS PANEL")]
    [SerializeField] private RectTransform CharactersRT;
    [SerializeField] private CanvasGroup CharactersCG;
    [SerializeField] private List<PurchaseCharacterController> PurchasableCharacters;   

    [Header("AUTO PANEL")]
    [SerializeField] private RectTransform AutoRT;
    [SerializeField] private CanvasGroup AutoCG;
    [field: SerializeField] public Button AutoMiningBtn { get; set; }
    [field: SerializeField] public Button AutoFarmingBtn { get; set; }
    [field: SerializeField] public Button AutoFishingBtn { get; set; }
    [field:  SerializeField] public Button AutoWoodcuttingBtn { get; set; }

    [Header("VIRTUAL CURRENCIES")]
    [SerializeField] private TextMeshProUGUI EZCoinTMP;
    [SerializeField] private TextMeshProUGUI EZGemTMP;

    [Header("OPTIONS BUTTONS")]
    [SerializeField] private Button AutoBtn;
    [SerializeField] private Button CharactersBtn;
    [SerializeField] private Image AutoImage;
    [SerializeField] private Image CharactersImage;
    [SerializeField] private Sprite AutoSelectedSprite;
    [SerializeField] private Sprite AutoNotSelectedSprite;
    [SerializeField] private Sprite CharactersSelectedSprite;
    [SerializeField] private Sprite CharactersNotSelectedSprite;
    //=========================================================
    #endregion

    public IEnumerator InitializeShopScene()
    {
        if(GameManager.Instance.DebugMode)
        {
            UpdateEZCoinDisplay();
            CheckAutopilotPurchasability();
            CheckCharacterPurchasability();
            EZGemTMP.text = PlayerData.EZGem.ToString("n0");

            if (PlayerData.OwnsAutoMining)
                AutoMiningBtn.interactable = false;
            if (PlayerData.OwnsAutoFarming)
                AutoFarmingBtn.interactable = false;
            if (PlayerData.OwnsAutoFishing)
                AutoFishingBtn.interactable = false;
            if (PlayerData.OwnsAutoWoodCutting)
                AutoWoodcuttingBtn.interactable = false;
            
            GameManager.Instance.AnimationsLT.FadePanel(CoreRT, null, CoreCG, 0, 1, () => { });
            GameManager.Instance.AnimationsLT.FadePanel(ShopRT, null, ShopCG, 0, 1, () => { });

            CurrentShopState = ShopStates.CHARACTERS;
        }
        else
        {

        }
        yield return null;
    }

    #region PANELS
    public void ShowCharactersPanel()
    {
        CharactersImage.sprite = CharactersSelectedSprite;
        AutoImage.sprite = AutoNotSelectedSprite;
        CharactersBtn.interactable = false;
        AutoBtn.interactable = true;
        if (AutoRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(AutoRT, AutoRT, AutoCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(CharactersRT, null, CharactersCG, 0, 1, () => { });
    }

    public void ShowAutoPanel()
    {
        CharactersImage.sprite = CharactersNotSelectedSprite;
        AutoImage.sprite = AutoSelectedSprite;
        AutoBtn.interactable = false;
        CharactersBtn.interactable = true;
        if (CharactersRT.gameObject.activeSelf)
            GameManager.Instance.AnimationsLT.FadePanel(CharactersRT, CharactersRT, CharactersCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(AutoRT, null, AutoCG, 0, 1, () => { });
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

    public void UpdateEZCoinDisplay()
    {
        EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
    }

    public void CheckAutopilotPurchasability()
    {
        if(PlayerData.EZCoin < 5000)
        {
            AutoMiningBtn.interactable = false;
            AutoFarmingBtn.interactable = false;
            AutoFishingBtn.interactable = false;
            AutoWoodcuttingBtn.interactable = false;
        }
    }

    public void CheckCharacterPurchasability()
    {
        if (PlayerData.EZCoin < 2000)
            foreach (PurchaseCharacterController character in PurchasableCharacters)
                character.PurchaseBtn.interactable = false;
    }


    #endregion
}
