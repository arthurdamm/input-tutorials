using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovementAgain : MonoBehaviour
{
    private StrategyController cameraActions;

    private InputAction movement; // cached so we don't rely on events every frame
    private Transform cameraTransform;
    
    // horizontal motion
    [SerializeField]
    private float maxSpeed = 20f;
    private float speed;
    [SerializeField]
    private float acceleration = 10f;
    [SerializeField]
    private float damping = 15f;
    
    // vertical motion - zooming
    [SerializeField]
    private float stepSize = 2f;
    [SerializeField]
    private float zoomDamping = 7.5f;
    [SerializeField]
    private float minHeight = 5f;
    [SerializeField]
    private float maxHeight = 50f;
    [SerializeField]
    private float zoomSpeed = 2f;
    
    // rotation
    [SerializeField]
    private float maxRotationSpeed = 1f;
    
    // screen edge motion
    [SerializeField]
    private float edgeTolerance = .05f;
    [SerializeField]
    private bool useScreenEdge = true;

    // updated by various functions -- used to update position of base camera obj
    private Vector3 targetPosition;
    private float zoomHeight = 10f;
    
    // used to track velocity w/o rigidbody
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;
    
    // tracks where dragging action started
    private Vector3 startDrag;


    private void Awake()
    {
        cameraActions = new();
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(transform);
        Debug.Log($"camera local.y: {cameraTransform.localPosition.y} world.y: {cameraTransform.position.y}");
        lastPosition = transform.position;
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.RotateCamera.performed += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }
    
    private void OnDisable()
    {
        cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Disable();
        // cameraActions.Disable(); // or this?
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetKeyboardMovement();
        CheckMouseAtScreenEdge();
        UpdateVelocity();
        UpdateCameraPosition(); // why not after base?
        UpdateBasePosition();
    }
    
    void UpdateVelocity()
    {
        horizontalVelocity = (transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0;
        lastPosition = transform.position;
    }

    void GetKeyboardMovement()
    {
        Vector2 rawInputValue = movement.ReadValue<Vector2>();
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight()
                             + movement.ReadValue<Vector2>().y * GetCameraForward();
        
        inputValue = inputValue.normalized;
        if (inputValue.sqrMagnitude > .1f)
        {
            targetPosition += inputValue;
            Debug.Log($"GetKeyboardMovement() inputValue:{rawInputValue} delta:{inputValue} tp:{targetPosition}");
        }

    }

    private Vector3 GetCameraForward()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        return forward;
    }

    private Vector3 GetCameraRight()
    {
        Vector3 right = cameraTransform.right;
        right.y = 0;
        return right;
    }

    private void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > .1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * speed * Time.deltaTime;
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero; // reset every frame?
    }
    
    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.rightButton.isPressed)
        {
            return;
        }

        float value = inputValue.ReadValue<Vector2>().x;
        float currAngleY = transform.rotation.eulerAngles.y;
        
        // why not cameraTransform?
        transform.rotation = Quaternion.Euler(0f, value * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
        Debug.Log($"RotateCamera() readValue:{value:F2} currAngle:{currAngleY} deltaAngle:{value * maxRotationSpeed} Quaternion: " + transform.rotation);

    }
    
    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        float value = -inputValue.ReadValue<Vector2>().y / 100f;
        if (Mathf.Abs(value) > 0.1f)
        {
            zoomHeight = cameraTransform.localPosition.y + value * stepSize;
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

    private void CheckMouseAtScreenEdge()
    {
        if (!useScreenEdge)
        {
            return;
        }
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseDirection = Vector3.zero;

        if (mousePosition.x < edgeTolerance * Screen.width)
        {
            mouseDirection += -GetCameraRight();
        } else if (mousePosition.x > (1f - edgeTolerance) * Screen.width)
        {
            mouseDirection += GetCameraRight();
        }
        
        if (mousePosition.y < edgeTolerance * Screen.height)
        {
            mouseDirection += -GetCameraForward();
        } else if (mousePosition.y > (1f - edgeTolerance) * Screen.height)
        {
            mouseDirection += GetCameraForward();
        }

        targetPosition += mouseDirection;


    }
    
}
