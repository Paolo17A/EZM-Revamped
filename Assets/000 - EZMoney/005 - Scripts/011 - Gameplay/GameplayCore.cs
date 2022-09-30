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
using static CharacterInstanceData;

public class GameplayCore : MonoBehaviour
{
    #region STATE MACHINE
    //================================================================================
    [SerializeField][ReadOnly] private GameplayStates gameplayState;
    public enum GameplayStates
    {
        NONE,
        CORE,
        CHARACTER,
        INVENTORY,
        AUTOPILOT
    }

    private event EventHandler gameplayStateChange;
    public event EventHandler onGameplayStateChange
    {
        add
        {
            if (gameplayStateChange == null || !gameplayStateChange.GetInvocationList().Contains(value))
                gameplayStateChange += value;
        }
        remove { gameplayStateChange -= value; }
    }

    public GameplayStates CurrentGameplayState
    {
        get => gameplayState;
        set
        {
            gameplayState = value;
            gameplayStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    //================================================================================
    #endregion

    #region VARIABLES
    //====================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private string Autopilot;
    [SerializeField] public Roles NeededRole;

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("CORE PANELS")]
    [SerializeField] private RectTransform VirtualCurrencyRT;
    [SerializeField] private CanvasGroup VirtualCurrencyCG;
    [SerializeField] private TextMeshProUGUI EZCoinTMP;
    [SerializeField] private TextMeshProUGUI EZGemTMP;
    [SerializeField] private RectTransform BottomButtonsRT;
    [SerializeField] private CanvasGroup BottomButtonsCG;
    [SerializeField] private Button AutopilotBtn;

    [Header("DEPLOYED CHARACTERS PANELS")]
    [SerializeField] private RectTransform DeployedCharactersRT;
    [SerializeField] private CanvasGroup DeployedCharactersCG;
    [SerializeField] public Sprite EmptySlotSprite;
    [SerializeField] private List<CharacterSlotController> AllCharacterSlots;
    [SerializeField][ReadOnly] public CharacterSlotController SelectedCharacterSlot;
    [SerializeField][ReadOnly] public List<CharacterInstanceData> DeployedCharacters;

    [Header("UNDEPLOYED CHARACTERS PANEL")]
    [SerializeField] private RectTransform UndeployedCharactersRT;
    [SerializeField][ReadOnly] public List<CharacterInstanceData> UndeployedCharacters;
    [SerializeField] private Button PreviousCharacterBtn;
    [SerializeField] private Button NextCharacterBtn;
    [SerializeField] private Button CurrentCharacterBtn;
    [SerializeField] private Image CurrentCharacterImage;
    [SerializeField][ReadOnly] private int CurrentCharacterIndex;

    [Header("INVENTORY PANEL")]
    [SerializeField] private RectTransform InventoryRT;
    [SerializeField] private CanvasGroup InventoryCG;
    [SerializeField] private TextMeshProUGUI IronOreTMP;
    [SerializeField] private TextMeshProUGUI TinOreTMP;
    [SerializeField] private TextMeshProUGUI CopperOreTMP;
    [SerializeField] private TextMeshProUGUI SilverOreTMP;
    [SerializeField] private TextMeshProUGUI GoldOreTMP;
    [SerializeField] private TextMeshProUGUI PlatinumOreTMP;
    [SerializeField] private TextMeshProUGUI DiamondOreTMP;
    [SerializeField] private TextMeshProUGUI TotalEZCoinValueTMP;
    [SerializeField][ReadOnly] private int TotalEZCoinValue;
    [SerializeField] private Button SellBtn;

    [Header("AUTOPILOT PANEL")]
    [SerializeField] private RectTransform AutopilotRT;
    [SerializeField] private CanvasGroup AutopilotCG;
    [SerializeField][ReadOnly] public List<CharacterInstanceData> AutomatedCharacters;
    [SerializeField] public List<CharacterSlotController> AutomatedCharacterSlots;
    [SerializeField] public Slider AutomationSlider;
    [SerializeField][ReadOnly] public bool AutomationActivated;
    [SerializeField] public Button StartAutomationBtn;
    [SerializeField] private List<CharacterAutomationController> AllAutomationControllers;
    [SerializeField][ReadOnly] private int TotalAutoEZCoinValue;
    [SerializeField] private Button SellAutoBtn;
    [field: SerializeField] public TextMeshProUGUI AutoIronTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI AutoCopperTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI AutoTinTMP { get; set; }
    [field: SerializeField][field: ReadOnly] public int AutoIron { get; set; }
    [field: SerializeField][field: ReadOnly] public int AutoCopper { get; set; }
    [field: SerializeField][field: ReadOnly] public int AutoTin { get; set; }


    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] public GetUserDataRequest getUserData;
    [ReadOnly] public GetUserInventoryRequest getUserInventory;
    [ReadOnly] public GetPlayerStatisticsRequest getPlayerStatistics;
    [ReadOnly] public ListUsersCharactersRequest listUsersCharacters;
    [ReadOnly] public GetCharacterDataRequest getCharacterData;
    [ReadOnly] public UpdateCharacterDataRequest updateCharacterData;
    [ReadOnly] public ConsumeItemRequest consumeItem;
    [ReadOnly] public UpdatePlayerStatisticsRequest updatePlayerStatistics;
    [ReadOnly] public StatisticUpdate statisticUpdate;
    [ReadOnly] public StatisticUpdate statisticUpdate1;
    [ReadOnly] public UpdateUserDataRequest updateUserData;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private bool DisplayingUndeployedCharacters;
    [SerializeField][ReadOnly] public List<CharacterInstanceData> ActualOwnedCharacters;
    [SerializeField][ReadOnly] public List<CharacterInstanceData> FilteredCharacters;
    [SerializeField][ReadOnly] private int failedCallbackCounter;
    private bool alreadyGettingInventory;
    //====================================================================
    #endregion

    #region INITIALIZATION
    public IEnumerator InitializeGameplay()
    {
        EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
        EZGemTMP.text = PlayerData.EZGem.ToString("n0");
        if (Autopilot == "Miningpilot" && PlayerData.OwnsAutoMining)
            AutopilotBtn.interactable = true;
        SellAutoBtn.interactable = false;

        if (NeededRole == Roles.MINER)
            AutomationSlider.value = PlayerData.AutoMiningTimeLeft / 1800;
        else if (NeededRole == Roles.FARMER)
            AutomationSlider.value = PlayerData.AutoFarmingTimeLeft / 1800;
        FilteredCharacters.Clear();
        foreach (CharacterInstanceData ownedCharacter in PlayerData.OwnedCharacters)
        {
            if (ownedCharacter.BaseCharacterData != null)
            {
                if(ownedCharacter.CharacterCurrentRole == NeededRole)
                {
                    FilteredCharacters.Add(ownedCharacter);
                    if(ownedCharacter.OnAutoPilot)
                    {
                        foreach (CharacterSlotController slot in AutomatedCharacterSlots)
                            if (slot.ThisCharacterInstance == null)
                            {
                                slot.ThisCharacterInstance = ownedCharacter;
                                slot.InitializeCharacterSlot();
                                AutomatedCharacters.Add(ownedCharacter);
                                break;
                            }
                    }
                    else
                        UndeployedCharacters.Add(ownedCharacter);
                }
            }
            else
                break;
        }
        ProcessInventoryPanel();
        CalculateEZCoinValue();
        yield return null;
    }

    public IEnumerator GetUserData()
    {
        GetUserDataPlayFab();
        yield return null;
    }

    private void GetUserDataPlayFab()
    {
        PlayFabClientAPI.GetUserData(getUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                {
                    PlayerData.DailyLogin = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "DailyCheckIn");
                    PlayerData.SocMedShared = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "SocMedShared");
                    PlayerData.AdsWatched = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "AdsWatched");
                    PlayerData.CoinsGained = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "EZCoinsGained");
                    PlayerData.DailyClaimed = GameManager.Instance.DeserializeIntValue(resultCallback.Data["Quests"].Value, "DailyQuestClaimed");

                    if (NeededRole == Roles.MINER)
                        AutomationSlider.value = PlayerData.AutoMiningTimeLeft / 1800;
                    else if (NeededRole == Roles.FARMER)
                        AutomationSlider.value = PlayerData.AutoFarmingTimeLeft / 1800;
                    AutoIronTMP.text = AutoIron.ToString("n0");
                    AutoTinTMP.text = AutoTin.ToString("n0");
                    AutoCopperTMP.text = AutoCopper.ToString("n0");
                    SellAutoBtn.interactable = false;

                    GameManager.Instance.SceneController.AddActionLoadinList(GetUserInventory());
                    GameManager.Instance.SceneController.AddActionLoadinList(GetPlayerStatistics());
                    GameManager.Instance.SceneController.AddActionLoadinList(ListAllCharacters());
                }
                else
                {
                    HideLoadingPanel();
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

    public IEnumerator GetUserInventory()
    {
        GetUserInventoryPlayFab();
        yield return null;
    }

    public void GetUserInventoryPlayFab()
    {
        if(!alreadyGettingInventory)
        {
            alreadyGettingInventory = true;
            PlayFabClientAPI.GetUserInventory(getUserInventory,
            resultCallback =>
            {
                alreadyGettingInventory = false;
                failedCallbackCounter = 0;
                PlayerData.EZCoin = resultCallback.VirtualCurrency["EC"];
                PlayerData.EZGem = resultCallback.VirtualCurrency["EG"];
                EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
                EZGemTMP.text = PlayerData.EZGem.ToString("n0");

                PlayerData.CopperCount = 0;
                PlayerData.CopperInstanceID = "";
                PlayerData.TinCount = 0;
                PlayerData.TinInstanceID = "";
                PlayerData.IronCount = 0;
                PlayerData.IronInstanceID = "";
                foreach (ItemInstance item in resultCallback.Inventory)
                {
                    if (item.ItemClass == "AUTO")
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
                    else if (item.ItemClass == "ORE")
                    {
                        if (item.ItemId == "CopperOre")
                        {
                            PlayerData.CopperCount = (int)item.RemainingUses;
                            PlayerData.CopperInstanceID = item.ItemInstanceId;
                        }
                        if (item.ItemId == "TinOre")
                        {
                            PlayerData.TinCount = (int)item.RemainingUses;
                            PlayerData.TinInstanceID = item.ItemInstanceId;
                        }
                        if (item.ItemId == "IronOre")
                        {
                            PlayerData.IronCount = (int)item.RemainingUses;
                            PlayerData.IronInstanceID = item.ItemInstanceId;
                        }
                    }
                }

                if ((Autopilot == "Miningpilot" && PlayerData.OwnsAutoMining) || (Autopilot == "Fishingpilot" && PlayerData.OwnsAutoFishing))
                    AutopilotBtn.interactable = true;

                ProcessInventoryPanel();
                CalculateEZCoinValue();
                HideLoadingPanel();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetUserInventoryPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
        }
        
    }

    public IEnumerator GetPlayerStatistics()
    {
        GetPlayerStatisticsPlayFab();
        yield return null;
    }

    private void GetPlayerStatisticsPlayFab()
    {
        PlayFabClientAPI.GetPlayerStatistics(getPlayerStatistics,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                foreach(StatisticValue stat in resultCallback.Statistics)
                {
                    if (stat.StatisticName == "TotalEZCoinOverall")
                        PlayerData.LifetimeEZCoin = stat.Value;
                    else if (stat.StatisticName == "TotalEZCoinMining")
                        PlayerData.MiningEZCoin = stat.Value;
                    else if (stat.StatisticName == "TotalEZCoinFarming")
                        PlayerData.FarmingEZCoin = stat.Value;
                    else if (stat.StatisticName == "TotalEZCoinFishing")
                        PlayerData.FishingEZCoin = stat.Value;
                    else if (stat.StatisticName == "TotalEZCoinWoodcutting")
                        PlayerData.WoodcuttingEZCoin = stat.Value;
                    else if (stat.StatisticName == "TotalEZGemOverall")
                        PlayerData.LifetimeEZGem = stat.Value;
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetPlayerStatisticsPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    public IEnumerator ListAllCharacters()
    {
        ListAllCharactersPlayFab();
        yield return null;
    }

    private void ListAllCharactersPlayFab()
    {
        PlayFabClientAPI.GetAllUsersCharacters(listUsersCharacters,
           resultCallback =>
           {
               foreach (CharacterInstanceData character in PlayerData.OwnedCharacters)
                   character.ResetCharacterInstance();
               for (int i = 0; i < resultCallback.Characters.Count; i++)
                   PlayerData.OwnedCharacters[i].CharacterInstanceID = resultCallback.Characters[i].CharacterId;

               foreach (CharacterInstanceData ownedCharacter in PlayerData.OwnedCharacters)
               {
                   if (ownedCharacter.CharacterInstanceID != "")
                       ActualOwnedCharacters.Add(ownedCharacter);
                   else
                       break;
               }
               GameManager.Instance.SceneController.AddActionLoadinList(GetCharacterData());
           },
           errorCallback =>
           {
               ErrorCallback(errorCallback.Error,
                   ListAllCharactersPlayFab,
                   () => ProcessError(errorCallback.ErrorMessage));
           });
    }

    private IEnumerator GetCharacterData()
    {
        GetCharacterDataPlayFab();
        yield return null;
    }

    private void GetCharacterDataPlayFab()
    {
        int checkedCharacters = 0;
        foreach(CharacterInstanceData ownedCharacter in ActualOwnedCharacters)
        {
            getCharacterData.CharacterId = ownedCharacter.CharacterInstanceID;
            PlayFabClientAPI.GetCharacterData(getCharacterData,
            resultCallback =>
            {
                ownedCharacter.BaseCharacterData = GameManager.Instance.GetProperCharacter(resultCallback.Data["AnimalID"].Value);
                ownedCharacter.CharacterCurrentStamina = int.Parse(resultCallback.Data["CurrentStamina"].Value);
                if (resultCallback.Data["OnAutopilot"].Value == "1")
                    ownedCharacter.OnAutoPilot = true;
                else
                    ownedCharacter.OnAutoPilot = false;

                switch (resultCallback.Data["Role"].Value)
                {
                    case "MINER":
                        ownedCharacter.CharacterCurrentRole = Roles.MINER;
                        break;
                    case "FARMER":
                        ownedCharacter.CharacterCurrentRole = Roles.FARMER;
                        break;
                    case "FISHER":
                        ownedCharacter.CharacterCurrentRole = Roles.FISHER;
                        break;
                    case "WOODCUTTER":
                        ownedCharacter.CharacterCurrentRole = Roles.WOODCUTTER;
                        break;
                }
                ownedCharacter.CharacterCurrentState = States.INVENTORY;

                checkedCharacters++;
                if(checkedCharacters == ActualOwnedCharacters.Count)
                {
                    FilteredCharacters.Clear();
                   foreach (CharacterInstanceData ownedCharacter in ActualOwnedCharacters)
                   {
                        if (ownedCharacter.CharacterCurrentRole == NeededRole)
                        {
                            FilteredCharacters.Add(ownedCharacter);
                            if (ownedCharacter.OnAutoPilot)
                            {
                                foreach(CharacterSlotController slot in AutomatedCharacterSlots)
                                    if(slot.ThisCharacterInstance == null)
                                    {
                                        slot.ThisCharacterInstance = ownedCharacter;
                                        slot.InitializeCharacterSlot();
                                        AutomatedCharacters.Add(ownedCharacter);
                                        break;
                                    }
                            }
                            else
                                UndeployedCharacters.Add(ownedCharacter);
                        }
                   }

                    if (AutomatedCharacters.Count > 0)
                    {
                        if (NeededRole == Roles.MINER && PlayerData.AutoMiningTimeLeft > 0)
                            StartAutomationBtn.interactable = true;
                        else
                            StartAutomationBtn.interactable = false;

                    }
                    else
                        StartAutomationBtn.interactable = false;
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetCharacterDataPlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
        }
    }


    #endregion

    #region PANELS
    public void ShowCorePanels()
    {
        GameManager.Instance.AnimationsLT.FadePanel(VirtualCurrencyRT, null, VirtualCurrencyCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(BottomButtonsRT, null, BottomButtonsCG, 0, 1, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(DeployedCharactersRT, null, DeployedCharactersCG, 0, 1, () => { });
    }

    private void HideCorePanels()
    {
        GameManager.Instance.AnimationsLT.FadePanel(VirtualCurrencyRT, VirtualCurrencyRT, VirtualCurrencyCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(BottomButtonsRT, BottomButtonsRT, BottomButtonsCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(DeployedCharactersRT, DeployedCharactersRT, DeployedCharactersCG, 1, 0, () => { });
    }

    public void ShowUndeployedCharactersPanel()
    {
        if (DisplayingUndeployedCharacters)
            return;
        HideCorePanels();
        GameManager.Instance.AnimationsLT.ShowSlide(UndeployedCharactersRT, new Vector2(1175, 0), new Vector2(735, 0), () => 
        { 
            DisplayingUndeployedCharacters = true;
            CurrentCharacterIndex = 0;
            PreviousCharacterBtn.interactable = false;
            if (UndeployedCharacters.Count > 1)
                NextCharacterBtn.interactable = true;
            else
                NextCharacterBtn.interactable = false;
            ProcessCurrentCharacterButton();
        });
    }

    public void HideUndeployedCharactersPanel()
    {
        if (!DisplayingUndeployedCharacters)
            return;
        GameManager.Instance.AnimationsLT.ShowSlide(UndeployedCharactersRT, new Vector2(735, 0), new Vector2(1175, 0), () => 
        { 
            DisplayingUndeployedCharacters = false;
            SelectedCharacterSlot = null;
            CurrentGameplayState = GameplayStates.CORE;
        });
    }

    public void ShowInventoryPanel()
    {
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(InventoryRT, null, InventoryCG, 0, 1, () => { });
    }

    public void HideInventoryPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(InventoryRT, InventoryRT, InventoryCG, 1, 0, () => { CurrentGameplayState = GameplayStates.CORE; });

    }

    public void ShowAutopilotPanel()
    {
        HideCorePanels();
        GameManager.Instance.AnimationsLT.FadePanel(AutopilotRT, null, AutopilotCG, 0, 1, () => { });
    }

    public void HideAutopilotPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(AutopilotRT, AutopilotRT, AutopilotCG, 1, 0, () => { CurrentGameplayState = GameplayStates.CORE; });

    }
    #endregion

    #region CHARACTER SELECTION
    public void PreviousCharacterButton()
    {
        CurrentCharacterIndex--;
        if (CurrentCharacterIndex == 0)
            PreviousCharacterBtn.interactable = false;
        NextCharacterBtn.interactable = true;

        ProcessCurrentCharacterButton();
    }

    public void NextCharacterButton()
    {
        CurrentCharacterIndex++;
        PreviousCharacterBtn.interactable = true;
        if (CurrentCharacterIndex == UndeployedCharacters.Count - 1)
            NextCharacterBtn.interactable = false;

        ProcessCurrentCharacterButton();
    }
    public void ProcessCurrentCharacterButton()
    {
        CurrentCharacterImage.sprite = UndeployedCharacters[CurrentCharacterIndex].BaseCharacterData.undeployedSprite;
        if (UndeployedCharacters[CurrentCharacterIndex].CharacterCurrentStamina > 0)
            CurrentCharacterBtn.interactable = true;
        else
            CurrentCharacterBtn.interactable = false;
    }

    public void DeployCurrentCharacter()
    {
        SelectedCharacterSlot.ThisCharacterInstance = UndeployedCharacters[CurrentCharacterIndex];
        SelectedCharacterSlot.InitializeCharacterSlot();

        UndeployedCharacters.RemoveAt(CurrentCharacterIndex);
        HideUndeployedCharactersPanel();
    }

    public void UndeployAllCharacters()
    {
        foreach(CharacterSlotController slot in AllCharacterSlots)
        {
            if (slot.ThisCharacterInstance != null)
                slot.UndeployThisCharacter();
        }
    }
    #endregion

    #region SELL
    public void SellAvailableOres()
    {
        if(GameManager.Instance.DebugMode)
        {
            PlayerData.IronCount = 0;
            PlayerData.TinCount = PlayerData.TinCount % 4;
            PlayerData.CopperCount = PlayerData.CopperCount % 4;
            PlayerData.EZCoin += TotalEZCoinValue;
            PlayerData.CoinsGained += TotalEZCoinValue;
            PlayerData.LifetimeEZCoin += TotalEZCoinValue;
            if (NeededRole == Roles.MINER)
                PlayerData.MiningEZCoin += TotalEZCoinValue;
            else if (NeededRole == Roles.FARMER)
                PlayerData.FarmingEZCoin += TotalEZCoinValue;
            else if (NeededRole == Roles.FISHER)
                PlayerData.FishingEZCoin += TotalEZCoinValue;
            else if (NeededRole == Roles.WOODCUTTER)
                PlayerData.WoodcuttingEZCoin += TotalEZCoinValue;
            EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
            ProcessInventoryPanel();
            CalculateEZCoinValue();
        }
        else
        {
            DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if(resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        DisplayLoadingPanel();
                        if(NeededRole == Roles.MINER)
                        {
                            if (PlayerData.IronCount > 0)
                                ConsumeIronOre(true);
                            else if (PlayerData.TinCount >= 4)
                                ConsumeTinOre(true);
                            else if (PlayerData.CopperCount >= 4)
                                ConsumeCopperOre(true);
                            else
                                UpdateStatistics(true);
                        }
                    }
                    else
                    {
                        HideLoadingPanel();
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        SellAvailableOres,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    public void SellAutoOres()
    {
        AutomationActivated = false;
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.IronCount -= AutoIron;
            PlayerData.TinCount -= AutoTin - (AutoTin % 4);
            PlayerData.CopperCount -= AutoCopper - (AutoCopper % 4);
            PlayerData.EZCoin += TotalAutoEZCoinValue;
            PlayerData.CoinsGained += TotalAutoEZCoinValue;
            PlayerData.LifetimeEZCoin += TotalAutoEZCoinValue;

            AutoIron = 0;
            AutoIronTMP.text = AutoIron.ToString("n0");
            AutoTin = (AutoTin % 4);
            AutoTinTMP.text = AutoTin.ToString("n0");
            AutoCopper = (AutoCopper % 4);
            AutoCopperTMP.text = AutoCopper.ToString("n0");

            if (NeededRole == Roles.MINER)
                PlayerData.MiningEZCoin += TotalAutoEZCoinValue;
            else if (NeededRole == Roles.FARMER)
                PlayerData.FarmingEZCoin += TotalAutoEZCoinValue;
            else if (NeededRole == Roles.FISHER)
                PlayerData.FishingEZCoin += TotalAutoEZCoinValue;
            else if (NeededRole == Roles.WOODCUTTER)
                PlayerData.WoodcuttingEZCoin += TotalAutoEZCoinValue;
            EZCoinTMP.text = PlayerData.EZCoin.ToString("n0");
            ProcessInventoryPanel();
            CalculateAutoEZCoinValue();
        }
        else
        {
            DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        DisplayLoadingPanel();
                        if (NeededRole == Roles.MINER)
                        {
                            if (PlayerData.IronCount > 0)
                                ConsumeIronOre(false);
                            else if (PlayerData.TinCount >= 4)
                                ConsumeTinOre(false);
                            else if (PlayerData.CopperCount >= 4)
                                ConsumeCopperOre(false);
                            else
                                UpdateStatistics(false);
                        }
                    }
                    else
                    {
                        HideLoadingPanel();
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                    }
                },
                errorCallback =>
                {
                    Debug.Log("a");
                    ErrorCallback(errorCallback.Error,
                        SellAvailableOres,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    #region CONSUMPTION
    private void ConsumeIronOre(bool sellingManual)
    {
        consumeItem.ItemInstanceId = PlayerData.IronInstanceID;
        if (sellingManual)
            consumeItem.ConsumeCount = PlayerData.IronCount;
        else
            consumeItem.ConsumeCount = AutoIron;
        PlayFabClientAPI.ConsumeItem(consumeItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if(!sellingManual)
                {
                    AutoIron = 0;
                    AutoIronTMP.text = AutoIron.ToString("n0");
                }

                if (PlayerData.TinCount >= 4)
                    ConsumeTinOre(sellingManual);
                else if (PlayerData.CopperCount >= 4)
                    ConsumeCopperOre(sellingManual);
                else
                    UpdateStatistics(sellingManual);
            },
            errorCallback =>
            {
                Debug.Log("a");
                ErrorCallback(errorCallback.Error,
                        () => ConsumeIronOre(sellingManual),
                        () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void ConsumeTinOre(bool sellingManual)
    {
        consumeItem.ItemInstanceId = PlayerData.TinInstanceID;
        if (sellingManual)
            consumeItem.ConsumeCount = PlayerData.TinCount - (PlayerData.TinCount % 4);
        else
            consumeItem.ConsumeCount = AutoTin - (AutoTin % 4);
        PlayFabClientAPI.ConsumeItem(consumeItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if(!sellingManual)
                {
                    AutoTin = (AutoTin % 4);
                    AutoTinTMP.text = AutoTin.ToString("n0");
                }
                if (PlayerData.CopperCount >= 4)
                    ConsumeCopperOre(sellingManual);
                else
                    UpdateStatistics(sellingManual);
            },
            errorCallback =>
            {
                Debug.Log("a");
                ErrorCallback(errorCallback.Error,
                        () => ConsumeTinOre(sellingManual),
                        () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void ConsumeCopperOre(bool sellingManual)
    {
        consumeItem.ItemInstanceId = PlayerData.CopperInstanceID;
        if (sellingManual)
            consumeItem.ConsumeCount = PlayerData.CopperCount - (PlayerData.CopperCount % 4);
        else
            consumeItem.ConsumeCount = AutoCopper - (AutoCopper % 4);
        PlayFabClientAPI.ConsumeItem(consumeItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if(!sellingManual)
                {
                    AutoCopper = (AutoCopper % 4);
                    AutoCopperTMP.text = AutoCopper.ToString("n0");
                }
                UpdateStatistics(sellingManual);
            },
            errorCallback =>
            {
                Debug.Log("a");
                ErrorCallback(errorCallback.Error,
                        () => ConsumeCopperOre(sellingManual),
                        () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    #endregion


    private void UpdateStatistics(bool sellingManual)
    {
        statisticUpdate.StatisticName = "TotalEZCoinOverall";
        if(sellingManual)
            statisticUpdate.Value = PlayerData.LifetimeEZCoin + TotalEZCoinValue;
        else
            statisticUpdate.Value = PlayerData.LifetimeEZCoin + TotalAutoEZCoinValue;


        if (NeededRole == Roles.MINER)
        {
            statisticUpdate1.StatisticName = "TotalEZCoinMining";
            if(sellingManual)
                statisticUpdate1.Value = PlayerData.MiningEZCoin + TotalEZCoinValue;
            else
                statisticUpdate1.Value = PlayerData.MiningEZCoin + TotalAutoEZCoinValue;
        }
        else if (NeededRole == Roles.FARMER)
        {
            statisticUpdate1.StatisticName = "TotalEZCoinFarming";
            if(sellingManual)
                statisticUpdate1.Value = PlayerData.FarmingEZCoin + TotalEZCoinValue;
            else
                statisticUpdate1.Value = PlayerData.FarmingEZCoin + TotalAutoEZCoinValue;
        }
        else if (NeededRole == Roles.FISHER)
        {
            statisticUpdate1.StatisticName = "TotalEZCoinFishing";
            if(sellingManual)
                statisticUpdate1.Value = PlayerData.FishingEZCoin + TotalEZCoinValue;
            else
                statisticUpdate1.Value = PlayerData.FishingEZCoin + TotalAutoEZCoinValue;
        }
        else if (NeededRole == Roles.WOODCUTTER)
        {
            statisticUpdate1.StatisticName = "TotalEZCoinWoodcutting";
            if(sellingManual)
                statisticUpdate1.Value = PlayerData.WoodcuttingEZCoin + TotalEZCoinValue;
            else
                statisticUpdate1.Value = PlayerData.WoodcuttingEZCoin + TotalAutoEZCoinValue;
        }

        updatePlayerStatistics.Statistics.Clear();
        updatePlayerStatistics.Statistics.Add(statisticUpdate);
        updatePlayerStatistics.Statistics.Add(statisticUpdate1);

        PlayFabClientAPI.UpdatePlayerStatistics(updatePlayerStatistics,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (sellingManual)
                    PlayerData.CoinsGained += TotalEZCoinValue;
                else
                    PlayerData.CoinsGained += TotalAutoEZCoinValue;
                UpdateQuestData(sellingManual);
            },
            errorCallback =>
            {
                Debug.Log("a");
                ErrorCallback(errorCallback.Error,
                    () => UpdateStatistics(sellingManual),
                    () => ProcessError(errorCallback.ErrorMessage)); ;
            });
    }

    private void UpdateQuestData(bool sellingManual)
    {
        updateUserData.Data.Clear();
        updateUserData.Data.Add("Quests", PlayerData.SerializeCurrentQuestData());
        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                AddEZCoin(sellingManual);
            },
            errorCallback =>
            {
                Debug.Log("a");
                ErrorCallback(errorCallback.Error,
                    () => UpdateQuestData(sellingManual),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void AddEZCoin(bool sellingManual)
    {
        int coinsToAdd = 0;
        if (sellingManual)
            coinsToAdd = TotalEZCoinValue;
        else
            coinsToAdd = TotalAutoEZCoinValue;
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddEZCoin",
            FunctionParameter = new { coin = coinsToAdd },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            GetUserInventoryPlayFab();
        },
        errorCallback =>
        {
            Debug.Log("a");
            ErrorCallback(errorCallback.Error,
                    () => AddEZCoin(sellingManual),
                    () => ProcessError(errorCallback.ErrorMessage));
        });
    }

    public void ProcessInventoryPanel()
    {
        IronOreTMP.text = PlayerData.IronCount.ToString("n0");
        TinOreTMP.text = PlayerData.TinCount.ToString("n0");
        CopperOreTMP.text = PlayerData.CopperCount.ToString("n0");
        SilverOreTMP.text = PlayerData.SilverCount.ToString("n0");
        GoldOreTMP.text = PlayerData.GoldCount.ToString("n0");
        PlatinumOreTMP.text = PlayerData.PlatinumCount.ToString("n0");
        DiamondOreTMP.text = PlayerData.DiamondCount.ToString("n0");
    }

    public void CalculateEZCoinValue()
    {
        TotalEZCoinValue = Mathf.FloorToInt(PlayerData.TinCount / 4) + Mathf.FloorToInt(PlayerData.CopperCount / 4) + PlayerData.IronCount;
        TotalEZCoinValueTMP.text = TotalEZCoinValue.ToString("n0");

        if (TotalEZCoinValue > 0)
            SellBtn.interactable = true;
        else
            SellBtn.interactable = false;
    }

    public void CalculateAutoEZCoinValue()
    {
        TotalAutoEZCoinValue = Mathf.FloorToInt(AutoTin / 4) + Mathf.FloorToInt(AutoCopper / 4) + AutoIron;
        if (TotalAutoEZCoinValue > 0)
            SellAutoBtn.interactable = true;
        else
            SellAutoBtn.interactable = false;
    }
    #endregion

    #region AUTOMATION
    public void StartAutomation()
    {
        if(!AutomationActivated)
        {
            AutomationActivated = true;
            foreach (CharacterAutomationController characterAutomation in AllAutomationControllers)
                characterAutomation.InitializeAutomatedCharacter();
        }
        
    }

    public void UpdateAutoTimeLeft()
    {
        updateUserData.Data.Clear();
        updateUserData.Data.Add("AutoPilot", PlayerData.SerializeCurrentAutoTimerData());
        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                        UpdateAutoTimeLeft,
                        () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    #endregion

    #region UTILITY
    public void OpenIslandScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "IslandScene";
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
