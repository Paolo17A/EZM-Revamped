using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PlayFab.ClientModels;
using MyBox;

public class IslandController : MonoBehaviour
{
    //=============================================================================
    [SerializeField] private IslandCore IslandCore;

    [Header("DEBUGGER")]
    private GameObject shrunkenLevel;
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;
    [SerializeField][ReadOnly] private bool newlyReleased;
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
        if(GameManager.Instance.InputManager.isPrimaryTouch && !EventSystem.current.IsPointerOverGameObject())
        {
            mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.InputManager.GetMousePosition());
            mousePos2D = new Vector2(mousePos.x, mousePos.y);
            hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
            if(hit.collider && hit.transform.tag == "IslandZone")
            {
                hit.collider.gameObject.GetComponent<ButtonScaler>().PushButtonDown();
                hit.collider.gameObject.GetComponent<ZoneController>().SetAsClickedZone();
                newlyReleased = false;
            }
        }
        else if(!GameManager.Instance.InputManager.isPrimaryTouch && IslandCore.ClickedZone != null && !newlyReleased)
        {
            IslandCore.ClickedZone.GetComponent<ZoneController>().ButtonScaler.PushButtonUp();
            newlyReleased = true;

            mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.InputManager.GetMousePosition());
            mousePos2D = new Vector2(mousePos.x, mousePos.y);
            hit = Physics2D.Raycast(mousePos2D, Vector3.forward);
            if(hit.collider && hit.transform.tag == "IslandZone" && hit.transform.GetComponent<ZoneController>().ZoneName == IslandCore.ClickedZone.ZoneName)
                IslandCore.ClickedZone.ProcessZoneInteraction();
        }
    }
}
