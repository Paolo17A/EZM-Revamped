using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZoneController : MonoBehaviour
{
    //=======================================================================
    [SerializeField] private IslandCore IslandCore;
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] public ButtonScaler ButtonScaler;

    [field: Header("ZONE DATA")]
    [field: SerializeField] public bool ZoneUnlocked { get; set; }
    [field: SerializeField] public string ZoneName { get; set; }
    [field: SerializeField] public int ZonePrice { get; set; }

    [Header("ZONE SPRITES")]
    [SerializeField] private SpriteRenderer NameSprite;
    [SerializeField] private SpriteRenderer ZoneSprite;
    //=======================================================================

    public void UnlockZone()
    {
        ZoneUnlocked = true;
        NameSprite.color = new Color(255, 255, 255, 255);
        ZoneSprite.color = new Color(255, 255, 255, 255);
    }

    public void SetAsClickedZone()
    {
        IslandCore.ClickedZone = this;
    }

    public void ProcessZoneInteraction()
    {
        //IslandCore.ClickedZone = this;
        if (ZoneUnlocked)
        {
            Debug.Log("You will go to " + ZoneName);
            if (ZoneName == "MineA")
            {
                GameManager.Instance.InterstitialAd.willSwitchScene = true;
                GameManager.Instance.InterstitialAd.sceneToLoad = "MineAScene";
                GameManager.Instance.InterstitialAd.ShowAd();
            }
            else
                GameManager.Instance.DisplayErrorPanel("COMING SOON");

        }
        else if (PlayerData.EZCoin < ZonePrice)
            GameManager.Instance.DisplayErrorPanel("You do not have enough EZCoins to purchase access to this zone");
        else
            IslandCore.ShowPurchasePanel();
    }
}
