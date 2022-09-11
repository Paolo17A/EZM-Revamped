using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDisplayController : MonoBehaviour
{
    //====================================================================================
    [SerializeField] private ProfileCore ProfileCore;
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private CharacterData CharacterData;
    //====================================================================================
    public void ChangeDisplayPicture()
    {
        if (GameManager.Instance.DebugMode)
        {
            ProfileCore.DisplayImage.sprite = CharacterData.displaySprite;
            PlayerData.DisplayPicture = CharacterData.animalID;
        }
        else
        {

        }
    }
}
