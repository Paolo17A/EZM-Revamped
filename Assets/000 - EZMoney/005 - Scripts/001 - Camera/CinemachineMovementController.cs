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
    [SerializeField][ReadOnly] public Vector3 defaultCenterPos;
    [SerializeField][ReadOnly] public float defaultMinXClamp;
    [SerializeField][ReadOnly] public float defaultMaxXClamp;
    [SerializeField][ReadOnly] public float defaultMinZClamp;
    [SerializeField][ReadOnly] public float defaultMaxZClamp;
    [SerializeField] public float minXClamp;
    [SerializeField] public float maxXClamp;
    [SerializeField] public float minZClamp;
    [SerializeField] public float maxZClamp;

    [Header("DEBUGGER")]
    Vector2 direction;
    [SerializeField][ReadOnly] public Vector3 destinationVector;
    [SerializeField] private bool isDragging;
    [SerializeField][ReadOnly] private bool flag;
    [SerializeField][ReadOnly] public bool travelling;
    //=================================================================================
    private void Start()
    {
        defaultCenterPos = VirtualCamera.transform.position;
        defaultMinXClamp = minXClamp;
        defaultMaxXClamp = maxXClamp;
        defaultMinZClamp = minZClamp;
        defaultMaxZClamp = maxZClamp;
    }

    void Update()
    {
        if (travelling)
            TravelCamera();
        else if (GameplayCore.CurrentGameplayState == GameplayCore.GameplayStates.CORE && !GameplayCore.PurchaseActive())
            PanCamera();

    }
    private void TravelCamera()
    {
        if(Vector3.Distance(transform.position, destinationVector) >= Mathf.Epsilon)
        {
            travelling = false;
        }
        else
        {
            VirtualCamera.transform.position = Vector3.MoveTowards(VirtualCamera.transform.position, destinationVector, 3 * Time.deltaTime);
        }
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
        direction = Camera.main.ScreenToWorldPoint(current_position) - Camera.main.ScreenToWorldPoint(hit_position);

        // Invert direction to that terrain appears to move with the mouse.
        direction = direction * -1f;

        target_position = new Vector3(camera_position.x + direction.x, VirtualCamera.position.y, camera_position.z + direction.y);
    }
}
