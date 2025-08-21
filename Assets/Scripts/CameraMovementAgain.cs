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
    private float zoomHeight;
    
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
        lastPosition = transform.position;
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.Enable();
    }
    
    // Update is called once per frame
    void Update()
    {
        GetKeyboardMovement();
        UpdateVelocity();
        UpdateBasePosition();
    }

    private void OnDisable()
    {
        cameraActions.Camera.Disable();
        // cameraActions.Disable(); // or this?
    }

    void UpdateVelocity()
    {
        horizontalVelocity = (transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0;
        lastPosition = transform.position;
    }

    void GetKeyboardMovement()
    {
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight()
                             + movement.ReadValue<Vector2>().y * GetCameraForward();
        inputValue = inputValue.normalized;
        if (inputValue.sqrMagnitude > .1f)
        {
            targetPosition += inputValue;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


}
