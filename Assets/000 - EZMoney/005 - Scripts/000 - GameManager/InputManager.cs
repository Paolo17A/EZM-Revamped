using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public GameController GameController { get; set; }
    private void Awake()
    {
        GameController = new GameController();
    }

    private void OnEnable()
    {
        GameController.Enable();
    }

    private void OnDisable()
    {
        GameController.Disable();
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
