using System;
using UnityEngine;
using UnityEngine.InputSystem ;
using UnityEngine.Rendering;

public class CameraMovementAndAgain : MonoBehaviour
{
    private StrategyControllerAgain cameraActions;

    private InputAction movement;

    [SerializeField]
    private Transform cameraTransform;

    // horizontal motion
    [SerializeField] private float maxSpeed = 5f;
    private float speed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float damping = 15f;
    
    // vertical motion - zoom
    [SerializeField] private float zoomStep = 2f;
    [SerializeField] private float zoomDamping = 7.5f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float zoomSpeed = 2f;
    
    // rotation
    [SerializeField] private float maxRotationSpeed = 1f;
    
    // screen edge motion
    [SerializeField] [Range(0f, 0.1f)] private float edgeTolerance = 0.05f;
    [SerializeField] private bool useScreenEdge = true;

    private Vector3 targetDirection;
    private float zoomHeight;
    
    // used to track velocity w/o rigidbody
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;

    private Vector3 startDrag;

    private Camera cam;

    private void Awake()
    {
        cameraActions = new();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        cam = Camera.main;
    }

    private void OnEnable()
    {
        lastPosition = transform.position;
        zoomHeight = cameraTransform.localPosition.y;
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.RotateCamera.performed += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Camera.Disable();
    }




    private void Update()
    {
        GetKeyboardMovement();
        CheckMouseAtScreenEdge();
        DragCamera();
        UpdateVelocity();
        UpdateBasePosition();
        UpdateCameraPosition();
    }

    private void UpdateVelocity()
    {
        horizontalVelocity = (transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0;
        lastPosition = transform.position;
    }

    private void GetKeyboardMovement()
    {
        Vector2 readValue = movement.ReadValue<Vector2>();
        Vector3 inputValue = readValue.x * GetCameraRight() + readValue.y * GetCameraForward();
        inputValue.Normalize();
        
        if (inputValue.sqrMagnitude > 0.1f)
        {
            targetDirection += inputValue;
        }
        
    }
    
    private void CheckMouseAtScreenEdge()
    {
        if (!useScreenEdge)
        {
            return;
        }
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseDirection = Vector3.zero;
        
        if (mousePosition.x < edgeTolerance * Screen.width)
            mouseDirection -= GetCameraRight();
        else if (mousePosition.x > (1f - edgeTolerance) * Screen.width)
            mouseDirection += GetCameraRight();

        if (mousePosition.y < edgeTolerance * Screen.height)
            mouseDirection -= GetCameraForward();
        else if (mousePosition.y > (1f - edgeTolerance) * Screen.height)
            mouseDirection += GetCameraForward();

        targetDirection += mouseDirection;
    }

    private void DragCamera()
    {
        if (!Mouse.current.middleButton.isPressed)
        {
            return;
        }

        var plane = new Plane(Vector3.up, Vector3.zero);

        if (cam.ScreenPointToRay(Mouse.current.position.ReadValue()) is { } ray
            && plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                startDrag = ray.GetPoint(distance);
            }
            else
            {
                targetDirection += startDrag - ray.GetPoint(distance);
            }
        }

    }

    private Vector3 GetCameraRight()
    {
        Vector3 right = cameraTransform.right;
        right.y = 0;
        return right;
    }
    
    private Vector3 GetCameraForward()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        return forward;
    }

    private void UpdateBasePosition()
    {
        if (targetDirection.sqrMagnitude > .1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, acceleration * Time.deltaTime);
            transform.position += targetDirection * (speed * Time.deltaTime);

        }
        else if (horizontalVelocity.sqrMagnitude > .1f)
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, damping * Time.deltaTime);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetDirection = Vector3.zero;
    }
    
    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.rightButton.isPressed)
        {
            return;
        }

        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, value * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
    }
    
    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        float value = -inputValue.ReadValue<Vector2>().y / 100f;
        if (Mathf.Abs(value) > 0.1f)
        {
            zoomHeight = cameraTransform.localPosition.y + value * zoomStep;
            zoomHeight = Mathf.Clamp(zoomHeight, minHeight, maxHeight);
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 zoomTarget = new Vector3(cameraTransform.localPosition.x, zoomHeight, cameraTransform.localPosition.z);
        zoomTarget -= zoomSpeed * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;
        
        cameraTransform.localPosition =
            Vector3.Lerp(cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDamping);
        cameraTransform.LookAt(transform);
    }
}
