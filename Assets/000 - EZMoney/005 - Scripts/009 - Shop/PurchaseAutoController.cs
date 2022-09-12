using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseAutoController : MonoBehaviour
{
    #region VARIABLES
    //=========================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private ShopCore ShopCore;
    [SerializeField] private string ItemID;


    //=========================================================================================================
    #endregion


    public void PurchaseAutopilotItem()
    {
        if(GameManager.Instance.DebugMode)
        {
            if(PlayerData.EZCoin >= 5000)
            {
                switch (ItemID)
                {
                    case "MiningPilot":
                        PlayerData.OwnsAutoMining = true;
                        ShopCore.AutoMiningBtn.interactable = false;
                        break;
                    case "FarmingPilot":
                        PlayerData.OwnsAutoFarming = true;
                        ShopCore.AutoFarmingBtn.interactable = false;
                        break;
                    case "FishingPilot":
                        PlayerData.OwnsAutoFishing = true;
                        ShopCore.AutoFishingBtn.interactable = false;
                        break;
                    case "WoodcuttingPilot":
                        PlayerData.OwnsAutoWoodCutting = true;
                        ShopCore.AutoWoodcuttingBtn.interactable = false;
                        break;
                }
                PlayerData.EZCoin -= 5000;
                ShopCore.UpdateEZCoinDisplay();
                ShopCore.CheckCharacterPurchasability();
                ShopCore.CheckAutopilotPurchasability();
            }
            else
                GameManager.Instance.DisplayErrorPanel("You do not own enough EZCoins to purchase this item");
        }
        else
        {

        }
    }
}
