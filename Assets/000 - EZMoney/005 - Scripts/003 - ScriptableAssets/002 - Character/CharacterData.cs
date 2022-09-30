using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "EZMoneyPH/Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    #region ENUMS
    public enum Animals
    {
        NONE, 
        BEAR,
        CAT,
        DOG,
        DINO,
        RABBIT,
        PANDA,
        SHEEP,
        DUCK,
        PENGUIN,
        CHICKEN
    }
    
    #endregion

    [Header("CORE ANIMAL DATA")]
    public Animals animalType;
    public string animalID;

    [Header("IMAGES")]
    public Sprite characterPanelSprite;
    //public GameObject animalSprite;
    public Sprite displaySprite;
    public Sprite undeployedSprite;
    public Sprite deployedSprite;

    [Header("ANIMATED PREFABS")]
    public GameObject AnimatedCharacterPrefab;

    [Header("BASE STATS")]
    public int strength;
    public int speed;
    public int stamina;

    [Header("PRICE")]
    public int price;

    /*[Header("BUFFS")]
    public int miningBuff;
    public int farmingBuff;
    public int fishingBuff;
    public int woodCuttingBuff;
    public int smithingBuff;
    public int cookingBuff;
    public int craftingBuff;
    public int huntingBuff;
    public int gardeningBuff;*/


    /*Notes on characters:
     * Roles and rarity are dynamic. An animal can have any role and any rarity. Rarity is assigned on creation but roles are chosen by the user. Once a role is chosen it can no longer be changed
     * Different animals have different buffs and debuffs. The value of which is based on a range depending on the rarity. 
     * Each animal has a random stamina that decides how long it can work in a day. The stamina value varies on the animal and the rarity
     * Each animal is to be treated as an NFT as EZMoneyPH will soon be on the blockchain and not be a fiat game anymore
     */
}
