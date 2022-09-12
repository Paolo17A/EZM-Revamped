using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

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
    [field: SerializeField] public bool OwnsAutoFarming { get; set; }
    [field: SerializeField] public bool OwnsAutoFishing { get; set; }
    [field: SerializeField] public bool OwnsAutoWoodCutting { get; set; }

    [field: Header("ZONES INVENTORY")]
    [field: SerializeField] public bool CanAccessMineA { get; set; }
    [field: SerializeField] public bool CanAccessMineB { get; set; }
    [field: SerializeField] public bool CanAccessFarmA { get; set; }
    [field: SerializeField] public bool CanAccessFarmB { get; set; }
    [field: SerializeField] public bool CanAccessPondA { get; set; }
    [field: SerializeField] public bool CanAccessPondB { get; set; }
    [field: SerializeField] public bool CanAccessForestA { get; set; }
    [field: SerializeField] public bool CanAccessForestB { get; set; }



    [field: Header("QUEST")]
    [field: SerializeField] public int DailyLogin { get; set; }
    [field: SerializeField] public int SocMedShared { get; set; }
    [field: SerializeField] public int AdsWatched { get; set; }
    [field: SerializeField] public int MinsPlayed { get; set; }
    [field: SerializeField] public int CoinsGained { get; set; }
    [field: SerializeField] public int DailyClaimed { get; set; }

    public void ResetPlayerData()
    {
        if(!GameManager.Instance.DebugMode)
        {

        }
    }
}
