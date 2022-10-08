using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedZoneController : MonoBehaviour
{
    //====================================================================================
    [SerializeField] GameplayCore GameplayCore;
    [SerializeField] CinemachineMovementController VirtualCamera;
    [SerializeField] PlayerData PlayerData;

    [field: Header("EXTENSION DATA")]
    [field: SerializeField] public bool ExtensionUnlocked { get; set; }
    [field: SerializeField] public string ExtensionName { get; set; }
    [field: SerializeField] public int ExtensionPrice { get; set; }

    [field: Header("ZONE CLAMPS")]
    [field: SerializeField] public GameplayCore.Zones ThisZone { get; set; } 
    [field: SerializeField] public Vector3 defaultCameraPos { get; set; }
    [field: SerializeField] public float minXClamp { get; set; }
    [field: SerializeField] public float maxXClamp { get; set; }
    [field: SerializeField] public float minZClamp { get; set; }
    [field: SerializeField] public float maxZClamp { get; set; }
    //====================================================================================

    public void ProcessExtensionClick()
    {
        GameplayCore.ClickedExtension = this;
        if(ExtensionUnlocked)
        {
            Debug.Log("You will go to " + ExtensionName);
            GameplayCore.CurrentZone = ThisZone;
            VirtualCamera.destinationVector = defaultCameraPos;
            VirtualCamera.minXClamp = minXClamp;
            VirtualCamera.maxXClamp = maxXClamp;
            VirtualCamera.minZClamp = minZClamp;
            VirtualCamera.maxZClamp = maxZClamp;
            VirtualCamera.travelling = true;
        }
        else
        {
            if (PlayerData.EZCoin >= ExtensionPrice)
                GameplayCore.ShowPurchasePanel();
            else
                GameManager.Instance.DisplayErrorPanel("You do not have enough EZCoin to purchase this extension");
        }
    }

    public void UnlockExtension()
    {
        ExtensionUnlocked = true;
    }    
}
