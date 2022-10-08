using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using MyBox;
using PlayFab.ClientModels;
using Cinemachine;

public class GameplayController : MonoBehaviour
{
    //=========================================================================================
    [SerializeField] private GameplayCore GameplayCore;
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private CinemachineMovementController VirtualCamera;
    private float secondsTimer;

    [Header("TIME")]
    [SerializeField][ReadOnly] private int ElapsedMinute;
    [SerializeField][ReadOnly] private int ElapsedSeconds;
    private TimeSpan oneSecond = new TimeSpan(0, 0, 1);


    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private CharacterPrefabCore SelectedCharacterPrefab;
    [SerializeField][ReadOnly] private ExtendedZoneController SelectedZoneExtension;
    [SerializeField][ReadOnly] private RaycastHit hit;
    [SerializeField][ReadOnly] private Vector3 clickedPos;
    [SerializeField][ReadOnly] private bool newlyReleased;
    Ray myRay;
    //=========================================================================================
    private void OnEnable()
    {
        GameplayCore.onGameplayStateChange += GameplayStateChange;
        if (GameManager.Instance.DebugMode)
            GameManager.Instance.SceneController.AddActionLoadinList(GameplayCore.InitializeGameplay());
        else
            GameManager.Instance.SceneController.AddActionLoadinList(GameplayCore.GetUserData());
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        GameplayCore.onGameplayStateChange -= GameplayStateChange;
    }

    private void Awake()
    {
        GameplayCore.getUserData = new GetUserDataRequest();
        GameplayCore.getUserInventory = new GetUserInventoryRequest();
        GameplayCore.getPlayerStatistics = new GetPlayerStatisticsRequest();
        GameplayCore.listUsersCharacters = new ListUsersCharactersRequest();
        GameplayCore.getCharacterData = new GetCharacterDataRequest();
        GameplayCore.updateCharacterData = new UpdateCharacterDataRequest();
        GameplayCore.updateCharacterData.Data = new Dictionary<string, string>();
        GameplayCore.consumeItem = new ConsumeItemRequest();
        GameplayCore.updatePlayerStatistics = new UpdatePlayerStatisticsRequest();
        GameplayCore.updatePlayerStatistics.Statistics = new List<StatisticUpdate>();
        GameplayCore.statisticUpdate = new StatisticUpdate();
        GameplayCore.statisticUpdate1 = new StatisticUpdate();
        GameplayCore.updateUserData = new UpdateUserDataRequest();
        GameplayCore.updateUserData.Data = new Dictionary<string, string>();
        GameplayCore.startPurchase = new StartPurchaseRequest();
        GameplayCore.startPurchase.Items = new List<ItemPurchaseRequest>();
        GameplayCore.payForPurchase = new PayForPurchaseRequest();
        GameplayCore.confirmPurchase = new ConfirmPurchaseRequest();

        GameplayCore.ActualOwnedCharacters = new List<CharacterInstanceData>();
        GameplayCore.FilteredCharacters = new List<CharacterInstanceData>();
        GameplayCore.UndeployedCharacters = new List<CharacterInstanceData>();
        GameplayCore.DeployedCharacters = new List<CharacterInstanceData>();
        GameplayCore.AutomatedCharacters = new List<CharacterInstanceData>();
    }

    private void Start()
    {
        secondsTimer = (float)PlayerData.ElapsedGameplayTime.TotalSeconds;
        GameplayCore.CurrentGameplayState = GameplayCore.GameplayStates.CORE;
    }

    private void Update()
    {
        #region TIME
        if (GameplayCore.AutomationActivated)
        {
            if (GameplayCore.NeededRole == CharacterInstanceData.Roles.MINER)
            {
                PlayerData.AutoMiningTimeLeft -= Time.deltaTime;
                GameplayCore.AutomationSlider.value = PlayerData.AutoMiningTimeLeft / 1800;
                if (PlayerData.AutoMiningTimeLeft <= 0)
                {
                    GameplayCore.StartAutomationBtn.interactable = false;
                    GameplayCore.AutomationActivated = false;
                    foreach (CharacterSlotController slot in GameplayCore.AutomatedCharacterSlots)
                        if(slot.ThisCharacterInstance != null)
                            slot.UndeployThisCharacter();
                    if (!GameManager.Instance.DebugMode)
                        GameplayCore.UpdateAutoTimeLeft();
                }
            }
        }

        secondsTimer += Time.deltaTime;
        PlayerData.ElapsedGameplayTime = TimeSpan.FromSeconds(secondsTimer);
        PlayerData.MinsPlayed = (int)PlayerData.ElapsedGameplayTime.TotalMinutes;
        ElapsedMinute = (int)PlayerData.ElapsedGameplayTime.TotalMinutes % 60;
        ElapsedSeconds = (int)PlayerData.ElapsedGameplayTime.TotalSeconds % 60;
        PlayerPrefs.SetInt("ElapsedMinutes", ElapsedMinute);
        PlayerPrefs.SetInt("ElapsedSeconds", ElapsedSeconds);
        #endregion

        if (GameManager.Instance.InputManager.isPrimaryTouch && !EventSystem.current.IsPointerOverGameObject())
        {
            newlyReleased = false;
            if (Physics.Raycast(GameManager.Instance.MainCamera.ScreenPointToRay(GameManager.Instance.InputManager.GetMousePosition()), out hit))
            {
                if(hit.collider)
                {
                    // User clicked on character
                    if (hit.transform.gameObject.layer == 6)
                    {
                        SelectedCharacterPrefab = hit.transform.GetComponent<CharacterPrefabCore>();
                        if(SelectedCharacterPrefab.CurrentCharacterState == CharacterPrefabCore.CharacterStates.IDLE && !SelectedCharacterPrefab.apiCallOngoing)
                            SelectedCharacterPrefab.isSelected = true;
                    }
                    //User clicked on the ground
                    else
                    {
                        //  Character will walk to new destination
                        clickedPos = GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.InputManager.GetMousePosition());
                        if (SelectedCharacterPrefab != null && SelectedCharacterPrefab.CurrentCharacterState == CharacterPrefabCore.CharacterStates.IDLE)
                        {
                            SelectedCharacterPrefab.CharacterNavMesh.enabled = true;
                            SelectedCharacterPrefab.destinationVector = new Vector3(hit.point.x, SelectedCharacterPrefab.transform.position.y, hit.point.z);

                            SelectedCharacterPrefab.CurrentCharacterState = CharacterPrefabCore.CharacterStates.WALKING;
                            SelectedCharacterPrefab.isSelected = false;
                            if (hit.transform.gameObject.layer == 7 && !hit.transform.GetComponent<OreController>().OnCooldown && hit.transform.GetComponent<OreController>().OccupyingCharacter == null)
                            {
                                SelectedCharacterPrefab.willMineOre = true;
                                SelectedCharacterPrefab.AssignedOre = hit.transform.GetComponent<OreController>();
                                hit.transform.GetComponent<OreController>().OccupyingCharacter = SelectedCharacterPrefab;
                            }
                            SelectedCharacterPrefab = null;
                        }
                    }
                }
            }
        }
        // User released click on extension object
        else if (!GameManager.Instance.InputManager.isPrimaryTouch && !newlyReleased)
        {
            newlyReleased = true;
            if (Physics.Raycast(GameManager.Instance.MainCamera.ScreenPointToRay(GameManager.Instance.InputManager.GetMousePosition()), out hit))
            {
                if(hit.collider && hit.transform.gameObject.layer == 8)
                {
                    if(hit.transform.tag == "Return")
                    {
                        VirtualCamera.destinationVector = VirtualCamera.defaultCenterPos;
                        VirtualCamera.minXClamp = VirtualCamera.defaultMinXClamp;
                        VirtualCamera.maxXClamp = VirtualCamera.defaultMaxXClamp;
                        VirtualCamera.minZClamp = VirtualCamera.defaultMinZClamp;
                        VirtualCamera.maxZClamp = VirtualCamera.defaultMaxZClamp;
                        GameplayCore.CurrentZone = GameplayCore.Zones.DEFAULT;
                        VirtualCamera.travelling = true;

                    }
                    else if (hit.transform.tag == "Extension")
                    {
                        SelectedZoneExtension = hit.transform.GetComponent<ExtendedZoneController>();
                        SelectedZoneExtension.ProcessExtensionClick();
                    }
                    
                }
            }
        }
    }

    private void GameplayStateChange(object sender, EventArgs e)
    {
        if (GameplayCore.CurrentGameplayState == GameplayCore.GameplayStates.CORE)
            GameplayCore.ShowCorePanels();
        else if (GameplayCore.CurrentGameplayState == GameplayCore.GameplayStates.CHARACTER)
            GameplayCore.ShowUndeployedCharactersPanel();
        else if (GameplayCore.CurrentGameplayState == GameplayCore.GameplayStates.INVENTORY)
            GameplayCore.ShowInventoryPanel();
        else if (GameplayCore.CurrentGameplayState == GameplayCore.GameplayStates.AUTOPILOT)
            GameplayCore.ShowAutopilotPanel();
    }

    public void GameplayStateToIndex(int state)
    {
        switch(state)
        {
            case (int)GameplayCore.GameplayStates.CORE:
                GameplayCore.CurrentGameplayState = GameplayCore.GameplayStates.CORE;
                break;
            case (int)GameplayCore.GameplayStates.CHARACTER:
                GameplayCore.CurrentGameplayState = GameplayCore.GameplayStates.CHARACTER;
                break;
            case (int)GameplayCore.GameplayStates.INVENTORY:
                GameplayCore.CurrentGameplayState = GameplayCore.GameplayStates.INVENTORY;
                break;
            case (int)GameplayCore.GameplayStates.AUTOPILOT:
                GameplayCore.CurrentGameplayState = GameplayCore.GameplayStates.AUTOPILOT;
                break;
        }
    }
}
