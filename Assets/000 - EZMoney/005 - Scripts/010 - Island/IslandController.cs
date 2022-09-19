using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PlayFab.ClientModels;

public class IslandController : MonoBehaviour
{
    //=============================================================================
    [SerializeField] private IslandCore IslandCore; 

    [Header("DEBUGGER")]
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;
    //=============================================================================
    private void Awake()
    {
        IslandCore.getUserData = new GetUserDataRequest();
        IslandCore.startPurchase = new StartPurchaseRequest();
        IslandCore.startPurchase.Items = new List<ItemPurchaseRequest>();
        IslandCore.payForPurchase = new PayForPurchaseRequest();
        IslandCore.confirmPurchase = new ConfirmPurchaseRequest();
        IslandCore.getUserInventory = new GetUserInventoryRequest();
    }

    private void Update()
    {
        if(GameManager.Instance.InputManager.GameController.Menu.Interact.triggered && !EventSystem.current.IsPointerOverGameObject())
        {
            mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.InputManager.GetMousePosition());
            mousePos2D = new Vector2(mousePos.x, mousePos.y);
            hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
            if(hit.collider && hit.transform.tag == "IslandZone")
            {
                hit.collider.GetComponent<ZoneController>().ProcessZoneInteraction();
            }
        }
    }
}
