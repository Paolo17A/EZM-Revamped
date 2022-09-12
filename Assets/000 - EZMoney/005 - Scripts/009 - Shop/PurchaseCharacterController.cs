using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchaseCharacterController : MonoBehaviour
{
    #region VARIABLES
    //=========================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private ShopCore ShopCore;
    [SerializeField] private CharacterData ThisCharacterData;
    [field: SerializeField] public Button PurchaseBtn { get; set; }

    [Header("CHARACTER STATS")]
    [SerializeField] private TextMeshProUGUI StrengthTMP;
    [SerializeField] private TextMeshProUGUI SpeedTMP;
    [SerializeField] private TextMeshProUGUI StaminaTMP;
    //=========================================================================================================
    #endregion

    private void Start()
    {
        StrengthTMP.text = ThisCharacterData.strength.ToString();
        SpeedTMP.text = ThisCharacterData.speed.ToString();
        StaminaTMP.text = ThisCharacterData.stamina.ToString();
    }

    public void PurchaseThisCharacter()
    {
        if(GameManager.Instance.DebugMode)
        {
            if (PlayerData.EZCoin >= ThisCharacterData.price)
            {
                for(int i = 0; i < PlayerData.OwnedCharacters.Count; i++)
                {
                    if (PlayerData.OwnedCharacters[i].BaseCharacterData == null)
                    {
                        PlayerData.OwnedCharacters[i].CharacterInstanceID = "newlyPurchasedCharacter " + i;
                        PlayerData.OwnedCharacters[i].BaseCharacterData = ThisCharacterData;
                        PlayerData.OwnedCharacters[i].CharacterCurrentRole = CharacterInstanceData.Roles.MINER;
                        PlayerData.OwnedCharacters[i].CharacterCurrentState = CharacterInstanceData.States.INVENTORY;
                        PlayerData.OwnedCharacters[i].CharacterCurrentStamina = ThisCharacterData.stamina;
                        break;
                    }
                }
                PlayerData.EZCoin -= ThisCharacterData.price;
                ShopCore.UpdateEZCoinDisplay();
                ShopCore.CheckCharacterPurchasability();
                ShopCore.CheckAutopilotPurchasability();
            }
            else
                GameManager.Instance.DisplayErrorPanel("You do not have enough EZCoins to purchase this character");
        }
        else
        {

        }
    }
}
