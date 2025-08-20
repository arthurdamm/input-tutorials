using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkAgain : MonoBehaviour
{
    [SerializeField] public InputActionAsset actionAsset;
    [SerializeField] private InputActionMap playerMap;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction lookAction;
    [SerializeField] private InputAction jumpAction;
    
    [SerializeField] private Animator animator;

    [SerializeField] private Rigidbody rb;

    [SerializeField] private float lookSpeed = 200f;

    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private Vector2 lookAmt;
    [SerializeField] private Vector2 moveAmt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        playerMap = actionAsset.FindActionMap("Player", true);
        playerMap.Enable();
    }

    private void OnDisable()
    {
        playerMap.Disable();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        jumpAction = playerMap.FindAction("Jump");
    }


    // Update is called once per frame
    void Update()
    {
        moveAmt = moveAction.ReadValue<Vector2>();
        lookAmt = lookAction.ReadValue<Vector2>();
        if (jumpAction.WasPerformedThisFrame())
        {
            Jump();
        }
        
    }

    private void FixedUpdate()
    {
        Walking();
        Rotating();
    }

    private void Rotating()
    {
        float angle = lookAmt.x * lookSpeed * Time.deltaTime;
        Quaternion q = Quaternion.Euler(0, angle, 0);
        rb.MoveRotation(rb.rotation * q);
    }

    private void Walking()
    {
        float distance = moveAmt.y * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + transform.forward * distance);
        animator.SetFloat("Speed", moveAmt.y);
    }

    private void Jump()
    {
        rb.AddForceAtPosition(new Vector3(0, 5f, 0), transform.position, ForceMode.Impulse);
    }
}
