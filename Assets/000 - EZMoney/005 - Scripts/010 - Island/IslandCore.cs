using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyBox;
using PlayFab.ClientModels;
using PlayFab;
using System;

public class IslandCore : MonoBehaviour
{
    //================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private LobbyCore LobbyCore;

    [Header("ISLAND ZONES")]
    [SerializeField] public ZoneController MineA;
    [SerializeField] public ZoneController MineB;
    [SerializeField] public ZoneController FarmA;
    [SerializeField] public ZoneController FarmB;
    [SerializeField] public ZoneController PondA;
    [SerializeField] public ZoneController PondB;
    [SerializeField] public ZoneController ForestA;
    [SerializeField] public ZoneController ForestB;

    [Header("PURCHASE VARIABLES")]
    [SerializeField] private RectTransform PurchaseRT;
    [SerializeField] private CanvasGroup PurchaseCG;
    [SerializeField] private TextMeshProUGUI PurchaseTMP;
    [SerializeField][ReadOnly] public ZoneController ClickedZone;

    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] public GetUserDataRequest getUserData;
    [ReadOnly] public StartPurchaseRequest startPurchase;
    [ReadOnly] public PayForPurchaseRequest payForPurchase;
    [ReadOnly] public ConfirmPurchaseRequest confirmPurchase;
    [ReadOnly] public GetUserInventoryRequest getUserInventory;
    [ReadOnly] private int failedCallbackCounter;
    //================================================================================

    public void UnlockIslandZones(string _zone)
    {
        switch (_zone)
        {
            case "MineA":
                PlayerData.CanAccessMineA = true;
                MineA.UnlockZone();
                break;
            case "MineB":
                PlayerData.CanAccessMineB = true;
                MineB.UnlockZone();
                break;
            case "FarmA":
                PlayerData.CanAccessFarmA = true;
                FarmA.UnlockZone();
                break;
            case "FarmB":
                PlayerData.CanAccessFarmB = true;
                FarmB.UnlockZone();
                break;
            case "PondA":
                PlayerData.CanAccessPondA = true;
                PondA.UnlockZone();
                break;
            case "PondB":
                PlayerData.CanAccessPondB = true;
                PondB.UnlockZone();
                break;
            case "ForestA":
                PlayerData.CanAccessForestA = true;
                ForestA.UnlockZone();
                break;
            case "ForestB":
                PlayerData.CanAccessForestB = true;
                ForestB.UnlockZone();
                break;
        }
    }

    public void ShowPurchasePanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(PurchaseRT, null, PurchaseCG, 0, 1, () => 
        {
            PurchaseTMP.text = "Would you like to purchase access to " + ClickedZone.ZoneName + " for " + ClickedZone.ZonePrice + " EZCoins?"; 
        });
    }

    public void HidePurchasePanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(PurchaseRT, PurchaseRT, PurchaseCG, 1, 0, () => { });
    }

    public void PurchaseThisZone()
    {
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.EZCoin -= ClickedZone.ZonePrice;
            LobbyCore.UpdateEZCoinDisplay();
            UnlockIslandZones(ClickedZone.ZoneName);
            ClickedZone.UnlockZone();
            HidePurchasePanel();
            ClickedZone = null;
        }
        else
            StartPurchasePlayFab();
    }

    private void StartPurchasePlayFab()
    {
        LobbyCore.DisplayLoadingPanel();
        startPurchase.CatalogVersion = "Zones";
        startPurchase.Items.Clear();
        startPurchase.Items.Add(new ItemPurchaseRequest() { ItemId = ClickedZone.ZoneName, Quantity = 1 });
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
                ClickedZone = null;
                HidePurchasePanel();
                LobbyCore.GetVirtualCurrencyPlayfab();
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
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
