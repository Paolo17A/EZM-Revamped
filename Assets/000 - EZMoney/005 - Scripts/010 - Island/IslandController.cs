using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandController : MonoBehaviour
{
    //=============================================================================
    [Header("DEBUGGER")]
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;
    //=============================================================================

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
