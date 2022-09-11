using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInstanceData", menuName = "EZMoneyPH/Data/CharacterInstanceData")]
public class CharacterInstanceData : ScriptableObject
{
    #region ENUMS
    public enum Rarity
    {
        NONE,
        COMMON,
        RARE,
        LEGENDARY,
        GOD
    }
    public enum Roles
    {
        NONE,
        MINER,
        FARMER,
        FISHER,
        WOODCUTTER,
        BLACKSMITH,
        CHEF,
        CRAFTER,
        HUNTER,
        GARDENDER
    }

    public enum States
    {
        NONE,
        INVENTORY,
        DEPLOYED,
        IDLE,
        WORKING
    }
    #endregion 

    [field: SerializeField] public string CharacterInstanceID { get; set; }
    [field: SerializeField] public CharacterData BaseCharacterData { get; set; }
    [field: SerializeField] public Roles CharacterCurrentRole { get; set; }
    [field: SerializeField] public States CharacterCurrentState { get; set; }
}
