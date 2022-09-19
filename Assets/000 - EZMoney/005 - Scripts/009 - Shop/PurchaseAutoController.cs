using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class PurchaseAutoController : MonoBehaviour
{
    #region VARIABLES
    //=========================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private ShopCore ShopCore;
    [SerializeField] private string ItemID;

    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] private GetUserDataRequest getUserData;
    [ReadOnly] private StartPurchaseRequest startPurchase;
    [ReadOnly] private PayForPurchaseRequest payForPurchase;
    [ReadOnly] private ConfirmPurchaseRequest confirmPurchase;
    [ReadOnly] private GetUserInventoryRequest getUserInventory;
    [ReadOnly] private int failedCallbackCounter;
    //=========================================================================================================
    #endregion
    private void Awake()
    {
        getUserData = new GetUserDataRequest();
        startPurchase = new StartPurchaseRequest();
        startPurchase.Items = new List<ItemPurchaseRequest>();
        payForPurchase = new PayForPurchaseRequest();
        confirmPurchase = new ConfirmPurchaseRequest();
        getUserInventory = new GetUserInventoryRequest();
    }

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
            ShopCore.DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(getUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                        StartPurchasePlayFab();
                    else
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        PurchaseAutopilotItem,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void StartPurchasePlayFab()
    {
        startPurchase.CatalogVersion = "AutoPilot";
        startPurchase.Items.Clear();
        startPurchase.Items.Add(new ItemPurchaseRequest() { ItemId = ItemID, Quantity = 1 });
        PlayFabClientAPI.StartPurchase(startPurchase,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PayForPurchase(resultCallback.OrderId, resultCallback.PaymentOptions[0].ProviderName);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    StartPurchasePlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void PayForPurchase(string _orderID, string _providerName)
    {
        payForPurchase.Currency = "EC";
        payForPurchase.OrderId = _orderID;
        payForPurchase.ProviderName = _providerName;

        PlayFabClientAPI.PayForPurchase(payForPurchase,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                ConfirmPurchase(_orderID);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => PayForPurchase(_orderID, _providerName),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void ConfirmPurchase(string _orderID)
    {
        confirmPurchase.OrderId = _orderID;

        PlayFabClientAPI.ConfirmPurchase(confirmPurchase,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                ShopCore.GetUserInventoryPlayfab();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => ConfirmPurchase(_orderID),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }


    #region UTILITY
    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                ProcessError("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
        ShopCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        ShopCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
