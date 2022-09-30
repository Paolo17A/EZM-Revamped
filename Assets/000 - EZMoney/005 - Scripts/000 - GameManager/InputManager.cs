using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public GameController GameController { get; set; }
    public bool isPrimaryTouch;
    private void Awake()
    {
        GameController = new GameController();
    }

    private void OnEnable()
    {
        GameController.Enable();
        GameController.Menu.Interact.started += _ => InteractStarted(true);
        GameController.Menu.Interact.canceled += _ => InteractStarted(false);
    }

    private void OnDisable()
    {
        GameController.Disable();
        GameController.Menu.Interact.started -= _ => InteractStarted(true);
        GameController.Menu.Interact.canceled -= _ => InteractStarted(false);
    }

    private void InteractStarted(bool value)
    {
        isPrimaryTouch = value;
    }

    public Vector2 GetMousePosition()
    {
        return GameController.Menu.Hover.ReadValue<Vector2>();
    }

    public Vector2 GetDraggedPosition()
    {
        return GameController.Menu.Drag.ReadValue<Vector2>();
    }    
}
