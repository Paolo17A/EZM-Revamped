using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

[CreateAssetMenu(fileName = "PlayerData", menuName = "EZMoneyPH/Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    [field: Header("USER TITLE DATA")]
    [field: SerializeField] public string PlayfabID { get; set; }
    [field: SerializeField][field: ReadOnly] public string DisplayName { get; set; }
    [field: SerializeField][field: ReadOnly] public string LUID { get; set; }
    [field: SerializeField] public string SubscriptionLevel { get; set; }

    [field: Header("VIRTUAL CURRENCIES")]
    [field: SerializeField] public int EZCoin { get; set; }
    [field: SerializeField] public int EZGem { get; set; }

    [field: Header("CHARACTERS")]
    [field: SerializeField] public string DisplayPicture { get; set; }
    [field: SerializeField] public List<CharacterInstanceData> OwnedCharacters { get; set; }

    [field: Header("STATISTICS")]
    [field: SerializeField] public int LifetimeEZCoin { get; set; }
    [field: SerializeField] public int LifetimeEZGem { get; set; }
    [field: SerializeField] public int MiningEZCoin { get; set; }
    [field: SerializeField] public int FishingEZCoin { get; set; }
    [field: SerializeField] public int FarmingEZCoin { get; set; }
    [field: SerializeField] public int WoodcuttingEZCoin { get; set; }

    [field: Header("AUTOPILOT INVENTORY")]
    [field: SerializeField] public bool OwnsAutoMining { get; set; }
    [field: SerializeField] public float AutoMiningTimeLeft { get; set; }
    [field: SerializeField] public bool OwnsAutoFarming { get; set; }
    [field: SerializeField] public float AutoFarmingTimeLeft { get; set; }
    [field: SerializeField] public bool OwnsAutoFishing { get; set; }
    [field: SerializeField] public float AutoFishingTimeLeft { get; set; }
    [field: SerializeField] public bool OwnsAutoWoodCutting { get; set; }
    [field: SerializeField] public float AutoWoodcuttingTimeLeft { get; set; }


    [field: Header("ZONES INVENTORY")]
    [field: SerializeField] public bool CanAccessMineA { get; set; }
    [field: SerializeField] public bool CanAccessMineB { get; set; }
    [field: SerializeField] public bool CanAccessFarmA { get; set; }
    [field: SerializeField] public bool CanAccessFarmB { get; set; }
    [field: SerializeField] public bool CanAccessPondA { get; set; }
    [field: SerializeField] public bool CanAccessPondB { get; set; }
    [field: SerializeField] public bool CanAccessForestA { get; set; }
    [field: SerializeField] public bool CanAccessForestB { get; set; }

    [field: Header("EXTENSIONS")]
    [field: SerializeField] public bool CanAccessMineANorth { get; set; }
    [field: SerializeField] public bool CanAccessMineASouth { get; set; }
    [field: SerializeField] public bool CanAccessMineAWest { get; set; }
    [field: SerializeField] public bool CanAccessMineAEast { get; set; }




    [field: Header("ORES")]
    [field: SerializeField] public string IronInstanceID { get; set; }
    [field: SerializeField] public int IronCount { get; set; }
    [field: SerializeField] public string AutoIronInstanceID { get; set; }
    [field: SerializeField] public int AutoIronCount { get; set; }
    [field: SerializeField] public string TinInstanceID { get; set; }
    [field: SerializeField] public int TinCount { get; set; }
    [field: SerializeField] public string AutoTinInstanceID { get; set; }
    [field: SerializeField] public int AutoTinCount { get; set; }
    [field: SerializeField] public string CopperInstanceID { get; set; }
    [field: SerializeField] public int CopperCount { get; set; }
    [field: SerializeField] public string AutoCopperInstanceID { get; set; }
    [field: SerializeField] public int AutoCopperCount { get; set; }
    [field: SerializeField] public string SilverInstanceID { get; set; }
    [field: SerializeField] public int SilverCount { get; set; }
    [field: SerializeField] public string GoldInstanceID { get; set; }
    [field: SerializeField] public int GoldCount { get; set; }
    [field: SerializeField] public string PlatinumInstanceID { get; set; }
    [field: SerializeField] public int PlatinumCount { get; set; }
    [field: SerializeField] public string DiamondInstanceID { get; set; }
    [field: SerializeField] public int DiamondCount { get; set; }

    [field: Header("QUEST")]
    [field: SerializeField] public int DailyLogin { get; set; }
    [field: SerializeField] public int SocMedShared { get; set; }
    [field: SerializeField] public int AdsWatched { get; set; }
    [field: SerializeField] public int MinsPlayed { get; set; }
    [field: SerializeField] public int CoinsGained { get; set; }
    [field: SerializeField] public int DailyClaimed { get; set; }
    [field: SerializeField] public TimeSpan ElapsedGameplayTime { get; set; }

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("ElapsedMinutes") && PlayerPrefs.HasKey("ElapsedSeconds"))
        {
            ElapsedGameplayTime = new TimeSpan(0, PlayerPrefs.GetInt("ElapsedMinutes"), PlayerPrefs.GetInt("ElapsedSeconds"));
        }
        else
            ElapsedGameplayTime = new TimeSpan(0,0,0);
        ResetPlayerData();
    }

    private void OnDisable()
    {
        ResetPlayerData();
    }

    public void ResetPlayerData()
    {
        if(!GameManager.Instance.DebugMode)
        {
            PlayfabID = "";
            DisplayName = "";
            LUID = "";
            SubscriptionLevel = "";
            EZCoin = 0;
            EZGem = 0;
            DisplayPicture = "";
            foreach(CharacterInstanceData character in OwnedCharacters)
                character.ResetCharacterInstance();
            LifetimeEZCoin = 0;
            LifetimeEZGem = 0;
            MiningEZCoin = 0;
            FarmingEZCoin = 0;
            FishingEZCoin = 0;
            WoodcuttingEZCoin = 0;
            OwnsAutoFarming = false;
            OwnsAutoFishing = false;
            OwnsAutoMining = false;
            OwnsAutoWoodCutting = false;
            AutoMiningTimeLeft = 0;
            AutoFarmingTimeLeft = 0;
            AutoFishingTimeLeft = 0;
            AutoWoodcuttingTimeLeft = 0;
            CanAccessFarmA = false;
            CanAccessFarmB = false;
            CanAccessForestA = false;
            CanAccessForestB = false;
            CanAccessMineA = false;
            CanAccessMineB = false;
            CanAccessPondA = false;
            CanAccessPondB = false;
            CanAccessMineANorth = false;
            CanAccessMineASouth = false;
            CanAccessMineAWest = false;
            CanAccessMineAEast = false;
            IronCount = 0;
            IronInstanceID = "";
            AutoIronCount = 0;
            AutoIronInstanceID = "";
            CopperCount = 0;
            CopperInstanceID = "";
            AutoCopperCount = 0;
            AutoCopperInstanceID = "";
            TinCount = 0;
            TinInstanceID = "";
            AutoTinCount = 0;
            AutoTinInstanceID = "";
            SilverCount = 0;
            SilverInstanceID = "";
            GoldCount = 0;
            GoldInstanceID = "";
            PlatinumCount = 0;
            PlatinumInstanceID = "";
            DiamondCount = 0;
            DiamondInstanceID = "";

            DailyLogin = 0;
            SocMedShared = 0;
            AdsWatched = 0;
            MinsPlayed = 0;
            CoinsGained = 0;
            DailyClaimed = 0;
        }
    }

    public string SerializeCurrentQuestData()
    {
        return GameManager.Instance.SerializeIntValue(
                                new List<string>
                                {
                                    "DailyCheckIn",
                                    "SocMedShared",
                                    "AdsWatched",
                                    "MinsPlayed",
                                    "EZCoinsGained",
                                    "DailyQuestClaimed"
                                },
                                new List<int>
                                {
                                    DailyLogin,
                                    SocMedShared,
                                    AdsWatched,
                                    MinsPlayed,
                                    CoinsGained,
                                    DailyClaimed
                                });
    }

    public string SerializeCurrentAutoTimerData()
    {
        return GameManager.Instance.SerializeIntValue(
                                new List<string>
                                {
                                    "Mining",
                                    "Farming",
                                    "Fishing",
                                    "Woodcutting"
                                },
                                new List<int>
                                {
                                    Mathf.CeilToInt(AutoMiningTimeLeft),
                                    Mathf.CeilToInt(AutoFarmingTimeLeft),
                                    Mathf.CeilToInt(AutoFishingTimeLeft),
                                    Mathf.CeilToInt(AutoWoodcuttingTimeLeft),
                                });
    }
}
