using UnityEngine;
using UnityEngine.InputSystem ;

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
    [SerializeField] private float stepSize = 2f;
    [SerializeField] private float zoomDamping = 7.5f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float zoomSpeed = 2f;
    
    // rotation
    [SerializeField] private float maxRotationSpeed = 1f;
    
    // screen edge motion
    [SerializeField] [Range(0f, 0.1f)] private float edgeTolerance = 0.05f;
    [SerializeField] private bool useScreenEdge = true;

    private Vector3 targetPosition;
    private float zoomHeight;
    
    // used to track velocity w/o rigidbody
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;

    private Vector3 startDrag;

    
}
