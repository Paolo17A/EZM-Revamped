using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyBox;

public class IslandCore : MonoBehaviour
{
    //================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private LobbyCore LobbyCore;

    [Header("ISLAND ZONES")]
    [SerializeField] private ZoneController MineA;
    [SerializeField] private ZoneController MineB;
    [SerializeField] private ZoneController FarmA;
    [SerializeField] private ZoneController FarmB;
    [SerializeField] private ZoneController PondA;
    [SerializeField] private ZoneController PondB;
    [SerializeField] private ZoneController ForestA;
    [SerializeField] private ZoneController ForestB;

    [Header("PURCHASE VARIABLES")]
    [SerializeField] private RectTransform PurchaseRT;
    [SerializeField] private CanvasGroup PurchaseCG;
    [SerializeField] private TextMeshProUGUI PurchaseTMP;
    [SerializeField][ReadOnly] public ZoneController ClickedZone;

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
    }

}
