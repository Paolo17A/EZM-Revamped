using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "PlayerData", menuName = "EZMoneyPH/Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    [field: Header("USER TITLE DATA")]
    [field: SerializeField][field: ReadOnly] public string PlayfabID { get; set; }
    [field: SerializeField][field: ReadOnly] public string DisplayName { get; set; }
    [field: SerializeField][field: ReadOnly] public string LUID { get; set; }
    [field: SerializeField] public string SubscriptionLevel { get; set; }

    public void ResetPlayerData()
    {
        
    }
}
