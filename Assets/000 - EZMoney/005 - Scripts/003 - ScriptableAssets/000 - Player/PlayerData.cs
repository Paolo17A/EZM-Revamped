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
    [field: SerializeField] public int LifetimeEZCoin;
    [field: SerializeField] public int MiningEZCoin;
    [field: SerializeField] public int FishingEZCoin;
    [field: SerializeField] public int FarmingEZCoin;
    [field: SerializeField] public int WoodcuttingEZCoin;

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
