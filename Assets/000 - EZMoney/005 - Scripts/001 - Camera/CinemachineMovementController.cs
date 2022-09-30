using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MyBox;
using Cinemachine;

public class CinemachineMovementController : MonoBehaviour
{
    //=================================================================================
    [SerializeField] private GameplayCore GameplayCore;
    [SerializeField] private Transform VirtualCamera;

    [Header("DRAG VARIABLES")]
    [SerializeField][ReadOnly] private Vector2 hit_position = Vector2.zero;
    [SerializeField][ReadOnly] private Vector2 current_position = Vector2.zero;
    [SerializeField][ReadOnly] private Vector3 camera_position = Vector3.zero;
    [SerializeField][ReadOnly] private Vector3 target_position;

    [Header("CLAMPS")]
    [SerializeField] private float minXClamp;
    [SerializeField] private float maxXClamp;
    [SerializeField] private float minZClamp;
    [SerializeField] private float maxZClamp;

    [SerializeField] private bool isDragging;
    [SerializeField][ReadOnly] private bool flag;
    //=================================================================================

    void Update()
    {
        if(GameplayCore.CurrentGameplayState == GameplayCore.GameplayStates.CORE)
            PanCamera();
    }

    private void PanCamera()
    {
        if(GameManager.Instance.InputManager.isPrimaryTouch)
        {
            if (hit_position == Vector2.zero)
            {
                hit_position = GameManager.Instance.InputManager.GetMousePosition();
                camera_position = VirtualCamera.transform.position;
            }
            current_position = GameManager.Instance.InputManager.GetMousePosition();

            if (current_position != hit_position)
            {
               Drag();
               flag = true;
            }
        }
        else
        {
            hit_position = Vector2.zero;
            camera_position = Vector2.zero;
            current_position = Vector2.zero;
        }

       if (flag)
        {
            VirtualCamera.transform.position = Vector3.Lerp(VirtualCamera.transform.position, new Vector3(Mathf.Clamp(target_position.x, minXClamp, maxXClamp),
                VirtualCamera.transform.position.y, Mathf.Clamp(target_position.z, minZClamp, maxZClamp)), Time.deltaTime * 5f);
            if (VirtualCamera.transform.position == target_position)//reached?
            {
                flag = false;// stop moving
            }
        }
    }
    private void Drag()
    {
        // Get direction of movement.  (Note: Don't normalize, the magnitude of change is going to be Vector3.Distance(current_position-hit_position)
        // anyways.  
        Vector2 direction = Camera.main.ScreenToWorldPoint(current_position) - Camera.main.ScreenToWorldPoint(hit_position);

        // Invert direction to that terrain appears to move with the mouse.
        direction = direction * -1f;

        target_position = new Vector3(camera_position.x + direction.x, VirtualCamera.position.y, camera_position.z + direction.y);
    }
}
